namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationHasInvalidStatusException : Exception
{
    public int NotificationId { get; }
    public NotificationStatus CurrentStatus { get; }
    public NotificationStatus ExpectedStatus { get; }

    public NotificationHasInvalidStatusException(int notificationId, NotificationStatus currentStatus, NotificationStatus expectedStatus)
        : base($"Notification with id '{notificationId}' has invalid status '{currentStatus}'. Expected status: '{expectedStatus}'.")
    {
        NotificationId = notificationId;
        CurrentStatus = currentStatus;
        ExpectedStatus = expectedStatus;
    }
}

