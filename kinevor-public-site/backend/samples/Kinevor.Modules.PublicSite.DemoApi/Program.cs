using Kinevor.Modules.PublicSite.DemoApi.Authentication;
using Kinevor.Modules.PublicSite.DemoApi.Authorization;
using Kinevor.Modules.PublicSite.DemoApi.Data;
using Kinevor.Modules.PublicSite.DemoApi.Middleware;
using Kinevor.Modules.PublicSite.DependencyInjection;
using Kinevor.Modules.PublicSite.EntityFrameworkCore;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PublicSite")
    ?? "Data Source=publicsite-demo.db";

builder.Services.AddDbContext<PublicSiteDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddPublicSiteModule();
builder.Services.AddPublicSiteEntityFrameworkCore();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IBranchContext, HttpBranchContext>();

builder.Services
    .AddAuthentication(DemoAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        DemoAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization(options => options.AddKinevorPolicies());

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(Kinevor.Modules.PublicSite.Api.PublicContentController).Assembly)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"{entry.Key} is invalid."
                        : error.ErrorMessage))
                .ToArray();

            return new BadRequestObjectResult(
                ApiResponse.Fail("The request is invalid.", errors));
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kinevor Public Site API",
        Version = "v1",
        Description = "Public website content, timetable and lead capture for Kinevor gyms.",
    });
    options.AddSecurityDefinition("DemoRole", new OpenApiSecurityScheme
    {
        Name = "X-Demo-Role",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Demo role for the staff lead inbox: Admin or Staff.",
    });
    options.OperationFilter<DemoAuthorizeOperationFilter>();
});

builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins("http://localhost:4208", "http://127.0.0.1:4208")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PublicSiteDbContext>();
    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    await db.Database.EnsureCreatedAsync();
    await DemoDataSeeder.SeedAsync(db, timeProvider);
}

app.UseCors("web");
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.Run();

/// <summary>Entry point marker used by integration tests.</summary>
public partial class Program { }

internal sealed class DemoAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requiresAuthorization = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .Any(metadata => metadata is IAuthorizeData);
        if (!requiresAuthorization)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "DemoRole",
                    },
                }] = Array.Empty<string>(),
            },
        ];
    }
}
