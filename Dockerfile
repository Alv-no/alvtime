FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS base
WORKDIR /app

ARG ASPNETCORE_ENVIRONMENT

COPY ["AlvTimeWebApi/AlvTimeWebApi.csproj", "AlvTimeWebApi/"]
COPY ["/AlvTime.Business/AlvTime.Business.csproj", "AlvTime.Business/"]
RUN dotnet restore "AlvTimeWebApi/AlvTimeWebApi.csproj"
COPY . .
RUN dotnet publish -c Debug -o out AlvTimeWebApi/AlvTimeWebApi.csproj

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0

WORKDIR /app
COPY --from=base /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "AlvTimeWebApi.dll"]