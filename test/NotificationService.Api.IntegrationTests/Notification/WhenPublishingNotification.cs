namespace NotificationService.Api.IntegrationTests.Notification;

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentAssertions;
using Newtonsoft.Json;
using NotificationService.Api.Abstractions;
using Xunit;

[Collection("NotificationServiceCollection")]
public class WhenPublishingNotification
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenPublishingNotification(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PublishNotification_WithValidId_ReturnsNoContent()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification first
        var createResponse = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor publicatie",
            Titel = "Test Titel voor Publicatie",
            Platformen = [Platform.Geoit, Platform.Lara],
            Rollen = [Rol.InterneBeheerder, Rol.NietIngelogd, Rol.StandaardGebruiker],
            Ernst = Ernst.Informatie,
            GeldigVanaf = DateTimeOffset.Now,
            GeldigTot = DateTimeOffset.Now.AddDays(1),
            KanSluiten = true,
            Links = [new MaakNotificatieLink("informatie", "https://basisregisters.vlaanderen.be/nl")]
        });

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createResult = JsonConvert.DeserializeObject<NotificatieAangemaakt>(await createResponse.Content.ReadAsStringAsync());
        var notificationId = createResult!.NotificatieId;

        // Publish notification
        var publishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PublishNotification_WithInvalidId_ReturnsNotFound()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var invalidNotificationId = 999999;

        // Try to publish non-existent notification
        var publishResponse = await client.PostAsync($"v1/notificaties/{invalidNotificationId}/acties/publiceren", null);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
