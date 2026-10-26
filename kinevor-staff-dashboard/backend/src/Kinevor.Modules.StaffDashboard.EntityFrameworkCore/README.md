# Kinevor.Modules.StaffDashboard.EntityFrameworkCore

EF Core persistence for the Kinevor staff dashboard. Register the context in the
host, then call `AddStaffDashboardEntityFrameworkCore()`.

```csharp
builder.Services.AddDbContext<DashboardDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddStaffDashboardEntityFrameworkCore();
```

The context stores one dashboard snapshot per branch with child tables for daily
trend points, activity entries and alerts. Call `Database.EnsureCreatedAsync()`
or provide migrations in the host before serving requests.
