# Kinevor.Modules.Trainers.EntityFrameworkCore

EF Core persistence for `Kinevor.Modules.Trainers`.

- `TrainersDbContext` with the trainer aggregate mapping.
- `TrainerRepository` implementing the module repository contract.
- `AddTrainersEntityFrameworkCore()` registration helper.

## Install

```powershell
dotnet add package Kinevor.Modules.Trainers.EntityFrameworkCore --version 0.1.0
```

Configure a provider in the host, for example:

```csharp
builder.Services.AddDbContext<TrainersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Trainers")));
```

The demo host uses SQLite and calls `EnsureCreatedAsync`; production hosts should use migrations.
