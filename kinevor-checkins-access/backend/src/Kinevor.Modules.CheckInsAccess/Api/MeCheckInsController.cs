using Kinevor.Modules.CheckInsAccess.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.CheckInsAccess.Api;

/// <summary>Member self-service check-in, status and visit-history endpoints.</summary>
[ApiController, Route("api/v1/me/checkins"), Authorize(Policy = Policies.MemberOrAbove)]
public sealed class MeCheckInsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Returns eligibility, live occupancy, the open visit and recent activity.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<MemberCheckInStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MemberCheckInStatusDto>>> Status(CancellationToken ct)
    {
        var result = await sender.Send(new GetMyCheckInStatusQuery(CurrentMemberId), ct);
        return Ok(ApiResponse<MemberCheckInStatusDto>.Ok(result));
    }

    /// <summary>Lists the member's own visits newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CheckInDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CheckInDto>>>> Visits(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetMyVisitsQuery(CurrentMemberId, from, to, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<CheckInDto>>.Ok(result));
    }

    /// <summary>Checks the member in at their home branch.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CheckInDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CheckInDto>>> CheckIn(
        [FromBody] SelfCheckInRequest? request,
        CancellationToken ct)
    {
        var result = await sender.Send(new SelfCheckInCommand(CurrentMemberId, request?.Notes), ct);
        return Created(
            $"/api/v1/me/checkins/{result.Id}",
            ApiResponse<CheckInDto>.Ok(result, "You are checked in."));
    }

    /// <summary>Closes the member's current visit.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<CheckInDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CheckInDto>>> CheckOut(CancellationToken ct)
    {
        var result = await sender.Send(new SelfCheckOutCommand(CurrentMemberId), ct);
        return Ok(ApiResponse<CheckInDto>.Ok(result, "You are checked out."));
    }

    private Guid CurrentMemberId =>
        currentUser.MemberId ?? throw new ForbiddenException("No linked member profile.");
}

/// <summary>Member self check-in request.</summary>
/// <param name="Notes">Optional notes for the visit.</param>
public sealed record SelfCheckInRequest(string? Notes);
