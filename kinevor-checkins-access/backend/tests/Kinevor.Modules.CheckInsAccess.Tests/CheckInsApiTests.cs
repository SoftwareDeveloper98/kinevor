using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kinevor.Modules.CheckInsAccess.DemoApi.Data;
using Xunit;

namespace Kinevor.Modules.CheckInsAccess.Tests;

/// <summary>HTTP contract for eligibility, check-in, occupancy, history and self-service.</summary>
public class CheckInsApiTests
{
    private static readonly Guid MayaId = DemoDataSeeder.PrimaryMemberId;
    private static readonly Guid SofiaId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid ShoreditchId = DemoDataSeeder.ShoreditchBranchId;
    private static readonly Guid CamdenId = DemoDataSeeder.CamdenBranchId;

    /// <summary>Staff can search the seeded access directory.</summary>
    [Fact]
    public async Task Members_ReturnsSeededProjectionsForStaff()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/members?page=1&pageSize=50"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(19, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(19, data.GetProperty("items").GetArrayLength());
        Assert.Contains(data.GetProperty("items").EnumerateArray(), item =>
            item.GetProperty("fullName").GetString() == "Maya Thompson" &&
            item.GetProperty("membershipStatus").GetString() == "Active");
    }

    /// <summary>Search matches member name, number and e-mail.</summary>
    [Fact]
    public async Task Members_SearchesByName()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/members?search=Maya"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(MayaId, data.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    /// <summary>Staff endpoints reject members and anonymous callers.</summary>
    [Fact]
    public async Task StaffEndpoints_RequireStaffRole()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var member = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/members", role: "Member"));
        using var anonymous = await client.SendAsync(new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/checkins/members"));

        Assert.Equal(HttpStatusCode.Forbidden, member.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    /// <summary>Eligibility allows Maya at her home branch and explains the decision.</summary>
    [Fact]
    public async Task Eligibility_AllowsActiveMemberAtHomeBranch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins/eligibility/{MayaId}"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(data.GetProperty("allowed").GetBoolean());
        Assert.Equal("Access.Allowed", data.GetProperty("code").GetString());
        Assert.Equal("Kinevor Shoreditch", data.GetProperty("branchName").GetString());
        Assert.Equal("Kinevor Shoreditch", data.GetProperty("homeBranchName").GetString());
        Assert.False(data.GetProperty("allClubsAccess").GetBoolean());
    }

    /// <summary>Eligibility denies frozen members with a stable code.</summary>
    [Fact]
    public async Task Eligibility_DeniesFrozenMember()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins/eligibility/{SofiaId}"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(data.GetProperty("allowed").GetBoolean());
        Assert.Equal("Access.MemberFrozen", data.GetProperty("code").GetString());
    }

    /// <summary>Branch-restricted members are denied at another branch.</summary>
    [Fact]
    public async Task Eligibility_EnforcesBranchRestriction()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get,
            $"/api/v1/checkins/eligibility/{MayaId}?branchId={CamdenId}"));
        var data = await DataAsync(response);

        Assert.False(data.GetProperty("allowed").GetBoolean());
        Assert.Equal("Access.BranchRestricted", data.GetProperty("code").GetString());
        Assert.Equal("Kinevor Camden", data.GetProperty("branchName").GetString());
    }

    /// <summary>Occupancy reports the seeded open visits and safe capacity.</summary>
    [Fact]
    public async Task Occupancy_ReturnsSeededSnapshot()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins/occupancy?branchId={ShoreditchId}"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Kinevor Shoreditch", data.GetProperty("branchName").GetString());
        Assert.Equal(50, data.GetProperty("capacity").GetInt32());
        Assert.Equal(12, data.GetProperty("occupied").GetInt32());
        Assert.Equal(38, data.GetProperty("available").GetInt32());
        Assert.Equal(24, data.GetProperty("utilizationPercent").GetDouble());
        Assert.Equal("Comfortable", data.GetProperty("status").GetString());
        Assert.Equal(12, data.GetProperty("openVisits").GetArrayLength());
    }

    /// <summary>The branch header scopes occupancy and history reads.</summary>
    [Fact]
    public async Task BranchHeader_ScopesOccupancy()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/occupancy", branchId: CamdenId));
        var data = await DataAsync(response);

        Assert.Equal("Kinevor Camden", data.GetProperty("branchName").GetString());
        Assert.Equal(3, data.GetProperty("occupied").GetInt32());
    }

    /// <summary>Branch selection returns both seeded branches with capacity.</summary>
    [Fact]
    public async Task Branches_ReturnsSeededBranches()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/branches", role: "Member"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, data.GetArrayLength());
        Assert.Contains(data.EnumerateArray(), branch =>
            branch.GetProperty("code").GetString() == "SHR" &&
            branch.GetProperty("capacity").GetInt32() == 50);
    }

    /// <summary>Daily statistics aggregate today's seeded visits for a branch.</summary>
    [Fact]
    public async Task Stats_AggregatesTodaysVisits()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins/stats?branchId={ShoreditchId}"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(12, data.GetProperty("totalCheckIns").GetInt32());
        Assert.Equal(12, data.GetProperty("uniqueMembers").GetInt32());
        Assert.Equal(12, data.GetProperty("openVisits").GetInt32());
        Assert.Equal("Kinevor Shoreditch", data.GetProperty("branchName").GetString());
    }

    /// <summary>History supports open-state, member and branch filters.</summary>
    [Fact]
    public async Task History_FiltersByStatusMemberAndBranch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var all = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins?page=1&pageSize=5"));
        var allData = await DataAsync(all);

        using var maya = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins?memberId={MayaId}&status=closed"));
        var mayaData = await DataAsync(maya);

        using var camden = await client.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/checkins?branchId={CamdenId}&status=open"));
        var camdenData = await DataAsync(camden);

        Assert.Equal(23, allData.GetProperty("totalCount").GetInt32());
        Assert.Equal(5, allData.GetProperty("items").GetArrayLength());
        Assert.Equal(8, mayaData.GetProperty("totalCount").GetInt32());
        Assert.All(mayaData.GetProperty("items").EnumerateArray(), item =>
            Assert.False(item.GetProperty("isOpen").GetBoolean()));
        Assert.Equal(3, camdenData.GetProperty("totalCount").GetInt32());
    }

    /// <summary>Audit history includes seeded check-ins, check-outs and denials.</summary>
    [Fact]
    public async Task Audit_ReturnsSeededDecisionLog()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/audit?page=1&pageSize=100"));
        var data = await DataAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(34, data.GetProperty("totalCount").GetInt32());
        Assert.Contains(data.GetProperty("items").EnumerateArray(), item =>
            item.GetProperty("action").GetString() == "AccessDenied" &&
            item.GetProperty("outcome").GetString() == "Denied");
    }

    /// <summary>Front-desk check-in creates a visit, blocks duplicates and closes it.</summary>
    [Fact]
    public async Task CheckInAndOut_CompletesVisitLifecycle()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var created = await client.SendAsync(Request(
            HttpMethod.Post,
            "/api/v1/checkins",
            body: new { memberId = MayaId, notes = "Morning session" }));
        var createdData = await DataAsync(created);
        var checkInId = createdData.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.True(createdData.GetProperty("isOpen").GetBoolean());
        Assert.Equal("FrontDesk", createdData.GetProperty("source").GetString());
        Assert.Equal("Morning session", createdData.GetProperty("notes").GetString());
        Assert.Equal("Kinevor Shoreditch", createdData.GetProperty("branchName").GetString());

        using var duplicate = await client.SendAsync(Request(
            HttpMethod.Post, "/api/v1/checkins", body: new { memberId = MayaId }));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var checkout = await client.SendAsync(Request(
            HttpMethod.Post, $"/api/v1/checkins/{checkInId}/checkout"));
        var checkoutData = await DataAsync(checkout);

        Assert.Equal(HttpStatusCode.OK, checkout.StatusCode);
        Assert.False(checkoutData.GetProperty("isOpen").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, checkoutData.GetProperty("checkedOutAt").ValueKind);

        using var again = await client.SendAsync(Request(
            HttpMethod.Post, $"/api/v1/checkins/{checkInId}/checkout"));
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    /// <summary>Denied check-ins add an audit record and do not create a visit.</summary>
    [Fact]
    public async Task CheckIn_DeniesIneligibleMember()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var denied = await client.SendAsync(Request(
            HttpMethod.Post, "/api/v1/checkins", body: new { memberId = SofiaId }));
        using var audit = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/checkins/audit?page=1&pageSize=100"));
        var auditData = await DataAsync(audit);

        Assert.Equal(HttpStatusCode.BadRequest, denied.StatusCode);
        Assert.Equal(35, auditData.GetProperty("totalCount").GetInt32());
    }

    /// <summary>Member self-service reports status, checks in and checks out.</summary>
    [Fact]
    public async Task SelfService_ChecksInAndOut()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var initial = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/me/checkins/status", role: "Member", memberId: MayaId));
        var initialData = await DataAsync(initial);

        Assert.Equal(HttpStatusCode.OK, initial.StatusCode);
        Assert.Equal("Maya Thompson", initialData.GetProperty("fullName").GetString());
        Assert.Equal(JsonValueKind.Null, initialData.GetProperty("openCheckIn").ValueKind);
        Assert.Equal("Access.Allowed", initialData.GetProperty("eligibility")
            .GetProperty("code").GetString());
        Assert.Equal(12, initialData.GetProperty("occupancy")
            .GetProperty("occupied").GetInt32());

        using var checkIn = await client.SendAsync(Request(
            HttpMethod.Post, "/api/v1/me/checkins", role: "Member", memberId: MayaId,
            body: new { notes = "Self-service entry" }));
        var checkInData = await DataAsync(checkIn);

        Assert.Equal(HttpStatusCode.Created, checkIn.StatusCode);
        Assert.Equal("MemberApp", checkInData.GetProperty("source").GetString());
        Assert.True(checkInData.GetProperty("isOpen").GetBoolean());

        using var open = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/me/checkins/status", role: "Member", memberId: MayaId));
        var openData = await DataAsync(open);
        Assert.NotEqual(JsonValueKind.Null, openData.GetProperty("openCheckIn").ValueKind);

        using var checkout = await client.SendAsync(Request(
            HttpMethod.Post, "/api/v1/me/checkins/checkout", role: "Member", memberId: MayaId));
        var checkoutData = await DataAsync(checkout);

        Assert.Equal(HttpStatusCode.OK, checkout.StatusCode);
        Assert.False(checkoutData.GetProperty("isOpen").GetBoolean());
    }

    /// <summary>Self-service requires a linked member claim and exposes own visits.</summary>
    [Fact]
    public async Task SelfService_RequiresLinkedMemberAndListsVisits()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var missing = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/me/checkins/status", role: "Member"));
        Assert.Equal(HttpStatusCode.Forbidden, missing.StatusCode);

        using var visits = await client.SendAsync(Request(
            HttpMethod.Get, "/api/v1/me/checkins?pageSize=100", role: "Member", memberId: MayaId));
        var visitsData = await DataAsync(visits);

        Assert.Equal(HttpStatusCode.OK, visits.StatusCode);
        Assert.Equal(8, visitsData.GetProperty("totalCount").GetInt32());
        Assert.All(visitsData.GetProperty("items").EnumerateArray(), item =>
            Assert.Equal(MayaId, item.GetProperty("memberId").GetGuid()));
    }

    private static HttpRequestMessage Request(
        HttpMethod method,
        string path,
        string role = "Staff",
        Guid? branchId = null,
        Guid? memberId = null,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Demo-Role", role);
        if (branchId is { } branch)
            request.Headers.Add("X-Branch-Id", branch.ToString());
        if (memberId is { } member)
            request.Headers.Add("X-Demo-Member", member.ToString());
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<JsonElement> DataAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.GetProperty("data").Clone();
    }
}
