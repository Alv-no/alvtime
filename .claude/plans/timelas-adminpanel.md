# Timelås — adminpanel (Blazor)

## Context
Handoff = React/HTML prototype from claude.ai/design (`lock.jsx`, `lockdata.js`, `Ferie.html`).
Target = `packages/adminpanel/Client` (Blazor WASM + MudBlazor + Alv theme). Recreate the *look*, not the prototype CSS.
Backend endpoints exist on branch `feature/lock-hours`; one small backend fix in scope (task 1).

### Backend contract (`Controllers/Admin/CustomerController.cs`)
- `GET api/admin/ActiveCustomers` → `CustomerResponse[]`; customers w/ hours last 2 months. Used fields: `Id`, `Name`, `LockedTo` (`DateTime?`).
- `PUT api/admin/Customers/Lock` m/ body `{ toDateInclusive, customersToExclude[] }` → sets `LockedTo` on **all** customers except excluded.
- `PUT api/admin/Customers/Lock/{customerId}?toDateInclusive=<date>` → sets `LockedTo` on one.
- Unlock endpoint removed → lock date can only be **changed**, never cleared.

### Decisions (from review)
- Bulk lock being DB-wide (incl. customers not shown) is fine.
- "Åpne opp alle igjen" = bulk `Lock` with an earlier date. Note: also gives a lock date to customers that had none.
- Drop the "Låst <dato> av <navn>" sub-line (no `LockedBy`/`LockedAt` in API).
- No `lastRegistered` per customer → footer says "med timer registrert siste 2 måneder"; no "vis alle"-toggle (endpoint filters server-side).
- Exclusion checkboxes = client-side session state only, no note text, borte ved reload.
- Knappen for masselås heter «Lås timer» (ikke «Lås N kunder») — endepunktet låser hele databasen, så N er ukjent for siden. Dialogen sier «Alle kunder, også de uten timer siste 2 måneder».
- Per-rad "lås opp" = flytt låsedatoen bakover via `Lock/{id}` → ingen egen rød "Lås opp"-knapp; radhandlingen er "Endre dato".

## Tasks

### Backend
1. [x] **Fix duplicate customers in `GetActiveCustomers`** — *test run pending (no NuGet access in sandbox)*.
   1. [x] 4 tests in `CustomerServiceTests`: several recent entries → once; entries på flere prosjekter → once; kun gamle entries → utelatt; `LockedTo` mappes.
   2. [x] Rewrite as single customer query: `_context.Customer.Where(c => c.Project.Any(p => p.Task.Any(t => t.Hours.Any(h => h.Date >= cutoff))))`, cutoff = `DateTime.Now.AddMonths(-2)`. Alle timerader teller (også `Value = 0`).
   3. [x] `TimeRegistrationServiceTests` called the deleted `CustomerStorage.UnlockCustomer` → rewritten as `LockCustomer(2021-12-31, 1)` and renamed `UpsertTimeEntry_CustomerLockDateMovedBackBeforeEntryDate_CanRegisterHours`.

1b. [x] **Bulk lock tar request body** i stedet for gjentatte query-parametre: ny `AlvTimeWebApi/Requests/LockCustomersRequest.cs`
   (`ToDateInclusive`, `CustomersToExclude`), `LockCustomers([FromBody] …)`. `Lock/{id}` beholder `?toDateInclusive=` (enkel skalar).

### Frontend (`packages/adminpanel/Client`)
2. [x] **Model**: add `DateTime? LockedTo` to `Models/CustomerModel.cs`.
3. [x] **ApiRoutes** (`Utils/ApiRoutes.cs`):
   1. [x] `ActiveCustomers => "api/admin/ActiveCustomers"`
   2. [x] `LockAllCustomers => api/admin/Customers/Lock` (+ `Requests/LockCustomersRequest.cs` for the body)
   3. [x] `LockCustomer(int id, DateTime toDateInclusive)` → `api/admin/Customers/Lock/{id}?toDateInclusive=yyyy-MM-dd`
4. [x] **NavMenu**: `<MudNavLink Href="/timelas" Match="NavLinkMatch.Prefix">Timelås</MudNavLink>` inside Admin `AuthorizeView`.
5. [x] **Page** `Pages/TimeLock/TimeLock.razor` (`@page "/timelas"`), following `Vacation.razor`
   (`AuthorizeView Roles="Admin"`, `Interceptor.RegisterEvent()`, `MudProgressLinear` while loading):
   1. [x] Title "Timelås" (`Typo.h2`) + subtitle "Lås registrerte timer etter at faktura er sendt, så de ikke kan endres i etterkant."
   2. [x] Load `GET ActiveCustomers` → rows `{ Id, Name, LockedTo, Excluded }`, sorted by name (nb-NO).
   3. [x] **"Lås alle kunder"-kort** (`MudPaper`): `MudDatePicker` "Lås til og med" (`MaxDate` = i dag, default = ut forrige måned),
      hurtigvalg "I dag" / "Ut forrige måned", primærknapp "Lås timer" → bekreftelsesdialog,
      live-tekst "Setter låsedato **X** for N kunder", tekstlenke "Åpne opp alle igjen" → dialog (task 7).
   4. [x] Søkefelt `MudTextField` (search-adornment) på navn.
   5. [x] **Kundetabell** `MudDataGrid<LockRow>`: ekskluder-checkbox | Kunde | Låst til og med (dato, ellers "Ikke låst") |
      Status (`MudChip` "Låst"/"Åpen") | handlinger ("Lås til dato" / "Endre dato").
   6. [x] Inline rad-redigering: `MudDatePicker` + "Lagre" / "Avbryt" → `PUT Customers/Lock/{id}`, reload etterpå.
   7. [x] Bunnlinje: "Viser N av M kunder — med timer registrert siste 2 måneder".
   8. [x] Ekskluderte rader: tonet bakgrunn + "Ekskludert fra masselås"-caption.
   9. [x] Kunden «Alv» (interntid) er ekskludert som standard ved sidelast — kan hakes inn igjen.
6. [x] **Dialog** `Shared/components/LockCustomersDialog.razor`: "Lås timer til og med <dato>?", oppsummering (antall som låses / navn som holdes åpne),
   infotekst, Avbryt / "Lås N kunder" → `PUT Customers/Lock` m/ ekskluderte id-er.
7. [x] **Dialog** `Shared/components/UnlockCustomersDialog.razor`: "Åpne opp alle igjen" m/ `MudDatePicker` "Ny låsedato"
   → `PUT Customers/Lock?toDateInclusive=<ny dato>` (ingen ekskluderte).
8. [x] Kvittering etter masselås: **snackbar** "Timene er låst til og med <dato> for N kunder" (ikke prototypens dialog). Reload liste etterpå.
9. [ ] Manual verify in running adminpanel (build/run by user — sandbox has no NuGet access).

## Out of scope
`LockedBy`/`LockedAt` columns; lock-aware validation on time registration (assumed already handled by `feature/lock-hours`).

## Avklart
- Ekskludering: session-only. Kvittering: snackbar. Tekster: hardkodet norsk (som `Vacation.razor`).
- Frontend har ikke testprosjekt → kun manuell verifisering.
