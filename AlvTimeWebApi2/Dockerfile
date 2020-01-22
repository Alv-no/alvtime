FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["AlvTimeWebApi2/AlvTimeWebApi2.csproj", "AlvTimeWebApi2/"]
RUN dotnet restore "AlvTimeWebApi2/AlvTimeWebApi2.csproj"
COPY . .
WORKDIR "/src/AlvTimeWebApi2"
RUN dotnet build "AlvTimeWebApi2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlvTimeWebApi2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlvTimeWebApi2.dll"]