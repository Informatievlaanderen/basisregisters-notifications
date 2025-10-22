namespace NotificationService.Api.Validation;

public static partial class ValidationErrors
{
    public static class CreateNotification
    {
        public static class TitelIsRequired
        {
            public const string Code = "NotificatieTitelVerplicht";
            public const string Message = "'Titel' mag niet leeg zijn.";
        }

        public static class InhoudIsRequired
        {
            public const string Code = "NotificatieInhoudVerplicht";
            public const string Message = "'Inhoud' mag niet leeg zijn.";
        }

        public static class PlatformenIsRequired
        {
            public const string Code = "NotificatiePlatformenVerplicht";
            public const string Message = "'Platformen' mag niet leeg zijn.";
        }

        public static class RollenIsRequired
        {
            public const string Code = "NotificatieRollenVerplicht";
            public const string Message = "'Rollen' mag niet leeg zijn.";
        }

        public static class GeldigTotMustBeInTheFuture
        {
            public const string Code = "NotificatieGeldigTotToekomst";
            public const string Message = "'GeldigTot' moet in de toekomst liggen.";
        }

        public static class GeldigVanafMustBeBeforeGeldigTot
        {
            public const string Code = "NotificatieGeldigVanafVoorGeldigTot";
            public const string Message = "'GeldigVanaf' moet vroeger zijn dan 'GeldigTot'.";
        }

        public static class LinkLabelIsRequired
        {
            public const string Code = "NotificatieLinkLabelVerplicht";
            public const string Message = "'Label' mag niet leeg zijn.";
        }

        public static class LinkLabelIsTooLong
        {
            public const string Code = "NotificatieLabelTeLang";
            public const string Message = "'Label' mag maximaal 100 karakters bevatten.";
        }

        public static class LinkUrlIsRequired
        {
            public const string Code = "NotificatieLinkUrlVerplicht";
            public const string Message = "'Url' mag niet leeg zijn.";
        }

        public static class LinkUrlIsInvalid
        {
            public const string Code = "NotificatieLinkUrlOngeldig";
            public const string Message = "'Url' moet een geldige URL zijn.";
        }
    }
}
