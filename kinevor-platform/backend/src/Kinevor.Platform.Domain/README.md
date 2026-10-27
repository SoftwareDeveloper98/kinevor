# Kinevor.Platform.Domain

Shared domain kernel for Kinevor feature modules.

- `Entity`, `AggregateRoot` and `ValueObject` base types.
- `Result<T>` and `Error` for explicit outcomes.
- Domain events and specifications.
- Common value objects for e-mail and phone numbers.
- Shared enums for classes and trainer status.

## Install

```bash
dotnet add package Kinevor.Platform.Domain --version 0.1.0
```

Outside the monorepo, consumers need the GitHub Packages NuGet source configured for the
`SoftwareDeveloper98` account.
