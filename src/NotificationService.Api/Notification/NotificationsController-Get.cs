namespace NotificationService.Api.Notification;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
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
    [HttpGet("")]
    [ProducesResponseType(typeof(Notificatie[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    public async Task<IActionResult> GetNotifications(
        [FromServices] NotificationsFilterValidator validator,
        [FromServices] INotificationsRepository notificationsRepository,
        CancellationToken cancellationToken)
    {
        var request = Request.ExtractFilteringRequest<NotificationsFilter>();
        if (request.Filter is not null)
        {
            await validator.ValidateAndThrowAsync(request.Filter, cancellationToken);
        }

        var result = await notificationsRepository.GetNotifications(
            status: request.Filter?.Status?.MapToNotificationStatus(),
            validFrom: request.Filter?.Vanaf,
            validTo: request.Filter?.Tot,
            cancellationToken: cancellationToken);

        var notificaties = result
            .Select(x => x.MapToNotificatie())
            .ToList();

        return Ok(notificaties);
    }
}

public class NotificationsFilterValidator : AbstractValidator<NotificationsFilter>
{
    public NotificationsFilterValidator()
    {
        When(x => x.Vanaf is not null && x.Tot is not null, () =>
        {
            RuleFor(x => x.Tot)
                .GreaterThan(x => x.Vanaf)
                .WithMessage(ValidationErrors.Get.VanafMustBeBeforeTot.Message)
                .WithErrorCode(ValidationErrors.Get.VanafMustBeBeforeTot.Code);
        });
    }
}
