namespace NotificationService.Api.IntegrationTests;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class AcmIdmTests : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public AcmIdmTests(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("/v1/notificaties", "dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen dv_wr_uitzonderingen")]
    public async Task ReturnsSuccess(string endpoint, string requiredScopes)
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));

        var response = await client.PostAsync(endpoint,
            new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/notificaties")]
    public async Task ReturnsUnauthorized(string endpoint)
    {
        var client = _fixture.TestServer.CreateClient();

        var response = await client.PostAsync(endpoint,
            new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/notificaties")]
    //[InlineData("/v2/adressen/acties/snapshot", "dv_ar_adres_beheer")]
    public async Task ReturnsForbidden(string endpoint, string scope = "")
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(scope));

        var response = await client.PostAsync(endpoint,
            new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
