namespace NotificationService.Api.Abstractions;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class MaakNotificatie
{
    [JsonPropertyName("geldigVanaf")]
    public DateTimeOffset? GeldigVanaf { get; set; }

    [JsonPropertyName("geldigTot")]
    public DateTimeOffset? GeldigTot { get; set; }

    [JsonPropertyName("ernst")]
    public Ernst Ernst { get; set; }

    [JsonPropertyName("titel")]
    public required string Titel { get; set; }

    [JsonPropertyName("inhoud")]
    public required string Inhoud { get; set; }

    [JsonPropertyName("platformen")]
    public required ICollection<Platform> Platformen { get; set; }

    [JsonPropertyName("rollen")]
    public required ICollection<Rol> Rollen { get; set; }

    [JsonPropertyName("sluitbaar")]
    public bool Sluitbaar { get; set; }

    [JsonPropertyName("links")]
    public required ICollection<NotificatieLink> Links { get; set; }
}

public record NotificatieLink(string Label, string Url);
