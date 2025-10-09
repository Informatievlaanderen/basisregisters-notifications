namespace NotificationService.Api.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Notification;

public sealed class Notificatie
{
    [JsonProperty("notificatieId")]
    public required int NotificatieId { get; init; }

    [JsonProperty("status")]
    public required NotificatieStatus Status { get; init; }

    [JsonProperty("geldigVanaf")]
    public required DateTimeOffset GeldigVanaf { get; init; }

    [JsonProperty("geldigTot")]
    public required DateTimeOffset GeldigTot { get; init; }

    [JsonProperty("ernst")]
    public required Ernst Ernst { get; init; }

    [JsonProperty("titel")]
    public required string Titel { get; init; }

    [JsonProperty("inhoud")]
    public required string Inhoud { get; init; }

    [JsonProperty("platformen")]
    public required ICollection<Platform> Platformen { get; init; }

    [JsonProperty("rollen")]
    public required ICollection<Rol> Rollen { get; init; }

    [JsonProperty("kanSluiten")]
    public required bool KanSluiten { get; init; }

    [JsonProperty("links")]
    public required ICollection<NotificatieLink> Links { get; init; }
}

public record NotificatieLink(string Label, string Url);

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
