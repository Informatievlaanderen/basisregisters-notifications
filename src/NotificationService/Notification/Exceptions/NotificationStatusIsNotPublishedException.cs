namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationStatusIsNotPublishedException : Exception
{
    public NotificationStatus CurrentStatus { get; }

    public NotificationStatusIsNotPublishedException(NotificationStatus currentStatus)
        : base($"Notification status is not '{nameof(NotificationStatus.Published)}', current status '{currentStatus}'.")
    {
        CurrentStatus = currentStatus;
    }
}
