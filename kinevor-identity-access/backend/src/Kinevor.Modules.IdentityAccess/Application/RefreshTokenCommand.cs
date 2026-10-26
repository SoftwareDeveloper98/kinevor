using Kinevor.Modules.IdentityAccess.Domain;
using Kinevor.Modules.IdentityAccess.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>Exchanges a refresh token for a new pair and revokes the old token.</summary>
/// <param name="RefreshToken">Opaque refresh token.</param>
public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthSessionDto>;

/// <summary>Handles <see cref="RefreshTokenCommand"/>.</summary>
public class RefreshTokenHandler(
    IUserAccountRepository users,
    IRefreshTokenRepository refreshTokens,
    IAuthTokenService tokens,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshTokenCommand, AuthSessionDto>
{
    /// <summary>Rotates the refresh token.</summary>
    public async Task<AuthSessionDto> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var hash = tokens.HashRefreshToken(command.RefreshToken);
        var stored = await refreshTokens.GetByHashAsync(hash, ct)
            ?? throw new AuthenticationFailedException("Invalid refresh token.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!stored.IsActive(now))
            throw new AuthenticationFailedException("Refresh token is expired or revoked.");

        var account = await users.GetByIdAsync(stored.UserId, ct)
            ?? throw new AuthenticationFailedException("Account no longer exists.");
        if (!account.IsActive)
            throw new AuthenticationFailedException("Account is disabled.");

        stored.Revoke(now);
        var session = AuthSessionFactory.Issue(account, tokens, now);
        refreshTokens.Add(RefreshToken.Create(account.Id, tokens.HashRefreshToken(session.RefreshToken), now));
        await unitOfWork.SaveChangesAsync(ct);
        return session;
    }
}
