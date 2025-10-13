namespace NotificationService.Api.IntegrationTests.Notification;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Abstractions;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

[Collection("DockerFixtureCollection")]
public class WhenCreatingNotification : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenCreatingNotification(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateNotification()
    {
        var client = _fixture.TestServer.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // create notification
        var notificationId = 0;
        var maakNotificatieRequest = new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud\n * Test item 1\n * Test item 2",
            Titel = "Test Titel",
            Platformen = [Platform.Geoit, Platform.Lara],
            Rollen = [Rol.InterneBeheerder, Rol.NietIngelogd, Rol.StandaardGebruiker],
            Ernst = NotificatieErnst.Waarschuwing,
            GeldigVanaf = DateTimeOffset.Now,
            GeldigTot = DateTimeOffset.Now.AddDays(1),
            KanSluiten = false,
            Links = [new MaakNotificatieLink("informatie", "https://basisregisters.vlaanderen.be/nl")]
        };

        var response = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", maakNotificatieRequest);

        if (response.IsSuccessStatusCode)
        {
            var createResult = JsonConvert.DeserializeObject<NotificatieAangemaakt>(await response.Content.ReadAsStringAsync());
            createResult.Should().NotBeNull();
            notificationId = createResult!.NotificatieId;
        }

        response.IsSuccessStatusCode.Should().BeTrue();
        notificationId.Should().BeGreaterThan(0);
    }
}
