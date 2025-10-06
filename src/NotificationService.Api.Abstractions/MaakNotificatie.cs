namespace NotificationService.Abstractions;

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
    public string Titel { get; set; }

    [JsonPropertyName("inhoud")]
    public string Inhoud { get; set; }

    [JsonPropertyName("platformen")]
    public ICollection<Platform> Platformen { get; set; }

    [JsonPropertyName("rollen")]
    public ICollection<Rol> Rollen { get; set; }

    [JsonPropertyName("sluitbaar")]
    public bool Sluitbaar { get; set; }

    [JsonPropertyName("links")]
    public ICollection<NotificatieLink> Links { get; set; }
}

public record NotificatieLink(string Label, string Url);
