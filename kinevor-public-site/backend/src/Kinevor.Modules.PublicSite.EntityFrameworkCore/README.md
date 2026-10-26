# Kinevor.Modules.PublicSite.EntityFrameworkCore

EF Core persistence for `Kinevor.Modules.PublicSite`.

`PublicSiteDbContext` maps the site profile, branches, plans, trainers, classes,
FAQs, articles, trials and enquiries. JSON-backed list properties are stored
with value converters and the context stamps entity audit timestamps on save.

## Registration

```csharp
builder.Services.AddDbContext<PublicSiteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PublicSite")));
builder.Services.AddPublicSiteEntityFrameworkCore();
```

The extension registers `IPublicSiteRepository` plus the module unit of work.
