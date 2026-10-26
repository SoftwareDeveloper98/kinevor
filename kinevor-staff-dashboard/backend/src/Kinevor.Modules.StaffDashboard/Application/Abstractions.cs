using Kinevor.Modules.StaffDashboard.Domain;

namespace Kinevor.Modules.StaffDashboard.Application;

/// <summary>Persistence contract for dashboard snapshots.</summary>
public interface IDashboardRepository
{
    /// <summary>Loads current snapshots, optionally scoped to one branch.</summary>
    Task<IReadOnlyList<DashboardSnapshot>> GetSnapshotsAsync(
        Guid? branchId,
        CancellationToken ct = default);

    /// <summary>Returns the branches known to the dashboard.</summary>
    Task<IReadOnlyList<DashboardBranchDto>> GetBranchesAsync(CancellationToken ct = default);
}
