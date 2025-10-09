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

public partial class NotificationsController
{
    [HttpDelete("{id}")]
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
            throw new ApiException("Notificatie niet gevonden", StatusCodes.Status404NotFound);
        }
        catch (NotificationHasInvalidStatusException ex)
        {
            throw new ValidationException([new ValidationFailure(string.Empty, "TODO MESSAGE"){ErrorCode = "TODO CODE"}]);
        }
    }
}

