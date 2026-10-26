# Kinevor.Platform.Api

Shared ASP.NET Core contracts for Kinevor feature modules.

- `ApiResponse<T>` success and failure envelope.
- Role constants for admin, staff, trainer and member users.
- `AddKinevorPolicies()` authorization policy registration.

## Install

```bash
dotnet add package Kinevor.Platform.Api --version 0.1.0
```

The package references `Kinevor.Platform.Application` and the ASP.NET Core
shared framework.
