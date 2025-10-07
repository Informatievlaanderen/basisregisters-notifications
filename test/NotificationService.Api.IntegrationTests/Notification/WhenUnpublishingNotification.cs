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
public class WhenUnpublishingNotification
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenUnpublishingNotification(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UnpublishNotification_WithValidId_ReturnsNoContent()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification first
        var createResponse = await client.PostAsync("v1/notificaties", JsonContent.Create(new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor intrekken",
            Titel = "Test Titel voor Intrekken",
            Platformen = [Platform.Geoit, Platform.Lara],
            Rollen = [Rol.InterneBeheerder, Rol.NietIngelogd, Rol.StandaardGebruiker],
            Ernst = Ernst.Informatie,
            GeldigVanaf = DateTimeOffset.Now,
            GeldigTot = DateTimeOffset.Now.AddDays(1),
            KanSluiten = true,
            Links = [new NotificatieLink("informatie", "https://basisregisters.vlaanderen.be/nl")]
        }));

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createResult = JsonConvert.DeserializeObject<NotificatieAangemaakt>(await createResponse.Content.ReadAsStringAsync());
        var notificationId = createResult!.NotificatieId;

        // Publish notification first
        var publishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Unpublish notification
        var unpublishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/intrekken", null);

        unpublishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnpublishNotification_WithInvalidId_ReturnsNotFound()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var invalidNotificationId = 999999;

        // Try to unpublish non-existent notification
        var unpublishResponse = await client.PostAsync($"v1/notificaties/{invalidNotificationId}/acties/intrekken", null);

        unpublishResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnpublishNotification_WithInvalidStatus_ReturnsBadRequest()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification (status will be Draft)
        var createResponse = await client.PostAsync("v1/notificaties", JsonContent.Create(new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor invalid status test",
            Titel = "Test Titel voor Invalid Status",
            Platformen = [Platform.Geoit, Platform.Lara],
            Rollen = [Rol.InterneBeheerder, Rol.NietIngelogd, Rol.StandaardGebruiker],
            Ernst = Ernst.Informatie,
            GeldigVanaf = DateTimeOffset.Now,
            GeldigTot = DateTimeOffset.Now.AddDays(1),
            KanSluiten = true,
            Links = [new NotificatieLink("informatie", "https://basisregisters.vlaanderen.be/nl")]
        }));

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createResult = JsonConvert.DeserializeObject<NotificatieAangemaakt>(await createResponse.Content.ReadAsStringAsync());
        var notificationId = createResult!.NotificatieId;

        // Try to unpublish notification that is in Draft status (not Published)
        var unpublishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/intrekken", null);

        unpublishResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
