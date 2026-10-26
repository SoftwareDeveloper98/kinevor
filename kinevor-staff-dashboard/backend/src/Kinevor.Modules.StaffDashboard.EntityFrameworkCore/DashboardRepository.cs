using Kinevor.Modules.StaffDashboard.Application;
using Kinevor.Modules.StaffDashboard.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.StaffDashboard.EntityFrameworkCore;

/// <summary>EF Core repository for dashboard snapshots and branch options.</summary>
public sealed class DashboardRepository(DashboardDbContext db) : IDashboardRepository
{
    /// <summary>Loads current snapshots with trend, activity and alert details.</summary>
    public async Task<IReadOnlyList<DashboardSnapshot>> GetSnapshotsAsync(
        Guid? branchId,
        CancellationToken ct = default)
    {
        var query = db.DashboardSnapshots
            .AsNoTracking()
            .Include(snapshot => snapshot.TrendPoints)
            .Include(snapshot => snapshot.Activities)
            .Include(snapshot => snapshot.Alerts)
            .AsSplitQuery();

        if (branchId is { } id)
            query = query.Where(snapshot => snapshot.BranchId == id);

        return await query
            .OrderBy(snapshot => snapshot.BranchName)
            .ToListAsync(ct);
    }

    /// <summary>Returns branch options ordered by display name.</summary>
    public async Task<IReadOnlyList<DashboardBranchDto>> GetBranchesAsync(CancellationToken ct = default)
    {
        var branches = await db.DashboardSnapshots
            .AsNoTracking()
            .OrderBy(snapshot => snapshot.BranchName)
            .Select(snapshot => new
            {
                snapshot.BranchId,
                snapshot.BranchName,
                snapshot.BranchCity,
                snapshot.Capacity,
                snapshot.ActiveMembers,
                snapshot.OccupancyNow,
            })
            .ToListAsync(ct);

        return branches
            .Select(branch => new DashboardBranchDto(
                branch.BranchId,
                branch.BranchName,
                branch.BranchCity,
                branch.Capacity,
                branch.ActiveMembers,
                branch.OccupancyNow,
                branch.Capacity == 0
                    ? 0
                    : Math.Round(branch.OccupancyNow * 100d / branch.Capacity, 1, MidpointRounding.AwayFromZero)))
            .ToList();
    }
}
