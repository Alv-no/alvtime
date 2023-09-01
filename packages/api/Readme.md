# AlvTime-WebApi

## Running backend locally

### Run locally

Clone the repository, navigate to root folder and run `docker-compose up --build api`. The API is available at `http://localhost:8081`. To call endpoints with the `[Authorize]`-tag an access token is needed. Use the following token `5801gj90-jf39-5j30-fjk3-480fj39kl409` for non-admin endpoints.
For admin endpoints you need an admin user token, which can be extracted from logging into frontend and viewing console. If you are not an Alv user you will need to either change the authorization provider in `appsettings.json` or remove the `[Authorize]`-tag.

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

The migrations will run automatically when the API starts. The database will also be seeded with test data if the environment is `Development`. The default connection string for local development is set in `appsettings.Development.json` and points to your local SQL Server. When running the API via `docker-compose` the connection string points to the docker database as specified in `docker-compose.yaml`.
