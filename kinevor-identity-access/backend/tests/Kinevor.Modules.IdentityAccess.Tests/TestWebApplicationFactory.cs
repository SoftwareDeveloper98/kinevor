using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Kinevor.Modules.IdentityAccess.Tests;

/// <summary>Hosts the demo API against a unique temporary SQLite database.</summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"kinevor-identity-{Guid.NewGuid():N}.db");

    /// <summary>Points the host at the temporary database and a deterministic signing key.</summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={_databasePath}");
        builder.UseSetting("Jwt:Issuer", "kinevor.identity.tests");
        builder.UseSetting("Jwt:Audience", "kinevor.api.tests");
        builder.UseSetting("Jwt:SigningKey", "kinevor-tests-signing-key-32-characters-minimum");
        builder.UseEnvironment("Development");
    }

    /// <summary>Clears pooled connections before deleting the temporary database.</summary>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();

        try
        {
            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
        }
        catch (IOException)
        {
            // A finalizer still holding the file must not fail the test run.
        }
    }
}
