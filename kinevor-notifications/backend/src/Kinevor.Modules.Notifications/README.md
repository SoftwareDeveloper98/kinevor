# Kinevor.Modules.Notifications

Notification module for Kinevor: member inbox, delivery records, templates,
preferences, device tokens and e-mail suppression. The module owns its records
and simulates delivery without calling any external e-mail or push provider.

## Install

```powershell
dotnet add package Kinevor.Modules.Notifications --version 0.1.0
dotnet add package Kinevor.Modules.Notifications.EntityFrameworkCore --version 0.1.0
```

The package depends on `Kinevor.Platform.Api`, `Kinevor.Platform.Application` and
`Kinevor.Platform.Domain` `0.1.0`.

## Register

```csharp
builder.Services.AddNotificationsModule();
builder.Services.AddNotificationsEntityFrameworkCore();

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Notifications")));

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(NotificationsController).Assembly);
```

Configure the platform `ICurrentUser` and `IBranchContext` implementations and
register the Kinevor authorization policies (`MemberOrAbove`, `StaffOrAdmin`).
The EF Core package supplies repositories and the module unit of work.

## What it includes

- `Notification` lifecycle from scheduled or sent through read or failed.
- Category and channel rules for e-mail, push and in-app delivery.
- Preferences with defaults for `class_reminders`, `membership_expiry`, `offers`
  and `receipts`.
- Templates with placeholder rendering and default category/channel values.
- Masked device-token projections and per-member device ownership.
- E-mail suppressions that fail delivery with a recorded reason.
- Member inbox/detail/read routes, preference and device routes.
- Staff ledger, compose, broadcast, dispatch, template, category and suppression
  routes.
- A deterministic `INotificationDeliverySimulator`; replace it with a real
  provider adapter in production.

## Delivery

Scheduled notifications remain queued until their due time and are processed by
the staff `POST /api/v1/notifications/dispatch` endpoint. Immediate notifications
are delivered during compose or broadcast. A delivery attempt records either a
sent timestamp or a failure reason, and only delivered notifications can be
marked read.

## License

MIT
