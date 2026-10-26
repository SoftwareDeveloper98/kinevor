using Kinevor.Modules.CheckInsAccess.Domain;
using Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.DemoApi.Data;

/// <summary>Seeds deterministic branches, access members, visits and audit records.</summary>
public static class DemoDataSeeder
{
    /// <summary>Id of the primary self-service member, used by demos and documentation.</summary>
    public static readonly Guid PrimaryMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Id of the Shoreditch demo branch.</summary>
    public static readonly Guid ShoreditchBranchId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>Id of the Camden demo branch.</summary>
    public static readonly Guid CamdenBranchId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    /// <summary>Adds demo data when the database is empty.</summary>
    /// <param name="db">Module database context.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task SeedAsync(CheckInsDbContext db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (db.Branches.Any())
            return;

        var now = DateTime.UtcNow;
        var shoreditch = ClubBranch.Create("Kinevor Shoreditch", "SHR", "London", 50);
        var camden = ClubBranch.Create("Kinevor Camden", "CAM", "London", 35);
        ThrowIfFailed(shoreditch);
        ThrowIfFailed(camden);
        db.Branches.Add(shoreditch.Value);
        db.Branches.Add(camden.Value);
        SetId(db, shoreditch.Value, ShoreditchBranchId);
        SetId(db, camden.Value, CamdenBranchId);
        await db.SaveChangesAsync(ct);

        var seeds = new[]
        {
            new Seed(PrimaryMemberId, "Maya Thompson", "maya.thompson@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Unlimited", ShoreditchBranchId, false, 8, null),
            new Seed(Id(2), "Daniel Okafor", "daniel.okafor@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Performance", ShoreditchBranchId, false, 8, 95),
            new Seed(Id(3), "Sofia Rossi", "sofia.rossi@kinevor.demo", AccessStatus.Frozen, MembershipStatus.Frozen, "Studio", ShoreditchBranchId, false, 6, null),
            new Seed(Id(4), "Liam Chen", "liam.chen@kinevor.demo", AccessStatus.Expired, MembershipStatus.Expired, "Performance", ShoreditchBranchId, false, -1, null),
            new Seed(Id(5), "Amina Yusuf", "amina.yusuf@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "All Club", CamdenBranchId, true, 9, 60),
            new Seed(Id(6), "Noah Williams", "noah.williams@kinevor.demo", AccessStatus.Suspended, MembershipStatus.Active, "Studio", CamdenBranchId, false, 7, null),
            new Seed(Id(7), "Priya Shah", "priya.shah@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Unlimited", ShoreditchBranchId, false, 10, 35),
            new Seed(Id(8), "Olivia Bennett", "olivia.bennett@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Performance", ShoreditchBranchId, false, 8, 18),
            new Seed(Id(9), "Marcus Reed", "marcus.reed@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Off-Peak", ShoreditchBranchId, false, 9, 12),
            new Seed(Id(10), "Elena Petrova", "elena.petrova@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Unlimited", ShoreditchBranchId, false, 8, 140),
            new Seed(Id(11), "Tom Hughes", "tom.hughes@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Studio", ShoreditchBranchId, false, 8, 75),
            new Seed(Id(12), "Aisha Rahman", "aisha.rahman@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Performance", ShoreditchBranchId, false, 9, 48),
            new Seed(Id(13), "Ben Carter", "ben.carter@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Off-Peak", ShoreditchBranchId, false, 8, 210),
            new Seed(Id(14), "Grace Kim", "grace.kim@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "All Club", ShoreditchBranchId, true, 10, 158),
            new Seed(Id(15), "Hugo Martin", "hugo.martin@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Studio", ShoreditchBranchId, false, 8, 26),
            new Seed(Id(16), "Ravi Patel", "ravi.patel@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Performance", ShoreditchBranchId, false, 9, 8),
            new Seed(Id(17), "Chloe Adams", "chloe.adams@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Unlimited", ShoreditchBranchId, false, 8, 130),
            new Seed(Id(18), "Layla Ahmed", "layla.ahmed@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Off-Peak", CamdenBranchId, false, 8, 90),
            new Seed(Id(19), "Ethan Brooks", "ethan.brooks@kinevor.demo", AccessStatus.Active, MembershipStatus.Active, "Performance", CamdenBranchId, false, 9, 22),
        };

        var members = new Dictionary<Guid, AccessMember>();
        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var start = now.AddMonths(seed.MonthsValid < 0 ? -12 : -6);
            var end = now.AddMonths(seed.MonthsValid);
            var created = AccessMember.Create(
                MembershipId(index + 1),
                $"KZN-2026-{index + 1:D5}",
                seed.FullName,
                seed.Email,
                seed.Status,
                seed.MembershipStatus,
                seed.Plan,
                start,
                end,
                seed.HomeBranchId,
                seed.AllClubs);
            ThrowIfFailed(created);
            var member = created.Value;
            members[seed.Id] = member;
            db.AccessMembers.Add(member);
            SetId(db, member, seed.Id);
        }

        await db.SaveChangesAsync(ct);

        var branchNames = new Dictionary<Guid, string>
        {
            [ShoreditchBranchId] = shoreditch.Value.Name,
            [CamdenBranchId] = camden.Value.Name,
        };

        foreach (var seed in seeds.Where(seed => seed.OpenMinutesAgo is not null))
        {
            var member = members[seed.Id];
            var branchId = seed.HomeBranchId!.Value;
            var checkedInAt = now.AddMinutes(-seed.OpenMinutesAgo!.Value);
            var visit = RecordVisit(
                member, branchId, branchNames[branchId], checkedInAt, CheckInSource.FrontDesk);
            db.CheckIns.Add(visit);
            SetId(db, visit, CheckInId(int.Parse(member.MemberNumber[^5..])));
            db.AccessAudits.Add(AccessAudit.Record(
                member, branchId, AccessAuditAction.CheckIn, AccessOutcome.Allowed,
                AccessDecision.Allow(), CheckInSource.FrontDesk, "demo-seed", "System",
                checkedInAt, visit.Id));
        }

        var history = new[]
        {
            ClosedVisit(now, 2, 18, 5, 75),
            ClosedVisit(now, 5, 7, 10, 80),
            ClosedVisit(now, 9, 17, 30, 78),
            ClosedVisit(now, 14, 6, 55, 70),
            ClosedVisit(now, 21, 18, 20, 85),
            ClosedVisit(now, 28, 7, 0, 72),
            ClosedVisit(now, 35, 17, 45, 80),
            ClosedVisit(now, 42, 7, 15, 75),
        };

        var maya = members[PrimaryMemberId];
        var historyIndex = 0;
        foreach (var (checkedInAt, duration) in history)
        {
            historyIndex++;
            var visit = RecordVisit(
                maya, ShoreditchBranchId, shoreditch.Value.Name, checkedInAt, CheckInSource.MemberApp);
            var closed = visit.CheckOut(checkedInAt.AddMinutes(duration));
            ThrowIfFailed(closed);
            db.CheckIns.Add(visit);
            SetId(db, visit, CheckInId(9000 + historyIndex));
            db.AccessAudits.Add(AccessAudit.Record(
                maya, ShoreditchBranchId, AccessAuditAction.CheckIn, AccessOutcome.Allowed,
                AccessDecision.Allow(), CheckInSource.MemberApp, PrimaryMemberId.ToString(), "Member",
                checkedInAt, visit.Id));
            db.AccessAudits.Add(AccessAudit.Record(
                maya, ShoreditchBranchId, AccessAuditAction.CheckOut, AccessOutcome.Allowed,
                AccessDecision.Allow(), CheckInSource.MemberApp, PrimaryMemberId.ToString(), "Member",
                checkedInAt.AddMinutes(duration), visit.Id));
        }

        foreach (var (memberId, branchId, deniedAt) in new[]
        {
            (Id(3), ShoreditchBranchId, now.AddHours(-2)),
            (Id(4), ShoreditchBranchId, now.AddHours(-3)),
            (Id(6), CamdenBranchId, now.AddHours(-1)),
        })
        {
            var member = members[memberId];
            var decision = AccessPolicy.Evaluate(member, deniedAt, branchId);
            db.AccessAudits.Add(AccessAudit.Record(
                member, branchId, AccessAuditAction.AccessDenied, AccessOutcome.Denied,
                decision, CheckInSource.FrontDesk, "demo-seed", "System", deniedAt));
        }

        await db.SaveChangesAsync(ct);
    }

    private static CheckIn RecordVisit(
        AccessMember member,
        Guid branchId,
        string branchName,
        DateTime checkedInAt,
        CheckInSource source)
    {
        var recorded = CheckIn.Record(
            checkedInAt,
            member,
            branchId,
            branchName,
            source,
            null,
            source == CheckInSource.MemberApp ? member.Id.ToString() : "demo-seed",
            source == CheckInSource.MemberApp ? "Member" : "System",
            AccessDecision.Allow());
        ThrowIfFailed(recorded);
        return recorded.Value;
    }

    private static (DateTime CheckedInAt, int Duration) ClosedVisit(
        DateTime now, int daysAgo, int hour, int minute, int duration) =>
        (now.Date.AddDays(-daysAgo).AddHours(hour).AddMinutes(minute), duration);

    private static Guid Id(int sequence) =>
        Guid.Parse($"00000000-0000-0000-0000-{sequence:D12}");

    private static Guid MembershipId(int sequence) =>
        Guid.Parse($"a0000000-0000-0000-0000-{sequence:D12}");

    private static Guid CheckInId(int sequence) =>
        Guid.Parse($"c0000000-0000-0000-0000-{(uint)sequence:D12}");

    private static void SetId<TEntity>(CheckInsDbContext db, TEntity entity, Guid id)
        where TEntity : Entity =>
        db.Entry(entity).Property(nameof(Entity.Id)).CurrentValue = id;

    private static void ThrowIfFailed(Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);
    }

    private static void ThrowIfFailed<T>(Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Message);
    }

    private sealed record Seed(
        Guid Id,
        string FullName,
        string Email,
        AccessStatus Status,
        MembershipStatus MembershipStatus,
        string Plan,
        Guid? HomeBranchId,
        bool AllClubs,
        int MonthsValid,
        int? OpenMinutesAgo);
}
