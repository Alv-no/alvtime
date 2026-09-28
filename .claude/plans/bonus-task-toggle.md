# Bonus-flagg på timekoder

`Bonus` (bool) på `Task`. Lagres i DB, eksponeres i admin- og bruker-API, i frontend-v3-typer (ingen UI), toggle «Gir bonus» i adminpanel. Følger mønsteret til `Imposed`.

Formål: grunnlag for senere bonusopptjening basert på faktureringsgrad (ny lønnsmodell). Regler/unntak for bonusgivende timer er for komplekse å utlede → eksplisitt flagg per timekode. Beregning er egen, senere jobb.

## Beslutninger
- Enkel bool, ingen historikk. Endring slår tilbake i tid → bekreftelsesdialog i adminpanel, samlet med kompensasjonstype-bekreftelsen
- Backfill: `true` der gjeldende timerate (nyeste `HourRate` med `FromDate <= i dag`) > 0, ellers `false`
- Property `Bonus`, label «Gir bonus»
- Egen Ja/Nei-kolonne i task-grid på `Customer.razor`
- Eksponeres: admin-API, bruker-API (`user/Tasks`, `user/LastUsedTasks`, V2-prosjekter), frontend-v3-type (kun data)

## Oppgaver

- [x] 1. **Tester først (API)**
  - [x] 1.1 `Tasks/TaskServiceTests.cs`: `CreateNewTask` med `Bonus = true` → lagret
  - [x] 1.2 `Tasks/TaskServiceTests.cs`: `UpdateTask` toggler `Bonus`, returnert `TaskDto.Bonus` stemmer
  - [x] 1.3 `Customers/`: admin-henting gir `TaskAdminDto.Bonus`
  - [x] 1.4 `Projects/`: `GetProjectsWithTasks` gir `TaskResponseDtoV2.Bonus`
  - [x] 1.5 `Tasks/`: `GetTasksForUser` gir `TaskResponseDto.Bonus`
- [x] 2. **Entitet** — `public bool Bonus { get; set; }` i `DatabaseModels/Task.cs`
- [x] 3. **Migrasjon** `Task_Add_Bonus` (`dotnet ef migrations add`)
  - [x] 3.1 `AddColumn<bool>("Bonus", "Task", type: "bit", nullable: false, defaultValue: false)`
  - [x] 3.2 Backfill-SQL (mønster fra `Task_Add_CompensationType`): `ROW_NUMBER() OVER (PARTITION BY TaskId ORDER BY FromDate DESC)` på `HourRate WHERE FromDate <= GETDATE()`, sett `Bonus = 1` der `rn = 1 AND Rate > 0`
  - [x] 3.3 Sjekk Designer + snapshot
- [x] 4. **Business-DTOer** + `Bonus`: `TaskDto` (`CreateTaskDto.cs`), `TaskResponseDto`, `TaskResponseDtoV2`, `TaskAdminDto`
- [x] 5. **Persistence**
  - [x] 5.1 `TaskStorage`: `GetTasks`, `CreateTask`, `UpdateTask`, `GetTaskById`
  - [x] 5.2 `CustomerStorage`: `TaskAdminDto`-select
  - [x] 5.3 `ProjectStorage`: `TaskResponseDtoV2`-select
- [x] 6. **TaskService** — mapping i `UpdateTask`, `GetTask`, `GetTaskById`
- [x] 7. **Web API**
  - [x] 7.1 `Requests/TaskUpsertRequest.cs` + `Bonus`
  - [x] 7.2 `Controllers/Utils/TaskMapper.cs`: begge `MapToTaskDto` + `MapToTaskResponseSimple`
  - [x] 7.3 `Responses/Admin/TaskResponseSimple.cs`, `Responses/TaskAdminResponse.cs` + `CustomerMapper.cs`
  - [x] 7.4 `Responses/TaskResponse.cs` (record) + `TasksController.FetchTasks`
- [x] 8. Kjør tester grønt
- [x] 9. **Adminpanel**
  - [x] 9.1 `Models/CustomerModel.cs` (`TaskModel`), `Requests/TaskUpsertRequest.cs`, `Mappers/TaskMapper.cs` + `Bonus`
  - [x] 9.2 `SharedContentStrings.resx` + `.Designer.cs`: `Common.Bonus` = «Gir bonus»
  - [x] 9.3 `TaskDialog.razor`: `_bonus` + `_originalBonus`, `MudSwitch` etter `Imposed`, init i `OnParametersSetAsync`, send i `Submit`
  - [x] 9.4 `TaskDialog.razor`: én samlet bekreftelse i edit-modus når kompensasjonstype og/eller bonus endres (utvid eksisterende `CompensationType`-dialog, melding lister endringene)
  - [x] 9.5 `Customer.razor`: mapping i `_aggregatedTasks` + Ja/Nei-kolonne
- [x] 10. **frontend-v3** — `bonus: boolean` i `Task` (`src/types/ProjectTypes.ts`), ingen UI
- [ ] 11. Manuell test
  - [x] 11.1 Migrasjon mot lokal DB: 4/7 `Bonus = 1`, rate 0 → `0`
  - [x] 11.2 Bruker-API: `user/Tasks`, `user/projects` gir `bonus` (`LastUsedTasks` tom lokalt, dekkes av `GetTasksForUser`)
  - [ ] 11.3 Adminpanel (krever AD-login): opprett m/bonus, kolonne Ja, toggle → bekreftelse, bonus + komp.type → én samlet dialog

## Utenfor scope
- Bonusberegning/opptjening
- UI i frontend-v3, slack-app, cli

## Uavklarte spørsmål

Ingen.
