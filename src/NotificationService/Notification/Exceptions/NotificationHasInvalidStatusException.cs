namespace NotificationService.Notification.Exceptions;

using System;

public class NotificationHasInvalidStatusException : Exception
{
    public int NotificationId { get; }
    public Status CurrentStatus { get; }
    public Status ExpectedStatus { get; }

    public NotificationHasInvalidStatusException(int notificationId, Status currentStatus, Status expectedStatus)
        : base($"Notification with id '{notificationId}' has invalid status '{currentStatus}'. Expected status: '{expectedStatus}'.")
    {
        NotificationId = notificationId;
        CurrentStatus = currentStatus;
        ExpectedStatus = expectedStatus;
    }
}

