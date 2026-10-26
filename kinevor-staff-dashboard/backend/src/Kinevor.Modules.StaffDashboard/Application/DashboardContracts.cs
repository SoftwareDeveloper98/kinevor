namespace Kinevor.Modules.StaffDashboard.Application;

/// <summary>Branch available in the dashboard branch selector and pulse list.</summary>
public record DashboardBranchDto(
    Guid Id,
    string Name,
    string City,
    int Capacity,
    int ActiveMembers,
    int OccupancyNow,
    double OccupancyPercent);

/// <summary>Period-over-period movement for headline KPIs.</summary>
public record DashboardComparisonDto(double ActiveMembersPercent, double RevenuePercent);

/// <summary>Aggregated KPI block for one branch or for all branches.</summary>
public record DashboardSummaryDto(
    int TotalMembers,
    int ActiveMembers,
    int OccupancyNow,
    double OccupancyPercent,
    int Capacity,
    int CheckInsToday,
    int BookingsToday,
    decimal RevenueMonth,
    int OutstandingInvoices,
    decimal OutstandingAmount,
    DashboardComparisonDto Comparison);

/// <summary>One day in the selected operational trend.</summary>
public record DashboardTrendPointDto(
    DateTime Date,
    string Label,
    int CheckIns,
    int Bookings,
    decimal Revenue);

/// <summary>Recent event shown in the staff activity feed.</summary>
public record DashboardActivityDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string BranchCity,
    string Kind,
    string Title,
    string Detail,
    DateTime OccurredAt);

/// <summary>Operational alert or task requiring staff attention.</summary>
public record DashboardAlertDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string BranchCity,
    string Type,
    string Severity,
    string Title,
    string Detail,
    DateTime? DueAt,
    string Reference);

/// <summary>Complete dashboard payload for the selected branch and range.</summary>
public record StaffDashboardDto(
    DateTime GeneratedAt,
    Guid? BranchId,
    string BranchName,
    string BranchCity,
    int Days,
    DashboardSummaryDto Summary,
    IReadOnlyList<DashboardTrendPointDto> Trend,
    IReadOnlyList<DashboardActivityDto> Activity,
    IReadOnlyList<DashboardAlertDto> Alerts,
    IReadOnlyList<DashboardBranchDto> Branches);

/// <summary>Alert inbox payload with the branch selector context.</summary>
public record DashboardAlertsDto(
    DateTime GeneratedAt,
    Guid? BranchId,
    string BranchName,
    string BranchCity,
    IReadOnlyList<DashboardAlertDto> Alerts,
    IReadOnlyList<DashboardBranchDto> Branches);
