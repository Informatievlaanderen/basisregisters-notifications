namespace NotificationService.Api.Validation;

public static partial class ValidationErrors
{
    //TODO-pr error codes and messages
    public static class Get
    {
        public static class VanafMustBeBeforeTot
        {
            public const string Code = "A";
            public const string Message = "'Vanaf' moet vroeger zijn dan 'Tot'.";
        }
    }
}
