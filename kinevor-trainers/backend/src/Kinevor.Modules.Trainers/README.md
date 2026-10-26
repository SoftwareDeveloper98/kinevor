# Kinevor.Modules.Trainers

Trainer directory module for the Kinevor gym platform.

- `Trainer` aggregate with name, contact, specializations, certifications, rating and status.
- MediatR commands and queries for creating and reading trainers.
- Minimal-API-compatible controller with the shared Kinevor authorization policies.
- Depends on the `Kinevor.Platform.*` packages for the API envelope, current-user and branch context.

## Install

```powershell
dotnet add package Kinevor.Modules.Trainers --version 0.1.0
```

## Register

```csharp
builder.Services.AddTrainersModule();
builder.Services.AddTrainersEntityFrameworkCore();
```

Map the controllers from `Kinevor.Modules.Trainers.Api` and configure the platform services
(`ICurrentUser`, `IBranchContext`) for the host.

See the repository README for the full demo and API documentation.
