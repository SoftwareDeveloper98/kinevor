using Kinevor.Modules.StaffDashboard.Domain;
using Xunit;

namespace Kinevor.Modules.StaffDashboard.Tests;

/// <summary>Invariants of the dashboard snapshot aggregate.</summary>
public class DashboardDomainTests
{
    private static readonly DateTime Today = new(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>A valid snapshot calculates occupancy from current occupancy and capacity.</summary>
    [Fact]
    public void Create_ComputesOccupancy()
    {
        var snapshot = Create("Main", 250, 151);

        Assert.Equal(60.4, snapshot.OccupancyPercent);
        Assert.Equal(Today, snapshot.CapturedOnUtc);
    }

    /// <summary>Occupancy above capacity is rejected.</summary>
    [Fact]
    public void Create_RejectsOccupancyAboveCapacity()
    {
        var result = DashboardSnapshot.Create(
            Guid.NewGuid(), "Main", "Manchester", Today, 100, 50, 60, 45, 101, 10, 4, 500m, 450m, 0, 0m);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.occupancyNow", result.Error.Code);
    }

    /// <summary>Trend points cannot be duplicated or dated after the snapshot.</summary>
    [Fact]
    public void AddTrendPoint_RejectsDuplicatesAndFutureDates()
    {
        var snapshot = Create("Main", 250, 120);

        Assert.True(snapshot.AddTrendPoint(Today, 40, 20, 500m).IsSuccess);
        Assert.True(snapshot.AddTrendPoint(Today, 40, 20, 500m).IsFailure);
        Assert.True(snapshot.AddTrendPoint(Today.AddDays(1), 40, 20, 500m).IsFailure);
    }

    /// <summary>Activity and alerts require their descriptive fields.</summary>
    [Fact]
    public void AddActivityAndAlert_ValidateRequiredFields()
    {
        var snapshot = Create("Main", 250, 120);

        Assert.True(snapshot.AddActivity(DashboardActivityKind.CheckIn, "Member checked in", "Floor", Today.AddHours(8)).IsSuccess);
        Assert.True(snapshot.AddActivity(DashboardActivityKind.CheckIn, "", "Floor", Today).IsFailure);
        Assert.True(snapshot.AddAlert(
            DashboardAlertType.OverdueInvoice,
            DashboardAlertSeverity.Critical,
            "Invoice overdue",
            "$85 outstanding",
            Today,
            "INV-1").IsSuccess);
        Assert.True(snapshot.AddAlert(
            DashboardAlertType.OverdueInvoice,
            DashboardAlertSeverity.Critical,
            "Invoice overdue",
            "$85 outstanding",
            Today,
            "").IsFailure);
    }

    private static Domain.DashboardSnapshot Create(string name, int capacity, int occupancy)
    {
        var result = Domain.DashboardSnapshot.Create(
            Guid.NewGuid(), name, "Manchester", Today, capacity, 120, 140, 110, occupancy, 40, 20, 5000m, 4700m, 2, 300m);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
