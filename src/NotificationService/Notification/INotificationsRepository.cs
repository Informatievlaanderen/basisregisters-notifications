namespace NotificationService.Notification;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface INotificationsRepository
{
    Task<int> CreateNotification(
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

    Task<Notification> PublishNotification(int notificationId, CancellationToken cancellationToken = default);

    Task<Notification> UnpublishNotification(int notificationId, CancellationToken cancellationToken = default);

    Task DeleteNotification(int notificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetActiveNotifications(string platform, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetNotifications(
        NotificationStatus? status = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
