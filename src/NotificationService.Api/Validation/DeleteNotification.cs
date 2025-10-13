namespace NotificationService.Api.Validation;

using Mapping;
using NotificationService.Notification;

public static partial class ValidationErrors
{
    public static class DeleteNotification
    {
        public static class StatusInvalid
        {
            public const string Code = "NotificatieStatusFoutief";
            private const string Message = "Notificaties met status '{status}' kunnen niet worden verwijderd. Gebruik de actie 'intrekken' om de notificatie publicatie te beëindigen.";

            public static string ToMessage(NotificationStatus status) => Message.Replace("{status}", status.MapToNotificatieStatus().ToString());
        }
    }
}
