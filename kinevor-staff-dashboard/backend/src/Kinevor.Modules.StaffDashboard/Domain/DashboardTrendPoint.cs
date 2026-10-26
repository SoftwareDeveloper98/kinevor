using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.StaffDashboard.Domain;

/// <summary>One day of operational movement inside a dashboard snapshot.</summary>
public sealed class DashboardTrendPoint : Entity
{
    private DashboardTrendPoint()
    {
    }

    internal DashboardTrendPoint(
        Guid snapshotId,
        DateTime dateUtc,
        int checkIns,
        int bookings,
        decimal revenue)
    {
        SnapshotId = snapshotId;
        DateUtc = dateUtc.Date;
        CheckIns = checkIns;
        Bookings = bookings;
        Revenue = revenue;
    }

    /// <summary>Snapshot that owns this point.</summary>
    public Guid SnapshotId { get; private set; }

    /// <summary>UTC calendar day the point describes.</summary>
    public DateTime DateUtc { get; private set; }

    /// <summary>Check-ins recorded on the day.</summary>
    public int CheckIns { get; private set; }

    /// <summary>Class bookings recorded on the day.</summary>
    public int Bookings { get; private set; }

    /// <summary>Revenue collected on the day.</summary>
    public decimal Revenue { get; private set; }
}
