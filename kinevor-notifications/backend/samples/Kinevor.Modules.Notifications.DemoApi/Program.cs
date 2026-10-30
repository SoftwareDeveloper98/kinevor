using Kinevor.Modules.Notifications.Application;
using Kinevor.Modules.Notifications.DemoApi.Authentication;
using Kinevor.Modules.Notifications.DemoApi.Authorization;
using Kinevor.Modules.Notifications.DemoApi.Data;
using Kinevor.Modules.Notifications.DemoApi.Middleware;
using Kinevor.Modules.Notifications.DependencyInjection;
using Kinevor.Modules.Notifications.EntityFrameworkCore;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Notifications")
    ?? "Data Source=notifications-demo.db";

builder.Services.AddDbContext<NotificationsDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddNotificationsModule();
builder.Services.AddNotificationsEntityFrameworkCore();
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
    .AddApplicationPart(typeof(Kinevor.Modules.Notifications.Api.NotificationsController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kinevor Notifications Module API",
        Version = "v1",
        Description = "Standalone member inbox, preferences, devices, staff composition and simulated delivery host.",
    });
    options.AddSecurityDefinition("DemoRole", new OpenApiSecurityScheme
    {
        Name = "X-Demo-Role",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Demo role: Admin, Staff, Trainer or Member.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "DemoRole",
            },
        }] = Array.Empty<string>(),
    });
});

builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins(
            "http://localhost:4200",
            "http://localhost:4201",
            "http://localhost:4202",
            "http://localhost:4203")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    throw new InvalidOperationException("The Kinevor demo API only runs in Development.");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DemoDataSeeder.SeedAsync(db);
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
