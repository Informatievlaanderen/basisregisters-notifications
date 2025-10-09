namespace NotificationService.Api.IntegrationTests;

using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using FluentAssertions;
using Newtonsoft.Json;

internal static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostAsJsonUsingNewtonsoftAsync<TValue>(this HttpClient client, string requestUri, TValue value, CancellationToken cancellationToken = default)
        => client.PostAsync(requestUri,
            new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"),
            cancellationToken);

    public static async Task<NotificatieAangemaakt> MaakNotificatie(this HttpClient client, MaakNotificatieRequest request)
    {
        var response = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", request);
        response.Should().BeSuccessful();

        var createResult = JsonConvert.DeserializeObject<NotificatieAangemaakt>(await response.Content.ReadAsStringAsync());
        createResult.Should().NotBeNull();
        return createResult!;
    }

    public static Task PubliceerNotificatie(this HttpClient client, int notificationId) =>
        client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);

    public static Task NotificatieIntrekken(this HttpClient client, int notificationId) =>
        client.PostAsync($"v1/notificaties/{notificationId}/acties/intrekken", null);

}
