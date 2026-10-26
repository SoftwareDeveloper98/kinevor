# Kinevor.Modules.PublicSite

Public-facing website content and lead capture for Kinevor gyms.

The package owns the public site profile, locations, membership plans,
trainers, class timetable, FAQs, blog articles, free-trial requests and website
enquiries. It exposes anonymous read and capture endpoints through
`PublicContentController`, plus a staff/admin lead inbox through
`PublicLeadsController`.

## Registration

```csharp
builder.Services.AddPublicSiteModule();
builder.Services.AddPublicSiteEntityFrameworkCore();
builder.Services.AddDbContext<PublicSiteDbContext>(options =>
    options.UseSqlite(connectionString));
```

Register an `IPublicSiteRepository` implementation through the EF Core package.
All public responses use `Kinevor.Platform.Api.ApiResponse<T>` and the lead inbox
uses the `StaffOrAdmin` platform policy.

## Endpoints

- `GET /api/v1/public/site`
- `GET /api/v1/public/branches`
- `GET /api/v1/public/branches/{slug}`
- `GET /api/v1/public/plans`
- `GET /api/v1/public/trainers`
- `GET /api/v1/public/schedule`
- `GET /api/v1/public/faqs`
- `GET /api/v1/public/posts`
- `GET /api/v1/public/posts/{slug}`
- `POST /api/v1/public/trials`
- `POST /api/v1/public/leads`
- `GET /api/v1/public/leads` (staff/admin)
