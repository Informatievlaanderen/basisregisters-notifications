namespace NotificationService.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface INotifications
{
    Task<Guid> CreateNotification(CancellationToken cancellationToken = default);
}
