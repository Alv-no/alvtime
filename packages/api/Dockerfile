FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app

COPY ["AlvTimeWebApi/AlvTimeWebApi.csproj", "AlvTimeWebApi/"]
COPY ["/AlvTime.Business/AlvTime.Business.csproj", "AlvTime.Business/"]
RUN dotnet restore "AlvTimeWebApi/AlvTimeWebApi.csproj"
COPY . .
RUN dotnet publish -c Debug -o out AlvTimeWebApi/AlvTimeWebApi.csproj
RUN dotnet dev-certs https
ENTRYPOINT ["dotnet",  "watch", "--project", "AlvTimeWebApi/AlvTimeWebApi.csproj", "run", "--urls", "http://0.0.0.0:80;https://0.0.0.0:443"]

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app
COPY --from=base /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "AlvTimeWebApi.dll"]
