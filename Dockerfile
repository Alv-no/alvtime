FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY ./AlvTimeWebApi2/*.csproj ./AlvTimeWebApi2/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

ENTRYPOINT ["dotnet", "AlvTimeWebApi2.dll"]