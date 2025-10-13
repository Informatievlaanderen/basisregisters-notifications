namespace NotificationService.Api.Notification;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.Abstractions;
using NotificationService.Notification;
using Swashbuckle.AspNetCore.Filters;

public partial class NotificationsController
{
    /// <summary>
    /// Vraag een lijst met actieve gepubliceerde notificaties op voor een bepaald platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="notificationsRepository"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de opvraging van een lijst met notificaties gelukt is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    /// <returns></returns>
    [HttpGet("{platform}")]
    [ProducesResponseType(typeof(Notificatie[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetNotificatiesResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
    public async Task<IActionResult> GetByPlatform(
        [FromRoute] Platform platform,
        [FromServices] INotificationsRepository notificationsRepository,
        CancellationToken cancellationToken)
    {
        var result = await notificationsRepository.GetActiveNotifications(
            platform.ToString(),
            cancellationToken);

        var notificaties = result
            .Select(x => x.MapToNotificatie())
            .ToList();

        return Ok(notificaties);
    }
}
