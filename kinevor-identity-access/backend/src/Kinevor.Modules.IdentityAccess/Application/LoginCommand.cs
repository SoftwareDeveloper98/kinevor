using Kinevor.Modules.IdentityAccess.Domain;
using Kinevor.Modules.IdentityAccess.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>Authenticates an account and issues a token pair.</summary>
/// <param name="Email">Login e-mail.</param>
/// <param name="Password">Plaintext password.</param>
public record LoginCommand(string Email, string Password) : IRequest<AuthSessionDto>;

/// <summary>Handles <see cref="LoginCommand"/>.</summary>
public class LoginHandler(
    IUserAccountRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IAuthTokenService tokens,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LoginCommand, AuthSessionDto>
{
    /// <summary>Verifies credentials and persists a refresh token.</summary>
    public async Task<AuthSessionDto> Handle(LoginCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await users.GetByEmailAsync(command.Email, ct);

        // Always run the hasher so a missing account costs the same as a wrong password,
        // preventing e-mail enumeration via timing differences.
        _ = passwordHasher.Hash(account is null ? command.Password : "kinevor-timing-dummy");

        if (account is null || !account.IsActive || !passwordHasher.Verify(command.Password, account.PasswordHash))
            throw new AuthenticationFailedException("Invalid e-mail or password.");

        if (passwordHasher.NeedsRehash(account.PasswordHash))
            account.UpdatePasswordHash(passwordHasher.Hash(command.Password));

        var now = timeProvider.GetUtcNow().UtcDateTime;
        account.RecordLogin(now);
        users.Update(account);

        var session = AuthSessionFactory.Issue(account, tokens, now);
        refreshTokens.Add(RefreshToken.Create(account.Id, tokens.HashRefreshToken(session.RefreshToken), now));
        await unitOfWork.SaveChangesAsync(ct);
        return session;
    }
}

/// <summary>Builds token pairs shared by login and refresh.</summary>
internal static class AuthSessionFactory
{
    internal static AuthSessionDto Issue(UserAccount account, IAuthTokenService tokens, DateTime now)
    {
        var access = tokens.CreateAccessToken(account);
        var refresh = tokens.CreateRefreshToken();
        return new AuthSessionDto(access.Value, refresh, access.ExpiresAt, UserAccountDto.From(account));
    }
}
