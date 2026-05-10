FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 5002

ENV ASPNETCORE_URLS="http://+:5001;https://+:5002"

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ImageGallery.IDP/ImageGallery.IDP.csproj", "ImageGallery.IDP/"]
RUN dotnet restore "ImageGallery.IDP/ImageGallery.IDP.csproj"
COPY . .
WORKDIR "/src/ImageGallery.IDP"
RUN dotnet build "ImageGallery.IDP.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ImageGallery.IDP.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageGallery.IDP.dll"]