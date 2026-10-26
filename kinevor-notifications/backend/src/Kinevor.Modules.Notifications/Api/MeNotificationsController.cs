using Kinevor.Modules.Notifications.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.Notifications.Api;

/// <summary>Member inbox endpoints.</summary>
[ApiController, Route("api/v1/me/notifications"), Authorize(Policy = Policies.MemberOrAbove)]
public class MeNotificationsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Lists the authenticated member's notifications with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> List(
        [FromQuery] string? category,
        [FromQuery] string? channel,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetMyNotificationsQuery(MemberId, category, channel, unreadOnly, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
    }

    /// <summary>Lists the authenticated member's unread notifications.</summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> Unread(CancellationToken ct)
    {
        var result = await sender.Send(new GetMyUnreadNotificationsQuery(MemberId), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result));
    }

    /// <summary>Returns the authenticated member's unread count.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadCountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UnreadCountDto>>> UnreadCount(CancellationToken ct)
    {
        var result = await sender.Send(new GetMyUnreadCountQuery(MemberId), ct);
        return Ok(ApiResponse<UnreadCountDto>.Ok(result));
    }

    /// <summary>Returns one notification belonging to the authenticated member.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyNotificationQuery(MemberId, id), ct);
        return Ok(ApiResponse<NotificationDto>.Ok(result));
    }

    /// <summary>Marks one notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkRead(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkNotificationReadCommand(MemberId, id), ct);
        return Ok(ApiResponse.Ok("Notification marked read."));
    }

    /// <summary>Marks every delivered notification as read.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(ApiResponse<ReadAllResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReadAllResultDto>>> MarkAllRead(CancellationToken ct)
    {
        var result = await sender.Send(new MarkAllNotificationsReadCommand(MemberId), ct);
        return Ok(ApiResponse<ReadAllResultDto>.Ok(result, "Notifications marked read."));
    }

    private Guid MemberId =>
        currentUser.MemberId ?? throw new ForbiddenException("No linked member profile.");
}
