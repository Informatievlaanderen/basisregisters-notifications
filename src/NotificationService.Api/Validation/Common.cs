namespace NotificationService.Api.Validation;

public static partial class ValidationErrors
{
    //TODO-pr error codes and messages
    public static class Common
    {
        public static class NotFound
        {
            public const string Message = "Notificatie niet gevonden";
        }

        public static class InvalidStatus
        {
            public const string Code = "TODO CODE";
            public const string Message = "TODO MESSAGE";
        }
    }
}
