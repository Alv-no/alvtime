FROM mcr.microsoft.com/dotnet/sdk:10.0 as build-env
WORKDIR /src
COPY AlvTime.Persistence/AlvTime.Persistence.csproj ./AlvTime.Persistence/
COPY AlvTime.MigrationClient/AlvTime.MigrationClient.csproj ./AlvTime.MigrationClient/
RUN dotnet restore AlvTime.MigrationClient/AlvTime.MigrationClient.csproj
COPY . ./
RUN dotnet publish AlvTime.MigrationClient/AlvTime.MigrationClient.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
ENTRYPOINT ["dotnet", "AlvTime.MigrationClient.dll"]
CMD ["-m"]