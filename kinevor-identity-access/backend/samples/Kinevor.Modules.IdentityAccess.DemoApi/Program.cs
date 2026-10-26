using System.Security.Claims;
using System.Text;
using Kinevor.Modules.IdentityAccess.Api;
using Kinevor.Modules.IdentityAccess.DemoApi.Authorization;
using Kinevor.Modules.IdentityAccess.DemoApi.Data;
using Kinevor.Modules.IdentityAccess.DemoApi.Middleware;
using Kinevor.Modules.IdentityAccess.DependencyInjection;
using Kinevor.Modules.IdentityAccess.EntityFrameworkCore;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Identity")
    ?? "Data Source=identity-demo.db";

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddIdentityAccessModule();
builder.Services.AddIdentityEntityFrameworkCore();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

var signingKey = builder.Configuration["Jwt:SigningKey"]
    ?? "kinevor-demo-signing-key-change-in-production-2026";
if (signingKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters long.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "kinevor.identity",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "kinevor.api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization(options => options.AddKinevorPolicies());

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AuthController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kinevor Identity & Access Module API",
        Version = "v1",
        Description = "Standalone authentication host with JWT access tokens and rotating refresh tokens.",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken returned by POST /api/v1/auth/login.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            },
        }] = Array.Empty<string>(),
    });
});

builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins("http://localhost:4201")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<Kinevor.Modules.IdentityAccess.Application.IPasswordHasher>();
    await db.Database.EnsureCreatedAsync();
    if (app.Environment.IsDevelopment())
        await DemoDataSeeder.SeedAsync(db, hasher);
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
