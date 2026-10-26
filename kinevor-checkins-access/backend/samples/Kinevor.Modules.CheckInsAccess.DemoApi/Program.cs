using Kinevor.Modules.CheckInsAccess.Application;
using Kinevor.Modules.CheckInsAccess.DemoApi.Authentication;
using Kinevor.Modules.CheckInsAccess.DemoApi.Authorization;
using Kinevor.Modules.CheckInsAccess.DemoApi.Data;
using Kinevor.Modules.CheckInsAccess.DemoApi.Middleware;
using Kinevor.Modules.CheckInsAccess.DependencyInjection;
using Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CheckInsAccess")
    ?? "Data Source=checkins-access-demo.db";

builder.Services.AddDbContext<CheckInsDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddCheckInsAccessModule();
builder.Services.AddCheckInsAccessEntityFrameworkCore();
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
    .AddApplicationPart(typeof(Kinevor.Modules.CheckInsAccess.Api.CheckInsController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kinevor Check-ins & Access Module API",
        Version = "v1",
        Description = "Standalone access eligibility, check-in, occupancy, visit history and audit host.",
    });
    options.AddSecurityDefinition("DemoRole", new OpenApiSecurityScheme
    {
        Name = "X-Demo-Role",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Demo role: Admin, Staff, Trainer or Member.",
    });
    options.AddSecurityDefinition("DemoBranch", new OpenApiSecurityScheme
    {
        Name = "X-Branch-Id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Optional branch context: aaaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa (Shoreditch) or bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb (Camden).",
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
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "DemoBranch",
            },
        }] = Array.Empty<string>(),
    });
});

builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins("http://localhost:4205")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CheckInsDbContext>();
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
