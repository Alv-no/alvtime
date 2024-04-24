FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Alvtime.Adminpanel.Server.csproj", "."]
RUN dotnet restore "Alvtime.Adminpanel.Server.csproj"
COPY . .
RUN dotnet build "Alvtime.Adminpanel.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alvtime.Adminpanel.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alvtime.Adminpanel.Server.dll"]