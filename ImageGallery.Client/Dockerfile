FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7184

ENV ASPNETCORE_URLS=http://+:7184

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ImageGallery.Client/ImageGallery.Client.csproj", "ImageGallery.Client/"]
RUN dotnet restore "ImageGallery.Client/ImageGallery.Client.csproj"
COPY . .
WORKDIR "/src/ImageGallery.Client"
RUN dotnet build "ImageGallery.Client.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ImageGallery.Client.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageGallery.Client.dll"]
