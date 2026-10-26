# Kinevor.Modules.Notifications.EntityFrameworkCore

EF Core persistence for the Kinevor notifications module.

## Install

```powershell
dotnet add package Kinevor.Modules.Notifications --version 0.1.0
dotnet add package Kinevor.Modules.Notifications.EntityFrameworkCore --version 0.1.0
```

## Register

```csharp
builder.Services.AddNotificationsModule();
builder.Services.AddNotificationsEntityFrameworkCore();

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Notifications")));
```

`AddNotificationsEntityFrameworkCore` registers the notification, preference,
device-token, suppression and template repositories plus
`INotificationsUnitOfWork`. The unit of work is implemented by
`NotificationsDbContext`, so save changes through the module abstraction rather
than a second context.

## Schema

`NotificationsDbContext` maps:

- `Notifications`
- `NotificationPreferences`
- `DeviceTokens`
- `EmailSuppressions`
- `NotificationTemplates`

Create the schema with `EnsureCreated` for demos or a module migration in the
composed application. Configure a relational provider such as Npgsql or SQLite in
the host; this package does not select one.

## License

MIT
