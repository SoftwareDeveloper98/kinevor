# Kinevor.Modules.IdentityAccess.EntityFrameworkCore

EF Core persistence for the Kinevor identity and access module.

## Install

```powershell
dotnet add package Kinevor.Modules.IdentityAccess.EntityFrameworkCore --version 0.1.0
```

## What it provides

- `IdentityDbContext` with `UserAccounts` and `RefreshTokens`.
- Unique e-mail and refresh-token-hash indexes.
- `IUserAccountRepository`, `IRefreshTokenRepository` and the module unit of work.

Register the context with your provider, then call `AddIdentityEntityFrameworkCore()`.
