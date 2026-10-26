using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kinevor.Modules.Trainers.Tests;

/// <summary>HTTP contract for the trainers module.</summary>
public class TrainersApiTests
{
    /// <summary>Anonymous requests are rejected.</summary>
    [Fact]
    public async Task List_RequiresAuthentication()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/trainers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Staff can list the seeded directory.</summary>
    [Fact]
    public async Task List_ReturnsSeededTrainersForStaff()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Role", "Staff");

        var response = await client.GetAsync("/api/v1/trainers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(6, data.GetProperty("items").GetArrayLength());
        Assert.Equal(6, data.GetProperty("totalCount").GetInt32());
    }

    /// <summary>Staff can create a trainer and read it back.</summary>
    [Fact]
    public async Task Create_ThenGetById_WorksForStaff()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Role", "Staff");

        var createResponse = await client.PostAsJsonAsync("/api/v1/trainers", new
        {
            firstName = "Mona",
            lastName = "Said",
            email = "mona.said@kinevor.demo",
            phone = "+201009876543",
            bio = "Strength and conditioning coach.",
            specializations = new[] { "CrossFit", "HIIT" },
            certifications = new[] { "NASM-CPT" },
            yearsExperience = 3,
            photoUrl = (string?)null,
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var getResponse = await client.GetAsync($"/api/v1/trainers/{id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var fetched = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        var trainer = fetched.RootElement.GetProperty("data");
        Assert.Equal("Mona Said", trainer.GetProperty("fullName").GetString());
        Assert.Equal("Active", trainer.GetProperty("status").GetString());
    }

    /// <summary>A trainer may read their own profile but not another trainer's profile.</summary>
    [Fact]
    public async Task GetById_EnforcesTrainerOwnership()
    {
        using var factory = new TestWebApplicationFactory();
        using var staffClient = factory.CreateClient();
        staffClient.DefaultRequestHeaders.Add("X-Demo-Role", "Staff");

        var listResponse = await staffClient.GetAsync("/api/v1/trainers");
        using var list = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var id = list.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("id").GetGuid();

        using var trainerClient = factory.CreateClient();
        trainerClient.DefaultRequestHeaders.Add("X-Demo-Role", "Trainer");
        trainerClient.DefaultRequestHeaders.Add("X-Demo-Trainer", Guid.NewGuid().ToString());

        var forbidden = await trainerClient.GetAsync($"/api/v1/trainers/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.Add("X-Demo-Role", "Trainer");
        ownerClient.DefaultRequestHeaders.Add("X-Demo-Trainer", id.ToString());

        var ownProfile = await ownerClient.GetAsync($"/api/v1/trainers/{id}");
        Assert.Equal(HttpStatusCode.OK, ownProfile.StatusCode);
    }

    /// <summary>Members cannot create trainers.</summary>
    [Fact]
    public async Task Create_ForbidsMembers()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Role", "Member");

        var response = await client.PostAsJsonAsync("/api/v1/trainers", new
        {
            firstName = "Mona",
            lastName = "Said",
            email = "mona.said@kinevor.demo",
            phone = "+201009876543",
            bio = "Strength coach.",
            specializations = new[] { "CrossFit" },
            certifications = Array.Empty<string>(),
            yearsExperience = 3,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>Invalid input is reported as a 400 with the shared failure envelope.</summary>
    [Fact]
    public async Task Create_RejectsInvalidPayload()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Role", "Staff");

        var response = await client.PostAsJsonAsync("/api/v1/trainers", new
        {
            firstName = "",
            lastName = "Said",
            email = "not-an-email",
            phone = "+201009876543",
            bio = "Strength coach.",
            specializations = new[] { "CrossFit" },
            certifications = Array.Empty<string>(),
            yearsExperience = 3,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
    }

    /// <summary>Search narrows the directory.</summary>
    [Fact]
    public async Task List_FiltersBySearchTerm()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-Role", "Staff");

        var response = await client.GetAsync("/api/v1/trainers?search=layla");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("data").GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal("Layla Hassan", items[0].GetProperty("fullName").GetString());
    }
}
