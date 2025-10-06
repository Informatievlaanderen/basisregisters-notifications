namespace NotificationService.Abstractions;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public record Notification(
    Guid NotificationId)
{
    private readonly DateTimeOffset _created = DateTimeOffset.UtcNow;
    private DateTimeOffset _lastModified = DateTimeOffset.UtcNow;

    /// <summary>
    /// De unieke identificator van de notificatie.
    /// </summary>
    public Guid NotificationId { get; set; } = NotificationId;

    /// <summary>
    /// De datum en het tijdstip waarop de notificatie is aangemaakt (timestamp volgens RFC 3339) (notatie: lokale tijd + verschil t.o.v. UTC).
    /// </summary>
    public DateTimeOffset Created
    {
        get => ToWesternEuropeDateTimeOffset(_created);
        init => _created = value;
    }

    /// <summary>
    /// De datum en het tijdstip waarop de notificatie laatst is aangepast (timestamp volgens RFC 3339) (notatie: lokale tijd + verschil t.o.v. UTC).
    /// </summary>
    public DateTimeOffset LastModified
    {
        get => ToWesternEuropeDateTimeOffset(_lastModified);
        set => _lastModified = value;
    }

    private static DateTimeOffset ToWesternEuropeDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        var utcOffset = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time").GetUtcOffset(dateTimeOffset);

        if (utcOffset == dateTimeOffset.Offset)
        {
            return dateTimeOffset;
        }

        var dateTime = TimeZoneInfo.ConvertTimeFromUtc(
            dateTimeOffset.DateTime,
            TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

        return new DateTimeOffset(
            dateTime,
            utcOffset);
    }
}
