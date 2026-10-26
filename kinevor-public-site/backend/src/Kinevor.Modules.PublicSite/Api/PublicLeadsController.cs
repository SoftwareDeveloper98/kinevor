using Kinevor.Modules.PublicSite.Application;
using Kinevor.Platform.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.PublicSite.Api;

/// <summary>Staff-facing inbox for trial requests and website enquiries.</summary>
[ApiController]
[Route("api/v1/public/leads")]
[Authorize(Policy = Policies.StaffOrAdmin)]
public sealed class PublicLeadsController(ISender sender) : ControllerBase
{
    /// <summary>Lists combined trial and enquiry leads with filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LeadDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<LeadDto>>>> GetLeads(
        [FromQuery] string? kind,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetLeadsQuery(kind, status, search, page, pageSize),
            ct);
        return Ok(ApiResponse<PagedResult<LeadDto>>.Ok(result));
    }
}
