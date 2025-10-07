namespace NotificationService.Notification;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;

public class MartenNotifications : INotifications
{
    private readonly IDocumentStore _store;

    public MartenNotifications(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<int> CreateNotification(
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo,
        Severity severity,
        string title,
        string bodyMd,
        ICollection<string> platforms,
        ICollection<string> roles,
        bool canClose,
        ICollection<NotificationLink> links,
        CancellationToken cancellationToken
    )
    {
        await using var session = _store.DirtyTrackedSession();
        var notification = new Notification(
            0,
            Status.Draft,
            severity,
            title,
            bodyMd,
            platforms,
            roles,
            validFrom ?? DateTimeOffset.UtcNow,
            validTo ?? new DateTimeOffset(new DateTime(9999, 1, 1), TimeSpan.Zero),
            canClose,
            links
        );

        session.Insert(notification);

        await session.SaveChangesAsync(cancellationToken);

        return notification.NotificationId;
    }

    public async Task<bool> PublishNotification(int notificationId, CancellationToken cancellationToken)
    {
        await using var session = _store.DirtyTrackedSession();

        var notification = await session.LoadAsync<Notification>(notificationId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.Status = Status.Published;
        notification.LastModified = DateTimeOffset.UtcNow;

        session.Update(notification);
        await session.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UnpublishNotification(int notificationId, CancellationToken cancellationToken)
    {
        await using var session = _store.DirtyTrackedSession();

        var notification = await session.LoadAsync<Notification>(notificationId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.Status = Status.Unpublished;
        notification.LastModified = DateTimeOffset.UtcNow;

        session.Update(notification);
        await session.SaveChangesAsync(cancellationToken);

        return true;
    }
}
