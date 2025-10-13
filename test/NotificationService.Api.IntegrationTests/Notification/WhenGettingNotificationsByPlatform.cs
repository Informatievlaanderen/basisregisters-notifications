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

[Collection("DockerFixtureCollection")]
public class WhenGettingNotificationsByPlatform : IClassFixture<NotificationServiceTestFixture>
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
            Ernst = NotificatieErnst.Informatie
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
            Ernst = NotificatieErnst.Waarschuwing
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

        var notificationsRepository = _fixture.TestServer.Services.GetRequiredService<INotificationsRepository>();

        var inactiveNotificationId = await notificationsRepository.CreateNotification(
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

        var activeNotificationId = await notificationsRepository.CreateNotification(
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
        await notificationsRepository.PublishNotification(activeNotificationId);

        // Get notifications for Lara platform
        var getResponseLara = await clientExtern.GetAsync("v1/notificaties/lara");
        getResponseLara.Should().BeSuccessful();

        // Assert
        var laraNotifications = JsonConvert.DeserializeObject<Notificatie[]>(await getResponseLara.Content.ReadAsStringAsync());
        laraNotifications.Should().NotBeNull();
        laraNotifications.Should().HaveCount(1);
        laraNotifications!.Single().NotificatieId.Should().Be(activeNotificationId);

        var allNotifications = await notificationsRepository.GetNotifications();
        allNotifications.Should().NotBeNull();
        allNotifications.Should().HaveCount(2);
        allNotifications.Should().Contain(x => x.NotificationId == inactiveNotificationId);
    }

    [Fact]
    public async Task GivenDraftOrUnpublishedNotifications_ReturnsOnlyPublishedNotifications()
    {
        var clientExtern = _fixture.TestServer.CreateClient();
        clientExtern.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(RequiredScopes));

        var notificationsRepository = _fixture.TestServer.Services.GetRequiredService<INotificationsRepository>();

        var draftNotificationId = await notificationsRepository.CreateNotification(
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

        var unpublishedNotificationId = await notificationsRepository.CreateNotification(
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
        await notificationsRepository.PublishNotification(unpublishedNotificationId);
        await notificationsRepository.UnpublishNotification(unpublishedNotificationId);

        var publishedNotificationId = await notificationsRepository.CreateNotification(
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
        await notificationsRepository.PublishNotification(publishedNotificationId);

        // Get notifications for Lara platform
        var getResponseLara = await clientExtern.GetAsync("v1/notificaties/lara");
        getResponseLara.Should().BeSuccessful();

        // Assert
        var laraNotifications = JsonConvert.DeserializeObject<Notificatie[]>(await getResponseLara.Content.ReadAsStringAsync());
        laraNotifications.Should().NotBeEmpty();
        laraNotifications!.All(x => x.Status == NotificatieStatus.Gepubliceerd).Should().BeTrue();

        var allNotifications = await notificationsRepository.GetNotifications();
        allNotifications.Should().NotBeEmpty();
        allNotifications.Should().Contain(x => x.NotificationId == draftNotificationId);
        allNotifications.Should().Contain(x => x.NotificationId == publishedNotificationId);
        allNotifications.Should().Contain(x => x.NotificationId == unpublishedNotificationId);
    }
}
