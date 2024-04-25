FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Server/Alvtime.Adminpanel.Server.csproj", "Server/"]
RUN dotnet restore "Server/Alvtime.Adminpanel.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "Alvtime.Adminpanel.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alvtime.Adminpanel.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alvtime.Adminpanel.Server.dll"]