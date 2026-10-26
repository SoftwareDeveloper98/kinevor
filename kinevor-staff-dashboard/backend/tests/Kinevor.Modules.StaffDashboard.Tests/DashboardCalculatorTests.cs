using Kinevor.Modules.StaffDashboard.Application;
using Kinevor.Modules.StaffDashboard.Domain;
using Xunit;

namespace Kinevor.Modules.StaffDashboard.Tests;

/// <summary>Aggregation and trend rules used by the dashboard.</summary>
public class DashboardCalculatorTests
{
    private static readonly DateTime Today = new(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Percentage change handles growth, decline and a zero baseline.</summary>
    [Fact]
    public void PercentChange_HandlesZeroBaseline()
    {
        Assert.Equal(12.5, DashboardCalculator.PercentChange(225, 200));
        Assert.Equal(-10, DashboardCalculator.PercentChange(90, 100));
        Assert.Equal(100, DashboardCalculator.PercentChange(10, 0));
        Assert.Equal(0, DashboardCalculator.PercentChange(0, 0));
    }

    /// <summary>The trend fills missing days and aggregates branches by date.</summary>
    [Fact]
    public void BuildTrend_FillsEveryDayAcrossBranches()
    {
        var first = CreateSnapshot("Main", 100, 60, 1000m);
        var second = CreateSnapshot("Downtown", 60, 40, 700m);

        Assert.True(first.AddTrendPoint(Today, 12, 5, 300m).IsSuccess);
        Assert.True(second.AddTrendPoint(Today, 8, 3, 200m).IsSuccess);
        Assert.True(first.AddTrendPoint(Today.AddDays(-1), 10, 4, 250m).IsSuccess);

        var trend = DashboardCalculator.BuildTrend([first, second], Today, 7);

        Assert.Equal(7, trend.Count);
        Assert.Equal(Today.AddDays(-6), trend[0].Date);
        Assert.Equal(0, trend[0].CheckIns);
        Assert.Equal(20, trend[^1].CheckIns);
        Assert.Equal(8, trend[^1].Bookings);
        Assert.Equal(500m, trend[^1].Revenue);
        Assert.Equal("Oct 20", trend[^1].Label);
    }

    /// <summary>The summary totals members, occupancy, revenue and comparisons.</summary>
    [Fact]
    public void BuildSummary_AggregatesBranches()
    {
        var first = CreateSnapshot("Main", 240, 140, 12000m);
        var second = CreateSnapshot("Downtown", 160, 80, 8000m);

        var summary = DashboardCalculator.BuildSummary([first, second]);

        Assert.Equal(240, summary.ActiveMembers);
        Assert.Equal(220, summary.OccupancyNow);
        Assert.Equal(400, summary.Capacity);
        Assert.Equal(55, summary.OccupancyPercent);
        Assert.Equal(20000m, summary.RevenueMonth);
        Assert.Equal(5.3, summary.Comparison.ActiveMembersPercent);
        Assert.Equal(6.1, summary.Comparison.RevenuePercent);
    }

    /// <summary>Alerts sort by severity before due date.</summary>
    [Fact]
    public void OrderAlerts_SortsBySeverityThenDueDate()
    {
        var snapshot = CreateSnapshot("Main", 100, 50, 1000m);

        Assert.True(snapshot.AddAlert(DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Info,
            "Info", "Detail", Today.AddDays(-4), "A").IsSuccess);
        Assert.True(snapshot.AddAlert(DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Critical,
            "Critical later", "Detail", Today.AddDays(1), "B").IsSuccess);
        Assert.True(snapshot.AddAlert(DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Critical,
            "Critical sooner", "Detail", Today, "C").IsSuccess);

        var ordered = DashboardCalculator.OrderAlerts(snapshot.Alerts).ToList();

        Assert.Equal("Critical sooner", ordered[0].Title);
        Assert.Equal("Critical later", ordered[1].Title);
        Assert.Equal("Info", ordered[2].Title);
    }

    private static DashboardSnapshot CreateSnapshot(string name, int capacity, int occupancy, decimal revenue)
    {
        var previousRevenue = Math.Round(revenue / 1.061m, 2);
        var result = DashboardSnapshot.Create(
            Guid.NewGuid(), name, "Manchester", Today, capacity, 120, 140, 114, occupancy, 40, 20, revenue, previousRevenue, 2, 300m);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
