namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>Lifecycle state of a member from the access module's point of view.</summary>
public enum AccessStatus
{
    /// <summary>The member may check in when the membership is also valid.</summary>
    Active,

    /// <summary>The membership is temporarily frozen.</summary>
    Frozen,

    /// <summary>Access is suspended by staff for policy or safety reasons.</summary>
    Suspended,

    /// <summary>The account or membership has expired.</summary>
    Expired,
}

/// <summary>Lifecycle state of the membership projection held by this module.</summary>
public enum MembershipStatus
{
    /// <summary>The membership is paid and running.</summary>
    Active,

    /// <summary>The membership is paused for a freeze period.</summary>
    Frozen,

    /// <summary>The membership was cancelled and cannot grant access.</summary>
    Cancelled,

    /// <summary>The membership period has ended.</summary>
    Expired,
}

/// <summary>Where a visit was recorded.</summary>
public enum CheckInSource
{
    /// <summary>Recorded by staff at the front desk.</summary>
    FrontDesk,

    /// <summary>Started by the member from the mobile application.</summary>
    MemberApp,
}

/// <summary>Audited access actions.</summary>
public enum AccessAuditAction
{
    /// <summary>A member was checked in.</summary>
    CheckIn,

    /// <summary>A member was checked out.</summary>
    CheckOut,

    /// <summary>A check-in attempt was rejected.</summary>
    AccessDenied,
}

/// <summary>Outcome of an audited access action.</summary>
public enum AccessOutcome
{
    /// <summary>The action was accepted.</summary>
    Allowed,

    /// <summary>The action was rejected.</summary>
    Denied,
}
