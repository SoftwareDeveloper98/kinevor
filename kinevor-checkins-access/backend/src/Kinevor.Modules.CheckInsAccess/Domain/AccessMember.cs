using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>
/// Local, standalone projection of the member and membership facts needed to decide gym access.
/// Only external member and membership identifiers are stored; the module never reads another
/// module's database.
/// </summary>
public sealed class AccessMember : AggregateRoot
{
    private AccessMember()
    {
    }

    /// <summary>External membership id owned by the memberships module.</summary>
    public Guid MembershipId { get; private set; }

    /// <summary>Human-friendly member number.</summary>
    public string MemberNumber { get; private set; } = string.Empty;

    /// <summary>Member display name captured for access decisions and visit records.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Contact e-mail used by the front desk to identify a member.</summary>
    public string? Email { get; private set; }

    /// <summary>Current member lifecycle status.</summary>
    public AccessStatus Status { get; private set; }

    /// <summary>Current membership lifecycle status.</summary>
    public MembershipStatus MembershipStatus { get; private set; }

    /// <summary>Name of the membership plan at the time of projection.</summary>
    public string MembershipPlan { get; private set; } = string.Empty;

    /// <summary>First day the membership grants access.</summary>
    public DateTime MembershipStartsAt { get; private set; }

    /// <summary>Last day the membership grants access.</summary>
    public DateTime MembershipEndsAt { get; private set; }

    /// <summary>Home branch, when access is branch-restricted.</summary>
    public Guid? HomeBranchId { get; private set; }

    /// <summary>Whether the membership grants access to every branch.</summary>
    public bool HasAllClubAccess { get; private set; }

    /// <summary>Creates a validated access projection.</summary>
    /// <param name="membershipId">External membership identifier.</param>
    /// <param name="memberNumber">Human-friendly member number.</param>
    /// <param name="fullName">Member display name.</param>
    /// <param name="email">Optional contact e-mail.</param>
    /// <param name="status">Member lifecycle status.</param>
    /// <param name="membershipStatus">Membership lifecycle status.</param>
    /// <param name="membershipPlan">Membership plan name.</param>
    /// <param name="membershipStartsAt">First day of the membership period.</param>
    /// <param name="membershipEndsAt">Last day of the membership period.</param>
    /// <param name="homeBranchId">Home branch when access is branch-restricted.</param>
    /// <param name="hasAllClubAccess">Whether every branch is available.</param>
    /// <returns>The created projection, or a validation failure.</returns>
    public static Result<AccessMember> Create(
        Guid membershipId,
        string memberNumber,
        string fullName,
        string? email,
        AccessStatus status,
        MembershipStatus membershipStatus,
        string membershipPlan,
        DateTime membershipStartsAt,
        DateTime membershipEndsAt,
        Guid? homeBranchId,
        bool hasAllClubAccess)
    {
        if (membershipId == Guid.Empty)
            return Result.Failure<AccessMember>(Error.Validation("MembershipId", "Membership id is required."));
        if (string.IsNullOrWhiteSpace(memberNumber))
            return Result.Failure<AccessMember>(Error.Validation("MemberNumber", "Member number is required."));
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<AccessMember>(Error.Validation("FullName", "Member name is required."));
        if (string.IsNullOrWhiteSpace(membershipPlan))
            return Result.Failure<AccessMember>(Error.Validation("MembershipPlan", "Membership plan is required."));
        if (membershipEndsAt.Date < membershipStartsAt.Date)
            return Result.Failure<AccessMember>(Error.Validation("MembershipEndsAt", "Membership end must follow the start."));
        if (!hasAllClubAccess && homeBranchId is null)
            return Result.Failure<AccessMember>(Error.Validation("HomeBranchId", "A home branch is required when access is branch-restricted."));
        if (memberNumber.Trim().Length > 30)
            return Result.Failure<AccessMember>(Error.Validation("MemberNumber", "Member number must be 30 characters or fewer."));
        if (fullName.Trim().Length > 160)
            return Result.Failure<AccessMember>(Error.Validation("FullName", "Member name must be 160 characters or fewer."));
        if (email is { Length: > 200 })
            return Result.Failure<AccessMember>(Error.Validation("Email", "E-mail must be 200 characters or fewer."));

        return Result.Success(new AccessMember
        {
            MembershipId = membershipId,
            MemberNumber = memberNumber.Trim(),
            FullName = fullName.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            Status = status,
            MembershipStatus = membershipStatus,
            MembershipPlan = membershipPlan.Trim(),
            MembershipStartsAt = membershipStartsAt.Date,
            MembershipEndsAt = membershipEndsAt.Date,
            HomeBranchId = homeBranchId,
            HasAllClubAccess = hasAllClubAccess,
        });
    }
}
