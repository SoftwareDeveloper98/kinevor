using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>
/// A member visit, from check-in to optional check-out. The record snapshots the member,
/// membership and access decision so history remains meaningful after later changes.
/// </summary>
public sealed class CheckIn : AggregateRoot
{
    private CheckIn()
    {
    }

    /// <summary>External member identifier.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>External membership identifier at check-in time.</summary>
    public Guid MembershipId { get; private set; }

    /// <summary>Member display name at check-in time.</summary>
    public string MemberName { get; private set; } = string.Empty;

    /// <summary>Member number at check-in time.</summary>
    public string MemberNumber { get; private set; } = string.Empty;

    /// <summary>Branch where the visit happened.</summary>
    public Guid BranchId { get; private set; }

    /// <summary>Branch name at check-in time.</summary>
    public string BranchName { get; private set; } = string.Empty;

    /// <summary>UTC time of entry.</summary>
    public DateTime CheckedInAt { get; private set; }

    /// <summary>UTC time of exit, when the visit is closed.</summary>
    public DateTime? CheckedOutAt { get; private set; }

    /// <summary>Visit source.</summary>
    public CheckInSource Source { get; private set; }

    /// <summary>Optional front-desk notes.</summary>
    public string? Notes { get; private set; }

    /// <summary>Identity of the staff member or member who recorded the visit.</summary>
    public string RecordedBy { get; private set; } = string.Empty;

    /// <summary>Role of the actor who recorded the visit.</summary>
    public string RecordedByRole { get; private set; } = string.Empty;

    /// <summary>Stable access-decision code captured at entry.</summary>
    public string AccessCode { get; private set; } = string.Empty;

    /// <summary>Access-decision message captured at entry.</summary>
    public string AccessMessage { get; private set; } = string.Empty;

    /// <summary>Whether the member is still inside.</summary>
    public bool IsOpen => CheckedOutAt is null;

    /// <summary>Elapsed visit duration; measured against <paramref name="now"/> while still open.</summary>
    /// <param name="now">Current UTC time.</param>
    /// <returns>The visit duration.</returns>
    public TimeSpan DurationAt(DateTime now) => (CheckedOutAt ?? now) - CheckedInAt;

    /// <summary>Records a new visit and raises <see cref="MemberCheckedInEvent"/>.</summary>
    /// <param name="now">Current UTC time.</param>
    /// <param name="member">Member projection used for the visit snapshot.</param>
    /// <param name="branchId">Branch identifier.</param>
    /// <param name="branchName">Branch name snapshot.</param>
    /// <param name="source">Visit source.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="recordedBy">Actor who recorded the visit.</param>
    /// <param name="recordedByRole">Actor role.</param>
    /// <param name="access">Access decision captured at entry.</param>
    /// <returns>The new visit, or a validation failure.</returns>
    public static Result<CheckIn> Record(
        DateTime now,
        AccessMember member,
        Guid branchId,
        string branchName,
        CheckInSource source,
        string? notes,
        string recordedBy,
        string recordedByRole,
        AccessDecision access)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(access);

        if (branchId == Guid.Empty)
            return Result.Failure<CheckIn>(Error.Validation("BranchId", "Branch is required."));
        if (string.IsNullOrWhiteSpace(branchName))
            return Result.Failure<CheckIn>(Error.Validation("BranchName", "Branch name is required."));
        if (string.IsNullOrWhiteSpace(recordedBy))
            return Result.Failure<CheckIn>(Error.Validation("RecordedBy", "The recording actor is required."));
        if (notes is { Length: > 500 })
            return Result.Failure<CheckIn>(Error.Validation("Notes", "Notes must be 500 characters or fewer."));
        if (!access.IsAllowed)
            return Result.Failure<CheckIn>(Error.Business(access.Code, access.Message));

        var visit = new CheckIn
        {
            MemberId = member.Id,
            MembershipId = member.MembershipId,
            MemberName = member.FullName,
            MemberNumber = member.MemberNumber,
            BranchId = branchId,
            BranchName = branchName.Trim(),
            CheckedInAt = now,
            Source = source,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            RecordedBy = recordedBy.Trim(),
            RecordedByRole = recordedByRole.Trim(),
            AccessCode = access.Code,
            AccessMessage = access.Message,
        };
        visit.AddDomainEvent(new MemberCheckedInEvent(
            visit.Id, visit.MemberId, visit.MemberName, visit.BranchId, visit.CheckedInAt));
        return Result.Success(visit);
    }

    /// <summary>Closes an open visit and raises <see cref="MemberCheckedOutEvent"/>.</summary>
    /// <param name="now">Current UTC time.</param>
    /// <returns>A success result, or a business failure when the visit is already closed.</returns>
    public Result CheckOut(DateTime now)
    {
        if (CheckedOutAt is not null)
            return Result.Failure(Error.Conflict("This visit is already closed."));
        if (now < CheckedInAt)
            return Result.Failure(Error.Validation("CheckedOutAt", "Check-out cannot precede check-in."));

        CheckedOutAt = now;
        AddDomainEvent(new MemberCheckedOutEvent(
            Id, MemberId, BranchId, now, now - CheckedInAt));
        return Result.Success();
    }
}
