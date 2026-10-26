using Kinevor.Modules.CheckInsAccess.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.CheckInsAccess.Api;

/// <summary>Front-desk check-in, eligibility, occupancy, history and audit endpoints.</summary>
[ApiController, Route("api/v1/checkins")]
public sealed class CheckInsController(
    ISender sender,
    ICurrentUser currentUser,
    IBranchContext branchContext) : ControllerBase
{
    /// <summary>Looks up members by name, member number or e-mail.</summary>
    [HttpGet("members")]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccessMemberSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AccessMemberSummaryDto>>>> Members(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAccessMembersQuery(search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AccessMemberSummaryDto>>.Ok(result));
    }

    /// <summary>Evaluates eligibility at the selected branch.</summary>
    [HttpGet("eligibility/{memberId:guid}")]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<EligibilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EligibilityDto>>> Eligibility(
        Guid memberId,
        [FromQuery] Guid? branchId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetMemberEligibilityQuery(memberId, branchId ?? branchContext.BranchId), ct);
        return Ok(ApiResponse<EligibilityDto>.Ok(result));
    }

    /// <summary>Searches visit history with member, branch, date and open-state filters.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CheckInDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CheckInDto>>>> History(
        [FromQuery] string? search,
        [FromQuery] Guid? memberId,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetCheckInsQuery(
            search,
            memberId,
            branchId ?? branchContext.BranchId,
            from,
            to,
            status,
            page,
            pageSize), ct);
        return Ok(ApiResponse<PagedResult<CheckInDto>>.Ok(result));
    }

    /// <summary>Returns aggregated visit statistics for one UTC day.</summary>
    [HttpGet("stats")]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<DailyStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DailyStatsDto>>> Stats(
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? branchId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetDailyStatsQuery(date, branchId ?? branchContext.BranchId), ct);
        return Ok(ApiResponse<DailyStatsDto>.Ok(result));
    }

    /// <summary>Returns the live occupancy snapshot for a branch.</summary>
    [HttpGet("occupancy")]
    [Authorize(Policy = Policies.MemberOrAbove)]
    [ProducesResponseType(typeof(ApiResponse<OccupancyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OccupancyDto>>> Occupancy(
        [FromQuery] Guid? branchId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetOccupancyQuery(branchId ?? branchContext.BranchId), ct);
        return Ok(ApiResponse<OccupancyDto>.Ok(result));
    }

    /// <summary>Lists branches with their safe occupancy capacity.</summary>
    [HttpGet("branches")]
    [Authorize(Policy = Policies.MemberOrAbove)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchDto>>>> Branches(CancellationToken ct)
    {
        var result = await sender.Send(new GetBranchesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.Ok(result));
    }

    /// <summary>Searches the append-only access audit log.</summary>
    [HttpGet("audit")]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccessAuditDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AccessAuditDto>>>> Audit(
        [FromQuery] Guid? memberId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAccessAuditsQuery(memberId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AccessAuditDto>>.Ok(result));
    }

    /// <summary>Checks a member in after validating eligibility and duplicate visits.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CheckInDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CheckInDto>>> CheckIn(
        [FromBody] CreateCheckInRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new CheckInCommand(
            request.MemberId,
            request.BranchId ?? branchContext.BranchId,
            request.Notes,
            ActorId,
            ActorRole), ct);

        return Created(
            $"/api/v1/checkins/{result.Id}",
            ApiResponse<CheckInDto>.Ok(result, "Member checked in."));
    }

    /// <summary>Closes an open visit.</summary>
    [HttpPost("{id:guid}/checkout")]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CheckInDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CheckInDto>>> CheckOut(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new CheckOutCommand(id, ActorId, ActorRole), ct);
        return Ok(ApiResponse<CheckInDto>.Ok(result, "Member checked out."));
    }

    private string ActorId =>
        currentUser.UserId?.ToString() ?? currentUser.Email ?? "staff.demo";

    private string ActorRole => currentUser.Roles.FirstOrDefault() ?? "Staff";
}

/// <summary>Front-desk check-in request.</summary>
/// <param name="MemberId">External member id.</param>
/// <param name="BranchId">Optional branch override; otherwise the request branch context is used.</param>
/// <param name="Notes">Optional front-desk notes.</param>
public sealed record CreateCheckInRequest(Guid MemberId, Guid? BranchId, string? Notes);
