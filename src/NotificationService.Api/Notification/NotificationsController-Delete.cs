namespace NotificationService.Api.Notification;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Notification;
using NotificationService.Notification.Exceptions;
using Validation;

public partial class NotificationsController
{
    [HttpPost("{id}/acties/verwijderen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromServices] INotificationsRepository notificationsRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            await notificationsRepository.DeleteNotification(id, cancellationToken);
            return NoContent();
        }
        catch (NotificationNotFoundException)
        {
            throw new ApiException(ValidationErrors.Common.NotFound.Message, StatusCodes.Status404NotFound);
        }
        catch (NotificationStatusIsNotDraftException ex)
        {
            throw new ValidationException([new ValidationFailure
            {
                PropertyName = string.Empty,
                ErrorCode = ValidationErrors.DeleteNotification.StatusInvalid.Code,
                ErrorMessage = ValidationErrors.DeleteNotification.StatusInvalid.ToMessage(ex.CurrentStatus)
            }]);
        }
    }
}
