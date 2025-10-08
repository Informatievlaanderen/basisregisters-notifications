namespace NotificationService.Tests;

using System;
using System.Collections.Generic;
using Api.Abstractions;
using Api.Notification;
using AutoFixture;
using FluentValidation.TestHelper;
using Xunit;

public sealed class CreateNotificationRequestValidatorTests
{
    private readonly CreateNotificationRequestValidator _validator;
    private readonly Fixture _fixture;


    public CreateNotificationRequestValidatorTests()
    {
        _validator = new CreateNotificationRequestValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void GivenEmptyTitel_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Titel, string.Empty)
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Titel)}")
            .WithErrorMessage("'Titel' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenEmptyInhoud_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Inhoud, string.Empty)
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Inhoud)}")
            .WithErrorMessage("'Inhoud' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenEmptyPlatformen_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Platformen, new List<Platform>())
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Platformen)}")
            .WithErrorMessage("'Platformen' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenEmptyRollen_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Rollen, new List<Rol>())
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Rollen)}")
            .WithErrorMessage("'Rollen' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenGeldigTotInThePast_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.GeldigTot, DateTimeOffset.Now.AddMinutes(-5))
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.GeldigTot)}")
            .WithErrorMessage("'GeldigTot' moet in de toekomst liggen.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenGeldigVanafLaterThanGeldigTot_ThenReturnsExpectedMessage()
    {
        // Arrange
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.GeldigVanaf, DateTime.UtcNow.AddMinutes(10))
            .With(x => x.GeldigTot, DateTime.UtcNow.AddMinutes(5))
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.GeldigVanaf)}")
            .WithErrorMessage("'GeldigVanaf' moet vroeger of gelijk zijn aan 'GeldigTot'.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenEmptyLinkLabel_ThenReturnsExpectedMessage()
    {
        // Arrange
        var link = _fixture.Build<MaakNotificatieLink>()
            .With(x => x.Label, string.Empty)
            .Create();
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Links, new List<MaakNotificatieLink> { link })
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Links)}[0].{nameof(link.Label)}")
            .WithErrorMessage("'Label' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenLinkLabelTooLarge_ThenReturnsExpectedMessage()
    {
        // Arrange
        var link = _fixture.Build<MaakNotificatieLink>()
            .With(x => x.Label, new string('a', 101))
            .Create();
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Links, new List<MaakNotificatieLink> { link })
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Links)}[0].{nameof(link.Label)}")
            .WithErrorMessage("'Label' mag maximaal 100 karakters bevatten.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenEmptyLinkUrl_ThenReturnsExpectedMessage()
    {
        // Arrange
        var link = _fixture.Build<MaakNotificatieLink>()
            .With(x => x.Url, string.Empty)
            .Create();
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Links, new List<MaakNotificatieLink> { link })
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Links)}[0].{nameof(link.Url)}")
            .WithErrorMessage("'Url' mag niet leeg zijn.")
            .WithErrorCode("A");
    }

    [Fact]
    public void GivenLinkUrlNotValidFormat_ThenReturnsExpectedMessage()
    {
        // Arrange
        var link = _fixture.Build<MaakNotificatieLink>()
            .With(x => x.Url, "invalid-url")
            .Create();
        var maakNotificatieRequest = _fixture.Build<MaakNotificatieRequest>()
            .With(x => x.Links, new List<MaakNotificatieLink> { link })
            .Create();

        // Act
        var result = _validator.TestValidate(maakNotificatieRequest);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(maakNotificatieRequest.Links)}[0].{nameof(link.Url)}")
            .WithErrorMessage("'Url' moet een geldige URL zijn.")
            .WithErrorCode("A");
    }
}
