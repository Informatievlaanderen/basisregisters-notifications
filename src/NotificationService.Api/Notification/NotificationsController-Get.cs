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
        [FromQuery] NotificatieStatus? status,
        [FromQuery] DateTimeOffset? vanaf,
        [FromQuery] DateTimeOffset? tot,
        [FromServices] GetNotificationsRequestValidator validator,
        [FromServices] INotifications notifications,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(new()
        {
            Vanaf = vanaf,
            Tot = tot
        }, cancellationToken);

        var result = await notifications.GetNotifications(
            status: status?.MapToNotificationStatus(),
            validFrom: vanaf,
            validTo: tot,
            cancellationToken: cancellationToken);

        var notificaties = result
            .Select(x => x.MapToNotificatie())
            .ToList();

        return Ok(notificaties);
    }

    public class GetNotificationsParameters
    {
        public required DateTimeOffset? Vanaf { get; init; }
        public required DateTimeOffset? Tot { get; init; }
    }
    public class GetNotificationsRequestValidator : AbstractValidator<GetNotificationsParameters>
    {
        public GetNotificationsRequestValidator()
        {
            When(x => x.Vanaf is not null && x.Tot is not null, () =>
            {
                //TODO-pr: add errorcodes
                RuleFor(x => x.Tot)
                    .GreaterThan(x => x.Vanaf)
                    .WithMessage("'tot' moet groter zijn dan 'vanaf'.")
                    .WithErrorCode("A");
            });
        }
    }
}
