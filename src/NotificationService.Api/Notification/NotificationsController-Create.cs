namespace NotificationService.Api.Notification;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Notification;
using Severity = NotificationService.Notification.Severity;

public partial class NotificationsController
{
    [HttpPost("")]
    [ProducesResponseType(typeof(NotificatieAangemaakt), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    public async Task<IActionResult> Create(
        [FromBody] MaakNotificatieRequest request,
        [FromServices] CreateNotificationRequestValidator validator,
        [FromServices] INotifications notifications,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var notificationId = await notifications.CreateNotification(
            request.GeldigVanaf,
            request.GeldigTot,
            MapToSeverity(request.Ernst),
            request.Titel,
            request.Inhoud,
            request.Platformen.Select(x => x.ToString()).ToList(),
            request.Rollen.Select(x => x.ToString()).ToList(),
            request.KanSluiten,
            request.Links.Select(x => new NotificationLink(x.Label, x.Url)).ToList(),
            cancellationToken);

        return Ok(new NotificatieAangemaakt(notificationId));
    }

    private static Severity MapToSeverity(Ernst ernst) =>
        ernst switch
        {
            Ernst.Informatie => Severity.Information,
            Ernst.Waarschuwing => Severity.Warning,
            Ernst.Fout => Severity.Error,
            _ => throw new NotImplementedException($"{nameof(Ernst)}.{ernst}")
        };
}

public class CreateNotificationRequestValidator : AbstractValidator<MaakNotificatieRequest>
{
    public CreateNotificationRequestValidator()
    {
        //TODO-pr: add errorcodes
        RuleFor(x => x.Titel)
            .NotEmpty()
            .WithMessage("'Titel' mag niet leeg zijn.")
            .WithErrorCode("A");

        RuleFor(x => x.Inhoud)
            .NotEmpty()
            .WithMessage("'Inhoud' mag niet leeg zijn.")
            .WithErrorCode("A");

        RuleFor(x => x.Platformen)
            .NotEmpty()
            .WithMessage("'Platformen' mag niet leeg zijn.")
            .WithErrorCode("A");

        RuleFor(x => x.Rollen)
            .NotEmpty()
            .WithMessage("'Rollen' mag niet leeg zijn.")
            .WithErrorCode("A");

        When(x => x.GeldigTot.HasValue, () =>
        {
            RuleFor(x => x.GeldigTot)
                .GreaterThan(DateTimeOffset.Now)
                .WithMessage("'GeldigTot' moet in de toekomst liggen.")
                .WithErrorCode("A");

            When(x => x.GeldigVanaf.HasValue, () =>
                RuleFor(x => x.GeldigVanaf)
                    .LessThanOrEqualTo(x => x.GeldigTot)
                    .WithMessage("'GeldigVanaf' moet vroeger of gelijk zijn aan 'GeldigTot'.")
                    .WithErrorCode("A")
            );
        });

        RuleForEach(x => x.Links)
            .SetValidator(new LinkValidator());
    }
}

public class LinkValidator : AbstractValidator<NotificatieLink>
{
    public LinkValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("'Label' mag niet leeg zijn.")
            .WithErrorCode("A")
            .MaximumLength(100)
            .WithMessage("'Label' mag maximaal 100 karakters bevatten.")
            .WithErrorCode("A");

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("'Url' mag niet leeg zijn.")
            .WithErrorCode("A")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("'Url' moet een geldige URL zijn.")
            .WithErrorCode("A");
    }
}
