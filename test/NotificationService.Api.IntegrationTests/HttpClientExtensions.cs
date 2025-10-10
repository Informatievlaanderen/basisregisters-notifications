namespace NotificationService.Api.IntegrationTests;

using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Api.Notification;
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

    public static async Task<Notificatie[]> GetNotificaties(this HttpClient client, NotificationsFilter? filter = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "v1/notificaties");
        if (filter is not null)
        {
            request.Headers.Add("X-Filtering", JsonConvert.SerializeObject(filter));
        }

        var response = await client.SendAsync(request);
        response.Should().BeSuccessful();

        // Assert
        var notifications = JsonConvert.DeserializeObject<Notificatie[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();

        return notifications!;
    }

    public static Task PubliceerNotificatie(this HttpClient client, int notificationId) =>
        client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);

    public static Task NotificatieIntrekken(this HttpClient client, int notificationId) =>
        client.PostAsync($"v1/notificaties/{notificationId}/acties/intrekken", null);

}
