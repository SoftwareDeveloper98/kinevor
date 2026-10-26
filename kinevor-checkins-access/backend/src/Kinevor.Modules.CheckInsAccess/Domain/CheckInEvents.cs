using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>A member entered a branch.</summary>
/// <param name="CheckInId">Visit identifier.</param>
/// <param name="MemberId">External member identifier.</param>
/// <param name="MemberName">Member display name.</param>
/// <param name="BranchId">Branch identifier.</param>
/// <param name="CheckedInAt">UTC check-in time.</param>
public sealed record MemberCheckedInEvent(
    Guid CheckInId,
    Guid MemberId,
    string MemberName,
    Guid BranchId,
    DateTime CheckedInAt) : IDomainEvent;

/// <summary>A member left a branch.</summary>
/// <param name="CheckInId">Visit identifier.</param>
/// <param name="MemberId">External member identifier.</param>
/// <param name="BranchId">Branch identifier.</param>
/// <param name="CheckedOutAt">UTC check-out time.</param>
/// <param name="Duration">Visit duration.</param>
public sealed record MemberCheckedOutEvent(
    Guid CheckInId,
    Guid MemberId,
    Guid BranchId,
    DateTime CheckedOutAt,
    TimeSpan Duration) : IDomainEvent;
