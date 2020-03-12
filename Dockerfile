FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet restore AlvTimeWebApi2/*.csproj
RUN dotnet publish AlvTimeWebApi2/*.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

ENTRYPOINT ["dotnet", "AlvTimeWebApi2.dll"]