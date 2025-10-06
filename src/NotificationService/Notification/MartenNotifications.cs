namespace NotificationService.Notification;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Schema.Identity;

public class MartenNotifications : INotifications
{
    private readonly IDocumentStore _store;

    public MartenNotifications(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Guid> CreateNotification(
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
        var notificationId = CombGuidIdGeneration.NewGuid();

        await using var session = _store.DirtyTrackedSession();

        session.Insert(new Notification(
            notificationId,
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
        ));

        await session.SaveChangesAsync(cancellationToken);

        return notificationId;
    }
}
