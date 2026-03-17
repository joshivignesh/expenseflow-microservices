# Identity Service

Handles all authentication and user identity concerns for the ExpenseFlow platform.

## Responsibility
- User registration with PBKDF2-SHA256 password hashing
- JWT access token issuance (HS256, 15-minute expiry)
- Cryptographic refresh token generation (64-byte CSPRNG)
- User profile retrieval

## Layer Structure

```
ExpenseFlow.Identity/
├── ExpenseFlow.Identity.Domain/          # Core business rules — no external deps
├── ExpenseFlow.Identity.Application/     # CQRS use cases via MediatR
├── ExpenseFlow.Identity.Infrastructure/  # EF Core, JWT, PBKDF2, SQL Server
└── ExpenseFlow.Identity.API/             # ASP.NET Controllers, middleware, Program.cs
```

## Domain Layer

### `User` Aggregate Root
The central entity. Created only via `User.Create()` factory — ensures every user starts with a valid, hashed password and a known role. Never allows direct property mutation from outside.

**State:** `Id`, `FirstName`, `LastName`, `Email` (value object), `PasswordHash`, `Role`, `CreatedAt`

**Domain Events:**
- `UserRegisteredEvent` — raised on creation, could trigger welcome email downstream

### `Email` Value Object
Wraps a string. Enforces format validation via regex on creation. Two `Email` objects with the same address are structurally equal (`sealed record`). Stored as an EF Core owned type (`Email_Value` column) — no join table.

### Exceptions → HTTP status mapping

| Exception | HTTP |
|---|---|
| `DuplicateEmailException` | 409 Conflict |
| `UserNotFoundException` | 404 Not Found |
| `InvalidPasswordException` | 401 Unauthorized |
| `InvalidEmailException` | 400 Bad Request |

## Application Layer

### Commands
| Command | What it does |
|---|---|
| `RegisterUserCommand` | Validates → hashes password → creates User → persists → issues JWT |
| `LoginUserCommand` | Loads user → verifies password (constant-time) → issues JWT |

### Queries
| Query | What it does |
|---|---|
| `GetUserProfileQuery` | Dapper SQL → flat `UserProfileDto` — no EF Core, no change tracking |

### MediatR Pipeline Behaviors (in order)
1. `LoggingBehavior` — logs request/response with timing
2. `ValidationBehavior` — runs FluentValidation; throws on failure (caught by middleware → 400)
3. `TransactionBehavior` — wraps handler in `SaveEntitiesAsync`

## Infrastructure Layer

### Password Hashing (`PasswordHasherService`)
PBKDF2-SHA256, 100,000 iterations, 16-byte random salt, 32-byte hash.
Stored format: `{iterations}.{base64salt}.{base64hash}`
`CryptographicOperations.FixedTimeEquals` prevents timing attacks during verification.

### JWT (`JwtTokenService`)
- Access token: HS256, claims: `sub`, `email`, `name`, `role`, `jti`
- Refresh token: `RandomNumberGenerator.GetBytes(64)` → base64
- Returns `(AccessToken, RefreshToken, ExpiresAt)` tuple

### `IdentityDbContext`
EF Core DbContext implementing `IUnitOfWork`. `SaveEntitiesAsync` persists then dispatches domain events. `ApplyConfigurationsFromAssembly` auto-discovers `UserConfiguration`.

### `DatabaseSeeder`
`IHostedService` — runs on startup. Calls `MigrateAsync()` then seeds `admin@expenseflow.local` with `Admin@12345!` if no admin exists. Uses `IServiceScopeFactory` to resolve scoped `DbContext` from singleton hosted service.

## API Layer

### Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | None | Register new user → 201 Created |
| POST | `/api/auth/login` | None | Login → 200 OK with tokens |
| GET | `/api/auth/profile/{userId}` | JWT | Get user profile → 200 or 404 |

### `ExceptionHandlingMiddleware`
Sits first in pipeline. Catches all unhandled exceptions. Maps to RFC 7807 problem details JSON. Includes `traceId` from `context.TraceIdentifier` for log correlation.

### `Program.cs` startup order
```
Infrastructure DI → MediatR → Behaviors → FluentValidation
→ JWT Auth → Swagger → App build
→ ExceptionMiddleware → HTTPS → Auth → AuthZ → Controllers
```
