namespace NotificationService.Api.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.DockerUtilities;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;
using static JwtTokenHelper;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateGetUpdatePendingErrorCompleteDelete()
    {
        // start postgres
        using var _ = DockerComposer.Compose("postgres_test.yml", "notification-service-integration-tests");

        await CreateDatabase("Host=localhost;Port=5433;Username=postgres;Password=postgres", "notifications");

        // construct claims identity
        var claimsIdentity = new ClaimsIdentity([new Claim("internal", "true")]);
        var jwtToken = CreateJwtToken(claimsIdentity);

        var application = new WebApplicationFactory<Startup>()
          .WithWebHostBuilder(builder =>
          {
              const string connectionString = "Host=localhost;Port=5433;Database=notifications;Username=postgres;Password=postgres";
              builder.ConfigureServices(services => services
                .AddScoped(_ => new ClaimsPrincipal(claimsIdentity))
                .AddMartenNotification(connectionString));
          });

        var client = application.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(jwtToken);

        // create notification
        const string originator = "originator";
        var notificationId = Guid.Empty;
        var response = await client.PostAsync("/notifications/create", JsonContent.Create(new Dictionary<string, string> { { originator, originator } }));

        if (response.IsSuccessStatusCode)
        {
            notificationId = await JsonSerializer.DeserializeAsync<Guid>(await response.Content.ReadAsStreamAsync());
        }
    }

    private async Task CreateDatabase(string connectionString, string database)
    {
        var createDbQuery = $"CREATE DATABASE {database}";

        await using var connection = new NpgsqlConnection(connectionString);
        await using var command = new NpgsqlCommand(createDbQuery, connection);

        var attempt = 1;
        while (attempt <= 5)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (Exception)
            {
                attempt++;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        await command.ExecuteNonQueryAsync();
    }
}
