FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

COPY ["AlvTimeWebApi/AlvTimeWebApi.csproj", "AlvTimeWebApi/"]
COPY ["/AlvTime.Business/AlvTime.Business.csproj", "AlvTime.Business/"]
RUN dotnet restore "AlvTimeWebApi/AlvTimeWebApi.csproj"
COPY . .
RUN dotnet publish -c Debug -o out AlvTimeWebApi/AlvTimeWebApi.csproj

WORKDIR /app/out
EXPOSE 8080
ENTRYPOINT ["dotnet", "AlvTimeWebApi.dll"]
