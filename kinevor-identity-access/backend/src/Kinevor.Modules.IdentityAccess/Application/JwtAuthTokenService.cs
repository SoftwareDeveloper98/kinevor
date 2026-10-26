using System.Security.Cryptography;
using System.Text;
using Kinevor.Modules.IdentityAccess.Domain;
using Kinevor.Platform.Domain.ValueObjects;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>Issues HS256 JWTs and SHA-256-hashed opaque refresh tokens.</summary>
public sealed class JwtAuthTokenService : IAuthTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _key;
    private readonly TimeSpan _accessLifetime;

    /// <summary>Reads JWT settings from configuration.</summary>
    /// <param name="configuration">Host configuration.</param>
    public JwtAuthTokenService(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _issuer = configuration["Jwt:Issuer"] ?? "kinevor.identity";
        _audience = configuration["Jwt:Audience"] ?? "kinevor.api";
        var key = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey must be configured; there is no demo fallback.");
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters long.");
        _key = Encoding.UTF8.GetBytes(key);
        _accessLifetime = TimeSpan.FromMinutes(
            int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 15);
    }

    /// <summary>Creates a signed JWT carrying identity and role claims.</summary>
    public AccessToken CreateAccessToken(UserAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var now = DateTime.UtcNow;
        var expires = now.Add(_accessLifetime);
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, account.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, account.DisplayName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, account.Email.Value),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, account.Role),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(_key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            _issuer, _audience, claims, notBefore: now, expires: expires, signingCredentials: credentials);
        return new AccessToken(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    /// <summary>Creates a 64-byte URL-safe opaque token.</summary>
    public string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    /// <summary>Returns the Base64 SHA-256 hash of a refresh token.</summary>
    public string HashRefreshToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
