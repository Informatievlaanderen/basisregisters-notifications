namespace NotificationService.Api.Notification;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

[ApiVersion("1.0")]
[AdvertiseApiVersions("1.0")]
[ApiRoute("notificaties")]
[ApiExplorerSettings(GroupName = "notificaties")]
public partial class NotificationsController : ApiController
{
    private readonly IActionContextAccessor _actionContextAccessor;

    public NotificationsController(IActionContextAccessor actionContextAccessor)
    {
        _actionContextAccessor = actionContextAccessor;
    }
}
