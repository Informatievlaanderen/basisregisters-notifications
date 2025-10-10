namespace NotificationService.Api.Mapping;

using System;
using System.Linq;
using Abstractions;
using NotificationService.Notification;

public static class NotificatieExtensions
{
    public static Notificatie MapToNotificatie(this Notification notification)
    {
        return new Notificatie
        {
            NotificatieId = notification.NotificationId,
            Status = notification.Status.MapToNotificatieStatus(),
            GeldigVanaf = notification.ValidFrom,
            GeldigTot = notification.ValidTo,
            Ernst = notification.Severity.MapToErnst(),
            Titel = notification.Title,
            Inhoud = notification.BodyMd,
            Platformen = notification.Platforms.Select(Enum.Parse<Platform>).ToList(),
            Rollen = notification.Roles.Select(Enum.Parse<Rol>).ToList(),
            KanSluiten = notification.CanClose,
            Links = notification.Links.Select(l => new NotificatieLink(l.Label, l.Url)).ToList()
        };
    }
}
