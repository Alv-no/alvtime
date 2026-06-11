# Ferieoversikt — adminpanel (Blazor)

## Context
Handoff = React/HTML prototype (claude.ai/design). Target = `packages/adminpanel` (Blazor WASM + MudBlazor),
matching screenshots of live admin.alvtime.no. Recreate look in MudBlazor + existing Alv theme, NOT pixel-copy prototype CSS.

**Backend already done** (commit `ec0f1e11`): `GET api/admin/vacationOverview?currentYear=` →
`List<VacationOverviewReport> { int UserId, VacationDaysDto VacationDaysDto }`.
`VacationDaysDto`: AvailableVacationDays, AvailableVacationDaysTransferredFromLastYear, PlannedVacationDaysThisYear,
UsedVacationDaysThisYear, PlannedTransactions[], UsedTransactions[] (TimeEntryResponseDto: Date, Value, Comment, CommentedAt, TaskId).

## Scope = frontend only (adminpanel/Client)

### Prototype → screens
1. **Ferie overview** (`overview.jsx`): nav item "Ferie", page title, search, summary band (4 stats), table of all employees
   (avatar, name, [role], tilgjengelig nå, brukt i år, planlagt i år, overført), sortable, row → detail. (Card layout = optional variant.)
2. **Ferie detail** (`detail.jsx`): employee header, 4 stat tiles (tilgjengelig/brukt/planlagt/overført), allocation bar
   (startedWith split brukt/planlagt/tilgjengelig), grouped vacation periods (expandable to per-day list).

### Data gaps (prototype richer than backend) — see questions
- Report carries only `UserId` → must join client-side w/ `GET api/admin/Users` for name/avatar.
- No job title/"role" in Alv data.
- No period note ("Sommerferie") — only per-entry `Comment` (vacation entries usually empty).
- No explicit "registered date" per day (closest = `CommentedAt`, only set if commented).
- No `startedWith` (year quota) field → derive ≈ UsedVacationDaysThisYear + AvailableVacationDays (imperfect; transfers span years).
- Half-day = entry Value 3.75 vs 7.5.

## Tasks
1. [ ] **Models** `Models/VacationModel.cs`: `VacationOverviewReportModel { int UserId; VacationDaysModel }`,
   `VacationDaysModel` mirroring backend DTO + `TimeEntryModel { DateTime Date; decimal Value; string? Comment; DateTimeOffset? CommentedAt }`.
2. [ ] **ApiRoutes**: add `VacationOverview(int? year)` → `api/admin/vacationOverview?currentYear=`.
3. [ ] **NavMenu**: add `<MudNavLink Href="/ferie">Ferie</MudNavLink>` (inside Admin AuthorizeView).
4. [ ] **Overview page** `Pages/Vacation/Vacation.razor` (`@page "/ferie"`): fetch vacation + users, join, MudTable
   (sortable, search via MudTextField like Employees.razor), summary stats band, row click → `/ferie/{id}`.
5. [ ] **Detail page** `Pages/Vacation/VacationDetail.razor` (`@page "/ferie/{EmployeeId}"`): stat tiles, allocation bar,
   grouped periods (group consecutive workdays from Used+Planned transactions; expandable).
6. [ ] **Period grouping** helper (port `groupPeriods`/`AllocationBar` logic to C#).
7. [ ] Localized strings (SharedContentStrings .resx) for new labels.
8. [ ] Manual verify in running adminpanel.

## Decisions (resolved)
1. **Layout**: table only. Drop card-grid + Tweaks panel.
2. **Period notes / registered dates**: omit. Group consecutive workdays → date-range + day-count, generic "Ferie" label.
3. **Detail page**: standalone `/ferie/{id}`.
4. **Year scope**: current year only.
5. **Allocation bar / "startedWith" quota**: derive client-side (available + taken). Accept imprecision.
6. **Role/title column**: omit (not in Alv data).
7. **"Uten planlagt sommerferie" stat**: keep; summer = planned days in Jun/Jul/Aug.
