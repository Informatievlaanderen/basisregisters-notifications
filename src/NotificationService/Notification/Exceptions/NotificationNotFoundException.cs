namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationNotFoundException : Exception
{
    public NotificationNotFoundException()
        : base("Notification was not found.")
    {
    }
}

