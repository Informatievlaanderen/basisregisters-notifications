namespace NotificationService.Api.Notification;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.Abstractions;
using NotificationService.Notification;

public partial class NotificationsController
{
    [HttpGet("{platform}")]
    [ProducesResponseType(typeof(Notificatie[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
    public async Task<IActionResult> GetByPlatform(
        [FromRoute] Platform platform,
        [FromServices] INotifications notifications,
        CancellationToken cancellationToken)
    {
        var result = await notifications.GetActiveNotifications(
            platform.ToString(),
            cancellationToken);

        var notificaties = result
            .Select(x => x.MapToNotificatie())
            .ToList();

        return Ok(notificaties);
    }
}
