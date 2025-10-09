namespace NotificationService.Api.Validation;

public static partial class ValidationErrors
{
    //TODO-pr add other validation errors
    public static class CreateNotification
    {
        public static class TitelIsRequired
        {
            public const string Code = "A";
            public const string Message = "'Titel' mag niet leeg zijn.";
        }
    }
}
