namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationNotFoundException : Exception
{
    public int NotificationId { get; }

    public NotificationNotFoundException(int notificationId)
        : base($"Notification with id '{notificationId}' was not found.")
    {
        NotificationId = notificationId;
    }
}

