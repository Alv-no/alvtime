This is an overview of the various business rules relevant for time keeping in Alv.

## Employment rate
- Employees can work part-time. An employment rate of 1.0 means full-time (7.5h/day); 0.5 means half-time (3.75h/day).
- A user can have multiple employment rates, each valid from/to a specific date.
- All calculations that reference anticipated work hours (overtime, payout validation, etc.) scale 7.5h by the employment rate in effect on that date.

## Overtime
- A workday is 7.5 hours (scaled by employment rate), except for weekends and red days which are 0 hours
- All hours worked above the anticipated hours for a day will be compensated
- The compensation rate for earned overtime is calculated based on the task type worked and the user's salary model. The table below explains the rates:

| Task type | Salary model | Imposed | Rate |
|---|---|---|---|
| `Billable` | any | `true` | 2.0 |
| `Billable` | `Static` | `false` | 1.5 |
| `Billable` | `InvoiceBased` | `false` | 1.4 |
| `Internal` | — | — | 1.0 |
| `Volunteer` | — | — | 0.5 |

- Earned overtime can be withdrawn as either payouts on your next paycheck or as flex
- Overtime is earned on a day to day basis
- Imposed tasks always count as overtime when exceeding the anticipated work horus for that day.
- When a day has multiple task types, they fill regular hours in descending order of compensation rate (highest first). The exception is imposed tasks which will always end up on top of any other task types. The remaining hours above the daily target are overtime.
  - Example: 7.5h billable + 2h internal → billable fills regular hours, the 2h internal is overtime at 1.0×
  - Example: 7.5h billable + 2h imposed → billable fills regular hours, the 2h imposed is overtime at 2.0×
- Payouts are calculated by looking at the total amount of overtime you have since starting, minus any previous payouts or flex registrations done and then calculating the remainder. Calculations are always done using the "lowest" worth of hours first. I.e. if you have 2 overtime hours with a compensation rate of 0.5 and 2 overtime hours with a compensation rate of 1.5, and you order 2 hours to your payout they will be taken from the 0.5 rate hours first.
- Conversely, flexing draws from the "highest" worth hours first. In the example above, if we replace payout with flexing, they will be taken from the 1.5 rate hours first.
- When calculating if you have enough hours in your bank to either order a payout or flex, the available hours BEFORE factoring in the compensation rate is used.

### Payout rules
- Payout hours must be entered in 0.25h increments
- Only one payout order is allowed per date
- All future flex entries must be removed before ordering a payout
- All workdays in the last 30 days (or since your start date, whichever is more recent) must be fully filled (≥ anticipated hours for each day) before a payout can be ordered
- Only the most recently ordered payout can be cancelled
- Payout cut-off: the 7th of the current month. Payouts ordered on or after the most recent 7th are cancellable; earlier payouts are locked

### Flex rules
- Flex cannot be registered on weekends or red days
- Cannot flex more hours than available overtime (measured before applying the compensation rate)
- Cannot register flex that would cause your overtime balance to go negative at any point in the future

### General time entry constraints
- All time entries must be in 0.25h increments
- Cannot register or edit time on or before a payout date in a way that would increase your overtime balance on that day (retroactive overtime inflation is blocked)

## Salary models
Alv has 2 salary models; static and invoice based. An employee is allowed to switch their salary model once a year, in June. They must have worked in Alv for at least one year to be allowed to change. So someone starting in January 2025, will not be eligible for change until June 2026.
### Static
- You are compensated for every hour you work as described above. Your billable overtime hours are compensated with a factor of 1.5.
### Invoice based
- Each accounting year (starting in june) you start with a bank of negative fifty (-50) internal hours (`TaskType.Internal` or `TaskType.Volunteer`). You are compensated for this by getting a higher base salary. Any internal hours you work beyond a normal workday up to 50 total hours will not be registered as overtime. After you have accumulated 50 hours of internal overtime they will starting counting towards your overtime bank as normal.
- Billable overtime hours are compensated with a factor of 1.4.
### Switching salary models
The 50 internal hours you are not compensated for when using the invoice based salary model introduces some complexity. When switching from one model to another, these need to be included in your time bank.

#### New employee
You start with a timebank of negative 50 internal hours. Accumulating 50 overtime hours of `TaskType.Volunteer` or `TaskType.Internal` (compensation rate 0.5 and 1.0) makes these hours start counting towards your time bank.

#### From static to invoice based 
Your time bank is reduced by up to 50 hours of compensation rate 0.5 or 1.0, starting with 0.5. If you do not have 50 hours of these types available, the rest will be added to a "debt". I.e. if you have 40 internal hours available, you will have to work another 10 hours of internal over for them to start counting towards your time bank.

#### From invoice based to static
When you had the invoice based salary model, the 50 internal hours were baked into your salary. This means that you will not suddenly get another 50 hours added to your time bank when you switch to a static model. In fact, since you have already been paid for these hours, you will need to earn the remaining "debt", same as above.


## Vacation
- All employees have 25 vacation days available per year
- Each vacation day corresponds to 7.5 hours of registered time
- Employees who have not worked a full year have their vacation days pro-rated: `(days employed in that year / 365.25) × 25`
- Unused vacation days from the previous year carry over into the current year's balance

## Sick leave
- Employees can self-certify sick leave for up to 3 consecutive calendar days per period
- Up to 4 such periods are allowed within any rolling 12-month window, giving a maximum of 12 self-certified sick days per year

## Red days
Red days are Norwegian public holidays on which the anticipated work hours are 0 (same as weekends). They include:
- New Year's Day (1 Jan)
- Easter period: Maundy Thursday, Good Friday, Easter Sunday, Easter Monday
- Ascension Day
- Pentecost Sunday and Monday
- Labour Day (1 May)
- Constitution Day (17 May)
- Christmas Eve through New Year's Eve (24–31 Dec)
