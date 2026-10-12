# AlvTime-WebApi

## Running backend locally

### Run locally with Docker (recommended)

Clone the repository, navigate to root folder and run `docker-compose up --build api`. The API is available at `http://localhost:8081`/`https://localhost:8082`. API documentation is available at `https://localhost:8082/scalar/v1`. Use the `Authorize` button on this page to log in as a test user.

### Run locally without Docker

In order to use the API in a meaningful way you will need to setup a local database. Install [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) locally, then start the API from your IDE or command line using `dotnet run`. The database migrations will run automatically against your local SQL Server and seed with test data. If you need to run the migrations against a different database, change the connection string in `appsettings.Development.json`.

## Webhooks

Webhook events are published through [Hookdeck Outpost](https://hookdeck.com/outpost). See the [root readme](../../Readme.md#webhooks-outpost) for access and configuration.

### Events

| Topic                | When                                             |
| -------------------- | ------------------------------------------------ |
| `time_entry.created` | A new time entry is registered                   |
| `time_entry.updated` | An existing time entry is changed (incl. to `0`) |

Time entries are never deleted. Removing hours sets `value` to `0`, which is sent as `time_entry.updated`.

The payload only identifies the entry, no hours, task or comment are sent:

```json
{
  "id": "<event id>",
  "topic": "time_entry.updated",
  "data": { "id": 123, "userId": 4, "date": "2026-10-08T00:00:00" }
}
```

Consumers fetch the details with `GET /api/admin/TimeEntries?fromDate=<date>&toDate=<date>` (requires Admin role), pick the entry matching `data.id`, and upsert it on `id`. Events can arrive more than once and out of order, use the event `id` to ignore duplicates.

### How it works

Events are published with `IDbContextScope.PublishAsync` inside `AsAtomic`, so they are stored in the Wolverine outbox in the same transaction as the data. After commit they are handled by `WebhookOutboxProcessor`, which posts them to Outpost. Failed deliveries are retried and end up in `wolverine.wolverine_dead_letters`.

### Adding a new event

1. Create a record implementing `IWebhookPayload` with a `Topic` and `Version`.
2. Add a `Handle(WebhookEvent<YourEvent> evt, CancellationToken ct) => Send(evt, ct);` method to `WebhookOutboxProcessor`.
3. Route it in `Startup.cs`: `opts.PublishMessage<WebhookEvent<YourEvent>>().ToLocalQueue("webhooks");`
4. Call `dbContextScope.PublishAsync(...)` inside `AsAtomic` where the change happens.

## Creating database migrations

Prerequisites:

- [.NET EF CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools)

This project uses Entity Framework migrations code first to update database models and schema. To add new entities or make changes to an existing one:

- Make the necessary changes to the relevant entity in the `AlvTime.Persistence/DatabaseModels` folder.
- If you are adding a new entity, remember to add it as a `DbSet` in the `AlvTime_dbContext.cs` class.
- Add any custom behaviour either to the entity as data annotations in the class itself or to the `OnModelCreating()` method in `AlvTime_dbContext.cs`.
- Then run the following command from the `AlvTime.Persistence` directory:

`dotnet ef migrations add NameOfMigration --startup-project ../AlvTimeWebApi --context AlvTime_dbContext --output-dir Migrations`

- Verify that the migration script was created as expected and any custom behaviour such as foreign keys, constraints and default values are correct.
- Run the migrations locally to verify that they behave as expected.

To run the migrations run the MigrationClient project. The migrations will run automatically when starting the solution in Docker. The database will also be seeded with test data if the environment is `Development`. The default connection string for local development is set in `appsettings.Development.json` and points to your local SQL Server. When running the API via `docker-compose` the connection string points to the docker database as specified in `docker-compose.yaml`.
