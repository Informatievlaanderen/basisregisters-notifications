namespace NotificationService.Tests;

using System;
using System.Linq;
using Api.Abstractions;
using Api.Mapping;
using FluentAssertions;
using Notification;
using Xunit;

public class ErnstMappingTests
{
    [Fact]
    public void EnsureAllApiEnumValuesAreMapped()
    {
        // Arrange
        var mappedValues = Enum.GetValues(typeof(Ernst))
            .Cast<Ernst>()
            .Select(ns => ns.MapToSeverity())
            .ToArray();

        mappedValues.Should().NotBeEmpty();
    }

    [Fact]
    public void EnsureAllDomainEnumValuesAreMapped()
    {
        // Arrange
        var mappedValues = Enum.GetValues(typeof(Severity))
            .Cast<Severity>()
            .Select(ns => ns.MapToErnst())
            .ToArray();

        mappedValues.Should().NotBeEmpty();
    }
}
