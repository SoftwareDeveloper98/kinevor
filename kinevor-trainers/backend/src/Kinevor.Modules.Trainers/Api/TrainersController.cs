using Kinevor.Modules.Trainers.Application;
using Kinevor.Platform.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.Trainers.Api;

/// <summary>Staff-facing trainer management endpoints.</summary>
[ApiController, Route("api/v1/trainers"), Authorize]
public class TrainersController(ISender sender) : ControllerBase
{
    /// <summary>Lists trainers with optional search and paging.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTrainersQuery(search, page, pageSize), ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Returns one trainer by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.TrainerOrAbove)]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTrainerByIdQuery(id), ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Creates a trainer profile.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOrAdmin)]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateTrainerCommand cmd, CancellationToken ct = default)
    {
        var id = await sender.Send(cmd, ct);
        return Created($"/api/v1/trainers/{id}", ApiResponse<object>.Ok(new { id }, "Trainer created."));
    }
}
