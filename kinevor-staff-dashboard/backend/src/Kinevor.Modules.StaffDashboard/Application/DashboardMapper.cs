using Kinevor.Modules.StaffDashboard.Domain;

namespace Kinevor.Modules.StaffDashboard.Application;

internal static class DashboardMapper
{
    internal static DashboardAlertDto ToAlert(DashboardSnapshot snapshot, DashboardAlert alert) => new(
        alert.Id,
        snapshot.BranchId,
        snapshot.BranchName,
        snapshot.BranchCity,
        alert.Type.ToString(),
        alert.Severity.ToString(),
        alert.Title,
        alert.Detail,
        alert.DueAtUtc,
        alert.Reference);

    internal static DashboardActivityDto ToActivity(DashboardSnapshot snapshot, DashboardActivity activity) => new(
        activity.Id,
        snapshot.BranchId,
        snapshot.BranchName,
        snapshot.BranchCity,
        activity.Kind.ToString(),
        activity.Title,
        activity.Detail,
        activity.OccurredAtUtc);
}
