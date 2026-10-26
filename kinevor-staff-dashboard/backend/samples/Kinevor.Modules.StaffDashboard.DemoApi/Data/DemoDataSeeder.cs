using Kinevor.Modules.StaffDashboard.Domain;
using Kinevor.Modules.StaffDashboard.EntityFrameworkCore;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.StaffDashboard.DemoApi.Data;

/// <summary>Seeds three deterministic branches with KPIs, trends, activity and alerts.</summary>
public static class DemoDataSeeder
{
    /// <summary>Id of the main branch used by demos and documentation.</summary>
    public static readonly Guid MainBranchId = Guid.Parse("a1111111-1111-4111-8111-111111111111");

    /// <summary>Id of the downtown branch used by demos and documentation.</summary>
    public static readonly Guid DowntownBranchId = Guid.Parse("b2222222-2222-4222-8222-222222222222");

    /// <summary>Id of the westside branch used by demos and documentation.</summary>
    public static readonly Guid WestsideBranchId = Guid.Parse("c3333333-3333-4333-8333-333333333333");

    /// <summary>Adds demo data when the dashboard database is empty.</summary>
    public static async Task SeedAsync(DashboardDbContext db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (db.DashboardSnapshots.Any())
            return;

        var today = DateTime.UtcNow.Date;
        var seeds = new[]
        {
            new BranchSeed(
                MainBranchId, "Kinevor Gym - Main", "Manchester", 250,
                184, 221, 172, 151, 63, 41, 14850m, 13900m, 7, 1240m, 505m),
            new BranchSeed(
                DowntownBranchId, "Kinevor Gym - Downtown", "Manchester", 180,
                149, 176, 141, 92, 44, 29, 11240m, 10750m, 5, 860m, 382m),
            new BranchSeed(
                WestsideBranchId, "Kinevor Gym - Westside", "Leeds", 150,
                96, 124, 91, 58, 21, 14, 7480m, 7200m, 3, 420m, 254m),
        };

        foreach (var seed in seeds)
        {
            var created = DashboardSnapshot.Create(
                seed.Id,
                seed.Name,
                seed.City,
                today,
                seed.Capacity,
                seed.ActiveMembers,
                seed.TotalMembers,
                seed.PreviousActiveMembers,
                seed.Occupancy,
                seed.CheckInsToday,
                seed.BookingsToday,
                seed.RevenueMonth,
                seed.RevenuePreviousMonth,
                seed.OutstandingInvoices,
                seed.OutstandingAmount);

            Ensure(created);

            var snapshot = created.Value;
            AddTrend(snapshot, today, seed);
            AddActivity(snapshot, today);
            AddAlerts(snapshot, today);
            db.DashboardSnapshots.Add(snapshot);
        }

        await db.SaveChangesAsync(ct);
    }

    private static void AddTrend(DashboardSnapshot snapshot, DateTime today, BranchSeed seed)
    {
        for (var offset = 29; offset >= 0; offset--)
        {
            var date = today.AddDays(-offset);
            var wave = ((offset * 17 + seed.Name.Length * 3) % 11) - 5;
            var checkIns = offset == 0 ? seed.CheckInsToday : Math.Max(8, seed.CheckInsToday + wave);
            var bookings = offset == 0
                ? seed.BookingsToday
                : Math.Max(3, (int)Math.Round(checkIns * 0.62) + offset % 3);
            var revenue = offset == 0
                ? Math.Round(seed.RevenueMonth / 30m, 2)
                : Math.Round(seed.DailyRevenueBase + offset % 7 * 35m, 2);

            Ensure(snapshot.AddTrendPoint(date, checkIns, bookings, revenue));
        }
    }

    private static void AddActivity(DashboardSnapshot snapshot, DateTime today)
    {
        if (snapshot.BranchId == MainBranchId)
        {
            Add(snapshot, DashboardActivityKind.CheckIn, "Maya Thompson checked in",
                "Strength floor - 08:42", today.AddHours(8).AddMinutes(42));
            Add(snapshot, DashboardActivityKind.ClassBooking, "Daniel Okafor booked Reformer Pilates",
                "Today at 18:30 - Studio 2", today.AddHours(9).AddMinutes(16));
            Add(snapshot, DashboardActivityKind.Payment, "Payment received from Sofia Rossi",
                "$85.00 monthly membership", today.AddHours(9).AddMinutes(48));
            Add(snapshot, DashboardActivityKind.MemberJoined, "Amina Yusuf joined as an annual member",
                "Premium annual - Main", today.AddHours(10).AddMinutes(7));
            Add(snapshot, DashboardActivityKind.Notification, "Class reminder delivered to 24 members",
                "Reformer Pilates - 100% delivered", today.AddHours(10).AddMinutes(24));
            Add(snapshot, DashboardActivityKind.CheckIn, "Liam Chen checked in",
                "Personal training - 11:00", today.AddHours(10).AddMinutes(41));
            return;
        }

        if (snapshot.BranchId == DowntownBranchId)
        {
            Add(snapshot, DashboardActivityKind.CheckIn, "Priya Sharma checked in",
                "Olympic lifting - 07:55", today.AddHours(7).AddMinutes(55));
            Add(snapshot, DashboardActivityKind.Membership, "Noah Williams renewed his membership",
                "12-month commitment - Downtown", today.AddHours(8).AddMinutes(31));
            Add(snapshot, DashboardActivityKind.ClassBooking, "Olivia Brown booked Spin 45",
                "Today at 17:15 - Cycle studio", today.AddHours(9).AddMinutes(12));
            Add(snapshot, DashboardActivityKind.Payment, "Payment received from Arjun Patel",
                "$120.00 personal training pack", today.AddHours(9).AddMinutes(57));
            return;
        }

        Add(snapshot, DashboardActivityKind.CheckIn, "Elena Garcia checked in",
            "Mat Pilates - 08:10", today.AddHours(8).AddMinutes(10));
        Add(snapshot, DashboardActivityKind.ClassBooking, "Tom Becker booked Mobility Flow",
            "Today at 19:00 - Studio 1", today.AddHours(8).AddMinutes(46));
        Add(snapshot, DashboardActivityKind.MemberJoined, "Grace Kim joined as a student member",
            "Student monthly - Westside", today.AddHours(9).AddMinutes(20));
        Add(snapshot, DashboardActivityKind.Notification, "New class schedule sent to 82 members",
            "November timetable - 98% delivered", today.AddHours(9).AddMinutes(44));
    }

    private static void AddAlerts(DashboardSnapshot snapshot, DateTime today)
    {
        if (snapshot.BranchId == MainBranchId)
        {
            AddAlert(snapshot, DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Critical,
                "7 overdue invoices need follow-up", "$1,240 outstanding across 7 members",
                today.AddDays(-1), "Billing - Main");
            AddAlert(snapshot, DashboardAlertType.ExpiringMembership, DashboardAlertSeverity.Warning,
                "3 memberships expire this week", "Maya Thompson, Liam Chen and 1 other",
                today.AddDays(2), "Memberships - Main");
            AddAlert(snapshot, DashboardAlertType.LowClassFill, DashboardAlertSeverity.Info,
                "Reformer Pilates is 40% full", "8 of 20 seats booked for tonight",
                today.AddHours(18).AddMinutes(30), "CLS-2418");
            AddAlert(snapshot, DashboardAlertType.FailedNotification, DashboardAlertSeverity.Warning,
                "2 payment reminders failed", "Two members have invalid push tokens",
                today.AddHours(9), "NOT-0881");
            return;
        }

        if (snapshot.BranchId == DowntownBranchId)
        {
            AddAlert(snapshot, DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Critical,
                "5 overdue invoices need follow-up", "$860 outstanding across 5 members",
                today.AddDays(-2), "Billing - Downtown");
            AddAlert(snapshot, DashboardAlertType.ExpiringMembership, DashboardAlertSeverity.Warning,
                "4 memberships expire this week", "Renewal calls assigned to the front desk",
                today.AddDays(3), "Memberships - Downtown");
            AddAlert(snapshot, DashboardAlertType.LowClassFill, DashboardAlertSeverity.Info,
                "Spin 45 is 55% full", "11 of 20 bikes booked for 17:15",
                today.AddHours(17).AddMinutes(15), "CLS-2431");
            AddAlert(snapshot, DashboardAlertType.FailedNotification, DashboardAlertSeverity.Info,
                "1 class reminder was not delivered", "Member has opted out of push notifications",
                today.AddHours(8), "NOT-0884");
            return;
        }

        AddAlert(snapshot, DashboardAlertType.OverdueInvoice, DashboardAlertSeverity.Warning,
            "3 overdue invoices need follow-up", "$420 outstanding across 3 members",
            today.AddDays(-1), "Billing - Westside");
        AddAlert(snapshot, DashboardAlertType.ExpiringMembership, DashboardAlertSeverity.Info,
            "2 memberships expire this week", "Both members are on monthly plans",
            today.AddDays(4), "Memberships - Westside");
        AddAlert(snapshot, DashboardAlertType.LowClassFill, DashboardAlertSeverity.Info,
            "Mobility Flow is 38% full", "6 of 16 mats booked for 19:00",
            today.AddHours(19), "CLS-2444");
        AddAlert(snapshot, DashboardAlertType.FailedNotification, DashboardAlertSeverity.Warning,
            "1 receipt was not delivered", "Retry the e-mail from the billing screen",
            today.AddHours(10), "NOT-0889");
    }

    private static void Add(
        DashboardSnapshot snapshot,
        DashboardActivityKind kind,
        string title,
        string detail,
        DateTime occurredAt)
    {
        Ensure(snapshot.AddActivity(kind, title, detail, occurredAt));
    }

    private static void AddAlert(
        DashboardSnapshot snapshot,
        DashboardAlertType type,
        DashboardAlertSeverity severity,
        string title,
        string detail,
        DateTime dueAt,
        string reference)
    {
        Ensure(snapshot.AddAlert(type, severity, title, detail, dueAt, reference));
    }

    private static void Ensure(Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);
    }

    private sealed record BranchSeed(
        Guid Id,
        string Name,
        string City,
        int Capacity,
        int ActiveMembers,
        int TotalMembers,
        int PreviousActiveMembers,
        int Occupancy,
        int CheckInsToday,
        int BookingsToday,
        decimal RevenueMonth,
        decimal RevenuePreviousMonth,
        int OutstandingInvoices,
        decimal OutstandingAmount,
        decimal DailyRevenueBase);
}
