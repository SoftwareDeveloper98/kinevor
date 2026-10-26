using Kinevor.Modules.CheckInsAccess.Domain;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Compact member row used by the front-desk lookup.</summary>
public sealed record AccessMemberSummaryDto(
    Guid Id,
    string MemberNumber,
    string FullName,
    string? Email,
    string Status,
    string MembershipStatus,
    string MembershipPlan,
    DateTime MembershipStartsAt,
    DateTime MembershipEndsAt,
    Guid? HomeBranchId,
    string? HomeBranchName,
    bool AllClubsAccess,
    bool IsInside,
    Guid? OpenCheckInId,
    DateTime? CheckedInAt);

/// <summary>Access decision and the member context used to produce it.</summary>
public sealed record EligibilityDto(
    Guid MemberId,
    string MemberNumber,
    string FullName,
    string MemberStatus,
    string MembershipStatus,
    string MembershipPlan,
    DateTime MembershipStartsAt,
    DateTime MembershipEndsAt,
    Guid? HomeBranchId,
    string? HomeBranchName,
    bool AllClubsAccess,
    Guid? BranchId,
    string? BranchName,
    bool Allowed,
    string Code,
    string Message,
    Guid? OpenCheckInId,
    DateTime? CheckedInAt,
    DateTime EvaluatedAt);

/// <summary>Visit projection shared by staff and member applications.</summary>
public sealed record CheckInDto(
    Guid Id,
    Guid MemberId,
    Guid MembershipId,
    string MemberName,
    string MemberNumber,
    Guid BranchId,
    string BranchName,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt,
    int? DurationMinutes,
    bool IsOpen,
    string Source,
    string? Notes,
    string RecordedBy,
    string RecordedByRole,
    string AccessCode);

/// <summary>Branch projection used by selectors and occupancy views.</summary>
public sealed record BranchDto(Guid Id, string Name, string Code, string City, int Capacity);

/// <summary>Point-in-time occupancy snapshot for a branch.</summary>
public sealed record OccupancyDto(
    Guid BranchId,
    string BranchName,
    string BranchCode,
    string City,
    int Capacity,
    int Occupied,
    int Available,
    double UtilizationPercent,
    string Status,
    DateTime AsOf,
    IReadOnlyList<CheckInDto> OpenVisits);

/// <summary>Aggregated visit statistics for one UTC day.</summary>
public sealed record DailyStatsDto(
    DateOnly Date,
    Guid? BranchId,
    string? BranchName,
    int TotalCheckIns,
    int UniqueMembers,
    int CompletedVisits,
    int OpenVisits,
    int AverageDurationMinutes,
    int? PeakHour,
    int PeakHourCheckIns);

/// <summary>Append-only access audit projection.</summary>
public sealed record AccessAuditDto(
    Guid Id,
    Guid MemberId,
    string MemberNumber,
    string MemberName,
    Guid? BranchId,
    Guid? CheckInId,
    string Action,
    string Outcome,
    string Code,
    string Message,
    string ActorId,
    string ActorRole,
    string Source,
    DateTime OccurredAt);

/// <summary>Member self-service status with eligibility, open visit, occupancy and recent activity.</summary>
public sealed record MemberCheckInStatusDto(
    Guid MemberId,
    string MemberNumber,
    string FullName,
    string MemberStatus,
    string MembershipStatus,
    EligibilityDto Eligibility,
    CheckInDto? OpenCheckIn,
    OccupancyDto? Occupancy,
    int VisitsThisMonth,
    CheckInDto? LastVisit);

internal static class CheckInMapper
{
    internal static AccessMemberSummaryDto ToSummary(
        AccessMember member, CheckIn? openCheckIn, ClubBranch? homeBranch) => new(
        member.Id,
        member.MemberNumber,
        member.FullName,
        member.Email,
        member.Status.ToString(),
        member.MembershipStatus.ToString(),
        member.MembershipPlan,
        member.MembershipStartsAt,
        member.MembershipEndsAt,
        member.HomeBranchId,
        homeBranch?.Name,
        member.HasAllClubAccess,
        openCheckIn is not null,
        openCheckIn?.Id,
        openCheckIn?.CheckedInAt);

    internal static EligibilityDto ToEligibility(
        AccessMember member,
        AccessDecision decision,
        Guid? branchId,
        ClubBranch? branch,
        ClubBranch? homeBranch,
        CheckIn? openCheckIn,
        DateTime now) => new(
        member.Id,
        member.MemberNumber,
        member.FullName,
        member.Status.ToString(),
        member.MembershipStatus.ToString(),
        member.MembershipPlan,
        member.MembershipStartsAt,
        member.MembershipEndsAt,
        member.HomeBranchId,
        homeBranch?.Name,
        member.HasAllClubAccess,
        branchId,
        branch?.Name,
        decision.IsAllowed,
        decision.Code,
        decision.Message,
        openCheckIn?.Id,
        openCheckIn?.CheckedInAt,
        now);

    internal static CheckInDto ToDto(CheckIn checkIn, DateTime now) => new(
        checkIn.Id,
        checkIn.MemberId,
        checkIn.MembershipId,
        checkIn.MemberName,
        checkIn.MemberNumber,
        checkIn.BranchId,
        checkIn.BranchName,
        checkIn.CheckedInAt,
        checkIn.CheckedOutAt,
        (int)Math.Round(checkIn.DurationAt(now).TotalMinutes),
        checkIn.IsOpen,
        checkIn.Source.ToString(),
        checkIn.Notes,
        checkIn.RecordedBy,
        checkIn.RecordedByRole,
        checkIn.AccessCode);

    internal static BranchDto ToDto(ClubBranch branch) => new(
        branch.Id, branch.Name, branch.Code, branch.City, branch.Capacity);

    internal static AccessAuditDto ToDto(AccessAudit audit) => new(
        audit.Id,
        audit.MemberId,
        audit.MemberNumber,
        audit.MemberName,
        audit.BranchId,
        audit.CheckInId,
        audit.Action.ToString(),
        audit.Outcome.ToString(),
        audit.Code,
        audit.Message,
        audit.ActorId,
        audit.ActorRole,
        audit.Source.ToString(),
        audit.OccurredAt);
}
