# Invoice-Based Salary Model: Fiscal Year Debt Calculation

## Context
Users on invoice-based salary receive a higher base salary in exchange for not being compensated for the first 50 internal/volunteer overtime hours **per fiscal year** (June 1 → May 31). This "debt" is deducted from available overtime on the fly — no separate DB table for the debt itself.

Because the current `User` table only stores one `LastSwitchedSalaryModel`, multi-switch histories can't be reconstructed. A `SalaryModelHistory` table is needed for correct multi-switch calculations.

## Schema Change

### 1. New table: `SalaryModelHistory`
| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `UserId` | int FK | |
| `SwitchDate` | DateTime | always a date in June |
| `PreviousModel` | SalaryModel | |
| `NewModel` | SalaryModel | |

Migration: seed from existing `LastSwitchedSalaryModel` data on `User` (if set). Keep `SalaryModel` on `User` (current model). `LastSwitchedSalaryModel` can be removed or kept as a redundant cache — decide during impl.

## Business Rules (confirmed)

- **Every fiscal year** on invoice-based: fresh 50h debt on internal/volunteer overtime.
- **Static → invoice switch (in June):** first fiscal year's 50h debt draws from **all-time** internal/volunteer hours in the bank (pre-switch + new), consuming 0.5× before 1.0×.
- **Invoice → static switch (in June):** remaining unfilled debt from the last invoice-based fiscal year carries over into the static period. No new 50h debt accrues while on static.

## Algorithm: `CompensateForFiscalYearDebt`

Follows the same pattern as `CompensateForPayouts` / `CompensateForFlexedHours`.

The deduction is always exactly **50h per fiscal year** on invoice-based — never capped at available hours. If the employee has fewer than 50h of internal/volunteer overtime to cover the debt, the net balance goes negative.

```
salaryHistory = GetSalaryModelHistory(userId)  // ordered by SwitchDate

For each fiscal year FY from user.StartDate to toDate:
    model = model active at start of FY (derived from salaryHistory + user.SalaryModel)

    if model == InvoiceBased:
        // Always deduct 50h — balance may go negative if insufficient hours available.
        // First fiscal year on invoice-based (includes switch year from static):
        //   draw from ALL internal/volunteer hours all-time (pre-switch + new).
        // Subsequent fiscal years: draw from hours earned >= FY.Start only.
        emit NegativeEntry(hours: -50, rate: 1.0, date: FY.Start, type: FiscalYearDebt)

    if model == Static AND previous FY was InvoiceBased:
        // The carry-over is already handled: the previous FY emitted -50h,
        // and the actual hours earned in that FY were ≤ 50h, so the net
        // effect is already a negative balance going into the static period.
        // No additional entry needed — the deficit persists naturally.
```

**Why no extra carry-over entry:** the -50h emitted for the last invoice-based FY + however many hours were earned in that year = the correct running balance. If 40h were earned and 50h deducted, the -10h deficit is already reflected in the total without a separate entry.

**Scope of internal/volunteer entries per fiscal year:**

| Case | First invoice-based FY scope | Subsequent FY scope |
|---|---|---|
| New employee (starts any month) | `Date >= StartDate` (no prior history) | `Date >= FY.Start` |
| Existing employee switching (always June) | All-time (`Date >= StartDate`, same as above, but also includes pre-switch years under static) | `Date >= FY.Start` |

Key difference: for an existing employee switching in June, "all-time" includes internal/volunteer hours earned in prior fiscal years under the static model — those pre-existing hours offset the -50h debt first. For a new employee, "all-time" simply means since their start date (no static history exists). Either way the debt is the full -50h.

## Tasks

- [x] 1. DB migration: create `SalaryModelHistory` table; seed from `LastSwitchedSalaryModel`
- [x] 2. Add `SalaryModelHistory` EF entity + `IUserRepository` methods
- [x] 3. Implement `UserRepository` methods (`GetSalaryModelHistory`, `AddSalaryModelHistory`, `UpdateSalaryModel`)
- [x] 4. Update `UserService.UpdateSalaryModel` to persist change and insert into `SalaryModelHistory`
- [x] 5. Add `GetSalaryModelHistory` to `UserService`
- [x] 6. Implement `CompensateForFiscalYearDebt` in `TimeRegistrationService` (ongoing + switch)
- [x] 7. Call it in `GetAvailableOvertimeHoursAtDate` after existing compensations
- [x] 8. Expose `CompensatedFiscalYearDebt` in `AvailableOvertimeDto`
- [x] 9. Write tests — `FiscalYearDebtTests` (8 ongoing) + `FiscalYearDebtSwitchTests` (5 Static→Invoice) + `FiscalYearDebtMultiSwitchTests` (1 multi-switch)

## Files to Modify
- `AlvTime.Persistence/Migrations/` — new migration
- `AlvTime.Persistence/DatabaseModels/SalaryModelHistory.cs` — new entity
- `AlvTime.Persistence/Repositories/TimeRegistrationStorage.cs` — add salary history query
- `AlvTime.Business/TimeRegistration/TimeRegistrationService.cs` — new method + call
- `AlvTime.Business/Overtime/AvailableOvertimeDto.cs` — add `CompensatedFiscalYearDebt`
- `AlvTime.Business/Users/UserService.cs` — write history on switch

## Test Cases
1. New employee starts November on invoice-based: earns 40h internal by May → balance is -10h (full 50h debt applies even for partial first FY).
2. New employee starts November on invoice-based: earns 55h internal by May → 5h available.
3. New employee, second fiscal year: 5h from year 1 in bank; year 2 emits fresh -50h; need 50h more internal to break even again.
4. Static → invoice switch with 40h existing internal: 40h removed + -10h deficit (balance = -10h); earn 10h more → back to 0.
5. Static → invoice switch with 60h existing: 50h deducted, 10h remain in bank (pre-switch hours covered full debt).
6. Invoice → static switch with 30h earned in last invoice year: -20h deficit persists into static period naturally (no extra entry needed).
7. Invoice → static switch with 60h earned in last invoice year: balance positive; no carry-over deficit.
8. Multi-switch (static→invoice→static→invoice): each period calculated independently using history table.

## Verification
- Run `InvoiceBasedFiscalYearDebtTests` + existing `SalaryModelSwitchTests`
- Manual: register internal overtime for invoice-based user; check `/api/user/timebank` response shows `CompensatedFiscalYearDebt` entries
