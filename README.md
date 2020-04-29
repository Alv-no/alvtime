# AlvTime-WebApi
## Back-end of alvtime

### Run locally
Clone the repository, navigate to root folder and run `docker-compose up --build`. The API is available at `http://localhost:8000`. To call endpoints with the `[Authorize]`-tag an access token is needed which can be extracted from logging into frontend and viewing console. If you are not an Alv user you will need to either change the authorization provider in `appsettings.json` or remove the `[Authorize]`-tag.
