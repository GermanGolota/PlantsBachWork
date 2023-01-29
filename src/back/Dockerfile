FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Plants.Presentation/Plants.Presentation.csproj", "Plants.Presentation/"]
COPY ["Domain/Plants.Domain.Infrastructure/Plants.Domain.Infrastructure.csproj", "Domain/Plants.Domain.Infrastructure/"]
COPY ["Plants.Shared/Plants.Shared.csproj", "Plants.Shared/"]
COPY ["Domain/Plants.Domain/Plants.Domain.csproj", "Domain/Plants.Domain/"]
COPY ["Plants.Aggregates.Infrastructure/Plants.Aggregates.Infrastructure.csproj", "Plants.Aggregates.Infrastructure/"]
COPY ["Plants.Aggregates/Plants.Aggregates.csproj", "Plants.Aggregates/"]
COPY ["Plants.Services/Plants.Services.csproj", "Plants.Services/"]
COPY ["Plants.Services.Infrastructure/Plants.Services.Infrastructure.csproj", "Plants.Services.Infrastructure/"]
COPY ["Plants.Application/Plants.Application.csproj", "Plants.Application/"]
COPY ["Plants.Core/Plants.Core.csproj", "Plants.Core/"]
COPY ["Plants.Infrastructure/Plants.Infrastructure.csproj", "Plants.Infrastructure/"]
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