using Kinevor.Modules.PublicSite.Application;
using Kinevor.Platform.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.PublicSite.Api;

/// <summary>Anonymous website content and lead-capture endpoints.</summary>
[ApiController]
[Route("api/v1/public")]
[AllowAnonymous]
public sealed class PublicContentController(ISender sender) : ControllerBase
{
    /// <summary>Returns the landing-page brand, contact, stats and branch payload.</summary>
    [HttpGet("site")]
    [ProducesResponseType(typeof(ApiResponse<PublicSiteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PublicSiteDto>>> GetSite(CancellationToken ct)
    {
        var site = await sender.Send(new GetPublicSiteQuery(), ct);
        return Ok(ApiResponse<PublicSiteDto>.Ok(site));
    }

    /// <summary>Lists published locations.</summary>
    [HttpGet("branches")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchDto>>>> GetBranches(CancellationToken ct)
    {
        var branches = await sender.Send(new GetBranchesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.Ok(branches));
    }

    /// <summary>Returns a published location by slug.</summary>
    [HttpGet("branches/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetBranch(string slug, CancellationToken ct)
    {
        var branch = await sender.Send(new GetBranchQuery(slug), ct);
        return Ok(ApiResponse<BranchDto>.Ok(branch));
    }

    /// <summary>Lists published membership plans.</summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MembershipPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MembershipPlanDto>>>> GetPlans(CancellationToken ct)
    {
        var plans = await sender.Send(new GetPlansQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<MembershipPlanDto>>.Ok(plans));
    }

    /// <summary>Lists published trainers.</summary>
    [HttpGet("trainers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TrainerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrainerDto>>>> GetTrainers(CancellationToken ct)
    {
        var trainers = await sender.Send(new GetTrainersQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<TrainerDto>>.Ok(trainers));
    }

    /// <summary>Returns the public timetable for an inclusive date window.</summary>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ClassSessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClassSessionDto>>>> GetSchedule(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? branchId,
        CancellationToken ct)
    {
        var sessions = await sender.Send(new GetScheduleQuery(from, to, branchId), ct);
        return Ok(ApiResponse<IReadOnlyList<ClassSessionDto>>.Ok(sessions));
    }

    /// <summary>Lists published frequently asked questions.</summary>
    [HttpGet("faqs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FaqDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FaqDto>>>> GetFaqs(CancellationToken ct)
    {
        var faqs = await sender.Send(new GetFaqsQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<FaqDto>>.Ok(faqs));
    }

    /// <summary>Lists published blog articles.</summary>
    [HttpGet("posts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BlogPostSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogPostSummaryDto>>>> GetPosts(CancellationToken ct)
    {
        var posts = await sender.Send(new GetBlogPostsQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<BlogPostSummaryDto>>.Ok(posts));
    }

    /// <summary>Returns a published blog article by slug.</summary>
    [HttpGet("posts/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> GetPost(string slug, CancellationToken ct)
    {
        var post = await sender.Send(new GetBlogPostQuery(slug), ct);
        return Ok(ApiResponse<BlogPostDto>.Ok(post));
    }

    /// <summary>Captures a free-trial request.</summary>
    [HttpPost("trials")]
    [ProducesResponseType(typeof(ApiResponse<TrialCreatedDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<TrialCreatedDto>>> CreateTrial(
        [FromBody] TrialRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateTrialCommand(request), ct);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<TrialCreatedDto>.Ok(result, result.Message));
    }

    /// <summary>Captures a general website enquiry.</summary>
    [HttpPost("leads")]
    [ProducesResponseType(typeof(ApiResponse<LeadCreatedDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LeadCreatedDto>>> CreateLead(
        [FromBody] LeadRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateLeadCommand(request), ct);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<LeadCreatedDto>.Ok(result, result.Message));
    }
}
