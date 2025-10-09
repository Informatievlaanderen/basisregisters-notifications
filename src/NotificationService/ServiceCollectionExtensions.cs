namespace NotificationService;

using System.Text.Json;
using System.Text.Json.Serialization;
using Marten;
using Marten.Schema.Identity.Sequences;
using Microsoft.Extensions.DependencyInjection;
using Notification;
using Weasel.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenNotification(this IServiceCollection services, string connectionString)
    {
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;

            var hiloSettings = new HiloSettings { MaxLo = 1 };
            options.Schema.For<Notification.Notification>()
                .Identity(x => x.NotificationId)
                .IdStrategy(new HiloIdGeneration(typeof(Notification.Notification), hiloSettings))
                .HiloSettings(hiloSettings);

            options.UseSystemTextJsonForSerialization(EnumStorage.AsString, Casing.CamelCase, serializerOptions =>
            {
                serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });

            // options.CreateDatabasesForTenants(options =>
            // {
            //     var dbName = connectionString.Split(";")
            //         .Where(x => x.StartsWith("Database="))
            //         .Select(x => x.Substring("Database=".Length))
            //         .FirstOrDefault();
            //     options.ForTenant(dbName ?? "notifications");
            // });
        });

        services.AddSingleton<INotificationsRepository, MartenNotificationsRepository>();
        return services;
    }
}
