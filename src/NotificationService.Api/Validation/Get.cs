namespace NotificationService.Api.Validation;

public static partial class ValidationErrors
{
    public static class Get
    {
        public static class VanafMustBeBeforeTot
        {
            public const string Code = "VanafVoorTot";
            public const string Message = "'Vanaf' moet vroeger zijn dan 'Tot'.";
        }
    }
}
