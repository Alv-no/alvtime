FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app

COPY ["AlvTimeWebApi/AlvTimeWebApi.csproj", "AlvTimeWebApi/"]
COPY ["/AlvTime.Business/AlvTime.Business.csproj", "AlvTime.Business/"]
RUN dotnet restore "AlvTimeWebApi/AlvTimeWebApi.csproj"
COPY . .
RUN dotnet publish -c Debug -o out AlvTimeWebApi/AlvTimeWebApi.csproj
RUN dotnet dev-certs https --trust
ENTRYPOINT ["dotnet",  "watch", "--project", "AlvTimeWebApi/AlvTimeWebApi.csproj", "run", "--urls", "http://0.0.0.0:8080;https://0.0.0.0:443"]

#FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app/out
#COPY --from=base /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AlvTimeWebApi.dll"]
