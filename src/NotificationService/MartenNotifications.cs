namespace NotificationService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Marten;
using Marten.Schema.Identity;

public class MartenNotifications : INotifications
{
    private readonly IDocumentStore _store;

    public MartenNotifications(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Guid> CreateNotification(CancellationToken cancellationToken = default)
    {
        var notificationId = CombGuidIdGeneration.NewGuid();

        await using var session = _store.DirtyTrackedSession();
        session.Insert(new Notification(notificationId));
        await session.SaveChangesAsync(cancellationToken);

        return notificationId;
    }
}
