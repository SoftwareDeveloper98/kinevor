using Kinevor.Modules.CheckInsAccess.Domain;
using Xunit;

namespace Kinevor.Modules.CheckInsAccess.Tests;

/// <summary>Domain invariants for access projections, eligibility, branches and visits.</summary>
public class CheckInsDomainTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid HomeBranchId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherBranchId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MembershipId = Guid.Parse("a0000000-0000-0000-0000-000000000001");

    /// <summary>An active member with a current membership is allowed at the home branch.</summary>
    [Fact]
    public void Evaluate_AllowsActiveMemberAtHomeBranch()
    {
        var member = CreateMember();

        var decision = AccessPolicy.Evaluate(member, Now, HomeBranchId);

        Assert.True(decision.IsAllowed);
        Assert.Equal("Access.Allowed", decision.Code);
    }

    /// <summary>Member lifecycle states produce distinct deny codes.</summary>
    [Theory]
    [InlineData(AccessStatus.Frozen, "Access.MemberFrozen")]
    [InlineData(AccessStatus.Suspended, "Access.MemberSuspended")]
    [InlineData(AccessStatus.Expired, "Access.MemberExpired")]
    public void Evaluate_DeniesInactiveMember(AccessStatus status, string code)
    {
        var member = CreateMember(status: status);

        var decision = AccessPolicy.Evaluate(member, Now, HomeBranchId);

        Assert.False(decision.IsAllowed);
        Assert.Equal(code, decision.Code);
    }

    /// <summary>Membership lifecycle states produce distinct deny codes.</summary>
    [Theory]
    [InlineData(MembershipStatus.Frozen, "Access.MembershipFrozen")]
    [InlineData(MembershipStatus.Cancelled, "Access.MembershipCancelled")]
    [InlineData(MembershipStatus.Expired, "Access.MembershipExpired")]
    public void Evaluate_DeniesInactiveMembership(MembershipStatus status, string code)
    {
        var member = CreateMember(membershipStatus: status);

        var decision = AccessPolicy.Evaluate(member, Now, HomeBranchId);

        Assert.False(decision.IsAllowed);
        Assert.Equal(code, decision.Code);
    }

    /// <summary>Membership windows are inclusive of both start and end dates.</summary>
    [Fact]
    public void Evaluate_EnforcesMembershipWindow()
    {
        var notStarted = CreateMember(startsAt: Now.Date.AddDays(2), endsAt: Now.Date.AddDays(30));
        var onLastDay = CreateMember(startsAt: Now.Date.AddDays(-30), endsAt: Now.Date);
        var ended = CreateMember(startsAt: Now.Date.AddDays(-30), endsAt: Now.Date.AddDays(-1));

        Assert.Equal("Access.MembershipNotStarted",
            AccessPolicy.Evaluate(notStarted, Now, HomeBranchId).Code);
        Assert.True(AccessPolicy.Evaluate(onLastDay, Now, HomeBranchId).IsAllowed);
        Assert.Equal("Access.MembershipExpired",
            AccessPolicy.Evaluate(ended, Now, HomeBranchId).Code);
    }

    /// <summary>Branch-restricted members are denied at another branch but allowed with no branch filter.</summary>
    [Fact]
    public void Evaluate_EnforcesBranchScope()
    {
        var member = CreateMember();

        var denied = AccessPolicy.Evaluate(member, Now, OtherBranchId);
        var unfiltered = AccessPolicy.Evaluate(member, Now, null);

        Assert.False(denied.IsAllowed);
        Assert.Equal("Access.BranchRestricted", denied.Code);
        Assert.True(unfiltered.IsAllowed);
    }

    /// <summary>All-club members may use any branch.</summary>
    [Fact]
    public void Evaluate_AllowsAllClubMembershipAnywhere()
    {
        var member = CreateMember(allClubs: true);

        var decision = AccessPolicy.Evaluate(member, Now, OtherBranchId);

        Assert.True(decision.IsAllowed);
    }

    /// <summary>Projections are normalized and validated.</summary>
    [Fact]
    public void AccessMember_Create_NormalizesAndValidates()
    {
        var member = AccessMember.Create(
            MembershipId,
            "  KZN-2026-00001  ",
            "  Maya Thompson  ",
            "  MAYA@Kinevor.Demo  ",
            AccessStatus.Active,
            MembershipStatus.Active,
            "  Unlimited  ",
            Now.Date.AddDays(-5),
            Now.Date.AddDays(30),
            HomeBranchId,
            false);

        Assert.True(member.IsSuccess);
        Assert.Equal("KZN-2026-00001", member.Value.MemberNumber);
        Assert.Equal("Maya Thompson", member.Value.FullName);
        Assert.Equal("maya@kinevor.demo", member.Value.Email);
        Assert.Equal("Unlimited", member.Value.MembershipPlan);
        Assert.Equal(Now.Date.AddDays(-5), member.Value.MembershipStartsAt);
    }

    /// <summary>Invalid access projections are rejected before persistence.</summary>
    [Fact]
    public void AccessMember_Create_RejectsInvalidValues()
    {
        var missingMembership = AccessMember.Create(
            Guid.Empty, "KZN-1", "Maya", null, AccessStatus.Active,
            MembershipStatus.Active, "Unlimited", Now, Now.AddDays(1), HomeBranchId, false);
        var reversedDates = AccessMember.Create(
            MembershipId, "KZN-1", "Maya", null, AccessStatus.Active,
            MembershipStatus.Active, "Unlimited", Now.AddDays(2), Now, HomeBranchId, false);
        var missingBranch = AccessMember.Create(
            MembershipId, "KZN-1", "Maya", null, AccessStatus.Active,
            MembershipStatus.Active, "Unlimited", Now, Now.AddDays(1), null, false);
        var missingName = AccessMember.Create(
            MembershipId, "KZN-1", " ", null, AccessStatus.Active,
            MembershipStatus.Active, "Unlimited", Now, Now.AddDays(1), HomeBranchId, false);

        Assert.Equal("Validation.MembershipId", missingMembership.Error.Code);
        Assert.Equal("Validation.MembershipEndsAt", reversedDates.Error.Code);
        Assert.Equal("Validation.HomeBranchId", missingBranch.Error.Code);
        Assert.Equal("Validation.FullName", missingName.Error.Code);
    }

    /// <summary>Branches normalize their code and validate capacity.</summary>
    [Fact]
    public void ClubBranch_Create_ValidatesAndNormalizes()
    {
        var branch = ClubBranch.Create(" Kinevor Shoreditch ", " shr ", " London ", 50);
        var invalid = ClubBranch.Create("Kinevor", "SHR", "London", 0);

        Assert.True(branch.IsSuccess);
        Assert.Equal("Kinevor Shoreditch", branch.Value.Name);
        Assert.Equal("SHR", branch.Value.Code);
        Assert.Equal(50, branch.Value.Capacity);
        Assert.Equal("Validation.Capacity", invalid.Error.Code);
    }

    /// <summary>A recorded visit snapshots member, branch, actor and access decision.</summary>
    [Fact]
    public void CheckIn_Record_CreatesOpenVisitAndRaisesEvent()
    {
        var member = CreateMember();
        var access = AccessPolicy.Evaluate(member, Now, HomeBranchId);

        var result = CheckIn.Record(
            Now, member, HomeBranchId, " Kinevor Shoreditch ", CheckInSource.FrontDesk,
            "  First visit  ", "staff-1", "Staff", access);

        Assert.True(result.IsSuccess);
        var visit = result.Value;
        Assert.Equal(member.Id, visit.MemberId);
        Assert.Equal(member.FullName, visit.MemberName);
        Assert.Equal("Kinevor Shoreditch", visit.BranchName);
        Assert.Equal("First visit", visit.Notes);
        Assert.Equal("Access.Allowed", visit.AccessCode);
        Assert.True(visit.IsOpen);
        Assert.Equal(TimeSpan.FromMinutes(45), visit.DurationAt(Now.AddMinutes(45)));
        Assert.IsType<MemberCheckedInEvent>(Assert.Single(visit.DomainEvents));
    }

    /// <summary>Visit creation rejects missing branch context and denied access.</summary>
    [Fact]
    public void CheckIn_Record_RejectsInvalidInput()
    {
        var member = CreateMember();
        var allowed = AccessDecision.Allow();
        var denied = AccessDecision.Deny("Access.MemberFrozen", "Frozen");

        var noBranch = CheckIn.Record(
            Now, member, Guid.Empty, "Kinevor", CheckInSource.FrontDesk,
            null, "staff-1", "Staff", allowed);
        var noActor = CheckIn.Record(
            Now, member, HomeBranchId, "Kinevor", CheckInSource.FrontDesk,
            null, " ", "Staff", allowed);
        var longNotes = CheckIn.Record(
            Now, member, HomeBranchId, "Kinevor", CheckInSource.FrontDesk,
            new string('a', 501), "staff-1", "Staff", allowed);
        var rejected = CheckIn.Record(
            Now, member, HomeBranchId, "Kinevor", CheckInSource.FrontDesk,
            null, "staff-1", "Staff", denied);

        Assert.Equal("Validation.BranchId", noBranch.Error.Code);
        Assert.Equal("Validation.RecordedBy", noActor.Error.Code);
        Assert.Equal("Validation.Notes", longNotes.Error.Code);
        Assert.Equal("Access.MemberFrozen", rejected.Error.Code);
        Assert.False(rejected.IsSuccess);
    }

    /// <summary>Checking out closes the visit and raises the departure event.</summary>
    [Fact]
    public void CheckIn_CheckOut_ClosesVisit()
    {
        var visit = NewVisit();

        var checkout = visit.CheckOut(Now.AddMinutes(70));

        Assert.True(checkout.IsSuccess);
        Assert.False(visit.IsOpen);
        Assert.Equal(Now.AddMinutes(70), visit.CheckedOutAt);
        Assert.Equal(TimeSpan.FromMinutes(70), visit.DurationAt(Now.AddHours(4)));
        Assert.Contains(visit.DomainEvents, domainEvent => domainEvent is MemberCheckedOutEvent);
    }

    /// <summary>A closed visit cannot be closed twice or before it started.</summary>
    [Fact]
    public void CheckIn_CheckOut_RejectsInvalidTransitions()
    {
        var visit = NewVisit();
        Assert.True(visit.CheckOut(Now.AddMinutes(30)).IsSuccess);

        var again = visit.CheckOut(Now.AddMinutes(40));
        var early = NewVisit().CheckOut(Now.AddMinutes(-1));

        Assert.Equal("Conflict", again.Error.Code);
        Assert.Equal("Validation.CheckedOutAt", early.Error.Code);
    }

    /// <summary>Audit records snapshot the decision and actor without credential fields.</summary>
    [Fact]
    public void AccessAudit_Record_CapturesDecision()
    {
        var member = CreateMember();
        var decision = AccessDecision.Deny("Access.BranchRestricted", "Not valid here.");

        var audit = AccessAudit.Record(
            member, OtherBranchId, AccessAuditAction.AccessDenied, AccessOutcome.Denied,
            decision, CheckInSource.MemberApp, "member-1", "Member", Now);

        Assert.Equal(member.Id, audit.MemberId);
        Assert.Equal("KZN-2026-00001", audit.MemberNumber);
        Assert.Equal(AccessAuditAction.AccessDenied, audit.Action);
        Assert.Equal(AccessOutcome.Denied, audit.Outcome);
        Assert.Equal("Access.BranchRestricted", audit.Code);
        Assert.Equal(CheckInSource.MemberApp, audit.Source);
        Assert.Equal(Now, audit.OccurredAt);
    }

    private static CheckIn NewVisit()
    {
        var member = CreateMember();
        return CheckIn.Record(
            Now, member, HomeBranchId, "Kinevor Shoreditch", CheckInSource.FrontDesk,
            null, "staff-1", "Staff", AccessDecision.Allow()).Value;
    }

    private static AccessMember CreateMember(
        AccessStatus status = AccessStatus.Active,
        MembershipStatus membershipStatus = MembershipStatus.Active,
        bool allClubs = false,
        DateTime? startsAt = null,
        DateTime? endsAt = null) =>
        AccessMember.Create(
            MembershipId,
            "KZN-2026-00001",
            "Maya Thompson",
            "maya.thompson@kinevor.demo",
            status,
            membershipStatus,
            "Unlimited",
            startsAt ?? Now.Date.AddDays(-30),
            endsAt ?? Now.Date.AddDays(30),
            allClubs ? null : HomeBranchId,
            allClubs).Value;
}
