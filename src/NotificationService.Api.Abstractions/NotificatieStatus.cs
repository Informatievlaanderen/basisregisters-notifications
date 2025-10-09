namespace NotificationService.Api.Abstractions;

using System;
using Notification;

public enum NotificatieStatus
{
    Concept,
    Gepubliceerd,
    Ingetrokken
}

public static class NotificatieStatusExtensions
{
    public static NotificationStatus MapToNotificationStatus(this NotificatieStatus status) =>
        status switch
        {
            NotificatieStatus.Concept => NotificationStatus.Draft,
            NotificatieStatus.Gepubliceerd => NotificationStatus.Published,
            NotificatieStatus.Ingetrokken => NotificationStatus.Unpublished,
            _ => throw new NotImplementedException($"{nameof(NotificatieStatus)}.{status}")
        };

    public static NotificatieStatus MapToNotificatieStatus(this NotificationStatus status) =>
        status switch
        {
            NotificationStatus.Draft => NotificatieStatus.Concept,
            NotificationStatus.Published => NotificatieStatus.Gepubliceerd,
            NotificationStatus.Unpublished => NotificatieStatus.Ingetrokken,
            _ => throw new NotImplementedException($"{nameof(NotificationStatus)}.{status}")
        };
}
