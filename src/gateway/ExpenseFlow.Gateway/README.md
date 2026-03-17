# API Gateway (YARP)

Single external entry point for all client traffic. Built on **YARP** (Yet Another Reverse Proxy) — Microsoft's production-grade reverse proxy library.

## Architectural Decision: Why a Gateway?

Without a gateway, the React frontend would need to know the addresses of both services and manage two base URLs. CORS would need configuring on every service. Rate limiting would be duplicated. JWT validation would run twice per request.

The gateway solves all of this in one place:
- **Single origin** for the frontend (`localhost:5000`)
- **JWT validation once** at the edge; downstream services trust forwarded requests
- **Rate limiting** centralised; auth endpoints get a stricter policy
- **CORS** handled once; services have no CORS config
- **Health-based routing** via YARP active health checks

## Why YARP over Nginx/Ocelot?

| | YARP | Nginx | Ocelot |
|---|---|---|---|
| Language | C# / ASP.NET | C | C# |
| Config | `appsettings.json` | nginx.conf | `ocelot.json` |
| Middleware | Full ASP.NET pipeline | Limited | Limited |
| Active health checks | Built-in | Manual | Basic |
| .NET integration | Native | None | Native |

YARP runs inside the same .NET process — you can use the full ASP.NET middleware pipeline (auth, rate limiting, custom logging) before proxying. No separate process, no nginx config syntax.

## Route Map

```
External                         Internal
/identity/auth/** (POST)   ──►  identity-service:8080/api/auth/**   [rate: 10/min]
/identity/**               ──►  identity-service:8080/api/**        [rate: 100/min]
/expenses/**               ──►  expense-service:8080/api/expenses/** [rate: 100/min]
```

`PathPattern` transforms strip the service prefix and prepend `/api/` — external URLs are clean, internal URLs follow each service's convention.

## Rate Limiting

Two ASP.NET 8 fixed-window policies:

- `"fixed"` — 100 requests / 60s per IP (general API calls)
- `"auth"` — 10 requests / 60s per IP (login/register — brute-force protection)

Rejected requests return `429 Too Many Requests`.

## Correlation IDs (`GatewayLoggingMiddleware`)

Every request gets an `X-Correlation-ID`:
1. If client sends one — it is preserved
2. If not — gateway generates a new UUID

The ID is injected into the forwarded request header so downstream services log it, and returned in the response so clients can reference it. This creates an unbroken correlation chain from browser → gateway → service → database log.

Slow requests (>1000ms) are logged as `Warning` — alerting on this threshold catches performance regressions automatically.

## Active Health Checks

YARP polls `/health` on each downstream service every 10 seconds. After 3 consecutive failures, YARP stops routing to that destination. When health recovers, routing resumes automatically. No manual intervention required.
