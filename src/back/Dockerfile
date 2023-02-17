FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Plants.Presentation/Plants.Presentation.csproj", "Plants.Presentation/"]
COPY ["Domain/Plants.Domain.Infrastructure/Plants.Domain.Infrastructure.csproj", "Domain/Plants.Domain.Infrastructure/"]
COPY ["Shared/Plants.Shared/Plants.Shared.csproj", "Shared/Plants.Shared/"]
COPY ["Domain/Plants.Domain/Plants.Domain.csproj", "Domain/Plants.Domain/"]
COPY ["Aggregates/Plants.Aggregates.Infrastructure/Plants.Aggregates.Infrastructure.csproj", "Aggregates/Plants.Aggregates.Infrastructure/"]
COPY ["Aggregates/Plants.Aggregates/Plants.Aggregates.csproj", "Aggregates/Plants.Aggregates/"]
COPY ["Shared/Plants.Services/Plants.Services.csproj", "Shared/Plants.Services/"]
COPY ["Shared/Plants.Services.Infrastructure/Plants.Services.Infrastructure.csproj", "Shared/Plants.Services.Infrastructure/"]
RUN dotnet restore "Plants.Presentation/Plants.Presentation.csproj"
COPY . .
WORKDIR "/src/Plants.Presentation"
RUN dotnet build "Plants.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Plants.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Plants.Presentation.dll"]