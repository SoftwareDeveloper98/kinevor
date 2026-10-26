using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kinevor.Modules.Notifications.DemoApi.Data;
using Xunit;

namespace Kinevor.Modules.Notifications.Tests;

/// <summary>HTTP contract for the notifications module.</summary>
public class NotificationsApiTests
{
    /// <summary>Maya's inbox lists every seeded notification owned by her.</summary>
    [Fact]
    public async Task Inbox_ListsSeededNotificationsForMaya()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = MemberRequest(HttpMethod.Get, "/api/v1/me/notifications?page=1&pageSize=20");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 5);

        foreach (var item in data.GetProperty("items").EnumerateArray())
            Assert.Equal(DemoDataSeeder.PrimaryMemberId, item.GetProperty("memberId").GetGuid());
    }

    /// <summary>Unread count, unread list and mark-all-read agree on the seeded inbox.</summary>
    [Fact]
    public async Task Inbox_UnreadCountAndMarkAllRead()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var countRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/notifications/unread-count");
        var countResponse = await client.SendAsync(countRequest);
        using var countDocument = JsonDocument.Parse(await countResponse.Content.ReadAsStringAsync());
        Assert.Equal(4, countDocument.RootElement.GetProperty("data").GetProperty("unread").GetInt32());

        using var unreadRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/notifications/unread");
        var unreadResponse = await client.SendAsync(unreadRequest);
        using var unreadDocument = JsonDocument.Parse(await unreadResponse.Content.ReadAsStringAsync());
        Assert.Equal(4, unreadDocument.RootElement.GetProperty("data").GetArrayLength());

        using var readAllRequest = MemberRequest(HttpMethod.Post, "/api/v1/me/notifications/read-all");
        var readAllResponse = await client.SendAsync(readAllRequest);
        using var readAllDocument = JsonDocument.Parse(await readAllResponse.Content.ReadAsStringAsync());
        Assert.Equal(4, readAllDocument.RootElement.GetProperty("data").GetProperty("marked").GetInt32());

        using var afterRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/notifications/unread-count");
        var afterResponse = await client.SendAsync(afterRequest);
        using var afterDocument = JsonDocument.Parse(await afterResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, afterDocument.RootElement.GetProperty("data").GetProperty("unread").GetInt32());
    }

    /// <summary>A member can open and mark one notification read.</summary>
    [Fact]
    public async Task Inbox_DetailAndMarkRead()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        var id = DemoDataSeeder.UnreadNotificationId;

        using var detailRequest = MemberRequest(HttpMethod.Get, $"/api/v1/me/notifications/{id}");
        var detailResponse = await client.SendAsync(detailRequest);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        using var detailDocument = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        Assert.True(detailDocument.RootElement.GetProperty("data").GetProperty("unread").GetBoolean());

        using var readRequest = MemberRequest(HttpMethod.Post, $"/api/v1/me/notifications/{id}/read");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(readRequest)).StatusCode);

        using var afterRequest = MemberRequest(HttpMethod.Get, $"/api/v1/me/notifications/{id}");
        var afterResponse = await client.SendAsync(afterRequest);
        using var afterDocument = JsonDocument.Parse(await afterResponse.Content.ReadAsStringAsync());
        var after = afterDocument.RootElement.GetProperty("data");
        Assert.False(after.GetProperty("unread").GetBoolean());
        Assert.Equal("Read", after.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.String, after.GetProperty("readAt").ValueKind);
    }

    /// <summary>Another member's notification is not visible.</summary>
    [Fact]
    public async Task Inbox_HidesOtherMembersNotifications()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = MemberRequest(
            HttpMethod.Get, $"/api/v1/me/notifications/{DemoDataSeeder.FailedNotificationId}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>Preferences return defaults and accept category updates.</summary>
    [Fact]
    public async Task Preferences_ReturnDefaultsAndUpdate()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var initialRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/notification-preferences");
        var initialResponse = await client.SendAsync(initialRequest);
        using var initialDocument = JsonDocument.Parse(await initialResponse.Content.ReadAsStringAsync());
        var initial = initialDocument.RootElement.GetProperty("data");
        Assert.Equal(4, initial.GetArrayLength());
        Assert.False(FindCategory(initial, "offers").GetProperty("email").GetBoolean());

        using var updateRequest = MemberRequest(HttpMethod.Put, "/api/v1/me/notification-preferences");
        updateRequest.Content = JsonContent.Create(new object[]
        {
            new { category = "offers", email = true, push = false, inApp = true },
            new { category = "class_reminders", email = false, push = true, inApp = false },
        });
        var updateResponse = await client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var updatedDocument = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync());
        var updated = updatedDocument.RootElement.GetProperty("data");
        Assert.True(FindCategory(updated, "offers").GetProperty("email").GetBoolean());
        Assert.False(FindCategory(updated, "class_reminders").GetProperty("email").GetBoolean());
        Assert.False(FindCategory(updated, "class_reminders").GetProperty("inApp").GetBoolean());
    }

    /// <summary>Devices can be registered, listed and unregistered.</summary>
    [Fact]
    public async Task Devices_RegisterListAndUnregister()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var registerRequest = MemberRequest(HttpMethod.Post, "/api/v1/me/devices");
        registerRequest.Content = JsonContent.Create(new { token = "test-device-token-1234567890", platform = "Web" });
        var registerResponse = await client.SendAsync(registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        using var registerDocument = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var deviceId = registerDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal("web", registerDocument.RootElement.GetProperty("data").GetProperty("platform").GetString());

        using var listRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/devices");
        var listResponse = await client.SendAsync(listRequest);
        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Contains(listDocument.RootElement.GetProperty("data").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == deviceId);

        using var deleteRequest = MemberRequest(HttpMethod.Delete, $"/api/v1/me/devices/{deviceId}");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(deleteRequest)).StatusCode);

        using var afterRequest = MemberRequest(HttpMethod.Get, "/api/v1/me/devices");
        var afterResponse = await client.SendAsync(afterRequest);
        using var afterDocument = JsonDocument.Parse(await afterResponse.Content.ReadAsStringAsync());
        Assert.DoesNotContain(afterDocument.RootElement.GetProperty("data").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == deviceId);
    }

    /// <summary>Compose applies template category and channel defaults and delivers immediately.</summary>
    [Fact]
    public async Task Compose_UsesTemplateDefaults()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var composeRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications");
        composeRequest.Content = JsonContent.Create(new
        {
            memberId = DemoDataSeeder.PrimaryMemberId,
            templateKey = "offer_announcement",
            variables = new Dictionary<string, string>
            {
                ["FirstName"] = "Maya",
                ["OfferTitle"] = "Spring offer",
                ["OfferBody"] = "Save 10% this week.",
            },
        });
        var composeResponse = await client.SendAsync(composeRequest);
        Assert.Equal(HttpStatusCode.Created, composeResponse.StatusCode);

        using var composeDocument = JsonDocument.Parse(await composeResponse.Content.ReadAsStringAsync());
        var result = composeDocument.RootElement.GetProperty("data");
        Assert.Equal(1, result.GetProperty("created").GetInt32());
        Assert.Equal(1, result.GetProperty("delivered").GetInt32());
        Assert.Equal(0, result.GetProperty("failed").GetInt32());

        using var ledgerRequest = StaffRequest(
            HttpMethod.Get,
            $"/api/v1/notifications?memberId={DemoDataSeeder.PrimaryMemberId}&category=offers&pageSize=100");
        var ledgerResponse = await client.SendAsync(ledgerRequest);
        using var ledgerDocument = JsonDocument.Parse(await ledgerResponse.Content.ReadAsStringAsync());
        var item = Assert.Single(
            ledgerDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            entry => entry.GetProperty("title").GetString() == "Spring offer");
        Assert.Equal("offers", item.GetProperty("category").GetString());
        Assert.Equal("InApp", item.GetProperty("channel").GetString());
        Assert.Equal("Sent", item.GetProperty("status").GetString());
        Assert.Equal("offer_announcement", item.GetProperty("templateKey").GetString());
    }

    /// <summary>Compose rejects unknown categories and missing message content.</summary>
    [Fact]
    public async Task Compose_ValidatesInput()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var unknownCategory = StaffRequest(HttpMethod.Post, "/api/v1/notifications");
        unknownCategory.Content = JsonContent.Create(new
        {
            memberId = DemoDataSeeder.PrimaryMemberId,
            title = "Title",
            body = "Body",
            category = "not_a_category",
        });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(unknownCategory)).StatusCode);

        using var missingContent = StaffRequest(HttpMethod.Post, "/api/v1/notifications");
        missingContent.Content = JsonContent.Create(new
        {
            memberId = DemoDataSeeder.PrimaryMemberId,
            category = "offers",
        });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(missingContent)).StatusCode);
    }

    /// <summary>Broadcast creates one record per distinct recipient.</summary>
    [Fact]
    public async Task Broadcast_CreatesForEachMember()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var broadcastRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications/broadcast");
        broadcastRequest.Content = JsonContent.Create(new
        {
            memberIds = new[]
            {
                DemoDataSeeder.SecondaryMemberId,
                DemoDataSeeder.TertiaryMemberId,
                DemoDataSeeder.TertiaryMemberId,
            },
            title = "Studio closure",
            body = "The studio closes at 14:00 on Friday.",
            category = "offers",
            channel = "InApp",
        });
        var broadcastResponse = await client.SendAsync(broadcastRequest);
        Assert.Equal(HttpStatusCode.Created, broadcastResponse.StatusCode);

        using var broadcastDocument = JsonDocument.Parse(await broadcastResponse.Content.ReadAsStringAsync());
        var result = broadcastDocument.RootElement.GetProperty("data");
        Assert.Equal(2, result.GetProperty("created").GetInt32());
        Assert.Equal(2, result.GetProperty("delivered").GetInt32());

        using var ledgerRequest = StaffRequest(
            HttpMethod.Get,
            $"/api/v1/notifications?memberId={DemoDataSeeder.TertiaryMemberId}&pageSize=100");
        var ledgerResponse = await client.SendAsync(ledgerRequest);
        using var ledgerDocument = JsonDocument.Parse(await ledgerResponse.Content.ReadAsStringAsync());
        Assert.Contains(ledgerDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            entry => entry.GetProperty("title").GetString() == "Studio closure");
    }

    /// <summary>An email to a suppressed address fails through the simulator.</summary>
    [Fact]
    public async Task Compose_SuppressedEmailFails()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var composeRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications");
        composeRequest.Content = JsonContent.Create(new
        {
            memberId = DemoDataSeeder.TertiaryMemberId,
            title = "Receipt for November",
            body = "Thank you for your payment.",
            category = "receipts",
            channel = "Email",
            recipientEmail = "bounced@kinevor.demo",
        });
        var composeResponse = await client.SendAsync(composeRequest);
        Assert.Equal(HttpStatusCode.Created, composeResponse.StatusCode);

        using var composeDocument = JsonDocument.Parse(await composeResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, composeDocument.RootElement.GetProperty("data").GetProperty("failed").GetInt32());

        using var ledgerRequest = StaffRequest(
            HttpMethod.Get,
            $"/api/v1/notifications?memberId={DemoDataSeeder.TertiaryMemberId}&status=Failed&pageSize=100");
        var ledgerResponse = await client.SendAsync(ledgerRequest);
        using var ledgerDocument = JsonDocument.Parse(await ledgerResponse.Content.ReadAsStringAsync());
        var failed = Assert.Single(
            ledgerDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            entry => entry.GetProperty("title").GetString() == "Receipt for November");
        Assert.Contains("suppressed", failed.GetProperty("failureReason").GetString());
    }

    /// <summary>Staff can list, add and remove e-mail suppressions.</summary>
    [Fact]
    public async Task Suppressions_CanBeListedAddedAndRemoved()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var listRequest = StaffRequest(HttpMethod.Get, "/api/v1/notifications/suppressions");
        var listResponse = await client.SendAsync(listRequest);
        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.True(listDocument.RootElement.GetProperty("data").GetArrayLength() >= 2);

        using var addRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications/suppressions");
        addRequest.Content = JsonContent.Create(new
        {
            email = "New@Kinevor.Demo",
            reason = "complaint",
            detail = "Spam report.",
        });
        var addResponse = await client.SendAsync(addRequest);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        using var addDocument = JsonDocument.Parse(await addResponse.Content.ReadAsStringAsync());
        Assert.Equal("new@kinevor.demo", addDocument.RootElement.GetProperty("data").GetProperty("email").GetString());

        using var deleteRequest = StaffRequest(
            HttpMethod.Delete,
            $"/api/v1/notifications/suppressions?email={Uri.EscapeDataString("NEW@kinevor.demo")}");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(deleteRequest)).StatusCode);

        using var searchRequest = StaffRequest(
            HttpMethod.Get,
            $"/api/v1/notifications/suppressions?search={Uri.EscapeDataString("new@kinevor.demo")}");
        var searchResponse = await client.SendAsync(searchRequest);
        using var searchDocument = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync());
        Assert.Empty(searchDocument.RootElement.GetProperty("data").EnumerateArray());
    }

    /// <summary>Templates and categories expose staff defaults.</summary>
    [Fact]
    public async Task TemplatesAndCategories_AreSeeded()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var templatesRequest = StaffRequest(HttpMethod.Get, "/api/v1/notifications/templates");
        var templatesResponse = await client.SendAsync(templatesRequest);
        using var templatesDocument = JsonDocument.Parse(await templatesResponse.Content.ReadAsStringAsync());
        var templates = templatesDocument.RootElement.GetProperty("data");
        Assert.True(templates.GetArrayLength() >= 5);
        Assert.Contains(templates.EnumerateArray(),
            template => template.GetProperty("key").GetString() == "offer_announcement");

        using var categoriesRequest = StaffRequest(HttpMethod.Get, "/api/v1/notifications/categories");
        var categoriesResponse = await client.SendAsync(categoriesRequest);
        using var categoriesDocument = JsonDocument.Parse(await categoriesResponse.Content.ReadAsStringAsync());
        Assert.Equal(4, categoriesDocument.RootElement.GetProperty("data").GetArrayLength());
    }

    /// <summary>Scheduled notifications wait for a later dispatch run.</summary>
    [Fact]
    public async Task ScheduledCompose_WaitsForDispatch()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var composeRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications");
        composeRequest.Content = JsonContent.Create(new
        {
            memberId = DemoDataSeeder.PrimaryMemberId,
            title = "Scheduled reminder",
            body = "This message is queued for later.",
            category = "offers",
            channel = "InApp",
            scheduledAt = DateTime.UtcNow.AddHours(2),
        });
        var composeResponse = await client.SendAsync(composeRequest);
        Assert.Equal(HttpStatusCode.Created, composeResponse.StatusCode);
        using var composeDocument = JsonDocument.Parse(await composeResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, composeDocument.RootElement.GetProperty("data").GetProperty("scheduled").GetInt32());

        using var dispatchRequest = StaffRequest(HttpMethod.Post, "/api/v1/notifications/dispatch");
        var dispatchResponse = await client.SendAsync(dispatchRequest);
        Assert.Equal(HttpStatusCode.OK, dispatchResponse.StatusCode);
        using var dispatchDocument = JsonDocument.Parse(await dispatchResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, dispatchDocument.RootElement.GetProperty("data").GetProperty("processed").GetInt32());
    }

    /// <summary>Member and staff policies protect the endpoints.</summary>
    [Fact]
    public async Task Policies_SeparateMemberAndStaffAccess()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        using var memberOnStaff = MemberRequest(HttpMethod.Get, "/api/v1/notifications");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(memberOnStaff)).StatusCode);

        using var staffWithoutMember = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me/notifications");
        staffWithoutMember.Headers.Add("X-Demo-Role", "Staff");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(staffWithoutMember)).StatusCode);

        using var staffOnStaff = StaffRequest(HttpMethod.Get, "/api/v1/notifications");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(staffOnStaff)).StatusCode);
    }

    private static JsonElement FindCategory(JsonElement categories, string category) =>
        Assert.Single(categories.EnumerateArray(),
            item => item.GetProperty("category").GetString() == category);

    private static HttpRequestMessage MemberRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Demo-Role", "Member");
        request.Headers.Add("X-Demo-Member", DemoDataSeeder.PrimaryMemberId.ToString());
        return request;
    }

    private static HttpRequestMessage StaffRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Demo-Role", "Staff");
        return request;
    }
}
