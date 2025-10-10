namespace NotificationService.Tests;

using System;
using System.Linq;
using Api.Abstractions;
using Api.Mapping;
using FluentAssertions;
using Notification;
using Xunit;

public class NotificatieStatusMappingTests
{
    [Fact]
    public void EnsureAllApiEnumValuesAreMapped()
    {
        // Arrange
        var mappedValues = Enum.GetValues(typeof(NotificatieStatus))
            .Cast<NotificatieStatus>()
            .Select(ns => ns.MapToNotificationStatus())
            .ToArray();

        mappedValues.Should().NotBeEmpty();
    }

    [Fact]
    public void EnsureAllDomainEnumValuesAreMapped()
    {
        // Arrange
        var mappedValues = Enum.GetValues(typeof(NotificationStatus))
            .Cast<NotificationStatus>()
            .Select(ns => ns.MapToNotificatieStatus())
            .ToArray();

        mappedValues.Should().NotBeEmpty();
    }
}
