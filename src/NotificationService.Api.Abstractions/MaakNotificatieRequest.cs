namespace NotificationService.Api.Abstractions;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class MaakNotificatieRequest
{
    [JsonProperty("geldigVanaf")]
    public DateTimeOffset? GeldigVanaf { get; set; }

    [JsonProperty("geldigTot")]
    public DateTimeOffset? GeldigTot { get; set; }

    [JsonProperty("ernst")]
    public Ernst Ernst { get; set; }

    [JsonProperty("titel")]
    public required string Titel { get; set; }

    [JsonProperty("inhoud")]
    public required string Inhoud { get; set; }

    [JsonProperty("platformen")]
    public required ICollection<Platform> Platformen { get; set; }

    [JsonProperty("rollen")]
    public required ICollection<Rol> Rollen { get; set; }

    [JsonProperty("kanSluiten")]
    public bool KanSluiten { get; set; }

    [JsonProperty("links")]
    public required ICollection<NotificatieLink> Links { get; set; }
}

public record NotificatieLink(string Label, string Url);
