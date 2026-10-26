using Kinevor.Modules.CheckInsAccess.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Reads the member's access, live visit and occupancy state.</summary>
/// <param name="MemberId">Linked external member id.</param>
public sealed record GetMyCheckInStatusQuery(Guid MemberId) : IRequest<MemberCheckInStatusDto>;

/// <summary>Handles the member self-service status screen.</summary>
public sealed class GetMyCheckInStatusHandler(
    IAccessMemberRepository members,
    ICheckInRepository checkIns,
    IClubBranchRepository branches,
    TimeProvider clock)
    : IRequestHandler<GetMyCheckInStatusQuery, MemberCheckInStatusDto>
{
    /// <summary>Builds the member's eligibility, occupancy and recent visit summary.</summary>
    public async Task<MemberCheckInStatusDto> Handle(GetMyCheckInStatusQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var member = await members.GetByIdAsync(query.MemberId, ct)
            ?? throw new NotFoundException("Member", query.MemberId);
        var now = clock.GetUtcNow().UtcDateTime;
        var homeBranch = member.HomeBranchId is { } homeId
            ? await branches.GetByIdAsync(homeId, ct)
            : null;
        var open = await checkIns.GetOpenByMemberAsync(member.Id, ct);
        var decision = AccessPolicy.Evaluate(member, now, member.HomeBranchId);
        var eligibility = CheckInMapper.ToEligibility(
            member, decision, member.HomeBranchId, homeBranch, homeBranch, open, now);

        OccupancyDto? occupancy = null;
        if (homeBranch is not null)
        {
            var openVisits = await checkIns.GetOpenByBranchAsync(homeBranch.Id, ct);
            occupancy = CheckInQuerySupport.ToOccupancy(homeBranch, openVisits, now);
        }

        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var visitsThisMonth = await checkIns.CountVisitsAsync(
            member.Id, monthStart, monthStart.AddMonths(1), ct);
        var (lastVisits, _) = await checkIns.SearchAsync(
            new CheckInSearch(null, member.Id, null, null, null, false, 1, 1), ct);
        var lastVisit = lastVisits.Count > 0 ? CheckInMapper.ToDto(lastVisits[0], now) : null;

        return new MemberCheckInStatusDto(
            member.Id,
            member.MemberNumber,
            member.FullName,
            member.Status.ToString(),
            member.MembershipStatus.ToString(),
            eligibility,
            open is null ? null : CheckInMapper.ToDto(open, now),
            occupancy,
            visitsThisMonth,
            lastVisit);
    }
}

/// <summary>Reads the member's own visit history.</summary>
/// <param name="MemberId">Linked external member id.</param>
/// <param name="From">Inclusive UTC start.</param>
/// <param name="To">Inclusive UTC end.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record GetMyVisitsQuery(
    Guid MemberId,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CheckInDto>>;

/// <summary>Handles member visit-history reads.</summary>
public sealed class GetMyVisitsHandler(ICheckInRepository checkIns, TimeProvider clock)
    : IRequestHandler<GetMyVisitsQuery, PagedResult<CheckInDto>>
{
    /// <summary>Returns the member's visits newest first.</summary>
    public async Task<PagedResult<CheckInDto>> Handle(GetMyVisitsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var search = new CheckInSearch(
            null,
            query.MemberId,
            null,
            query.From?.ToUniversalTime(),
            query.To?.ToUniversalTime(),
            null,
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
