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
    public async Task<IActionResult> Create(
        [FromBody] MaakNotificatie request,
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
            request.Sluitbaar,
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

public class CreateNotificationRequestValidator : AbstractValidator<MaakNotificatie>
{
    public CreateNotificationRequestValidator()
    {
        //TODO-pr
        // RuleFor(x => x.ValidFrom)
        //     .LessThan(x => x.ValidTo)
        //     .WithMessage("'ValidFrom' must be earlier than 'ValidTo'.");
        //
        // RuleFor(x => x.Title)
        //     .NotEmpty()
        //     .WithMessage("'Title' must not be empty.")
        //     .MaximumLength(200)
        //     .WithMessage("'Title' must not exceed 200 characters.");
        //
        // RuleFor(x => x.BodyMd)
        //     .NotEmpty()
        //     .WithMessage("'BodyMd' must not be empty.");
        //
        // RuleFor(x => x.Platforms)
        //     .NotEmpty()
        //     .WithMessage("'Platforms' must contain at least one platform.");
        //
        // RuleFor(x => x.Roles)
        //     .NotEmpty()
        //     .WithMessage("'Roles' must contain at least one role.");

        RuleForEach(x => x.Links)
            .SetValidator(new LinkValidator());
    }
}

public class LinkValidator : AbstractValidator<NotificatieLink>
{
    public LinkValidator()
    {
        // RuleFor(x => x.Label)
        //     .NotEmpty()
        //     .WithMessage("'Label' must not be empty.")
        //     .MaximumLength(100)
        //     .WithMessage("'Label' must not exceed 100 characters.");
        //
        // RuleFor(x => x.Url)
        //     .NotEmpty()
        //     .WithMessage("'Url' must not be empty.")
        //     .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        //     .WithMessage("'Url' must be a valid absolute URL.");
    }
}
