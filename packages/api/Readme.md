# AlvTime-WebApi

## Running backend locally

### Run locally with Docker (recommended)

Clone the repository, navigate to root folder and run `docker-compose up --build api`. The API is available at `http://localhost:8081`/`https://localhost:8082`. API documentation is available at `https://localhost:8082/scalar/v1`. Use the `Authorize` button on this page to log in as a test user.

### Run locally without Docker

In order to use the API in a meaningful way you will need to setup a local database. Install [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) locally, then start the API from your IDE or command line using `dotnet run`. The database migrations will run automatically against your local SQL Server and seed with test data. If you need to run the migrations against a different database, change the connection string in `appsettings.Development.json`.

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
