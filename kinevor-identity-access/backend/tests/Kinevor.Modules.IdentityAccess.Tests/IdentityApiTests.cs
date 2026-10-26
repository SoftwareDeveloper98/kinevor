using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kinevor.Modules.IdentityAccess.Tests;

/// <summary>HTTP contract for the identity and access module.</summary>
public class IdentityApiTests
{
    /// <summary>Valid credentials return a JWT and a refresh token.</summary>
    [Fact]
    public async Task Login_ReturnsTokenPair()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var session = await LoginAsync(client, "admin@kinevor.demo", "Kinevor!2026");

        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
        Assert.Equal("Admin", session.Role);
        Assert.True(session.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>Wrong credentials return 401 with the shared failure envelope.</summary>
    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "member@kinevor.demo",
            password = "not-the-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
    }

    /// <summary>The session endpoint requires a bearer token and returns the caller.</summary>
    [Fact]
    public async Task Session_RequiresBearerTokenAndReturnsAccount()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/auth/session");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var session = await LoginAsync(client, "trainer@kinevor.demo", "Kinevor!2026");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await client.GetAsync("/api/v1/auth/session");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var account = document.RootElement.GetProperty("data");
        Assert.Equal("trainer@kinevor.demo", account.GetProperty("email").GetString());
        Assert.Equal("Trainer", account.GetProperty("role").GetString());
    }

    /// <summary>Refreshing rotates the token: the old token is rejected.</summary>
    [Fact]
    public async Task Refresh_RotatesRefreshToken()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var session = await LoginAsync(client, "staff@kinevor.demo", "Kinevor!2026");
        var refreshed = await RefreshAsync(client, session.RefreshToken);

        var replay = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = session.RefreshToken,
        });

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.NotEqual(session.RefreshToken, refreshed.RefreshToken);
    }

    /// <summary>Logout revokes the refresh token.</summary>
    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var session = await LoginAsync(client, "member@kinevor.demo", "Kinevor!2026");
        var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new
        {
            refreshToken = session.RefreshToken,
        });
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = session.RefreshToken,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    /// <summary>Only administrators can list accounts.</summary>
    [Fact]
    public async Task Accounts_RequiresAdminRole()
    {
        using var factory = new TestWebApplicationFactory();
        using var memberClient = factory.CreateClient();
        var member = await LoginAsync(memberClient, "member@kinevor.demo", "Kinevor!2026");
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", member.AccessToken);

        var forbidden = await memberClient.GetAsync("/api/v1/auth/accounts");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var adminClient = factory.CreateClient();
        var admin = await LoginAsync(adminClient, "admin@kinevor.demo", "Kinevor!2026");
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

        var response = await adminClient.GetAsync("/api/v1/auth/accounts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(4, document.RootElement.GetProperty("data").GetArrayLength());
    }

    private static async Task<Session> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        return new Session(
            data.GetProperty("accessToken").GetString()!,
            data.GetProperty("refreshToken").GetString()!,
            data.GetProperty("expiresAt").GetDateTime(),
            data.GetProperty("user").GetProperty("role").GetString()!);
    }

    private static async Task<Session> RefreshAsync(HttpClient client, string refreshToken)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        return new Session(
            data.GetProperty("accessToken").GetString()!,
            data.GetProperty("refreshToken").GetString()!,
            data.GetProperty("expiresAt").GetDateTime(),
            data.GetProperty("user").GetProperty("role").GetString()!);
    }

    private sealed record Session(string AccessToken, string RefreshToken, DateTime ExpiresAt, string Role);
}
