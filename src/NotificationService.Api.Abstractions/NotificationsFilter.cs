namespace NotificationService.Api.Abstractions;

using System;

public class NotificationsFilter
{
    public NotificatieStatus? Status { get; init; }
    public DateTimeOffset? Vanaf { get; init; }
    public DateTimeOffset? Tot { get; init; }
}
