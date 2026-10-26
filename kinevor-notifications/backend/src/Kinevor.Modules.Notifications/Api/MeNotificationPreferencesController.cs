using Kinevor.Modules.Notifications.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.Notifications.Api;

/// <summary>Member notification preference endpoints.</summary>
[ApiController, Route("api/v1/me/notification-preferences"), Authorize(Policy = Policies.MemberOrAbove)]
public class MeNotificationPreferencesController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Lists the authenticated member's preferences for every category.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationPreferenceDto>>>> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetNotificationPreferencesQuery(MemberId), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>.Ok(result));
    }

    /// <summary>Upserts several category preferences.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationPreferenceDto>>>> Update(
        [FromBody] IReadOnlyList<NotificationPreferenceInput> preferences, CancellationToken ct)
    {
        await sender.Send(new UpdateNotificationPreferencesCommand(MemberId, preferences), ct);
        var result = await sender.Send(new GetNotificationPreferencesQuery(MemberId), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>.Ok(result, "Preferences updated."));
    }

    /// <summary>Updates the channel flags for one category.</summary>
    [HttpPatch("{category}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationPreferenceDto>>>> UpdateCategory(
        string category, [FromBody] UpdateNotificationPreferenceRequest request, CancellationToken ct)
    {
        await sender.Send(new UpdateNotificationPreferencesCommand(
            MemberId,
            [new NotificationPreferenceInput(category, request.Email, request.Push, request.InApp)]), ct);

        var result = await sender.Send(new GetNotificationPreferencesQuery(MemberId), ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationPreferenceDto>>.Ok(result, "Preference updated."));
    }

    private Guid MemberId =>
        currentUser.MemberId ?? throw new ForbiddenException("No linked member profile.");
}

/// <summary>Channel flags for one category update.</summary>
public record UpdateNotificationPreferenceRequest(bool Email, bool Push, bool InApp);
