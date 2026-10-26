using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.StaffDashboard.Domain;

/// <summary>A recent operational event shown in the staff activity feed.</summary>
public sealed class DashboardActivity : Entity
{
    private DashboardActivity()
    {
    }

    internal DashboardActivity(
        Guid snapshotId,
        DashboardActivityKind kind,
        string title,
        string detail,
        DateTime occurredAtUtc)
    {
        SnapshotId = snapshotId;
        Kind = kind;
        Title = title;
        Detail = detail;
        OccurredAtUtc = occurredAtUtc;
    }

    /// <summary>Snapshot that owns this activity.</summary>
    public Guid SnapshotId { get; private set; }

    /// <summary>Activity category.</summary>
    public DashboardActivityKind Kind { get; private set; }

    /// <summary>Short headline, usually the member and action.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Supporting detail such as class or invoice information.</summary>
    public string Detail { get; private set; } = string.Empty;

    /// <summary>UTC timestamp of the event.</summary>
    public DateTime OccurredAtUtc { get; private set; }
}
