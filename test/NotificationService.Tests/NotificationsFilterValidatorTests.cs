namespace NotificationService.Tests;

using System;
using Api.Abstractions;
using Api.Notification;
using AutoFixture;
using FluentValidation.TestHelper;
using Xunit;

public sealed class NotificationsFilterValidatorTests
{
    private readonly NotificationsFilterValidator _validator;
    private readonly Fixture _fixture;


    public NotificationsFilterValidatorTests()
    {
        _validator = new NotificationsFilterValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void WhenTotIsBeforeVanaf_ThenReturnsExpectedMessage()
    {
        // Arrange
        var value = _fixture.Build<NotificationsFilter>()
            .With(x => x.Tot, DateTimeOffset.Now.AddDays(-2))
            .With(x => x.Vanaf, DateTimeOffset.Now)
            .Create();

        // Act
        var result = _validator.TestValidate(value);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(value.Tot)}")
            .WithErrorMessage("'Vanaf' moet vroeger zijn dan 'Tot'.")
            .WithErrorCode("VanafVoorTot");
    }
}
