namespace NotificationService.Api.Notification;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Notification;

public partial class NotificationsController
{
    [HttpPost("{id}/acties/intrekken")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    public async Task<IActionResult> Unpublish(
        [FromRoute] int id,
        [FromServices] INotifications notifications,
        CancellationToken cancellationToken)
    {
        var unpublished = await notifications.UnpublishNotification(id, cancellationToken);

        if (!unpublished)
        {
            return NotFound();
        }

        return NoContent();
    }
}

