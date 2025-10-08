namespace NotificationService.Api.IntegrationTests;

using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

internal static class Extensions
{
    public static Task<HttpResponseMessage> PostAsJsonUsingNewtonsoftAsync<TValue>(this HttpClient client, string requestUri, TValue value, CancellationToken cancellationToken = default)
        => client.PostAsync(requestUri,
            new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"),
            cancellationToken);
}
