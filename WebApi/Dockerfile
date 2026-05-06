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

# postgresql-client provides pg_dump for the daily backup BackgroundService.
# Debian Bookworm's stock package only ships pg_dump 15, which refuses to
# dump from a Postgres 16+ server ("server version mismatch"). We install
# from the official PGDG apt repository so the client version matches the
# server (currently PG 16). Bumping the server later means bumping the
# `postgresql-client-16` line below — no other changes needed.
#
# The signed-by approach (apt-key is deprecated) installs the PGDG signing
# key into a dedicated keyring file, then references it from the sources
# list — current best practice on Debian Bookworm.
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl ca-certificates gnupg && \
    install -d /usr/share/postgresql-common/pgdg && \
    curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc \
        -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc && \
    echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt $(. /etc/os-release; echo $VERSION_CODENAME)-pgdg main" \
        > /etc/apt/sources.list.d/pgdg.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends postgresql-client-16 && \
    apt-get purge -y curl gnupg && \
    apt-get autoremove -y && \
    rm -rf /var/lib/apt/lists/*

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
