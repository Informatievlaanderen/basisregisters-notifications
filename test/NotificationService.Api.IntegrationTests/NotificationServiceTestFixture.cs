namespace NotificationService.Api.IntegrationTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.DockerUtilities;
using Ductus.FluentDocker.Services;
using IdentityModel;
using IdentityModel.Client;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Xunit;

[CollectionDefinition("NotificationServiceCollection")]
public class NotificationServiceCollection : ICollectionFixture<NotificationServiceTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class NotificationServiceTestFixture : IAsyncLifetime
{
    internal const string RequiredScopes = $"{Scopes.DvArAdresUitzonderingen} {Scopes.DvGrGeschetstgebouwUitzonderingen} {Scopes.DvGrIngemetengebouwUitzonderingen} {Scopes.DvWrUitzonderingenBeheer}";

    private string _clientId;
    private string _clientSecret;
    private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();
    private ICompositeService _docker;

    public TestServer TestServer { get; private set; }

    public async Task InitializeAsync()
    {
        JsonConvert.DefaultSettings = new JsonSerializerSettings().ConfigureDefaultForApi;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        _clientId = configuration.GetValue<string>("ClientId")!;
        _clientSecret = configuration.GetValue<string>("ClientSecret")!;

        // start postgres
        _docker = DockerComposer.Compose("postgres_test.yml", "notification-service-integration-tests");

        await CreateDatabase(configuration.GetConnectionString("Marten")!, "notifications");

        var hostBuilder = new WebHostBuilder()
            .UseConfiguration(configuration)
            .UseStartup<Startup>()
            .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
            .UseTestServer();

        TestServer = new TestServer(hostBuilder);
    }

    public async Task<string> GetAccessToken(string requiredScopes = "")
    {
        if (_accessTokens.ContainsKey(requiredScopes) && !_accessTokens[requiredScopes].IsExpired)
        {
            return _accessTokens[requiredScopes].Token;
        }

        var tokenClient = new TokenClient(
            () => new HttpClient(),
            new TokenClientOptions
            {
                Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Parameters = new Parameters([new KeyValuePair<string, string>("scope", requiredScopes)])
            });

        var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

        _accessTokens[requiredScopes] = new AccessToken(response.AccessToken, response.ExpiresIn);

        return _accessTokens[requiredScopes].Token;
    }

    public Task DisposeAsync()
    {
        _docker.Dispose();
        return Task.CompletedTask;
    }

    private async Task CreateDatabase(string connectionString, string database)
    {
        var createDbQuery = $"CREATE DATABASE \"{database}\"";

        await using var connection = new NpgsqlConnection(connectionString);
        await using var command = new NpgsqlCommand(createDbQuery, connection);

        var attempt = 1;
        while (attempt <= 5)
        {
            try
            {
                await connection.OpenAsync();
                break;
            }
            catch
            {
                attempt++;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        await command.ExecuteNonQueryAsync();
    }
}

public class AccessToken
{
    private readonly DateTime _expiresAt;

    public string Token { get; }

    // Let's regard it as expired 10 seconds before it actually expires.
    public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

    public AccessToken(string token, int expiresIn)
    {
        _expiresAt = DateTime.Now.AddSeconds(expiresIn);
        Token = token;
    }
}
