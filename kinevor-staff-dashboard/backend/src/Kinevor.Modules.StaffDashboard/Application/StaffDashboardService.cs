using Kinevor.Platform.Application.Exceptions;
using Kinevor.Modules.StaffDashboard.Domain;

namespace Kinevor.Modules.StaffDashboard.Application;

/// <summary>Assembles branch-scoped dashboard and alert payloads.</summary>
public sealed class StaffDashboardService(IDashboardRepository repository, TimeProvider clock)
{
    private const int DashboardAlertLimit = 6;

    /// <summary>Returns KPIs, trend, activity and alerts for the selected branch and range.</summary>
    public async Task<StaffDashboardDto> GetAsync(
        Guid? branchId,
        int days,
        CancellationToken ct = default)
    {
        ValidateDays(days);

        var branches = await repository.GetBranchesAsync(ct);
        EnsureBranchExists(branches, branchId);

        var snapshots = await repository.GetSnapshotsAsync(branchId, ct);
        if (snapshots.Count == 0)
            throw new NotFoundException("Dashboard snapshot", branchId?.ToString() ?? "current");

        var generatedAt = clock.GetUtcNow().UtcDateTime;
        var (branchName, branchCity) = ResolveBranchLabel(snapshots, branchId, branches.Count);
        var activity = snapshots
            .SelectMany(snapshot => snapshot.Activities.Select(item => DashboardMapper.ToActivity(snapshot, item)))
            .OrderByDescending(item => item.OccurredAt)
            .Take(12)
            .ToList();
        var alerts = OrderAlerts(snapshots)
            .Take(DashboardAlertLimit)
            .ToList();

        return new StaffDashboardDto(
            generatedAt,
            branchId,
            branchName,
            branchCity,
            days,
            DashboardCalculator.BuildSummary(snapshots),
            DashboardCalculator.BuildTrend(snapshots, generatedAt.Date, days),
            activity,
            alerts,
            branches);
    }

    /// <summary>Returns the branches available in the dashboard selector.</summary>
    public Task<IReadOnlyList<DashboardBranchDto>> GetBranchesAsync(CancellationToken ct = default) =>
        repository.GetBranchesAsync(ct);

    /// <summary>Returns the complete alert inbox for the selected branch.</summary>
    public async Task<DashboardAlertsDto> GetAlertsAsync(
        Guid? branchId,
        CancellationToken ct = default)
    {
        var branches = await repository.GetBranchesAsync(ct);
        EnsureBranchExists(branches, branchId);

        var snapshots = await repository.GetSnapshotsAsync(branchId, ct);
        if (snapshots.Count == 0)
            throw new NotFoundException("Dashboard snapshot", branchId?.ToString() ?? "current");

        var (branchName, branchCity) = ResolveBranchLabel(snapshots, branchId, branches.Count);
        return new DashboardAlertsDto(
            clock.GetUtcNow().UtcDateTime,
            branchId,
            branchName,
            branchCity,
            OrderAlerts(snapshots),
            branches);
    }

    private static IReadOnlyList<DashboardAlertDto> OrderAlerts(
        IReadOnlyCollection<DashboardSnapshot> snapshots) =>
        DashboardCalculator.OrderAlerts(snapshots.SelectMany(snapshot => snapshot.Alerts))
            .Select(alert => DashboardMapper.ToAlert(
                snapshots.First(snapshot => snapshot.Alerts.Contains(alert)),
                alert))
            .ToList();

    private static (string Name, string City) ResolveBranchLabel(
        IReadOnlyCollection<DashboardSnapshot> snapshots,
        Guid? branchId,
        int branchCount)
    {
        if (branchId is null)
            return ("All branches", $"{branchCount} locations");

        var snapshot = snapshots.First();
        return (snapshot.BranchName, snapshot.BranchCity);
    }

    private static void EnsureBranchExists(IReadOnlyCollection<DashboardBranchDto> branches, Guid? branchId)
    {
        if (branchId is { } id && branches.All(branch => branch.Id != id))
            throw new NotFoundException("Branch", id);
    }

    private static void ValidateDays(int days)
    {
        if (days is not (7 or 30))
            throw new DomainException("The dashboard range must be 7 or 30 days.");
    }
}
