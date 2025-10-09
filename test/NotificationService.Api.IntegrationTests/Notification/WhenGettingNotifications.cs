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

[Collection("NotificationServiceCollection")]
public class WhenGettingNotifications
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenGettingNotifications(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WithoutFilters_ReturnsAllNotificationsSortedByValidFromDescending()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create multiple notifications with different GeldigVanaf dates
        await CreateNotification(client, DateTimeOffset.Now.AddDays(-2));
        await CreateNotification(client, DateTimeOffset.Now);
        await CreateNotification(client, DateTimeOffset.Now.AddDays(-1));

        // Get all notifications
        var response = await client.GetAsync("v1/notificaties");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<Notificatie[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();
        notifications.Should().NotBeEmpty();

        // Verify sorting by GeldigVanaf descending
        for (var i = 0; i < notifications!.Length - 1; i++)
        {
            notifications[i].GeldigVanaf.Should().BeOnOrAfter(notifications[i + 1].GeldigVanaf);
        }
    }

    [Theory]
    [InlineData("Concept")]
    [InlineData("Gepubliceerd")]
    [InlineData("Ingetrokkken")]
    public async Task WithStatusFilter_ReturnsNotificationsWithSpecificStatus(string statusFilter)
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Create a notification
        var draftNotificationId = await CreateNotification(client, DateTimeOffset.Now);
        var publishedNotificationId = await CreateNotification(client, DateTimeOffset.Now);
        await client.PubliceerNotificatie(publishedNotificationId);
        var unpublishedNotificationId = await CreateNotification(client, DateTimeOffset.Now);
        await client.PubliceerNotificatie(unpublishedNotificationId);
        await client.NotificatieIntrekken(unpublishedNotificationId);

        // Get notifications with status filter
        var response = await client.GetAsync($"v1/notificaties?status={statusFilter.ToLower()}");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<Notificatie[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeEmpty();

        // Verify all returned notifications have the specified status
        foreach (var notification in notifications!)
        {
            notification.Status.ToString().Should().Be(statusFilter);
        }
    }

    [Fact]
    public async Task GetNotifications_WithVanafFilter_ReturnsNotificationsFromSpecifiedDate()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var filterDate = DateTimeOffset.Now.AddDays(-1);

        // Create notifications with different ValidFrom dates
        await CreateNotification(client, DateTimeOffset.Now.AddDays(-3), Platform.Geoit);
        var recentNotificationId = await CreateNotification(client, DateTimeOffset.Now, Platform.Lara);

        // Get notifications with vanaf filter
        var response = await client.GetAsync($"v1/notificaties?vanaf={filterDate:yyyy-MM-ddTHH:mm:ssZ}");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<NotificationService.Notification.Notification[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();

        // Verify all returned notifications have ValidFrom >= filterDate
        foreach (var notification in notifications!)
        {
            notification.ValidFrom.Should().BeOnOrAfter(filterDate);
        }
    }

    [Fact]
    public async Task GetNotifications_WithTotFilter_ReturnsNotificationsUpToSpecifiedDate()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var filterDate = DateTimeOffset.Now.AddDays(1);

        // Create notifications with different ValidTo dates
        await CreateNotification(client, DateTimeOffset.Now, Platform.Geoit, filterDate.AddDays(-1));
        await CreateNotification(client, DateTimeOffset.Now, Platform.Lara, filterDate.AddDays(1));

        // Get notifications with tot filter
        var response = await client.GetAsync($"v1/notificaties?tot={filterDate:yyyy-MM-ddTHH:mm:ssZ}");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<NotificationService.Notification.Notification[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();

        // Verify all returned notifications have ValidTo <= filterDate
        foreach (var notification in notifications!)
        {
            notification.ValidTo.Should().BeOnOrBefore(filterDate);
        }
    }

    [Fact]
    public async Task GetNotifications_WithAllFilters_ReturnsFilteredAndSortedResults()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var vanaf = DateTimeOffset.Now.AddDays(-1);
        var tot = DateTimeOffset.Now.AddDays(2);

        // Create a notification and publish it
        var notificationId = await CreateNotification(client, DateTimeOffset.Now, Platform.Geoit, tot.AddDays(-1));
        await client.PostAsync($"v1/notificaties/{notificationId}/acties/publiceren", new StringContent("", Encoding.UTF8, "application/json"));

        // Get notifications with all filters
        var response = await client.GetAsync($"v1/notificaties?status=Published&vanaf={vanaf:yyyy-MM-ddTHH:mm:ssZ}&tot={tot:yyyy-MM-ddTHH:mm:ssZ}");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<NotificationService.Notification.Notification[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();

        // Verify filters are applied correctly
        foreach (var notification in notifications!)
        {
            notification.Status.Should().Be(NotificationService.Notification.NotificationStatus.Published);
            notification.ValidFrom.Should().BeOnOrAfter(vanaf);
            notification.ValidTo.Should().BeOnOrBefore(tot);
        }
    }

    [Fact]
    public async Task GetNotifications_ReturnsMaximum100Records()
    {
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        // Get notifications (assuming there might be many in the database)
        var response = await client.GetAsync("v1/notificaties");
        response.Should().BeSuccessful();

        var notifications = JsonConvert.DeserializeObject<NotificationService.Notification.Notification[]>(await response.Content.ReadAsStringAsync());
        notifications.Should().NotBeNull();
        notifications!.Length.Should().BeLessOrEqualTo(100);
    }

    private async Task<int> CreateNotification(HttpClient client, DateTimeOffset geldigVanaf, Platform platform = Platform.Lara, DateTimeOffset? geldigTot = null)
    {
        var request = new MaakNotificatieRequest
        {
            Inhoud = "#Test Notification",
            Titel = "Test Notification",
            Platformen = [platform],
            Rollen = [Rol.InterneBeheerder],
            Ernst = Ernst.Informatie,
            GeldigVanaf = geldigVanaf,
            GeldigTot = geldigTot,
            KanSluiten = false,
            Links = []
        };

        var createResult = await client.MaakNotificatie(request);
        return createResult.NotificatieId;
    }
}
