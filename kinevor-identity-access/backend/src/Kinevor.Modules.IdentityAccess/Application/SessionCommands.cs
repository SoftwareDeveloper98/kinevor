using Kinevor.Modules.IdentityAccess.Application.Exceptions;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Common;
using MediatR;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>Returns the signed-in account.</summary>
public record GetSessionQuery : IRequest<UserAccountDto>;

/// <summary>Handles <see cref="GetSessionQuery"/>.</summary>
public class GetSessionHandler(IUserAccountRepository users, ICurrentUser currentUser)
    : IRequestHandler<GetSessionQuery, UserAccountDto>
{
    /// <summary>Loads the current account from the identity claim.</summary>
    public async Task<UserAccountDto> Handle(GetSessionQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (currentUser.UserId is null)
            throw new AuthenticationFailedException("No authenticated account.");

        var account = await users.GetByIdAsync(currentUser.UserId.Value, ct)
            ?? throw new NotFoundException("UserAccount", currentUser.UserId.Value);
        return UserAccountDto.From(account);
    }
}

/// <summary>Revokes a refresh token.</summary>
/// <param name="RefreshToken">Opaque refresh token.</param>
public record LogoutCommand(string? RefreshToken) : IRequest<Result>;

/// <summary>Handles <see cref="LogoutCommand"/>.</summary>
public class LogoutHandler(
    IRefreshTokenRepository refreshTokens,
    IAuthTokenService tokens,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <summary>Revokes the token when it exists, remaining idempotent otherwise.</summary>
    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            return Result.Success();

        var stored = await refreshTokens.GetByHashAsync(tokens.HashRefreshToken(command.RefreshToken), ct);
        if (stored is null)
            return Result.Success();

        stored.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Lists accounts for administrators.</summary>
public record GetAccountsQuery : IRequest<IReadOnlyList<UserAccountDto>>;

/// <summary>Handles <see cref="GetAccountsQuery"/>.</summary>
public class GetAccountsHandler(IUserAccountRepository users)
    : IRequestHandler<GetAccountsQuery, IReadOnlyList<UserAccountDto>>
{
    /// <summary>Returns every account ordered by display name.</summary>
    public async Task<IReadOnlyList<UserAccountDto>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var accounts = await users.ListAsync(ct);
        return accounts.Select(UserAccountDto.From).ToList();
    }
}
