namespace NotificationService.Api.Mapping;

using System;
using Abstractions;
using NotificationService.Notification;

public static class ErnstExtensions
{
    public static Severity MapToSeverity(this NotificatieErnst ernst) =>
        ernst switch
        {
            NotificatieErnst.Informatie => Severity.Information,
            NotificatieErnst.Waarschuwing => Severity.Warning,
            NotificatieErnst.Fout => Severity.Error,
            _ => throw new NotImplementedException($"{nameof(NotificatieErnst)}.{ernst}")
        };

    public static NotificatieErnst MapToErnst(this Severity severity) =>
        severity switch
        {
            Severity.Information => NotificatieErnst.Informatie,
            Severity.Warning => NotificatieErnst.Waarschuwing,
            Severity.Error => NotificatieErnst.Fout,
            _ => throw new NotImplementedException($"{nameof(Severity)}.{severity}")
        };
}
