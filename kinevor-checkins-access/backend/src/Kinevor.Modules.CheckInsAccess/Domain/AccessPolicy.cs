namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>Result of evaluating whether a member may enter a branch.</summary>
/// <param name="IsAllowed">Whether access is granted.</param>
/// <param name="Code">Stable machine-readable outcome code.</param>
/// <param name="Message">Human-readable explanation for staff and members.</param>
public sealed record AccessDecision(bool IsAllowed, string Code, string Message)
{
    /// <summary>Creates an allowed decision.</summary>
    /// <returns>An allowed decision with the standard code and message.</returns>
    public static AccessDecision Allow() =>
        new(true, "Access.Allowed", "Access granted. Enjoy your session.");

    /// <summary>Creates a denied decision.</summary>
    /// <param name="code">Stable outcome code.</param>
    /// <param name="message">Human-readable explanation.</param>
    /// <returns>A denied decision.</returns>
    public static AccessDecision Deny(string code, string message) => new(false, code, message);
}

/// <summary>Pure rules that decide whether a projected member may access a branch now.</summary>
public static class AccessPolicy
{
    /// <summary>Evaluates member status, membership status, membership window and branch scope.</summary>
    /// <param name="member">Local access projection.</param>
    /// <param name="now">Current UTC time.</param>
    /// <param name="branchId">Requested branch, when the host supplied one.</param>
    /// <returns>An allow or deny decision with a stable reason code.</returns>
    public static AccessDecision Evaluate(AccessMember member, DateTime now, Guid? branchId)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (member.Status != AccessStatus.Active)
        {
            return member.Status switch
            {
                AccessStatus.Frozen => AccessDecision.Deny(
                    "Access.MemberFrozen",
                    "This membership is frozen. Contact the front desk to reactivate access."),
                AccessStatus.Suspended => AccessDecision.Deny(
                    "Access.MemberSuspended",
                    "Access is suspended for this member. Speak to a duty manager."),
                _ => AccessDecision.Deny(
                    "Access.MemberExpired",
                    "This membership has expired. Renew at the front desk to continue training."),
            };
        }

        if (member.MembershipStatus != MembershipStatus.Active)
        {
            return member.MembershipStatus switch
            {
                MembershipStatus.Frozen => AccessDecision.Deny(
                    "Access.MembershipFrozen",
                    "The membership is frozen and cannot be used for entry."),
                MembershipStatus.Cancelled => AccessDecision.Deny(
                    "Access.MembershipCancelled",
                    "The membership was cancelled and no longer grants access."),
                _ => AccessDecision.Deny(
                    "Access.MembershipExpired",
                    "The membership period has ended. Renew to continue training."),
            };
        }

        if (now.Date < member.MembershipStartsAt.Date)
        {
            return AccessDecision.Deny(
                "Access.MembershipNotStarted",
                $"This membership starts on {member.MembershipStartsAt:yyyy-MM-dd}.");
        }

        if (now.Date > member.MembershipEndsAt.Date)
        {
            return AccessDecision.Deny(
                "Access.MembershipExpired",
                $"This membership ended on {member.MembershipEndsAt:yyyy-MM-dd}.");
        }

        if (branchId is { } requested &&
            !member.HasAllClubAccess &&
            member.HomeBranchId != requested)
        {
            return AccessDecision.Deny(
                "Access.BranchRestricted",
                "This membership is not valid at the selected branch.");
        }

        return AccessDecision.Allow();
    }
}
