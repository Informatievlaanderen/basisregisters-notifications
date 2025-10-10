namespace NotificationService.Api.Mapping;

using System;
using Abstractions;
using NotificationService.Notification;

public static class ErnstExtensions
{
    public static Severity MapToSeverity(this Ernst ernst) =>
        ernst switch
        {
            Ernst.Informatie => Severity.Information,
            Ernst.Waarschuwing => Severity.Warning,
            Ernst.Fout => Severity.Error,
            _ => throw new NotImplementedException($"{nameof(Ernst)}.{ernst}")
        };

    public static Ernst MapToErnst(this Severity severity) =>
        severity switch
        {
            Severity.Information => Ernst.Informatie,
            Severity.Warning => Ernst.Waarschuwing,
            Severity.Error => Ernst.Fout,
            _ => throw new NotImplementedException($"{nameof(Severity)}.{severity}")
        };
}
