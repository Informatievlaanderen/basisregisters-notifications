namespace NotificationService.Notification;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using NotificationService.Notification.Exceptions;

public class MartenNotificationsRepository : INotificationsRepository
{
    private readonly IDocumentStore _store;

    public MartenNotificationsRepository(IDocumentStore store)
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
            NotificationStatus.Draft,
            severity,
            title,
            bodyMd,
            platforms,
            roles,
            validFrom ?? DateTimeOffset.UtcNow,
            validTo ?? new DateTimeOffset(new DateTime(9999, 12, 31), TimeSpan.Zero),
            canClose,
            links
        );

        session.Insert(notification);

        await session.SaveChangesAsync(cancellationToken);

        return notification.NotificationId;
    }

    public async Task PublishNotification(int notificationId, CancellationToken cancellationToken)
    {
        await using var session = _store.DirtyTrackedSession();

        var notification = await session.LoadAsync<Notification>(notificationId, cancellationToken);

        if (notification is null)
        {
            throw new NotificationNotFoundException();
        }

        notification.Status = NotificationStatus.Published;
        notification.LastModified = DateTimeOffset.UtcNow;

        session.Update(notification);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishNotification(int notificationId, CancellationToken cancellationToken)
    {
        await using var session = _store.DirtyTrackedSession();

        var notification = await session.LoadAsync<Notification>(notificationId, cancellationToken);

        if (notification is null)
        {
            throw new NotificationNotFoundException();
        }

        if (notification.Status != NotificationStatus.Published)
        {
            throw new NotificationStatusIsNotPublishedException(notification.Status);
        }

        notification.Status = NotificationStatus.Unpublished;
        notification.LastModified = DateTimeOffset.UtcNow;

        session.Update(notification);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteNotification(int notificationId, CancellationToken cancellationToken)
    {
        await using var session = _store.DirtyTrackedSession();

        var notification = await session.LoadAsync<Notification>(notificationId, cancellationToken);

        if (notification is null)
        {
            throw new NotificationNotFoundException();
        }

        if (notification.Status != NotificationStatus.Draft)
        {
            throw new NotificationStatusIsNotDraftException(notification.Status);
        }

        session.Delete(notification);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetActiveNotifications(string platform, CancellationToken cancellationToken)
    {
        await using var session = _store.QuerySession();

        var notifications = await session.Query<Notification>()
            .Where(x =>
                x.Platforms.Contains(platform)
                && x.Status == NotificationStatus.Published
                && x.ValidFrom < DateTimeOffset.UtcNow
                && x.ValidTo > DateTimeOffset.UtcNow
            )
            .ToListAsync(cancellationToken);

        return notifications;
    }

    public async Task<IReadOnlyList<Notification>> GetNotifications(
        NotificationStatus? status = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        await using var session = _store.QuerySession();

        var query = session.Query<Notification>().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status);
        }

        if (validFrom.HasValue)
        {
            query = query.Where(x => x.ValidFrom >= validFrom.Value);
        }

        if (validTo.HasValue)
        {
            query = query.Where(x => x.ValidTo <= validTo.Value);
        }

        // Sort by ValidFrom descending and limit records
        var notifications = await query
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.ValidTo)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
