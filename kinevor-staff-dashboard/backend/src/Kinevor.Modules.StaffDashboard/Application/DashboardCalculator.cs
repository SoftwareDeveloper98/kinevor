using System.Globalization;
using Kinevor.Modules.StaffDashboard.Domain;

namespace Kinevor.Modules.StaffDashboard.Application;

/// <summary>Pure aggregation rules shared by the dashboard service and tests.</summary>
public static class DashboardCalculator
{
    /// <summary>Calculates the percentage change between two periods.</summary>
    public static double PercentChange(decimal current, decimal previous)
    {
        if (previous == 0)
            return current == 0 ? 0 : 100;

        return (double)Math.Round((current - previous) * 100 / previous, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>Builds a continuous daily trend for the last <paramref name="days"/> days.</summary>
    public static IReadOnlyList<DashboardTrendPointDto> BuildTrend(
        IReadOnlyCollection<DashboardSnapshot> snapshots,
        DateTime todayUtc,
        int days)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "The trend range must be positive.");

        var points = snapshots.SelectMany(snapshot => snapshot.TrendPoints).ToList();
        var start = todayUtc.Date.AddDays(1 - days);
        var trend = new List<DashboardTrendPointDto>(days);

        for (var day = start; day <= todayUtc.Date; day = day.AddDays(1))
        {
            var daily = points.Where(point => point.DateUtc.Date == day).ToList();
            trend.Add(new DashboardTrendPointDto(
                day,
                day.ToString("MMM d", CultureInfo.InvariantCulture),
                daily.Sum(point => point.CheckIns),
                daily.Sum(point => point.Bookings),
                daily.Sum(point => point.Revenue)));
        }

        return trend;
    }

    /// <summary>Aggregates headline KPIs across the supplied snapshots.</summary>
    public static DashboardSummaryDto BuildSummary(IReadOnlyCollection<DashboardSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        if (snapshots.Count == 0)
            throw new ArgumentException("At least one snapshot is required.", nameof(snapshots));

        var activeMembers = snapshots.Sum(snapshot => snapshot.ActiveMembers);
        var previousActive = snapshots.Sum(snapshot => snapshot.ActiveMembersPreviousMonth);
        var revenue = snapshots.Sum(snapshot => snapshot.RevenueMonth);
        var previousRevenue = snapshots.Sum(snapshot => snapshot.RevenuePreviousMonth);
        var occupancy = snapshots.Sum(snapshot => snapshot.OccupancyNow);
        var capacity = snapshots.Sum(snapshot => snapshot.Capacity);

        return new DashboardSummaryDto(
            snapshots.Sum(snapshot => snapshot.TotalMembers),
            activeMembers,
            occupancy,
            capacity == 0
                ? 0
                : Math.Round(occupancy * 100d / capacity, 1, MidpointRounding.AwayFromZero),
            capacity,
            snapshots.Sum(snapshot => snapshot.CheckInsToday),
            snapshots.Sum(snapshot => snapshot.BookingsToday),
            revenue,
            snapshots.Sum(snapshot => snapshot.OutstandingInvoices),
            snapshots.Sum(snapshot => snapshot.OutstandingAmount),
            new DashboardComparisonDto(
                PercentChange(activeMembers, previousActive),
                PercentChange(revenue, previousRevenue)));
    }

    /// <summary>Orders alerts by severity and due date.</summary>
    public static IEnumerable<DashboardAlert> OrderAlerts(IEnumerable<DashboardAlert> alerts) =>
        alerts
            .OrderBy(alert => alert.Severity)
            .ThenBy(alert => alert.DueAtUtc ?? DateTime.MaxValue)
            .ThenByDescending(alert => alert.CreatedAt);
}
