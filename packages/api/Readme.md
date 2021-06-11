# AlvTime-WebApi
## Back-end of alvtime

### Run locally
Clone the repository, navigate to root folder and run `docker-compose up --build api`. The API is available at `http://localhost:8081`. To call endpoints with the `[Authorize]`-tag an access token is needed. Use the following token `5801gj90-jf39-5j30-fjk3-480fj39kl409` for non-admin endpoints.
For admin endpoints you need an admin user token, which can be extracted from logging into frontend and viewing console. If you are not an Alv user you will need to either change the authorization provider in `appsettings.json` or remove the `[Authorize]`-tag. 

### Run locally without Docker
In order to use the API in a meaningful way you will need to setup a local database. In `Alvtime.Persistence/Database` you will find two scripts, `CreateDatabase.sql` and `InsertData.sql`. In a database management application of your choice (i.e. SQL Server Management Studio) run these two scripts (starting with `CreateDatabase.sql`) on your local database server. Then find the connection string to your newly created database and replace the existing connection string in `AlvTimeWebApi/appsettings.Development.json` with it. Now start the API and start making requests using the token found in the section above.