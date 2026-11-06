# Fraværsplanlegger

Velg fraværskode (dropdown) + periode (fra–til, også bakover i tid, maks 1 år) + andel av dagen → timer registreres på alle arbeidsdager i perioden. Helg + røde dager (`RedDays`) hoppes over. Dekker ferie, sykemelding, foreldrepermisjon o.l.

## Beslutninger
- Koder i dropdown: inkluderingsliste `TimeEntryOptions.AbsencePlanTasks` (task-id-er per miljø). Med: Ferie, Syk, Foreldrepermisjon, Fødselspermisjon, Sykemelding, Ubetalt permisjon, Sykmelding ifbm graviditet, Refusjonsberettiget fravær. Ikke med: Velferdspermisjon, Sykt barn, Lege/tannlege, Avspasering
- Andel av dagen (standard 100 %) i første versjon. Mål per dag = `7.5 * stillingsprosent * andel`
- Dager med timer på annen kode: avkrysning «Fyll opp dager som har andre timer», standard av
  - Av → dagen hoppes over (`HasOtherHours`)
  - På → `min(mål, full dag − andre timer)`; 0 → hoppes over
- Bakover i tid tillatt. Maks 1 år per registrering
- Ferie over saldo: kun advarsel
- Inngang: knapp i Timebanken + på forsiden
- Kun backend-tester (ingen vitest)

## Funn
- Backend avviser ferie/avspasering på helg/rød dag (`ValidateTimeEntry`). Øvrige koder tillates der, men skal uansett ikke føres.
- Stillingsprosent finnes ikke i frontend → beregning i backend.
- `UpsertTimeEntry` returnerer ved første valideringsfeil → delvis lagring. Må pakkes atomisk.
- Bakover i tid treffer sperrer i `ValidateTimeEntry` (låst kode, kunde-timelås, utbetaling påvirkes) → forhåndsvisning tørrkjører samme validering per dag.
- Lange perioder (~250 dager) kan gi treg lagring (spørringer + overtid/flex-omregning per dag). Må måles.
- `RedDays` (backend) er fasit; `holidayHelper.ts` gir kun navn på røde dager i UI.
- `@vuepic/vue-datepicker` brukes allerede (`StatisticsView.vue`), støtter range.

## Løsning
Tre endepunkt:
- `GET api/user/AbsencePlan/Tasks` → kodene i `AbsencePlanTasks` (ikke låste), ferie først
- `GET api/user/AbsencePlan?taskId&fromDateInclusive&toDateInclusive&percentage&fillPartialDays` → forhåndsvisning
- `POST api/user/AbsencePlan` `{ taskId, fromDateInclusive, toDateInclusive, percentage, fillPartialDays, comment? }` → beregner på nytt, lagrer kun `Register`-dager, atomisk

Status per dag:
| Status | Betydning | Lagres |
|---|---|---|
| `Register` | arbeidsdag, validering OK → beregnede timer | ja |
| `Weekend` | lør/søn | nei |
| `RedDay` | i `RedDays` | nei |
| `AlreadyRegistered` | har allerede mål-timer på valgt kode | nei |
| `HasOtherHours` | timer på annen kode, avkrysning av (eller ingen plass igjen) | nei |
| `Blocked` | `ValidateTimeEntry` feiler + feilmelding | nei |

Respons: `{ days: [{ date, status, hours, existingOtherHours, reason? }], totalDays, totalHours }`.
Validering: `taskId` i `AbsencePlanTasks`, `from <= to`, periode ≤ 1 år, `0 < percentage <= 100`.

## UX
Modal (`ModalComponent`), åpnes fra Timebanken (`VacationOverview.vue`) og forsiden:
```
┌ Registrer fravær ─────────────────────── ✕ ┐
│ Type    [ Sykemelding                  ▾ ]  │
│ Periode [ 30.06.2026 – 17.07.2026      ▾ ]  │
│ Andel av dagen [ 100 ] %                    │
│ ☐ Fyll opp dager som har andre timer        │
│                                             │
│ 13 dager · 97,5t                            │
│ (ferie: Feriedager igjen 21 → 8 / ⚠ over)   │
│                                             │
│ Hoppes over (3)                             │
│  to 09.07  Har 3t på Kunde X                │
│  fr 17.07  Allerede registrert              │
│  ma 06.07  Timelås – kontakt admin          │
│  + 6 helgedager, 0 røde dager               │
│                                             │
│ ▸ Vis alle dager (uke for uke)              │
│ Kommentar (valgfri) [__________]            │
│               [Avbryt] [Registrer 13 dager] │
└─────────────────────────────────────────────┘
```
- Forhåndsvisning hentes når type + periode er valgt, og ved endring av andel/avkrysning (debounce).
- Unntak øverst; full dagsliste sammenfoldet. Helg/røde dager kun som antall; rød dag med navn i full liste.
- Feriesaldo + advarsel kun for ferie.
- Knapp deaktivert ved 0 dager. Spinner under lagring.
- Etter lagring: lukk, oppdater `vacationStore`, `timeBankStore`, time entries for synlig uke. Feil i `ErrorBox`.
- Mobil: modal har `min-width: 500px` → justeres.

## Oppgaver
- [x] 1. **Tester først (API)** — `Tests/UnitTests/Absence/AbsencePlanServiceTests.cs` (26 tester, røde mot skjelett `AbsencePlanService` + `AbsencePlanDtos.cs`)
  - [x] 1.1 `GetAbsencePlanTasks`: kun id-er i `AbsencePlanTasks`, uten låste, ferie først
  - [x] 1.2 Ukedager → `Register`, 7,5t; 80 % stilling → 6t; andel 50 % → 3,75t
  - [x] 1.3 Helg → `Weekend`; rød dag (17. mai, Kristi himmelfart) → `RedDay`, også for sykemelding
  - [x] 1.4 Full dag på valgt kode → `AlreadyRegistered`; halv dag → `Register` (oppdateres)
  - [x] 1.5 Andre timer, `fillPartialDays=false` → `HasOtherHours`; `true` → rest av dag (maks andel); full dag → `HasOtherHours`
  - [x] 1.6 Bakover i tid: kunde-timelås → `Blocked` m/ melding. (Utbetaling-sperre utelatt: kan ikke utløses når fravær aldri overstiger full dag)
  - [x] 1.7 Ugyldig `taskId` (avspasering, prosjekt, låst), `from > to`, > 1 år, ugyldig andel → feil
  - [x] 1.8 Lagring: kun `Register`-dager lagres m/ kommentar; delvis dag oppdateres; ugyldig forespørsel → ingenting lagret
  - [x] 1.9 ~~Atomisk test~~ utgår: alle dager forhåndsvalideres, blokkerte hoppes over → ingen domenefeil midt i lagring. `DbContextScope.AsAtomic` støtter ikke nestede transaksjoner (relasjonell DB) → ikke pakk inn `UpsertTimeEntry`
- [x] 2. **Config** — `AbsencePlanTasks` (`List<int>`) i `TimeEntryOptions` + `appsettings*.json`
  - [x] 2.1 Lokal `[6]` (kun Ferie seedet)
  - [x] 2.2 Test `[13, 14, 33, 76, 63, 1252]` (Ubetalt permisjon og Sykmelding ifbm graviditet finnes ikke i test)
  - [x] 2.3 Prod `[13, 14, 33, 76, 63, 314, 224, 377]`
- [x] 3. **TimeRegistrationService** — gjør `ValidateTimeEntry` tilgjengelig for tørrkjøring, uendret oppførsel
- [x] 4. **Business** — `AlvTime.Business/Absence/AbsencePlanService.cs` (konkret klasse, ingen interface, jf. `packages/api/CLAUDE.md`), registrer `AddScoped<AbsencePlanService>()` i `ServiceRegistrator`
  - [x] 4.1 `GetAbsencePlanTasks()`
  - [x] 4.2 `GetPlan(...)` — `RedDays`, stillingsprosent, entries for hele perioden i én spørring, `ValidateTimeEntry` per kandidatdag
  - [x] 4.3 `RegisterPlan(...)` — `GetPlan` → `UpsertTimeEntry` for `Register`-dager (uten ytre transaksjon, se 1.9)
  - [x] 4.4 Timer rundes til nærmeste kvarter (`[QuarterHour]` på `CreateTimeEntryDto`)
- [x] 5. **Web API** — `AbsencePlanController`, request/response-typer + mapping i controller. `status` som streng i respons
- [x] 6. Kjør tester grønt
- [ ] 7. Ytelse: mål `POST` med ~250 dager lokalt; optimaliser kun ved behov
- [x] 8. **frontend-v3**
  - [x] 8.1 `absenceService`: `getAbsencePlanTasks`, `getAbsencePlan`, `registerAbsencePlan`; typer i `types/`
  - [x] 8.2 `stores/absencePlanStore.ts`: koder, plan, lagring, refresh av relaterte stores
  - [x] 8.3 `components/AbsencePlanner/AbsencePlanner.vue` (dropdown, range-picker, andel, avkrysning, oppsummering, unntak, sammenfoldet dagsliste)
  - [x] 8.4 Knapp i `VacationOverview.vue` + på forsiden
  - [x] 8.5 Mobiltilpasning av modal (`min-width: min(500px, 90vw)`, `z-index` på overlay)
  - [x] 8.6 Backend: `isPaidHoliday` på fraværskoder (test først) → feriesaldo kun for ferie
  - [x] 8.7 `vue-tsc` + `npm run build` OK; ingen nye lint-feil
- [ ] 9. Manuell test: ferie over påske/17. mai, sykemelding bakover i tid, foreldrepermisjon ~1 år, 50 % gradert, 80 %-bruker, dag med prosjekttimer (av/på), timelåst periode, ferie over saldo, mobil

## Utenfor scope
- Slette fravær for periode (naturlig oppfølger)
- Avspasering i periode
- Admin-styring av kodeutvalg (flagg på task)
- Frontend-tester, slack-app, cli

## Uavklarte spørsmål

Ingen.
