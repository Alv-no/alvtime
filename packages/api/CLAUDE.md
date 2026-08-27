# AlvTime API — Project Guidance

Scoped to `packages/api/`. The monorepo root `CLAUDE.md` still applies (workflow rules, TDD,
plan format, and the `/c/` filesystem build quirk) — this file does not repeat it.

## Solution Layout

`AlvTimeWebApi.sln` — 6 projects, all `net9.0`.

| Project | Purpose |
|---|---|
| `AlvTimeWebApi` | ASP.NET Core web layer: controllers, request/response contracts, auth, CORS, error handling, DI wiring |
| `AlvTime.Business` | Domain logic: services, storage interfaces, DTOs, validators, `Result` type. No EF, no ASP.NET |
| `AlvTime.Persistence` | EF Core: `AlvTime_dbContext`, `DatabaseModels/`, storage implementations, migrations |
| `AlvTime.Common` | Shared configuration bootstrapping (JSON, env vars, user secrets, Azure Key Vault) |
| `AlvTime.MigrationClient` | Console app that applies EF migrations |
| `Tests` | xUnit unit tests |

## The Dependency Rule

```
AlvTimeWebApi ──► AlvTime.Business ◄── AlvTime.Persistence
```

`AlvTime.Business` references **neither** the web nor the persistence project. Storage interfaces
(`ITimeRegistrationStorage`, `IPayoutStorage`, …) are declared in the Business feature folder that
owns them; implementations live in `AlvTime.Persistence/Repositories/` as `*Storage.cs`.

**Never add a project reference from `AlvTime.Business` to `AlvTime.Persistence`.** If Business needs
data, declare an interface in Business and implement it in Persistence.

This is dependency-inverted layering — not classic N-tier, not Clean Architecture, not vertical slices.
Match it; do not restructure toward another architecture without being asked.

## Feature Folders in AlvTime.Business

One folder per domain area (`Payouts/`, `TimeRegistration/`, `Overtime/`, `Customers/`, …), each
holding its service, storage interface, and DTOs together:

```
AlvTime.Business/TimeRegistration/
    TimeRegistrationService.cs      # the service
    ITimeRegistrationStorage.cs     # interface, implemented in Persistence
    CreateTimeEntryDto.cs           # inbound DTO
    TimeEntryResponseDto.cs         # outbound DTO
```

New domain logic goes in the matching feature folder, or a new one. Do not create a shared
`Services/` or `Models/` folder.

## Result Pattern — No Exceptions for Domain Failures

`AlvTime.Business/Results/` defines `Result` and `Result<TValue>` as `readonly struct`s with implicit
conversions from `Error` and `List<Error>`:

```csharp
public record Error(ErrorCodes ErrorCode, string Description);

public enum ErrorCodes
{
    InvalidAction = 1, MissingEntity = 2, EntityAlreadyExists = 3,
    RequestMissingProperty = 4, RequestInvalidProperty = 5, SQLError = 6, AuthorizationError = 7
}
```

Rules:

- Services return `Result<T>` (or `Result`) for anything that can fail for domain reasons.
- Return failures via the implicit conversion — `return new Error(ErrorCodes.MissingEntity, "...")`
  — not by throwing.
- Reserve exceptions for genuinely exceptional conditions; `ErrorHandlingMiddleware` handles those.
- `Result<TValue>` uses `[MemberNotNullWhen(true, nameof(Value))]`, so `Value` is only safe to read
  after checking `IsSuccess` or inside `Match`.

## Controller Conventions

Controllers are the **only** place a `Result` is unwrapped. They terminate the result with `Match`:

```csharp
[Route("api/user")]
[ApiController]
[Authorize]
public class PayoutController : ControllerBase
{
    private readonly PayoutService _payoutService;

    public PayoutController(PayoutService payoutService) => _payoutService = payoutService;

    [HttpGet("Payouts")]
    public async Task<ActionResult<PayoutsResponse>> FetchPaidOvertime()
    {
        var result = await _payoutService.GetRegisteredPayouts();
        return result.Match<ActionResult<PayoutsResponse>>(
            payouts => Ok(new PayoutsResponse { /* explicit mapping */ }),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent utbetalinger feilet med følgende feil")));
    }
}
```

- Controller-based MVC. **Not** minimal APIs — do not introduce `MapGet`/`MapPost` endpoints.
- Constructor-inject the concrete service type (`PayoutService`), not an interface.
- Inbound bodies bind to `Requests/*Request`; responses are `Responses/*Response`.
- Map Business DTO → response **explicitly, by hand**. There is no AutoMapper — do not add one.
- Failure messages passed to `ToValidationProblemDetails` (in `ErrorHandling/ErrorCollectionExtentions.cs`)
  are **Norwegian**, since they surface to users. Follow the existing phrasing style.
- Admin endpoints live under `Controllers/Admin/`.

## Dependency Injection

All registrations go in `ServiceRegistrator.AddAlvtimeServices` (`AlvTimeWebApi/ServiceRegistrator.cs`)
— one central list, not per-feature extension methods.

- Services: `AddScoped<PayoutService>()` — concrete type, no interface.
- Storages: `AddScoped<IPayoutStorage, PayoutStorage>()` — interface to implementation.
- Add new registrations to that method rather than creating a parallel registration path.

## Startup & Configuration

- `Program.cs` uses `Host.CreateDefaultBuilder(...).ConfigureWebHostDefaults(w => w.UseStartup<Startup>())`.
  This is the **`Startup.cs` pattern**, not top-level-statement `WebApplicationBuilder`. Keep service
  registration in `ConfigureServices` and middleware in `Configure`.
- Configuration is built via `CommonConfigure<Startup>()` from `AlvTime.Common/Configuration` —
  JSON + env vars + user secrets, plus Azure Key Vault outside development. Do not hand-roll a
  `ConfigurationBuilder` chain.
- Settings bind through the Options pattern: `services.Configure<TimeEntryOptions>(...)`, consumed as
  `IOptionsMonitor<TimeEntryOptions>` (note: **Monitor**, matching existing services).
- Four environments: Development, Test, Production, plus `env.IsTest()` from `EnvironmentExtensions.cs`.
  CORS and HTTPS redirection differ per environment — check `Startup.Configure` before changing pipeline order.
- Middleware order is deliberate: error handling → routing → CORS → CSRF → authentication → authorization → endpoints.

## Persistence

- EF Core 9 + SQL Server. `AlvTime_dbContext` is registered `Scoped`.
- Entities in `DatabaseModels/`. Note `Task` is an entity name — alias it where it collides with
  `System.Threading.Tasks.Task` (`using Task = AlvTime.Persistence.DatabaseModels.Task;`).
- Reusable query logic goes in `*QueryableExtension.cs` files (`IQueryable<T>` extension methods),
  not inline in storages.
- `IDbContextScope` / `DbContextScope` wraps multi-step transactional work.
- Migrations live in `AlvTime.Persistence/Migrations/` and are applied by `AlvTime.MigrationClient`.

## Auth

- JWT bearer **and** OpenID Connect, wired by `AddAlvtimeAuthentication` (`Authentication/`).
- Policy-based authorization via `AddAlvtimeAuthorization` (`Authorization/`).
- Current user is reached through `IUserContext` / `UserContext` — scoped. Do not read claims
  directly from `HttpContext` in services.
- Microsoft Graph access via `GraphService` (`Infrastructure/`).
- Access tokens (personal API tokens) are a separate mechanism: `AccessTokenService` + `IAccessTokenStorage`.
- `UseCsrfMiddleware()` protects cookie-authenticated requests.

## API Documentation

`Microsoft.AspNetCore.OpenApi` (`AddOpenApi` / `MapOpenApi`) with Scalar UI (`MapScalarApiReference`,
Kepler theme). Customization uses transformers: `OpenApiLoginTransformer` (document) and
`DefaultHeaderTransformer` (operation). **No Swashbuckle** — don't add it.

## Testing

Follow TDD (root `CLAUDE.md`): failing test first, then implementation.

Existing stack — **this is the convention, and it overrides the dotnet-claude-kit defaults**:

- **xUnit 2.4** with `Moq` 4.16. Not xUnit v3, no `Assert.Multiple`, no `ITestOutputHelper` v3 APIs.
- **`AlvTimeDbContextBuilder`** (`Tests/UnitTests/AlvTimeDbContextBuilder.cs`) is the fluent seeding
  entry point for every data-touching test:

  ```csharp
  _context = new AlvTimeDbContextBuilder()
      .WithTasks()
      .WithProjects()
      .WithCustomers()
      .WithInvoiceBasedSalaryUsers()
      .CreateDbContext();
  ```

- **EF InMemory** by default (fresh `Guid`-named database per test); pass
  `new AlvTimeDbContextBuilder(isSqlite: true)` when the test needs real transactions or relational behaviour.
- Options are faked with `Mock.Of<IOptionsMonitor<TimeEntryOptions>>(o => o.CurrentValue == entryOptions)`.
- Tests live in `Tests/UnitTests/<Area>/`, named `<Subject>Tests.cs`. Shared helpers in
  `Tests/UnitTests/TestUtils/`.

**Do not introduce** Testcontainers, `WebApplicationFactory`, Verify snapshots, FluentAssertions, or
xUnit v3 unless explicitly asked. There are currently no integration tests — the suite is unit tests only.

Running tests requires the `/c/` obj-deletion workaround from the root `CLAUDE.md`.

## Validation

FluentValidation **10.2.3** in `AlvTime.Business/Validators/`. It is several major versions behind
current — check the 10.x API surface before using newer syntax.

## Kit Defaults That Do NOT Apply Here

dotnet-claude-kit skills assume a greenfield .NET 10 project. In this codebase:

| Kit default | Reality here |
|---|---|
| .NET 10 / C# 14 | `net9.0`; `ImplicitUsings`/`Nullable` enabled only in `AlvTime.Common` and `AlvTime.MigrationClient` — other projects use explicit `using` blocks |
| Minimal APIs, `TypedResults` | Controller-based MVC returning `ActionResult<T>` |
| Top-level `Program.cs` | `Startup.cs` + `CreateHostBuilder` |
| `.slnx` solution | Legacy `.sln` |
| Central package management (`Directory.Packages.props`) | Per-project `PackageReference` versions; no `Directory.Build.props` |
| xUnit v3 + Testcontainers + Verify | xUnit 2.4 + Moq + EF InMemory (see Testing) |
| HybridCache | `AddMemoryCache()` / `Microsoft.Extensions.Caching.Abstractions` |
| MediatR / Wolverine / MassTransit | None — direct service calls, no message bus |
| Result libraries (`FluentResults`, `ErrorOr`) | Hand-rolled `Result` / `Result<T>` in `AlvTime.Business/Results/` |

When a kit skill's guidance conflicts with this file, **this file wins**.

## Known Rough Edges

Pre-existing, not to be "fixed" incidentally:

- `AlvTimeWebApi.csproj` sets a Windows-absolute `DocumentationFile` path (`C:\AlvTime-WebApi\...`)
  in Debug, which creates a stray `C:/` directory when built on Linux.
- `Tests.csproj` pins `Microsoft.NET.Test.Sdk` 16.2.0 and `coverlet.collector` 1.0.1 — both very old
  relative to `net9.0`.
- `AlvTime.Persistence` references `Microsoft.AspNetCore.Mvc.Core` 2.2.5, a web dependency in the
  data layer.
