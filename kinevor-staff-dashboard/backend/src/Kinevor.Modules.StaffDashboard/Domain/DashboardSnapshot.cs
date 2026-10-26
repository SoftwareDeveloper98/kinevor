using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.StaffDashboard.Domain;

/// <summary>
/// Read-model snapshot for one branch on one operational day. It owns the
/// branch KPIs, daily trend points, recent activity and open alerts, and never
/// references another module's database.
/// </summary>
public sealed class DashboardSnapshot : Entity
{
    private readonly List<DashboardTrendPoint> _trendPoints = [];
    private readonly List<DashboardActivity> _activities = [];
    private readonly List<DashboardAlert> _alerts = [];

    private DashboardSnapshot()
    {
    }

    private DashboardSnapshot(
        Guid branchId,
        string branchName,
        string branchCity,
        DateTime capturedOnUtc,
        int capacity,
        int activeMembers,
        int totalMembers,
        int activeMembersPreviousMonth,
        int occupancyNow,
        int checkInsToday,
        int bookingsToday,
        decimal revenueMonth,
        decimal revenuePreviousMonth,
        int outstandingInvoices,
        decimal outstandingAmount)
    {
        BranchId = branchId;
        BranchName = branchName;
        BranchCity = branchCity;
        CapturedOnUtc = capturedOnUtc;
        Capacity = capacity;
        ActiveMembers = activeMembers;
        TotalMembers = totalMembers;
        ActiveMembersPreviousMonth = activeMembersPreviousMonth;
        OccupancyNow = occupancyNow;
        CheckInsToday = checkInsToday;
        BookingsToday = bookingsToday;
        RevenueMonth = revenueMonth;
        RevenuePreviousMonth = revenuePreviousMonth;
        OutstandingInvoices = outstandingInvoices;
        OutstandingAmount = outstandingAmount;
    }

    /// <summary>Branch represented by the snapshot.</summary>
    public Guid BranchId { get; private set; }

    /// <summary>Display name of the branch.</summary>
    public string BranchName { get; private set; } = string.Empty;

    /// <summary>City shown beside the branch name.</summary>
    public string BranchCity { get; private set; } = string.Empty;

    /// <summary>UTC day the snapshot was captured.</summary>
    public DateTime CapturedOnUtc { get; private set; }

    /// <summary>Maximum simultaneous occupancy for the branch.</summary>
    public int Capacity { get; private set; }

    /// <summary>Members with an active membership.</summary>
    public int ActiveMembers { get; private set; }

    /// <summary>Members of any lifecycle status.</summary>
    public int TotalMembers { get; private set; }

    /// <summary>Active members one month before the snapshot.</summary>
    public int ActiveMembersPreviousMonth { get; private set; }

    /// <summary>People currently inside the branch.</summary>
    public int OccupancyNow { get; private set; }

    /// <summary>Check-ins recorded since midnight.</summary>
    public int CheckInsToday { get; private set; }

    /// <summary>Class bookings recorded for today.</summary>
    public int BookingsToday { get; private set; }

    /// <summary>Revenue collected in the current calendar month.</summary>
    public decimal RevenueMonth { get; private set; }

    /// <summary>Revenue collected in the previous calendar month.</summary>
    public decimal RevenuePreviousMonth { get; private set; }

    /// <summary>Number of invoices currently overdue.</summary>
    public int OutstandingInvoices { get; private set; }

    /// <summary>Total value of overdue invoices.</summary>
    public decimal OutstandingAmount { get; private set; }

    /// <summary>Current occupancy as a percentage of <see cref="Capacity"/>.</summary>
    public double OccupancyPercent =>
        Capacity == 0 ? 0 : Math.Round(OccupancyNow * 100d / Capacity, 1, MidpointRounding.AwayFromZero);

    /// <summary>Daily operational trend owned by the snapshot.</summary>
    public IReadOnlyCollection<DashboardTrendPoint> TrendPoints => _trendPoints;

    /// <summary>Recent member and operational activity.</summary>
    public IReadOnlyCollection<DashboardActivity> Activities => _activities;

    /// <summary>Open operational alerts and tasks.</summary>
    public IReadOnlyCollection<DashboardAlert> Alerts => _alerts;

    /// <summary>Creates a validated snapshot.</summary>
    public static Result<DashboardSnapshot> Create(
        Guid branchId,
        string branchName,
        string branchCity,
        DateTime capturedOnUtc,
        int capacity,
        int activeMembers,
        int totalMembers,
        int activeMembersPreviousMonth,
        int occupancyNow,
        int checkInsToday,
        int bookingsToday,
        decimal revenueMonth,
        decimal revenuePreviousMonth,
        int outstandingInvoices,
        decimal outstandingAmount)
    {
        if (branchId == Guid.Empty)
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(branchId), "A branch id is required."));

        if (string.IsNullOrWhiteSpace(branchName))
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(branchName), "A branch name is required."));

        if (string.IsNullOrWhiteSpace(branchCity))
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(branchCity), "A branch city is required."));

        if (capturedOnUtc == default)
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(capturedOnUtc), "A capture date is required."));

        if (capacity <= 0)
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(capacity), "Capacity must be positive."));

        if (activeMembers < 0 || totalMembers < 0 || activeMembersPreviousMonth < 0)
            return Result.Failure<DashboardSnapshot>(Error.Validation("members", "Member counts cannot be negative."));

        if (activeMembers > totalMembers)
            return Result.Failure<DashboardSnapshot>(Error.Validation("members", "Active members cannot exceed total members."));

        if (occupancyNow < 0 || occupancyNow > capacity)
            return Result.Failure<DashboardSnapshot>(Error.Validation(nameof(occupancyNow), "Current occupancy must be between zero and capacity."));

        if (checkInsToday < 0 || bookingsToday < 0)
            return Result.Failure<DashboardSnapshot>(Error.Validation("activity", "Check-ins and bookings cannot be negative."));

        if (revenueMonth < 0 || revenuePreviousMonth < 0)
            return Result.Failure<DashboardSnapshot>(Error.Validation("revenue", "Revenue cannot be negative."));

        if (outstandingInvoices < 0 || outstandingAmount < 0)
            return Result.Failure<DashboardSnapshot>(Error.Validation("invoices", "Outstanding invoices cannot be negative."));

        return Result.Success(new DashboardSnapshot(
            branchId,
            branchName.Trim(),
            branchCity.Trim(),
            DateTime.SpecifyKind(capturedOnUtc.Date, DateTimeKind.Utc),
            capacity,
            activeMembers,
            totalMembers,
            activeMembersPreviousMonth,
            occupancyNow,
            checkInsToday,
            bookingsToday,
            revenueMonth,
            revenuePreviousMonth,
            outstandingInvoices,
            outstandingAmount));
    }

    /// <summary>Adds a daily trend point, rejecting duplicates and future dates.</summary>
    public Result AddTrendPoint(DateTime dateUtc, int checkIns, int bookings, decimal revenue)
    {
        var date = DateTime.SpecifyKind(dateUtc.Date, DateTimeKind.Utc);
        if (date > CapturedOnUtc)
            return Result.Failure(Error.Validation(nameof(dateUtc), "A trend point cannot be in the future."));

        if (checkIns < 0 || bookings < 0 || revenue < 0)
            return Result.Failure(Error.Validation("trend", "Trend values cannot be negative."));

        if (_trendPoints.Any(point => point.DateUtc == date))
            return Result.Failure(Error.Conflict($"A trend point for {date:yyyy-MM-dd} already exists."));

        _trendPoints.Add(new DashboardTrendPoint(Id, date, checkIns, bookings, revenue));
        return Result.Success();
    }

    /// <summary>Adds a recent activity entry.</summary>
    public Result AddActivity(
        DashboardActivityKind kind,
        string title,
        string detail,
        DateTime occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation(nameof(title), "An activity title is required."));

        if (string.IsNullOrWhiteSpace(detail))
            return Result.Failure(Error.Validation(nameof(detail), "Activity detail is required."));

        if (occurredAtUtc == default || occurredAtUtc > CapturedOnUtc.AddDays(1))
            return Result.Failure(Error.Validation(nameof(occurredAtUtc), "Activity must fall on or before the snapshot day."));

        _activities.Add(new DashboardActivity(Id, kind, title.Trim(), detail.Trim(), occurredAtUtc));
        return Result.Success();
    }

    /// <summary>Adds an operational alert or task.</summary>
    public Result AddAlert(
        DashboardAlertType type,
        DashboardAlertSeverity severity,
        string title,
        string detail,
        DateTime? dueAtUtc,
        string reference)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation(nameof(title), "An alert title is required."));

        if (string.IsNullOrWhiteSpace(detail))
            return Result.Failure(Error.Validation(nameof(detail), "Alert detail is required."));

        if (string.IsNullOrWhiteSpace(reference))
            return Result.Failure(Error.Validation(nameof(reference), "An alert reference is required."));

        if (dueAtUtc == default)
            return Result.Failure(Error.Validation(nameof(dueAtUtc), "A due date cannot be the default value."));

        _alerts.Add(new DashboardAlert(
            Id,
            type,
            severity,
            title.Trim(),
            detail.Trim(),
            dueAtUtc,
            reference.Trim()));
        return Result.Success();
    }
}
