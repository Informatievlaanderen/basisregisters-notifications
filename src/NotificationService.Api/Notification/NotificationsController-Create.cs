namespace NotificationService.Api.Notification;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Notification;
using Validation;

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
        [FromServices] INotificationsRepository notificationsRepository,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var notificationId = await notificationsRepository.CreateNotification(
            request.GeldigVanaf,
            request.GeldigTot,
            request.Ernst.MapToSeverity(),
            request.Titel,
            request.Inhoud,
            request.Platformen.Select(x => x.ToString()).ToList(),
            request.Rollen.Select(x => x.ToString()).ToList(),
            request.KanSluiten,
            request.Links.Select(x => new NotificationLink(x.Label, x.Url)).ToList(),
            cancellationToken);

        return Ok(new NotificatieAangemaakt(notificationId));
    }
}

public class CreateNotificationRequestValidator : AbstractValidator<MaakNotificatieRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.Titel)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.TitelIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.TitelIsRequired.Code);

        RuleFor(x => x.Inhoud)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.InhoudIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.InhoudIsRequired.Code);

        RuleFor(x => x.Platformen)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.PlatformenIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.PlatformenIsRequired.Code);

        RuleFor(x => x.Rollen)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.RollenIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.RollenIsRequired.Code);

        When(x => x.GeldigTot.HasValue, () =>
        {
            RuleFor(x => x.GeldigTot)
                .GreaterThan(DateTimeOffset.Now)
                .WithMessage(ValidationErrors.CreateNotification.GeldigTotMustBeInTheFuture.Message)
                .WithErrorCode(ValidationErrors.CreateNotification.GeldigTotMustBeInTheFuture.Code);

            When(x => x.GeldigVanaf.HasValue, () =>
                RuleFor(x => x.GeldigVanaf)
                    .LessThanOrEqualTo(x => x.GeldigTot)
                    .WithMessage(ValidationErrors.CreateNotification.GeldigVanafMustBeBeforeGeldigTot.Message)
                    .WithErrorCode(ValidationErrors.CreateNotification.GeldigVanafMustBeBeforeGeldigTot.Code)
            );
        });

        RuleForEach(x => x.Links)
            .SetValidator(new LinkValidator());
    }
}

public class LinkValidator : AbstractValidator<MaakNotificatieLink>
{
    public LinkValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.LinkLabelIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.LinkLabelIsRequired.Code)
            .MaximumLength(100)
            .WithMessage(ValidationErrors.CreateNotification.LinkLabelIsTooLong.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.LinkLabelIsTooLong.Code);

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage(ValidationErrors.CreateNotification.LinkUrlIsRequired.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.LinkUrlIsRequired.Code)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage(ValidationErrors.CreateNotification.LinkUrlIsInvalid.Message)
            .WithErrorCode(ValidationErrors.CreateNotification.LinkUrlIsInvalid.Code);
    }
}
