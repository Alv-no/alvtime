#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ./AlvTimeWebApi2/*.csproj AlvTimeWebApi2/
COPY ./Microsoft.Identity.Web/Microsoft.Identity.Web.csproj ./Microsoft.Identity.Web/
COPY ./Microsoft.Identity.Web/NuGet.Config ./Microsoft.Identity.Web/
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