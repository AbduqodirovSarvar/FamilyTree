# Build context: repository root (c:/Sarvar-Apps/FT/FamilyTree).
# docker build -f WebApi/Dockerfile -t family-tree-api .

# ───────────── Build stage ─────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching for `dotnet restore`.
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Persistence/Persistence.csproj", "Persistence/"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
RUN dotnet restore "WebApi/WebApi.csproj"

# Copy the rest of the source.
COPY . .
WORKDIR /src/WebApi
RUN dotnet publish "WebApi.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ───────────── Runtime stage ─────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Listen on plain HTTP inside the container; TLS should terminate at a reverse proxy.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=false

EXPOSE 8080

COPY --from=build /app/publish .

# Pre-create wwwroot so the volume mount target exists when running as non-root.
RUN mkdir -p /app/wwwroot && chown -R $APP_UID:$APP_UID /app
USER $APP_UID

ENTRYPOINT ["dotnet", "WebApi.dll"]
