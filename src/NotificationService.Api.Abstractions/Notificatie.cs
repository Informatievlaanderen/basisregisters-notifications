namespace NotificationService.Api.Abstractions;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class Notificatie
{
    [JsonProperty("notificatieId")]
    public required int NotificatieId { get; init; }

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
