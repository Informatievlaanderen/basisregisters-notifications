namespace NotificationService.Api.IntegrationTests.Notification;

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NotificationService.Api.Abstractions;
using Xunit;

[Collection("DockerFixtureCollection")]
public class WhenDeletingNotification : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenDeletingNotification(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DeleteNotification_WithDraftStatus_ReturnsNoContent()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification (status will be Draft)
        var createResponse = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor verwijderen",
            Titel = "Test Titel voor Verwijderen",
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

        // Delete notification
        var deleteResponse = await client.DeleteAsync($"v1/notificaties/{notificationId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteNotification_WithInvalidId_ReturnsNotFound()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var invalidNotificationId = 999999;

        // Try to delete non-existent notification
        var deleteResponse = await client.DeleteAsync($"v1/notificaties/{invalidNotificationId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteNotification_WithPublishedStatus_ReturnsBadRequest()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification
        var createResponse = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor delete test",
            Titel = "Test Titel voor Delete met Published Status",
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

        // Publish the notification
        var publishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Try to delete published notification (should fail)
        var deleteResponse = await client.DeleteAsync($"v1/notificaties/{notificationId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteNotification_WithUnpublishedStatus_ReturnsBadRequest()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create notification
        var createResponse = await client.PostAsJsonUsingNewtonsoftAsync("v1/notificaties", new MaakNotificatieRequest
        {
            Inhoud = "#Test Inhoud voor delete test",
            Titel = "Test Titel voor Delete met Unpublished Status",
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

        // Publish the notification
        var publishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Unpublish the notification
        var unpublishResponse = await client.PostAsync($"v1/notificaties/{notificationId}/acties/intrekken", null);
        unpublishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Try to delete unpublished notification (should fail)
        var deleteResponse = await client.DeleteAsync($"v1/notificaties/{notificationId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

