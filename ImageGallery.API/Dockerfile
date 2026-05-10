FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5075

ENV ASPNETCORE_URLS=http://+:5075

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ImageGallery.API/ImageGallery.API.csproj", "ImageGallery.API/"]
RUN dotnet restore "ImageGallery.API/ImageGallery.API.csproj"
COPY . .
WORKDIR "/src/ImageGallery.API"
RUN dotnet build "ImageGallery.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ImageGallery.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ImageGallery.API/ImageGallery.db /app/ImageGallery.db
# NOTE:  SQLite requires write access to BOTH the DB file AND its directory to create WAL/journal files (e.g. *.db-wal, *.db-shm).
# By default /app is owned by root, so the non-root 'app' user cannot write there, causing runtime errors like "attempt to write a readonly database".
# Fix: make the entire /app directory owned by 'app' so SQLite can function.
USER root
RUN chown -R app:app /app
# Switch back to app user (important)
USER app

ENTRYPOINT ["dotnet", "ImageGallery.API.dll"]