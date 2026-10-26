using System.Net;
using System.Text.Json;
using Kinevor.Modules.StaffDashboard.DemoApi.Data;
using Xunit;

namespace Kinevor.Modules.StaffDashboard.Tests;

/// <summary>HTTP contract for the staff dashboard module.</summary>
public class StaffDashboardApiTests
{
    /// <summary>Anonymous requests are challenged.</summary>
    [Fact]
    public async Task Dashboard_RequiresAuthentication()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/staff-dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Members cannot read staff KPIs.</summary>
    [Fact]
    public async Task Dashboard_ForbidsMembers()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/staff-dashboard");
        request.Headers.Add("X-Demo-Role", "Member");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>Staff receive aggregated KPIs, a seven-day trend and seeded branches.</summary>
    [Fact]
    public async Task Dashboard_ReturnsAllBranchSummaryForStaff()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/staff-dashboard");
        request.Headers.Add("X-Demo-Role", "Staff");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var summary = data.GetProperty("summary");

        Assert.Equal("All branches", data.GetProperty("branchName").GetString());
        Assert.Equal(3, data.GetProperty("branches").GetArrayLength());
        Assert.Equal(7, data.GetProperty("trend").GetArrayLength());
        Assert.Equal(429, summary.GetProperty("activeMembers").GetInt32());
        Assert.Equal(521, summary.GetProperty("totalMembers").GetInt32());
        Assert.Equal(128, summary.GetProperty("checkInsToday").GetInt32());
        Assert.Equal(84, summary.GetProperty("bookingsToday").GetInt32());
        Assert.Equal(33570m, summary.GetProperty("revenueMonth").GetDecimal());
        Assert.Equal(51.9, summary.GetProperty("occupancyPercent").GetDouble());
        Assert.Equal(6, data.GetProperty("alerts").GetArrayLength());
    }

    /// <summary>A branch filter and 30-day range are honored.</summary>
    [Fact]
    public async Task Dashboard_FiltersBranchAndRange()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        var path = $"/api/v1/staff-dashboard?branchId={DemoDataSeeder.MainBranchId}&days=30";
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Demo-Role", "Staff");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Kinevor Gym - Main", data.GetProperty("branchName").GetString());
        Assert.Equal(30, data.GetProperty("trend").GetArrayLength());
        Assert.Equal(184, data.GetProperty("summary").GetProperty("activeMembers").GetInt32());
        Assert.Equal(14850m, data.GetProperty("summary").GetProperty("revenueMonth").GetDecimal());
    }

    /// <summary>Only 7 and 30-day ranges are supported.</summary>
    [Fact]
    public async Task Dashboard_RejectsUnsupportedRange()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/staff-dashboard?days=14");
        request.Headers.Add("X-Demo-Role", "Staff");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>Unknown branches return the standard not-found envelope.</summary>
    [Fact]
    public async Task Dashboard_ReturnsNotFoundForUnknownBranch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/staff-dashboard?branchId={Guid.NewGuid()}");
        request.Headers.Add("X-Demo-Role", "Staff");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
    }

    /// <summary>The alert inbox returns every seeded alert and filters by branch.</summary>
    [Fact]
    public async Task Alerts_ReturnAllAndFilterByBranch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var allRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/staff-dashboard/alerts");
        allRequest.Headers.Add("X-Demo-Role", "Staff");
        var allResponse = await client.SendAsync(allRequest);
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        using var allDocument = JsonDocument.Parse(await allResponse.Content.ReadAsStringAsync());
        Assert.Equal(12, allDocument.RootElement.GetProperty("data").GetProperty("alerts").GetArrayLength());

        var branchPath = $"/api/v1/staff-dashboard/alerts?branchId={DemoDataSeeder.WestsideBranchId}";
        using var branchRequest = new HttpRequestMessage(HttpMethod.Get, branchPath);
        branchRequest.Headers.Add("X-Demo-Role", "Staff");
        var branchResponse = await client.SendAsync(branchRequest);
        using var branchDocument = JsonDocument.Parse(await branchResponse.Content.ReadAsStringAsync());
        var branchData = branchDocument.RootElement.GetProperty("data");
        Assert.Equal("Kinevor Gym - Westside", branchData.GetProperty("branchName").GetString());
        Assert.Equal(4, branchData.GetProperty("alerts").GetArrayLength());
    }

    /// <summary>Branch options expose all seeded locations.</summary>
    [Fact]
    public async Task Branches_ReturnThreeLocations()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/staff-dashboard/branches");
        request.Headers.Add("X-Demo-Role", "Admin");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var branches = document.RootElement.GetProperty("data");
        Assert.Equal(3, branches.GetArrayLength());
        Assert.Equal(580, branches.EnumerateArray().Sum(branch => branch.GetProperty("capacity").GetInt32()));
    }
}
