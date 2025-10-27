namespace NotificationService.Api.Notification;

using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using FluentValidation.Results;
using Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Notification;
using NotificationService.Notification.Exceptions;
using Validation;

public partial class NotificationsController
{
    [HttpPost("{id}/acties/intrekken")]
    [ProducesResponseType(typeof(Notificatie), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    public async Task<IActionResult> Unpublish(
        [FromRoute] int id,
        [FromServices] INotificationsRepository notificationsRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await notificationsRepository.UnpublishNotification(id, cancellationToken);
            return Ok(notification.MapToNotificatie());
        }
        catch (NotificationNotFoundException)
        {
            throw new ApiException(ValidationErrors.Common.NotFound.Message, StatusCodes.Status404NotFound);
        }
        catch (NotificationStatusIsNotPublishedException ex)
        {
            throw new ValidationException([new ValidationFailure
            {
                PropertyName = string.Empty,
                ErrorCode = ValidationErrors.UnpublishNotification.StatusInvalid.Code,
                ErrorMessage = ValidationErrors.UnpublishNotification.StatusInvalid.ToMessage(ex.CurrentStatus)
            }]);
        }
    }
}
