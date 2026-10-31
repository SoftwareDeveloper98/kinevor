# Kinevor Trainers Backend

.NET 10 trainer directory module, EF Core persistence, demo API and tests.

## Projects

| Project | Purpose |
| --- | --- |
| `src/Kinevor.Modules.Trainers` | Domain aggregate, application handlers, HTTP controller |
| `src/Kinevor.Modules.Trainers.EntityFrameworkCore` | `TrainersDbContext`, mapping, repository |
| `samples/Kinevor.Modules.Trainers.DemoApi` | SQLite demo host with seed data and Swagger |
| `tests/Kinevor.Modules.Trainers.Tests` | Domain, application, repository and API tests |

The module consumes the published `Kinevor.Platform.*` packages. `nuget.config`
also points at `..\..\kinevor-platform\backend\artifacts\packages` for local
development.

## Build

From the repository root:

```powershell
dotnet pack ..\kinevor-platform\backend\Kinevor.Platform.slnx -c Release `
  -o ..\kinevor-platform\backend\artifacts\packages

dotnet restore backend\Kinevor.Modules.Trainers.slnx
dotnet build backend\Kinevor.Modules.Trainers.slnx -c Release
dotnet test backend\Kinevor.Modules.Trainers.slnx -c Release
```

Pack this module:

```powershell
dotnet pack backend\src\Kinevor.Modules.Trainers\Kinevor.Modules.Trainers.csproj `
  -c Release -o backend\artifacts\packages
dotnet pack backend\src\Kinevor.Modules.Trainers.EntityFrameworkCore\Kinevor.Modules.Trainers.EntityFrameworkCore.csproj `
  -c Release -o backend\artifacts\packages
```

## Run the demo

```powershell
dotnet run --project backend\samples\Kinevor.Modules.Trainers.DemoApi
```

- API: `http://localhost:5081/api/v1/trainers`
- Swagger: `http://localhost:5081/swagger`
- Health: `http://localhost:5081/health`

The demo uses SQLite and seeds six trainers. Authentication is a development-only
header adapter: `X-Demo-Role`, `X-Demo-Email`, `X-Demo-Trainer`, `X-Branch-Id`.
Never enable it in production.

## Registration

```csharp
builder.Services.AddTrainersModule();
builder.Services.AddTrainersEntityFrameworkCore();

builder.Services.AddDbContext<TrainersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Trainers")));
```

The API controller uses the platform `Policies.StaffOrAdmin` and
`Policies.TrainerOrAbove` policies. Configure `ICurrentUser` and `IBranchContext`
in the host.
