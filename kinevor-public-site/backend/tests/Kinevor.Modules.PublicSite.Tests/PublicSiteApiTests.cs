using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kinevor.Modules.PublicSite.DemoApi.Data;
using Xunit;

namespace Kinevor.Modules.PublicSite.Tests;

/// <summary>Anonymous content and lead-capture HTTP contract.</summary>
public class PublicSiteApiTests
{
    [Fact]
    public async Task Site_IsAnonymousAndIncludesSeededBranches()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/public/site");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ReadDocument(response);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Kinevor", data.GetProperty("name").GetString());
        Assert.Equal(2400, data.GetProperty("memberCount").GetInt32());
        Assert.Equal(3, data.GetProperty("branches").GetArrayLength());
        Assert.Equal(4, data.GetProperty("highlights").GetArrayLength());
    }

    [Fact]
    public async Task Plans_ReturnOnlyPublishedOffersInOrder()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/public/plans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ReadDocument(response);
        var plans = document.RootElement.GetProperty("data").EnumerateArray().ToArray();
        Assert.Equal(4, plans.Length);
        Assert.Equal("Essential", plans[0].GetProperty("name").GetString());
        Assert.Equal("Performance", plans[1].GetProperty("name").GetString());
        Assert.True(plans[1].GetProperty("isFeatured").GetBoolean());
        Assert.Equal("per month", plans[0].GetProperty("billingLabel").GetString());
        Assert.Equal("per year", plans[3].GetProperty("billingLabel").GetString());
        Assert.DoesNotContain(plans, plan => plan.GetProperty("name").GetString() == "Day Pass");
    }

    [Fact]
    public async Task TrainersAndFaqs_HideDrafts()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var trainersResponse = await client.GetAsync("/api/v1/public/trainers");
        using var trainersDocument = await ReadDocument(trainersResponse);
        Assert.Equal(4, trainersDocument.RootElement.GetProperty("data").GetArrayLength());

        var faqsResponse = await client.GetAsync("/api/v1/public/faqs");
        using var faqsDocument = await ReadDocument(faqsResponse);
        var faqs = faqsDocument.RootElement.GetProperty("data").EnumerateArray().ToArray();
        Assert.Equal(6, faqs.Length);
        Assert.DoesNotContain(faqs, faq =>
            faq.GetProperty("question").GetString() == "Draft billing question");
    }

    [Fact]
    public async Task Schedule_ReturnsUpcomingPublishedSessions()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/public/schedule");
        using var document = await ReadDocument(response);
        var sessions = document.RootElement.GetProperty("data").EnumerateArray().ToArray();

        Assert.Equal(8, sessions.Length);
        Assert.DoesNotContain(sessions, session =>
            session.GetProperty("name").GetString() == "Draft Pilates");

        var starts = sessions
            .Select(session => session.GetProperty("startsAt").GetDateTime())
            .ToArray();
        Assert.Equal(starts.OrderBy(value => value), starts);
    }

    [Fact]
    public async Task Schedule_CanFilterByBranch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/public/schedule?branchId={DemoDataSeeder.WestsideBranchId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ReadDocument(response);
        var sessions = document.RootElement.GetProperty("data").EnumerateArray().ToArray();
        Assert.Equal(2, sessions.Length);
        Assert.All(sessions, session =>
            Assert.Equal(DemoDataSeeder.WestsideBranchId, session.GetProperty("branchId").GetGuid()));
    }

    [Fact]
    public async Task Posts_ReturnPublishedIndexAndDetail()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var indexResponse = await client.GetAsync("/api/v1/public/posts");
        using var indexDocument = await ReadDocument(indexResponse);
        var posts = indexDocument.RootElement.GetProperty("data").EnumerateArray().ToArray();
        Assert.Equal(3, posts.Length);
        Assert.DoesNotContain(posts, post => post.GetProperty("slug").GetString() == "draft-article");

        var detailResponse = await client.GetAsync("/api/v1/public/posts/strength-that-travels");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        using var detailDocument = await ReadDocument(detailResponse);
        Assert.Contains("five patterns", detailDocument.RootElement
            .GetProperty("data").GetProperty("content").GetString());

        var missingResponse = await client.GetAsync("/api/v1/public/posts/draft-article");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Branch_ReturnsNotFoundEnvelopeForUnknownSlug()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var found = await client.GetAsync("/api/v1/public/branches/main");
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);

        var missing = await client.GetAsync("/api/v1/public/branches/nowhere");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var document = await ReadDocument(missing);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Trial_CreatesRequestAndRejectsDuplicate()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        var body = new
        {
            firstName = "Avery",
            lastName = "Stone",
            email = "avery.stone@example.com",
            phone = "+1 (646) 555-0199",
            classSessionId = DemoDataSeeder.FirstClassId,
            branchId = DemoDataSeeder.MainBranchId,
            goal = "Get stronger",
            notes = "Evening availability preferred.",
            consentToContact = true,
        };

        var created = await client.PostAsJsonAsync("/api/v1/public/trials", body);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDocument = await ReadDocument(created);
        Assert.NotEqual(Guid.Empty, createdDocument.RootElement
            .GetProperty("data").GetProperty("id").GetGuid());

        var duplicate = await client.PostAsJsonAsync("/api/v1/public/trials", body);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Trial_ValidatesConsentEmailAndPastDate()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var noConsent = await client.PostAsJsonAsync("/api/v1/public/trials", new
        {
            firstName = "Avery",
            lastName = "Stone",
            email = "avery.stone@example.com",
            phone = "+1 (646) 555-0199",
            preferredDate = DateTime.Today.AddDays(2),
            consentToContact = false,
        });
        Assert.Equal(HttpStatusCode.BadRequest, noConsent.StatusCode);

        var badEmail = await client.PostAsJsonAsync("/api/v1/public/trials", new
        {
            firstName = "Avery",
            lastName = "Stone",
            email = "not-an-email",
            phone = "+1 (646) 555-0199",
            preferredDate = DateTime.Today.AddDays(2),
            consentToContact = true,
        });
        Assert.Equal(HttpStatusCode.BadRequest, badEmail.StatusCode);

        var pastDate = await client.PostAsJsonAsync("/api/v1/public/trials", new
        {
            firstName = "Avery",
            lastName = "Stone",
            email = "avery.stone@example.com",
            phone = "+1 (646) 555-0199",
            preferredDate = DateTime.Today.AddDays(-1),
            consentToContact = true,
        });
        Assert.Equal(HttpStatusCode.BadRequest, pastDate.StatusCode);
    }

    [Fact]
    public async Task Leads_CaptureEnquiryAndRequireStaffToRead()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/v1/public/leads", new
        {
            name = "Morgan Reed",
            email = "morgan.reed@example.com",
            phone = "+1 (917) 555-0110",
            interest = "Membership",
            message = "I would like a tour of the Westside location this week.",
            consentToContact = true,
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var anonymous = await client.GetAsync("/api/v1/public/leads");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        using var memberRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/public/leads");
        memberRequest.Headers.Add("X-Demo-Role", "Member");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(memberRequest)).StatusCode);

        using var staffRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/public/leads?pageSize=100");
        staffRequest.Headers.Add("X-Demo-Role", "Staff");
        var staff = await client.SendAsync(staffRequest);
        Assert.Equal(HttpStatusCode.OK, staff.StatusCode);

        using var document = await ReadDocument(staff);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(4, data.GetProperty("totalCount").GetInt32());
        Assert.Contains(data.GetProperty("items").EnumerateArray(), item =>
            item.GetProperty("email").GetString() == "morgan.reed@example.com");
    }

    [Fact]
    public async Task Leads_SupportKindSearchAndPagination()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var trialRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/public/leads?kind=Trial&pageSize=10");
        trialRequest.Headers.Add("X-Demo-Role", "Admin");
        var trialsResponse = await client.SendAsync(trialRequest);
        using var trialsDocument = await ReadDocument(trialsResponse);
        var trials = trialsDocument.RootElement.GetProperty("data");
        Assert.Equal(1, trials.GetProperty("totalCount").GetInt32());
        Assert.Equal("Trial", trials.GetProperty("items")[0].GetProperty("kind").GetString());

        using var searchRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/public/leads?search=priya");
        searchRequest.Headers.Add("X-Demo-Role", "Staff");
        var searchResponse = await client.SendAsync(searchRequest);
        using var searchDocument = await ReadDocument(searchResponse);
        Assert.Equal(1, searchDocument.RootElement
            .GetProperty("data").GetProperty("totalCount").GetInt32());

        using var pageRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/public/leads?page=2&pageSize=1");
        pageRequest.Headers.Add("X-Demo-Role", "Staff");
        var pageResponse = await client.SendAsync(pageRequest);
        using var pageDocument = await ReadDocument(pageResponse);
        var page = pageDocument.RootElement.GetProperty("data");
        Assert.Equal(3, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, page.GetProperty("totalPages").GetInt32());
        Assert.True(page.GetProperty("hasNext").GetBoolean());
        Assert.True(page.GetProperty("hasPrev").GetBoolean());
    }

    [Fact]
    public async Task Health_IsAnonymous()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonDocument> ReadDocument(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
