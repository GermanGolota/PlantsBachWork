FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Plants.DbMigrator/Plants.DbMigrator.csproj", "."]
RUN dotnet restore "./Plants.DbMigrator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Plants.DbMigrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Plants.DbMigrator.csproj" -c Release -o /app/publish

FROM base AS final
ENV Migration:ScriptsLocation=DatabaseScripts
WORKDIR /app
COPY --from=publish /app/publish .
COPY DatabaseScripts DatabaseScripts
ENTRYPOINT ["dotnet", "Plants.DbMigrator.dll"]