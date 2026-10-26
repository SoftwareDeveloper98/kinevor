# Kinevor.Platform.Application

Shared application contracts for Kinevor feature modules.

- Read, write and aggregate repository interfaces.
- Unit-of-work contract.
- `ICurrentUser` and `IBranchContext` abstractions.
- `PagedResult<T>` for list queries.
- Domain, not-found, conflict and forbidden exceptions.

## Install

```bash
dotnet add package Kinevor.Platform.Application --version 0.1.0
```

The package references `Kinevor.Platform.Domain` and MediatR 12.4.1.
