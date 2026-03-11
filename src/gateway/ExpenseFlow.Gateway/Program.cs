using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using ExpenseFlow.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. YARP Reverse Proxy ────────────────────────────────────────────────────
// Reads all route and cluster config from appsettings.json "ReverseProxy" section.
// No code changes needed when adding new routes — just update config.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── 2. JWT Authentication ────────────────────────────────────────────────────
// Gateway validates the token once. Downstream services trust the forwarded header.
// All services share the same signing key so tokens are valid everywhere.
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey  = jwtSection["SecretKey"]  ?? throw new InvalidOperationException("JwtSettings:SecretKey missing.");
var issuer     = jwtSection["Issuer"]     ?? throw new InvalidOperationException("JwtSettings:Issuer missing.");
var audience   = jwtSection["Audience"]   ?? throw new InvalidOperationException("JwtSettings:Audience missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── 3. Rate Limiting ─────────────────────────────────────────────────────────
// Two policies:
//   "fixed"  — 100 requests / 60 s per IP  (general API calls)
//   "auth"   — 10  requests / 60 s per IP  (login / register — brute-force protection)
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opts.AddFixedWindowLimiter("fixed", o =>
    {
        o.Window            = TimeSpan.FromSeconds(60);
        o.PermitLimit       = 100;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit        = 0;
    });

    opts.AddFixedWindowLimiter("auth", o =>
    {
        o.Window            = TimeSpan.FromSeconds(60);
        o.PermitLimit       = 10;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit        = 0;
    });
});

// ── 4. CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? ["http://localhost:3000"])
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GatewayLoggingMiddleware>();  // request/response logging
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// YARP handles all proxying — no MapControllers needed
app.MapReverseProxy();

app.Run();
