using Kinevor.Modules.Notifications.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.Notifications.Api;

/// <summary>Staff notification composition, ledger, template and suppression endpoints.</summary>
[ApiController, Route("api/v1/notifications"), Authorize(Policy = Policies.StaffOrAdmin)]
public class NotificationsController(ISender sender) : ControllerBase
{
    /// <summary>Filters and pages the notification ledger.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> List(
        [FromQuery] Guid? memberId,
        [FromQuery] string? category,
        [FromQuery] string? channel,
        [FromQuery] string? status,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new SearchNotificationsQuery(memberId, category, channel, status, unreadOnly, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
    }

    /// <summary>Returns one notification from the ledger.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetNotificationForStaffQuery(id), ct);
        return Ok(ApiResponse<NotificationDto>.Ok(result));
    }

    /// <summary>Composes and delivers a notification for one member.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SendResultDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SendResultDto>>> Compose(
        [FromBody] ComposeNotificationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ComposeNotificationCommand(
            request.MemberId,
            request.TemplateKey,
            request.Title,
            request.Body,
            request.Category,
            request.Channel,
            request.RecipientEmail,
            request.ScheduledAt,
            request.Variables), ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<SendResultDto>.Ok(result, "Notification created."));
    }

    /// <summary>Composes and delivers the same notification for several members.</summary>
    [HttpPost("broadcast")]
    [ProducesResponseType(typeof(ApiResponse<SendResultDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SendResultDto>>> Broadcast(
        [FromBody] BroadcastNotificationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new BroadcastNotificationCommand(
            request.MemberIds,
            request.TemplateKey,
            request.Title,
            request.Body,
            request.Category,
            request.Channel,
            request.ScheduledAt,
            request.Variables), ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<SendResultDto>.Ok(result, "Broadcast created."));
    }

    /// <summary>Processes every queued notification that is due.</summary>
    [HttpPost("dispatch")]
    [ProducesResponseType(typeof(ApiResponse<DispatchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DispatchResultDto>>> Dispatch(CancellationToken ct)
    {
        var result = await sender.Send(new DispatchDueNotificationsCommand(), ct);
        return Ok(ApiResponse<DispatchResultDto>.Ok(result));
    }

    /// <summary>Lists the message templates available to staff.</summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationTemplateDto>>>> Templates(CancellationToken ct)
    {
        var result = await sender.Send(new GetNotificationTemplatesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationTemplateDto>>.Ok(result));
    }

    /// <summary>Lists the notification categories and their default channel flags.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationCategoryDto>>>> Categories(CancellationToken ct)
    {
        var result = await sender.Send(new GetNotificationCategoriesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationCategoryDto>>.Ok(result));
    }

    /// <summary>Lists suppressed e-mail addresses.</summary>
    [HttpGet("suppressions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmailSuppressionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmailSuppressionDto>>>> Suppressions(
        [FromQuery] string? search, CancellationToken ct)
    {
        var result = await sender.Send(new GetEmailSuppressionsQuery(search), ct);
        return Ok(ApiResponse<IReadOnlyList<EmailSuppressionDto>>.Ok(result));
    }

    /// <summary>Records or updates an e-mail suppression.</summary>
    [HttpPost("suppressions")]
    [ProducesResponseType(typeof(ApiResponse<EmailSuppressionDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EmailSuppressionDto>>> AddSuppression(
        [FromBody] AddEmailSuppressionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new AddEmailSuppressionCommand(request.Email, request.Reason, request.Detail), ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EmailSuppressionDto>.Ok(result, "E-mail suppression recorded."));
    }

    /// <summary>Removes an e-mail suppression by address.</summary>
    [HttpDelete("suppressions")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> RemoveSuppression(
        [FromQuery] string email, CancellationToken ct)
    {
        await sender.Send(new RemoveEmailSuppressionCommand(email), ct);
        return Ok(ApiResponse.Ok("E-mail suppression removed."));
    }
}

/// <summary>Staff compose request body.</summary>
public record ComposeNotificationRequest(
    Guid MemberId,
    string? TemplateKey = null,
    string? Title = null,
    string? Body = null,
    string? Category = null,
    string? Channel = null,
    string? RecipientEmail = null,
    DateTime? ScheduledAt = null,
    IReadOnlyDictionary<string, string>? Variables = null);

/// <summary>Staff broadcast request body.</summary>
public record BroadcastNotificationRequest(
    IReadOnlyList<Guid> MemberIds,
    string? TemplateKey = null,
    string? Title = null,
    string? Body = null,
    string? Category = null,
    string? Channel = null,
    DateTime? ScheduledAt = null,
    IReadOnlyDictionary<string, string>? Variables = null);

/// <summary>E-mail suppression request body.</summary>
public record AddEmailSuppressionRequest(string Email, string Reason, string? Detail = null);
