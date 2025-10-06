namespace NotificationService.Api.Endpoints;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Datadog.Trace;
using Extensions;
using Microsoft.AspNetCore.Mvc;

public static class Handlers
{
    public static async Task<Guid> Create([FromBody] IDictionary<string, string>? metadata, INotifications notifications,
        CancellationToken cancellationToken = default)
    {
        var notificationId = await notifications.CreateNotification(cancellationToken);

        using(var scope = Tracer.Instance.StartActive("CreateNotification", new SpanCreationSettings { Parent = new SpanContext(null, notificationId.ToULong()) }))
        {
            scope.Span.SetTag("notificationId", notificationId.ToString("D"));
            scope.Span.SetTag("status", "Created");
            scope.Span.SetTag("createdTimestamp", DateTime.UtcNow.ToString("o"));
        }

        return notificationId;
    }
}
