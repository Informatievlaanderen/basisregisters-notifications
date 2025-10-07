namespace NotificationService.Api.IntegrationTests;

using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

public sealed class NotificationServiceTests : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public NotificationServiceTests(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateNotification()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken($"{Scopes.DvArAdresUitzonderingen} {Scopes.DvGrGeschetstgebouwUitzonderingen}"));

        // create notification
        var notificationId = 0;
        var response = await client.PostAsync("v1/notificaties", JsonContent.Create(new MaakNotificatie
        {
            Inhoud = "#Test Inhoud\n * Test item 1\n * Test item 2",
            Titel = "Test Titel",
            Platformen = [Platform.Geoit, Platform.Lara],
            Rollen = [Rol.InterneBeheerder, Rol.NietIngelogd, Rol.StandaardGebruiker],
            Ernst = Ernst.Waarschuwing,
            GeldigVanaf = DateTimeOffset.Now,
            GeldigTot = DateTimeOffset.Now.AddDays(1),
            Sluitbaar = false,
            Links = [new NotificatieLink("informatie", "https://basisregisters.vlaanderen.be/nl")]
        }));

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
