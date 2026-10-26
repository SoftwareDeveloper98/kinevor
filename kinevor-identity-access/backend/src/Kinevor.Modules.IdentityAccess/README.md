# Kinevor.Modules.IdentityAccess

Identity and access module for the Kinevor gym platform.

## Install

```powershell
dotnet add package Kinevor.Modules.IdentityAccess --version 0.1.0
```

## What it provides

- `UserAccount` and `RefreshToken` aggregates.
- PBKDF2 password hashing and HS256 JWT access tokens.
- Single-use refresh tokens with rotation and revocation.
- MediatR login, refresh, logout, session and account-list handlers.
- `AuthController` endpoints under `api/v1/auth`.

Register the module with `AddIdentityAccessModule()` and load `Jwt:Issuer`, `Jwt:Audience`
and a signing key of at least 32 characters from configuration.
