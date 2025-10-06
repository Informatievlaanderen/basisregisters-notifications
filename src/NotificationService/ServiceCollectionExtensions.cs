namespace NotificationService;

using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions;
using Marten;
using Marten.Schema.Identity;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenNotification(this IServiceCollection services, string connectionString)
    {
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Schema.For<Notification>()
                .IdStrategy(new CombGuidIdGeneration())
                .Identity(x => x.NotificationId);

            options.UseSystemTextJsonForSerialization();

            // Optionally configure the serializer directly
            var systemTextJsonSerializer = new SystemTextJsonSerializer
            {
                // Optionally override the enum storage
                EnumStorage = EnumStorage.AsString,

                // Optionally override the member casing
                Casing = Casing.CamelCase,
            };

            systemTextJsonSerializer.Configure(serializerOptions =>
            {
                serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            });

            options.Serializer(systemTextJsonSerializer);
        });

        services.AddSingleton<INotifications, MartenNotifications>();
        return services;
    }
}
