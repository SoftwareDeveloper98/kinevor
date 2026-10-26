using Kinevor.Modules.CheckInsAccess.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Looks up access members for the front desk.</summary>
/// <param name="Search">Name, member-number or e-mail term.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record GetAccessMembersQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AccessMemberSummaryDto>>;

/// <summary>Handles front-desk member lookup.</summary>
public sealed class GetAccessMembersHandler(
    IAccessMemberRepository members,
    ICheckInRepository checkIns,
    IClubBranchRepository branches)
    : IRequestHandler<GetAccessMembersQuery, PagedResult<AccessMemberSummaryDto>>
{
    /// <summary>Returns a page of members with their live occupancy state.</summary>
    public async Task<PagedResult<AccessMemberSummaryDto>> Handle(GetAccessMembersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, total) = await members.SearchAsync(query.Search, page, pageSize, ct);
        var openVisits = await checkIns.GetOpenByMembersAsync(
            items.Select(member => member.Id).ToArray(), ct);
        var branchLookup = (await branches.ListAsync(ct)).ToDictionary(branch => branch.Id);

        return new PagedResult<AccessMemberSummaryDto>(
            items.Select(member => CheckInMapper.ToSummary(
                member,
                openVisits.GetValueOrDefault(member.Id),
                member.HomeBranchId is { } homeId ? branchLookup.GetValueOrDefault(homeId) : null))
                .ToList(),
            total,
            page,
            pageSize);
    }
}

/// <summary>Evaluates a member's access at a branch.</summary>
/// <param name="MemberId">External member id.</param>
/// <param name="BranchId">Requested branch; defaults to the member's home branch.</param>
public sealed record GetMemberEligibilityQuery(Guid MemberId, Guid? BranchId = null)
    : IRequest<EligibilityDto>;

/// <summary>Handles eligibility evaluation.</summary>
public sealed class GetMemberEligibilityHandler(
    IAccessMemberRepository members,
    ICheckInRepository checkIns,
    IClubBranchRepository branches,
    TimeProvider clock)
    : IRequestHandler<GetMemberEligibilityQuery, EligibilityDto>
{
    /// <summary>Evaluates the membership and returns a staff-readable explanation.</summary>
    public async Task<EligibilityDto> Handle(GetMemberEligibilityQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var member = await members.GetByIdAsync(query.MemberId, ct)
            ?? throw new NotFoundException("Member", query.MemberId);
        var branchId = query.BranchId ?? member.HomeBranchId;
        var branch = branchId is { } id ? await branches.GetByIdAsync(id, ct) : null;
        var homeBranch = member.HomeBranchId is { } homeId
            ? await branches.GetByIdAsync(homeId, ct)
            : null;
        var open = await checkIns.GetOpenByMemberAsync(member.Id, ct);
        var now = clock.GetUtcNow().UtcDateTime;
        var decision = AccessPolicy.Evaluate(member, now, branchId);

        return CheckInMapper.ToEligibility(member, decision, branchId, branch, homeBranch, open, now);
    }
}

/// <summary>Searches visit history.</summary>
/// <param name="Search">Member name or number search term.</param>
/// <param name="MemberId">Optional member filter.</param>
/// <param name="BranchId">Optional branch filter.</param>
/// <param name="From">Inclusive UTC start.</param>
/// <param name="To">Inclusive UTC end.</param>
/// <param name="Status">Optional `open` or `closed` filter.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record GetCheckInsQuery(
    string? Search = null,
    Guid? MemberId = null,
    Guid? BranchId = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 25) : IRequest<PagedResult<CheckInDto>>;

/// <summary>Handles visit history searches.</summary>
public sealed class GetCheckInsHandler(ICheckInRepository checkIns, TimeProvider clock)
    : IRequestHandler<GetCheckInsQuery, PagedResult<CheckInDto>>
{
    /// <summary>Returns a page of visits newest first.</summary>
    public async Task<PagedResult<CheckInDto>> Handle(GetCheckInsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        bool? open = query.Status?.Trim().ToLowerInvariant() switch
        {
            "open" => true,
            "closed" => false,
            _ => null,
        };

        var search = new CheckInSearch(
            query.Search,
            query.MemberId,
            query.BranchId,
            query.From?.ToUniversalTime(),
            query.To?.ToUniversalTime(),
            open,
            page,
            pageSize);
        var (items, total) = await checkIns.SearchAsync(search, ct);
        var now = clock.GetUtcNow().UtcDateTime;

        return new PagedResult<CheckInDto>(
            items.Select(item => CheckInMapper.ToDto(item, now)).ToList(),
            total,
            page,
            pageSize);
    }
}

/// <summary>Reads a point-in-time branch occupancy snapshot.</summary>
/// <param name="BranchId">Requested branch.</param>
public sealed record GetOccupancyQuery(Guid? BranchId) : IRequest<OccupancyDto>;

/// <summary>Handles branch occupancy snapshots.</summary>
public sealed class GetOccupancyHandler(
    IClubBranchRepository branches,
    ICheckInRepository checkIns,
    TimeProvider clock)
    : IRequestHandler<GetOccupancyQuery, OccupancyDto>
{
    /// <summary>Builds the occupancy snapshot and live visit list.</summary>
    public async Task<OccupancyDto> Handle(GetOccupancyQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var branchId = query.BranchId
            ?? throw new DomainException("A branch is required to read occupancy.");
        var branch = await branches.GetByIdAsync(branchId, ct)
            ?? throw new NotFoundException("Branch", branchId);
        var openVisits = await checkIns.GetOpenByBranchAsync(branchId, ct);
        var now = clock.GetUtcNow().UtcDateTime;

        return CheckInQuerySupport.ToOccupancy(branch, openVisits, now);
    }
}

/// <summary>Lists branches available to the current host.</summary>
public sealed record GetBranchesQuery : IRequest<IReadOnlyList<BranchDto>>;

/// <summary>Handles branch listing.</summary>
public sealed class GetBranchesHandler(IClubBranchRepository branches)
    : IRequestHandler<GetBranchesQuery, IReadOnlyList<BranchDto>>
{
    /// <summary>Returns every branch ordered by name.</summary>
    public async Task<IReadOnlyList<BranchDto>> Handle(GetBranchesQuery query, CancellationToken ct)
    {
        var items = await branches.ListAsync(ct);
        return items.Select(CheckInMapper.ToDto).ToList();
    }
}

/// <summary>Reads daily visit statistics for one branch.</summary>
/// <param name="Date">UTC day, defaulting to today.</param>
/// <param name="BranchId">Optional branch filter.</param>
public sealed record GetDailyStatsQuery(DateOnly? Date = null, Guid? BranchId = null)
    : IRequest<DailyStatsDto>;

/// <summary>Handles daily statistics.</summary>
public sealed class GetDailyStatsHandler(
    ICheckInRepository checkIns,
    IClubBranchRepository branches,
    TimeProvider clock)
    : IRequestHandler<GetDailyStatsQuery, DailyStatsDto>
{
    /// <summary>Aggregates check-ins, unique members, durations and the busiest hour.</summary>
    public async Task<DailyStatsDto> Handle(GetDailyStatsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = clock.GetUtcNow().UtcDateTime;
        var date = query.Date ?? DateOnly.FromDateTime(now);
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var visits = await checkIns.GetRangeAsync(start, end, query.BranchId, ct);
        var branch = query.BranchId is { } branchId
            ? await branches.GetByIdAsync(branchId, ct)
            : null;
        var completed = visits.Where(visit => !visit.IsOpen).ToList();
        var peak = visits
            .GroupBy(visit => visit.CheckedInAt.Hour)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .FirstOrDefault();

        return new DailyStatsDto(
            date,
            query.BranchId,
            branch?.Name,
            visits.Count,
            visits.Select(visit => visit.MemberId).Distinct().Count(),
            completed.Count,
            visits.Count(visit => visit.IsOpen),
            completed.Count == 0
                ? 0
                : (int)Math.Round(completed.Average(visit => visit.DurationAt(now).TotalMinutes)),
            peak?.Key,
            peak?.Count() ?? 0);
    }
}

/// <summary>Searches access audit records.</summary>
/// <param name="MemberId">Optional member filter.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record GetAccessAuditsQuery(Guid? MemberId = null, int Page = 1, int PageSize = 25)
    : IRequest<PagedResult<AccessAuditDto>>;

/// <summary>Handles audit searches.</summary>
public sealed class GetAccessAuditsHandler(IAccessAuditRepository audits)
    : IRequestHandler<GetAccessAuditsQuery, PagedResult<AccessAuditDto>>
{
    /// <summary>Returns audit records newest first.</summary>
    public async Task<PagedResult<AccessAuditDto>> Handle(GetAccessAuditsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, total) = await audits.SearchAsync(query.MemberId, page, pageSize, ct);
        return new PagedResult<AccessAuditDto>(
            items.Select(CheckInMapper.ToDto).ToList(), total, page, pageSize);
    }
}

internal static class CheckInQuerySupport
{
    internal static OccupancyDto ToOccupancy(
        ClubBranch branch, IReadOnlyList<CheckIn> openVisits, DateTime now)
    {
        var occupied = openVisits.Count;
        var available = Math.Max(0, branch.Capacity - occupied);
        var ratio = branch.Capacity == 0 ? 0 : (double)occupied / branch.Capacity;
        var status = ratio switch
        {
            >= 1 => "Full",
            >= 0.85 => "NearlyFull",
            >= 0.6 => "Busy",
            _ => "Comfortable",
        };

        return new OccupancyDto(
            branch.Id,
            branch.Name,
            branch.Code,
            branch.City,
            branch.Capacity,
            occupied,
            available,
            Math.Round(ratio * 100, 1),
            status,
            now,
            openVisits.OrderByDescending(visit => visit.CheckedInAt)
                .Select(visit => CheckInMapper.ToDto(visit, now))
                .ToList());
    }
}
