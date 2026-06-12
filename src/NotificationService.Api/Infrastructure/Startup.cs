namespace NotificationService.Api.Infrastructure;

using System;
using System.Linq;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Configuration;
using Duende.AspNetCore.Authentication.OAuth2Introspection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

/// <summary>Represents the startup process for the application.</summary>
public class Startup
{
    private const string DatabaseTag = "db";

    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Configures services for the application.</summary>
    /// <param name="services">The collection of services to configure the application with.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        var oAuth2IntrospectionOptions = _configuration
            .GetSection(nameof(OAuth2IntrospectionOptions))
            .Get<OAuth2IntrospectionOptions>();

        var baseUrl = _configuration.GetValue<string>("BaseUrl")!.TrimEnd('/');

        services.AddAcmIdmAuthentication(oAuth2IntrospectionOptions!);

        services
            .ConfigureDefaultForApi<Startup>(new StartupConfigureOptions
                {
                    Cors =
                    {
                        Origins = _configuration
                            .GetSection("Cors")
                            .GetChildren()
                            .Select(c => c.Value)
                            .ToArray()!
                    },
                    Server =
                    {
                        BaseUrl = baseUrl
                    },
                    Swagger =
                    {
                        ApiInfo = (_, description) => new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = "Basisregisters Vlaanderen Notificaties API",
                            Description = GetApiLeadingText(description),
                            Contact = new OpenApiContact
                            {
                                Name = "Digitaal Vlaanderen",
                                Email = "digitaal.vlaanderen@vlaanderen.be",
                                Url = new Uri("https://backoffice.basisregisters.vlaanderen")
                            }
                        },
                        XmlCommentPaths = [typeof(Startup).GetTypeInfo().Assembly.GetName().Name!]
                    },
                    MiddlewareHooks =
                    {
                        AfterHealthChecks = health =>
                        {
                            var connectionStrings = _configuration
                                .GetSection("ConnectionStrings")
                                .GetChildren();

                            foreach (var connectionString in connectionStrings)
                            {
                                health.AddNpgSql(
                                    connectionString.Value!,
                                    name: $"npgsql-{connectionString.Key.ToLowerInvariant()}",
                                    tags: [DatabaseTag, "sql", "npgsql"]);
                            }
                        },
                        Authorization = options => { options.AddAddressPolicies([]).AddBuildingPolicies([]).AddRoadPolicies([]); }
                    }
                }
                .EnableJsonErrorActionFilterOption())
            .AddValidatorsFromAssemblyContaining<Startup>()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public void Configure(
        IServiceProvider serviceProvider,
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IHostApplicationLifetime appLifetime,
        ILoggerFactory loggerFactory,
        IApiVersionDescriptionProvider apiVersionProvider,
        HealthCheckService healthCheckService)
    {
        app
            .UseDefaultForApi(new StartupUseOptions
            {
                Common =
                {
                    ApplicationContainer = serviceProvider.GetAutofacRoot(),
                    ServiceProvider = serviceProvider,
                    HostingEnvironment = env,
                    ApplicationLifetime = appLifetime,
                    LoggerFactory = loggerFactory,
                },
                Api =
                {
                    VersionProvider = apiVersionProvider,
                    Info = groupName => $"Basisregisters Vlaanderen - Notificaties API {groupName}",
                    CSharpClientOptions =
                    {
                        ClassName = ".",
                        Namespace = "."
                    },
                    TypeScriptClientOptions =
                    {
                        ClassName = "."
                    }
                },
                Server =
                {
                    PoweredByName = "Vlaamse overheid - Basisregisters Vlaanderen",
                    ServerName = "Digitaal Vlaanderen"
                },
                MiddlewareHooks =
                {
                    EnableAuthorization = true,
                    AfterMiddleware = x => x.UseMiddleware<AddNoCacheHeadersMiddleware>(),
                }
            });

        StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();
    }

    private static string GetApiLeadingText(ApiVersionDescription description)
        => $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Notificaties API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
}
