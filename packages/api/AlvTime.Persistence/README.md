## Scaffolding
To update database models after changes in the database schema, run the following command:

`dotnet ef dbcontext scaffold "Server=.\;Database=AlvDevDB;Trusted_Connection=False;User ID=sa;Password=AlvTimeTestErMoro32" Microsoft.EntityFrameworkCore.SqlServer -o DataBaseModels -c "AlvTime_dbContext" --no-pluralize`

If you have made the changes in your local Docker database you can use the following server:
`localhost,1433`.