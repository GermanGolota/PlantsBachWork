#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Plants.Presentation/Plants.Presentation.csproj", "Plants.Presentation/"]
COPY ["Plants.Application/Plants.Application.csproj", "Plants.Application/"]
COPY ["Plants.Core/Plants.Core.csproj", "Plants.Core/"]
COPY ["Plants.Infrastructure/Plants.Infrastructure.csproj", "Plants.Infrastructure/"]
RUN dotnet restore "Plants.Presentation/Plants.Presentation.csproj"
COPY . .
WORKDIR "/src/Plants.Presentation"
RUN dotnet build "Plants.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Plants.Presentation.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Plants.Presentation.dll"]