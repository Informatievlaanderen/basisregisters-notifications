namespace NotificationService.Api.IntegrationTests.Notification;

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Abstractions;
using Api.Notification;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Notification;
using Xunit;

[Collection("DockerFixtureCollection")]
public class WhenGettingNotifications: IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public WhenGettingNotifications(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WithoutFilters_ReturnsAllNotificationsSortedByValidFromDescending()
    {
        // Arrange
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        await CreateNotification(DateTimeOffset.Now.AddDays(-2));
        await CreateNotification(DateTimeOffset.Now);
        await CreateNotification(DateTimeOffset.Now.AddDays(-1));

        // Act
        var notifications = await client.GetNotificaties();

        // Assert
        notifications.Should().NotBeEmpty();

        for (var i = 0; i < notifications.Length - 1; i++)
        {
            notifications[i].GeldigVanaf.Should().BeOnOrAfter(notifications[i + 1].GeldigVanaf);
        }
    }

    [Theory]
    [InlineData(NotificatieStatus.Concept)]
    [InlineData(NotificatieStatus.Gepubliceerd)]
    [InlineData(NotificatieStatus.Ingetrokken)]
    public async Task WithStatusFilter_ReturnsNotificationsWithSpecificStatus(NotificatieStatus statusFilter)
    {
        // Arrange
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        await CreateNotification(DateTimeOffset.Now);
        var publishedNotificationId = await CreateNotification(DateTimeOffset.Now);
        await client.PubliceerNotificatie(publishedNotificationId);
        var unpublishedNotificationId = await CreateNotification(DateTimeOffset.Now);
        await client.PubliceerNotificatie(unpublishedNotificationId);
        await client.NotificatieIntrekken(unpublishedNotificationId);

        // Act
        var notifications = await client.GetNotificaties(new NotificationsFilter
        {
            Status = statusFilter
        });

        // Assert
        notifications.Should().NotBeEmpty();

        foreach (var notification in notifications)
        {
            notification.Status.Should().Be(statusFilter);
        }
    }

    [Fact]
    public async Task WithVanafFilter_ReturnsNotificationsFromSpecifiedDate()
    {
        // Arrange
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var filterDate = DateTimeOffset.Now.AddDays(-1);

        await CreateNotification(DateTimeOffset.Now.AddDays(-3));
        await CreateNotification(DateTimeOffset.Now);

        // Act
        var notifications = await client.GetNotificaties(new NotificationsFilter
        {
            Vanaf = filterDate
        });

        // Assert
        notifications.Should().NotBeNull();

        foreach (var notification in notifications)
        {
            notification.GeldigVanaf.Should().BeOnOrAfter(filterDate);
        }
    }

    [Fact]
    public async Task WithTotFilter_ReturnsNotificationsUpToSpecifiedDate()
    {
        // Arrange
        var client = _fixture.TestServer.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(NotificationServiceTestFixture.RequiredScopes));

        var filterDate = DateTimeOffset.Now.AddDays(1);

        await CreateNotification(DateTimeOffset.Now, filterDate.AddDays(-1));
        await CreateNotification(DateTimeOffset.Now, filterDate.AddDays(1));

        // Act
        var notifications = await client.GetNotificaties(new NotificationsFilter
        {
            Tot = filterDate
        });

        // Assert
        notifications.Should().NotBeNull();

        foreach (var notification in notifications)
        {
            notification.GeldigTot.Should().BeOnOrBefore(filterDate);
        }
    }

    [Fact]
    public async Task WhenLimit_ThenReturnsMaximumAllowedRecords()
    {
        // Arrange
        await CreateNotification(DateTimeOffset.Now);
        await CreateNotification(DateTimeOffset.Now);

        var notificationsRepository = _fixture.TestServer.Services.GetRequiredService<INotificationsRepository>();

        // Act
        var notifications = await notificationsRepository.GetNotifications(limit: 1);

        // Assert
        notifications.Should().HaveCount(1);
    }

    private async Task<int> CreateNotification(DateTimeOffset geldigVanaf, DateTimeOffset? geldigTot = null, Platform platform = Platform.Lara)
    {
        var notificationsRepository = _fixture.TestServer.Services.GetRequiredService<INotificationsRepository>();

        var notificationId = await notificationsRepository.CreateNotification(
            geldigVanaf,
            geldigTot ?? DateTimeOffset.Now.AddYears(1000),
            Severity.Information,
            "Test Notification",
            "#Test Notification",
            [platform.ToString()],
            [nameof(Rol.InterneBeheerder)],
            false,
            []);

        return notificationId;
    }
}
