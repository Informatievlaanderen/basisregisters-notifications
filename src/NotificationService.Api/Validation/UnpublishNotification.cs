namespace NotificationService.Api.Validation;

using Mapping;
using NotificationService.Notification;

public static partial class ValidationErrors
{
    public static class UnpublishNotification
    {
        public static class StatusInvalid
        {
            public const string Code = "NotificatieStatusFoutief";
            private const string Message = "Notificaties met status '{status}' kunnen niet worden ingetrokken. Gebruik de actie 'verwijderen' om de notificatie te verwijderen.";

            public static string ToMessage(NotificationStatus status) => Message.Replace("{status}", status.MapToNotificatieStatus().ToString());
        }
    }
}
