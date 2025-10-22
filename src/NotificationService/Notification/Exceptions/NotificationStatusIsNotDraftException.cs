namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationStatusIsNotDraftException : Exception
{
    public NotificationStatus CurrentStatus { get; }

    public NotificationStatusIsNotDraftException(NotificationStatus currentStatus)
        : base($"Notification status is not '{nameof(NotificationStatus.Draft)}', current status '{currentStatus}'.")
    {
        CurrentStatus = currentStatus;
    }
}
