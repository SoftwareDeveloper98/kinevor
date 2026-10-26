using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.StaffDashboard.Domain;

/// <summary>An operational task or risk surfaced to staff.</summary>
public sealed class DashboardAlert : Entity
{
    private DashboardAlert()
    {
    }

    internal DashboardAlert(
        Guid snapshotId,
        DashboardAlertType type,
        DashboardAlertSeverity severity,
        string title,
        string detail,
        DateTime? dueAtUtc,
        string reference)
    {
        SnapshotId = snapshotId;
        Type = type;
        Severity = severity;
        Title = title;
        Detail = detail;
        DueAtUtc = dueAtUtc;
        Reference = reference;
    }

    /// <summary>Snapshot that owns this alert.</summary>
    public Guid SnapshotId { get; private set; }

    /// <summary>Alert category.</summary>
    public DashboardAlertType Type { get; private set; }

    /// <summary>Priority of the alert.</summary>
    public DashboardAlertSeverity Severity { get; private set; }

    /// <summary>Short action-oriented title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Context such as member, class or amount.</summary>
    public string Detail { get; private set; } = string.Empty;

    /// <summary>UTC due date, when the alert is time-bound.</summary>
    public DateTime? DueAtUtc { get; private set; }

    /// <summary>Invoice, class or member reference used by the host.</summary>
    public string Reference { get; private set; } = string.Empty;
}
