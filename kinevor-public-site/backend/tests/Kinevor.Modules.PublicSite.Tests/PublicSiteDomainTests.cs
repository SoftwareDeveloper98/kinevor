using Kinevor.Modules.PublicSite.Domain;
using Kinevor.Platform.Application.Exceptions;
using Xunit;

namespace Kinevor.Modules.PublicSite.Tests;

/// <summary>Domain rules for trial and enquiry capture.</summary>
public class PublicSiteDomainTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Trial_NormalizesContactAndSetsNewStatus()
    {
        var trial = TrialBooking.Create(
            Now,
            "  Avery ",
            "Stone ",
            " AVERY.STONE@EXAMPLE.COM ",
            " +1 (646) 555-0199 ",
            Now.Date.AddDays(2),
            null,
            null,
            null,
            null,
            " Build strength ",
            null,
            true);

        Assert.Equal("Avery Stone", trial.FullName);
        Assert.Equal("avery.stone@example.com", trial.Email);
        Assert.Equal(LeadLifecycle.New, trial.Status);
    }

    [Fact]
    public void Trial_RequiresContactConsent()
    {
        var exception = Assert.Throws<DomainException>(() => TrialBooking.Create(
            Now,
            "Avery",
            "Stone",
            "avery.stone@example.com",
            "+1 (646) 555-0199",
            Now.Date.AddDays(2),
            null,
            null,
            null,
            null,
            null,
            null,
            false));

        Assert.Contains("Consent", exception.Message);
    }

    [Fact]
    public void Trial_RejectsPastPreferredDate()
    {
        var exception = Assert.Throws<DomainException>(() => TrialBooking.Create(
            Now,
            "Avery",
            "Stone",
            "avery.stone@example.com",
            "+1 (646) 555-0199",
            Now.Date.AddDays(-1),
            null,
            null,
            null,
            null,
            null,
            null,
            true));

        Assert.Contains("past", exception.Message);
    }

    [Fact]
    public void Trial_RejectsInvalidEmailAndShortPhone()
    {
        Assert.Throws<DomainException>(() => TrialBooking.Create(
            Now,
            "Avery",
            "Stone",
            "not-an-email",
            "+1 (646) 555-0199",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true));

        Assert.Throws<DomainException>(() => TrialBooking.Create(
            Now,
            "Avery",
            "Stone",
            "avery.stone@example.com",
            "123",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true));
    }

    [Fact]
    public void Lead_RequiresConsentAndUsefulMessage()
    {
        Assert.Throws<DomainException>(() => Lead.Create(
            "Morgan Reed",
            "morgan.reed@example.com",
            null,
            "Membership",
            "I would like a tour of the Westside location.",
            false));

        Assert.Throws<DomainException>(() => Lead.Create(
            "Morgan Reed",
            "morgan.reed@example.com",
            null,
            "Membership",
            "Too short",
            true));
    }

    [Fact]
    public void Lead_NormalizesEmailAndTrimsFields()
    {
        var lead = Lead.Create(
            " Morgan Reed ",
            " MORGAN.REED@EXAMPLE.COM ",
            " +1 (917) 555-0110 ",
            " Membership ",
            " I would like a tour of the Westside location. ",
            true);

        Assert.Equal("Morgan Reed", lead.Name);
        Assert.Equal("morgan.reed@example.com", lead.Email);
        Assert.Equal("Membership", lead.Interest);
    }

    [Fact]
    public void MembershipPlan_RejectsNegativePrice()
    {
        Assert.Throws<DomainException>(() => MembershipPlan.Seed(
            Guid.NewGuid(),
            "Basic",
            "basic",
            "Basic plan",
            "Basic",
            -1,
            "USD",
            1,
            ["Gym access"],
            1));
    }
}
