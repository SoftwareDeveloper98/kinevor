using Kinevor.Modules.StaffDashboard.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.StaffDashboard.Api;

/// <summary>Staff-facing dashboard KPIs, trends, activity and alerts.</summary>
[ApiController, Route("api/v1/staff-dashboard"), Authorize(Policy = Policies.StaffOrAdmin)]
public sealed class StaffDashboardController(
    StaffDashboardService dashboard,
    IBranchContext branchContext) : ControllerBase
{
    /// <summary>Returns the dashboard for a branch, or all branches when none is selected.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<StaffDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StaffDashboardDto>>> Get(
        [FromQuery] Guid? branchId,
        [FromQuery] int days = 7,
        CancellationToken ct = default)
    {
        var selectedBranch = branchId ?? branchContext.BranchId;
        var result = await dashboard.GetAsync(selectedBranch, days, ct);
        return Ok(ApiResponse<StaffDashboardDto>.Ok(result));
    }

    /// <summary>Returns the complete operational alert inbox.</summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<DashboardAlertsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DashboardAlertsDto>>> GetAlerts(
        [FromQuery] Guid? branchId,
        CancellationToken ct = default)
    {
        var selectedBranch = branchId ?? branchContext.BranchId;
        var result = await dashboard.GetAlertsAsync(selectedBranch, ct);
        return Ok(ApiResponse<DashboardAlertsDto>.Ok(result));
    }

    /// <summary>Lists the branches available in the dashboard selector.</summary>
    [HttpGet("branches")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardBranchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DashboardBranchDto>>>> GetBranches(
        CancellationToken ct = default)
    {
        var result = await dashboard.GetBranchesAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<DashboardBranchDto>>.Ok(result));
    }
}
