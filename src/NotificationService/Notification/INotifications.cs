namespace NotificationService.Notification;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface INotifications
{
    Task<Guid> CreateNotification(
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo,
        Severity severity,
        string title,
        string bodyMd,
        ICollection<string> platforms,
        ICollection<string> roles,
        bool canClose,
        ICollection<NotificationLink> links,
        CancellationToken cancellationToken = default);
}
