namespace NotificationService.Api.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;

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

public class GetNotificatiesResponseExample : IExamplesProvider<Notificatie[]>
{
    public Notificatie[] GetExamples()
    {
        return
        [
            new Notificatie
            {
                NotificatieId = 1,
                Status = NotificatieStatus.Gepubliceerd,
                GeldigVanaf = DateTimeOffset.Now.Date.AddDays(-2),
                GeldigTot = DateTimeOffset.Now.AddDays(7),
                Ernst = Ernst.Informatie,
                Titel = "Systeemonderhoud",
                Inhoud = "Er is gepland systeemonderhoud op 15 juni.",
                Platformen = [ Platform.Lara, Platform.Geoit ],
                Rollen = [ Rol.NietIngelogd, Rol.StandaardGebruiker, Rol.InterneBeheerder ],
                KanSluiten = true,
                Links = [ new NotificatieLink("Meer info", "https://example.com/onderhoud") ]
            }
        ];
    }
}
