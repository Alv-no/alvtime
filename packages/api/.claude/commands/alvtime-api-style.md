# AlvTime API — kodestil og mønstre

Bruk dette som referanse når du legger til nye features i `AlvTime.Business` / `AlvTime.Persistence`.

---

## Lagdeling

```
AlvTime.Business/
  {Domain}/
    I{Entity}Storage.cs     ← grensesnitt (kontrakt)
    {Entity}Service.cs      ← forretningslogikk
    {Entity}ValidationService.cs  ← valideringslogikk
    {Entity}Dto.cs          ← data transfer objects

AlvTime.Persistence/
  Repositories/
    {Entity}Storage.cs      ← EF Core-implementasjon av I{Entity}Storage
  DatabaseModels/
    AlvTime_dbContext.cs    ← EF DbContext
    {Entity}.cs             ← EF-entiteter (partial class)
```

**Regel:** Business-laget definerer kontraktene (`IStorage`, `IDbContextScope`, `IUserContext`).
Persistence-laget implementerer dem. Aldri importer Persistence fra Business.

---

## Result/Error-mønster

Alle service-metoder som kan feile returnerer `Result<T>` eller `Result`:

```csharp
// Returner verdi
public async Task<Result<CustomerDto>> CreateCustomer(CustomerDto customer)
{
    var errors = new List<Error>();
    await ValidateCustomer(customer, errors);
    if (errors.Any()) return errors;   // implicit konvertering → Result<T>

    await _customerStorage.CreateCustomer(customer);
    return (await GetCustomer(customer.Name, customer.Id)).Single();
}

// Returnere feil direkte
return new List<Error> { new(ErrorCodes.MissingEntity, "Fant ikke kunden.") };
return new Error(ErrorCodes.InvalidAction, "Handlingen er ikke tillatt.");

// Konsumere i controller
var result = await _service.CreateCustomer(dto);
return result.Match(
    success => Ok(success),
    errors  => BadRequest(errors));
```

**ErrorCodes** (fra `AlvTime.Business/Results/ErrorCodes.cs`):
- `InvalidAction` — ugyldig tilstand
- `MissingEntity` — entitet finnes ikke
- `EntityAlreadyExists`
- `RequestMissingProperty` / `RequestInvalidProperty`
- `SQLError`
- `AuthorizationError`

---

## Grensesnitt-navning

| Type | Konvensjon | Eksempel |
|------|-----------|---------|
| Lagringslag | `I{Entity}Storage` | `IProjectStorage`, `ITimeRegistrationStorage` |
| Infrastruktur | `I{Capability}` | `IDbContextScope`, `IUserContext` |
| Service-klasse | Ingen `I`-prefix | `ProjectService`, `CustomerService` |
| Validering | `{Entity}ValidationService` | `PayoutValidationService` |

---

## DI-livstid

```csharp
// Scoped (standard — én per HTTP-request)
services.AddScoped<IProjectStorage, ProjectStorage>();
services.AddScoped<ProjectService>();
services.AddScoped<IDbContextScope, DbContextScope>();

// Transient (stateless hjelpere)
services.AddTransient<IUserRepository, UserRepository>();

// DbContext — alltid Scoped
services.AddDbContext<AlvTime_dbContext>(opts => opts.UseSqlServer(cs));
```

---

## Transaksjoner — IDbContextScope

For operasjoner som berører flere tabeller, bruk alltid `IDbContextScope.AsAtomic()`:

```csharp
public class DbContextScope : IDbContextScope
{
    public async Task AsAtomic(Func<Task> atomicAction)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await atomicAction.Invoke();
        await transaction.CommitAsync();
    }
}

// Bruk i service
await _dbContextScope.AsAtomic(async () =>
{
    var entry = await _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
    await UpdateEarnedOvertime(entriesOnDay, userId);
    await UpdateRegisteredFlex(entriesOnDay, userId);
});
```

**Direkte transaksjoner** (uten IDbContextScope) brukes bare i migrasjonsverktøy og ad-hoc-scripts.

---

## Valideringsmønster

Valideringslogikk bor i en dedikert `{Entity}ValidationService` og returnerer `List<Error>`:

```csharp
public class PayoutValidationService
{
    public async Task<List<Error>> ValidatePayout(GenericPayoutHourEntry request, int userId)
    {
        var errors = new List<Error>();

        if (request.Hours % 0.25M != 0)
            errors.Add(new(ErrorCodes.RequestInvalidProperty, "Bestilling må gå opp i kvarter."));

        // ... videre sjekker

        return errors;
    }
}

// I service:
var errors = await _payoutValidationService.ValidatePayout(request, userId);
if (errors.Any()) return errors;
```

---

## EF Core-mønstre

```csharp
// Les-spørringer: bruk AsNoTracking + projektion til DTO
var result = await _context.Project
    .AsNoTracking()
    .Filter(criteria)                          // extension-metode for dynamiske filter
    .Include(p => p.CustomerNavigation)
    .Select(p => new ProjectDto { Id = p.Id, Name = p.Name })
    .ToListAsync();

// Oppdateringer: last entitet med tracking, endre, kall SaveChangesAsync
var entity = await _context.Hours.FindAsync(id);
entity.Value = newValue;
entity.TimeRegistered = DateTime.UtcNow;
await _context.SaveChangesAsync();

// Batch-sletting: ExecuteDeleteAsync uten å laste entitetene
await _context.ProjectFavorite
    .Where(pf => pf.UserId == userId)
    .ExecuteDeleteAsync();

// Innen samme transaksjon: AsNoTracking tvinger DB-runde for korrekte tall
var totals = await _context.Hours
    .AsNoTracking()
    .Where(h => userIds.Contains(h.User))
    .GroupBy(h => h.User)
    .Select(g => new { UserId = g.Key, Total = g.Sum(h => h.Value) })
    .ToDictionaryAsync(x => x.UserId, x => x.Total);
```

**Husk:** EF Core sin identity map gjør at `FindAsync` på samme Id innenfor én DbContext-instans
returnerer det *samme* sporede objektet. To forskjellige steder som oppdaterer samme entitet
overskriver hverandre — bruk unike Id-er eller guard-sjekker for å unngå dette.

---

## Query-objekter

Filtreringskriterier pakkes i et `{Entity}QuerySearch`-objekt definert i Business-laget:

```csharp
public class ProjectQuerySearch
{
    public int?   Id       { get; set; }
    public string Name     { get; set; }
    public int?   Customer { get; set; }
}
```

---

## AlviterMigration — avviker fra standard

Migrasjonsverktøyet (`AlvTime.AlviterMigration`) er et frittstående console-program og bruker
enklere mønstre:

- Ingen `Result<T>` — kaster exceptions som ruller tilbake transaksjonen
- Direkte `_context.Database.BeginTransactionAsync()` (ingen IDbContextScope)
- Alle services registrert som `Transient` (deles ikke)
- Konstantfilen `MigrationConstants.cs` holder `TargetTaskId = 336`
- Validering etter skriving: hent aggregerte totaler med `AsNoTracking` og sammenlign
