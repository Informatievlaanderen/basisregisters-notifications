namespace NotificationService.Api.Notification;

using System;
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
            .Select(x => new Notificatie
            {
                NotificatieId = x.NotificationId,
                GeldigVanaf = x.ValidFrom,
                GeldigTot = x.ValidTo,
                Ernst = MapToErnst(x.Severity),
                Titel = x.Title,
                Inhoud = x.BodyMd,
                Platformen = x.Platforms.Select(Enum.Parse<Platform>).ToList(),
                Rollen = x.Roles.Select(Enum.Parse<Rol>).ToList(),
                KanSluiten = x.CanClose,
                Links = x.Links.Select(l => new NotificatieLink(l.Label, l.Url)).ToList()
            })
            .ToList();

        return Ok(notificaties);
    }

    private static Ernst MapToErnst(Severity severity) =>
        severity switch
        {
            Severity.Information => Ernst.Informatie,
            Severity.Warning => Ernst.Waarschuwing,
            Severity.Error => Ernst.Fout,
            _ => throw new NotImplementedException($"{nameof(Severity)}.{severity}")
        };
}
