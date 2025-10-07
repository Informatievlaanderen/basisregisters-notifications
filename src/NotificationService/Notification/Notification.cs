namespace NotificationService.Notification;

using System;
using System.Collections.Generic;

public record Notification(
    int NotificationId,
    Status Status,
    Severity Severity,
    string Title,
    string BodyMd,
    ICollection<string> Platforms,
    ICollection<string> Roles,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool CanClose,
    ICollection<NotificationLink> Links)
{
    private readonly DateTimeOffset _created = DateTimeOffset.UtcNow;
    private DateTimeOffset _lastModified = DateTimeOffset.UtcNow;

    public int NotificationId { get; set; } = NotificationId;
    public Status Status { get; set; } = Status;
    public Severity Severity { get; set; } = Severity;
    public string Title { get; set; } = Title;
    public string BodyMd { get; set; } = BodyMd;
    public ICollection<string> Platforms { get; set; } = Platforms;
    public ICollection<string> Roles { get; set; } = Roles;
    public DateTimeOffset ValidFrom { get; set; } = ValidFrom;
    public DateTimeOffset ValidTo { get; set; } = ValidTo;
    public bool CanClose { get; set; } = CanClose;
    public ICollection<NotificationLink> Links { get; set; } = Links;

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
