using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>
/// Append-only audit record for successful and denied access actions. Audits never store
/// credentials or tokens; they capture the decision code, actor and branch context.
/// </summary>
public sealed class AccessAudit : Entity
{
    private AccessAudit()
    {
    }

    /// <summary>External member identifier.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Member number snapshot.</summary>
    public string MemberNumber { get; private set; } = string.Empty;

    /// <summary>Member name snapshot.</summary>
    public string MemberName { get; private set; } = string.Empty;

    /// <summary>Branch context, when known.</summary>
    public Guid? BranchId { get; private set; }

    /// <summary>Visit identifier for check-in and check-out audits.</summary>
    public Guid? CheckInId { get; private set; }

    /// <summary>Audited action.</summary>
    public AccessAuditAction Action { get; private set; }

    /// <summary>Action outcome.</summary>
    public AccessOutcome Outcome { get; private set; }

    /// <summary>Stable access-decision code.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Human-readable decision message.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Actor identity.</summary>
    public string ActorId { get; private set; } = string.Empty;

    /// <summary>Actor role.</summary>
    public string ActorRole { get; private set; } = string.Empty;

    /// <summary>Visit source.</summary>
    public CheckInSource Source { get; private set; }

    /// <summary>UTC time of the audited action.</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>Creates an audit record.</summary>
    /// <param name="member">Member projection involved.</param>
    /// <param name="branchId">Branch context.</param>
    /// <param name="action">Audited action.</param>
    /// <param name="outcome">Action outcome.</param>
    /// <param name="decision">Access decision.</param>
    /// <param name="source">Visit source.</param>
    /// <param name="actorId">Actor identity.</param>
    /// <param name="actorRole">Actor role.</param>
    /// <param name="now">Current UTC time.</param>
    /// <param name="checkInId">Optional visit identifier.</param>
    /// <returns>The audit record.</returns>
    public static AccessAudit Record(
        AccessMember member,
        Guid? branchId,
        AccessAuditAction action,
        AccessOutcome outcome,
        AccessDecision decision,
        CheckInSource source,
        string actorId,
        string actorRole,
        DateTime now,
        Guid? checkInId = null)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(decision);

        return new AccessAudit
        {
            MemberId = member.Id,
            MemberNumber = member.MemberNumber,
            MemberName = member.FullName,
            BranchId = branchId,
            CheckInId = checkInId,
            Action = action,
            Outcome = outcome,
            Code = decision.Code,
            Message = decision.Message,
            ActorId = actorId,
            ActorRole = actorRole,
            Source = source,
            OccurredAt = now,
        };
    }
}
