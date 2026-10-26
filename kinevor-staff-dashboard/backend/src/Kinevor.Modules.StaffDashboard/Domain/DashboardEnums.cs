namespace Kinevor.Modules.StaffDashboard.Domain;

/// <summary>Category of an entry in the dashboard activity feed.</summary>
public enum DashboardActivityKind
{
    /// <summary>A member entered the gym.</summary>
    CheckIn,

    /// <summary>A payment or invoice event.</summary>
    Payment,

    /// <summary>A new member joined.</summary>
    MemberJoined,

    /// <summary>A class booking was created.</summary>
    ClassBooking,

    /// <summary>A membership was renewed or changed.</summary>
    Membership,

    /// <summary>A notification delivery event.</summary>
    Notification,
}

/// <summary>Operational alert category surfaced to staff.</summary>
public enum DashboardAlertType
{
    /// <summary>An invoice is past its due date.</summary>
    OverdueInvoice,

    /// <summary>A membership expires soon.</summary>
    ExpiringMembership,

    /// <summary>A class has low occupancy ahead of its start time.</summary>
    LowClassFill,

    /// <summary>A member notification could not be delivered.</summary>
    FailedNotification,
}

/// <summary>Priority assigned to an operational alert.</summary>
public enum DashboardAlertSeverity
{
    /// <summary>Requires action today.</summary>
    Critical,

    /// <summary>Needs attention during the current shift.</summary>
    Warning,

    /// <summary>Informational follow-up.</summary>
    Info,
}
