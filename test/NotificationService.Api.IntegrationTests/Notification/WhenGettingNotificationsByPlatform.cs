namespace NotificationService.Api.IntegrationTests.Notification;

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NotificationService.Notification;
using Xunit;

[Collection("NotificationServiceCollection")]
public class WhenGettingNotificationsByPlatform
{
    private readonly NotificationServiceTestFixture _fixture;

    private const string RequiredScopes = $"{Scopes.DvArAdresBeheer}";

    public WhenGettingNotificationsByPlatform(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetNotificationsByPlatform_ReturnsNotificationsForSpecificPlatform()
    {
        var clientIntern = _fixture.TestServer.CreateClient();
        clientIntern.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var clientExtern = _fixture.TestServer.CreateClient();
        clientExtern.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(RequiredScopes));

        // Create a notification for Geoit platform
        var geoitNotificationRequest = new MaakNotificatieRequest
        {
            Inhoud = "#Geoit Test Notification\n * Test item 1",
            Titel = "Geoit Test Titel",
            Platformen = [Platform.Geoit],
            Rollen = [Rol.InterneBeheerder],
            Ernst = Ernst.Informatie
        };
        var createResult = await clientIntern.MaakNotificatie(geoitNotificationRequest);
        await clientIntern.PubliceerNotificatie(createResult.NotificatieId);

        // Create a notification for Lara platform
        var laraNotificationRequest = new MaakNotificatieRequest
        {
            Inhoud = "#Lara Test Notification\n * Test item 1",
            Titel = "Lara Test Titel",
            Platformen = [Platform.Lara],
            Rollen = [Rol.InterneBeheerder],
            Ernst = Ernst.Waarschuwing
        };

        createResult = await clientIntern.MaakNotificatie(laraNotificationRequest);
        await clientIntern.PubliceerNotificatie(createResult.NotificatieId);

        // Get notifications for Geoit platform
        var getGeoitResponse = await clientExtern.GetAsync("v1/notificaties/geoit");
        getGeoitResponse.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<Notificatie[]>(await getGeoitResponse.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();
        notifications.Should().NotBeEmpty();

        // Verify that all returned notifications contain only the Geoit platform
        foreach (var notification in notifications!)
        {
            notification.Platformen.Should().Contain(Platform.Geoit);
            notification.Platformen.Should().NotContain(Platform.Lara);
        }

        // Get notifications for Lara platform
        var getResponseLara = await clientExtern.GetAsync("v1/notificaties/lara");
        getResponseLara.Should().BeSuccessful();

        var laraNotifications = JsonConvert.DeserializeObject<Notificatie[]>(await getResponseLara.Content.ReadAsStringAsync());
        laraNotifications.Should().NotBeNull();
        laraNotifications.Should().NotBeEmpty();

        // Verify that all returned notifications contain the Lara platform
        foreach (var notification in laraNotifications!)
        {
            notification.Platformen.Should().Contain(Platform.Lara);
            notification.Platformen.Should().NotContain(Platform.Geoit);
        }
    }

    [Theory]
    [InlineData("geoit")]
    [InlineData("lara")]
    public async Task GetNotificationsByPlatform_WithValidPlatform_ReturnsOk(string platform)
    {
        var client = _fixture.TestServer.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(RequiredScopes));

        var response = await client.GetAsync($"v1/notificaties/{platform}");
        response.Should().BeSuccessful();
    }

    [Fact]
    public async Task GivenInactiveNotifications_ReturnsOnlyActiveNotifications()
    {
        var clientExtern = _fixture.TestServer.CreateClient();
        clientExtern.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(RequiredScopes));

        var notifications = _fixture.TestServer.Services.GetRequiredService<INotifications>();

        var inactiveNotificationId = await notifications.CreateNotification(
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1),
            Severity.Information,
            "abc",
            "abc",
            [nameof(Platform.Lara)],
            [nameof(Rol.InterneBeheerder)],
            false,
            []
        );

        var activeNotificationId = await notifications.CreateNotification(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            Severity.Information,
            "abc",
            "abc",
            [nameof(Platform.Lara)],
            [nameof(Rol.InterneBeheerder)],
            false,
            []
        );
        await notifications.PublishNotification(activeNotificationId);

        // Get notifications for Lara platform
        var getResponseLara = await clientExtern.GetAsync("v1/notificaties/lara");
        getResponseLara.Should().BeSuccessful();

        // Assert
        var laraNotifications = JsonConvert.DeserializeObject<Notificatie[]>(await getResponseLara.Content.ReadAsStringAsync());
        laraNotifications.Should().NotBeNull();
        laraNotifications.Should().HaveCount(1);
        laraNotifications!.Single().NotificatieId.Should().Be(activeNotificationId);

        var allNotifications = await notifications.GetNotifications();
        allNotifications.Should().NotBeNull();
        allNotifications.Should().HaveCount(2);
        allNotifications.Should().Contain(x => x.NotificationId == inactiveNotificationId);
    }

    [Fact]
    public async Task GivenDraftOrUnpublishedNotifications_ReturnsOnlyPublishedNotifications()
    {
        var clientExtern = _fixture.TestServer.CreateClient();
        clientExtern.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(RequiredScopes));

        var notifications = _fixture.TestServer.Services.GetRequiredService<INotifications>();

        (await notifications.GetNotifications()).Should().BeEmpty();

        var draftNotificationId = await notifications.CreateNotification(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            Severity.Information,
            "abc",
            "abc",
            [nameof(Platform.Lara)],
            [nameof(Rol.InterneBeheerder)],
            false,
            []
        );

        var unpublishedNotificationId = await notifications.CreateNotification(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            Severity.Information,
            "abc",
            "abc",
            [nameof(Platform.Lara)],
            [nameof(Rol.InterneBeheerder)],
            false,
            []
        );
        await notifications.PublishNotification(unpublishedNotificationId);
        await notifications.UnpublishNotification(unpublishedNotificationId);

        var publishedNotificationId = await notifications.CreateNotification(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            Severity.Information,
            "abc",
            "abc",
            [nameof(Platform.Lara)],
            [nameof(Rol.InterneBeheerder)],
            false,
            []
        );
        await notifications.PublishNotification(publishedNotificationId);

        // Get notifications for Lara platform
        var getResponseLara = await clientExtern.GetAsync("v1/notificaties/lara");
        getResponseLara.Should().BeSuccessful();

        // Assert
        var laraNotifications = JsonConvert.DeserializeObject<Notificatie[]>(await getResponseLara.Content.ReadAsStringAsync());
        laraNotifications.Should().NotBeEmpty();
        laraNotifications!.All(x => x.Status == NotificatieStatus.Gepubliceerd).Should().BeTrue();

        var allNotifications = await notifications.GetNotifications();
        allNotifications.Should().NotBeEmpty();
        allNotifications.Should().Contain(x => x.NotificationId == draftNotificationId);
        allNotifications.Should().Contain(x => x.NotificationId == publishedNotificationId);
        allNotifications.Should().Contain(x => x.NotificationId == unpublishedNotificationId);
    }
}
