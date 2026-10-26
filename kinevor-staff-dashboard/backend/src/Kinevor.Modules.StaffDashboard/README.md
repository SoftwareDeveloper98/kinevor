# Kinevor.Modules.StaffDashboard

Application and API layer for the Kinevor staff dashboard. It exposes branch
KPIs, 7/30-day trends, a recent activity feed and operational alerts.

## Registration

```csharp
builder.Services.AddStaffDashboardModule();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(StaffDashboardController).Assembly);
```

The host supplies `IBranchContext`. The default range is seven days and the
accepted ranges are 7 and 30 days. Endpoints are protected by
`Policies.StaffOrAdmin`.

## Endpoints

- `GET /api/v1/staff-dashboard?branchId={guid}&days=7`
- `GET /api/v1/staff-dashboard/alerts?branchId={guid}`
- `GET /api/v1/staff-dashboard/branches`
