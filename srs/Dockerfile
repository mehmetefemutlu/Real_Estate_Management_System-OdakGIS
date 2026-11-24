# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["srs/srs.csproj", "srs/"]
RUN dotnet restore "./srs/srs.csproj"
COPY ["srs/*.cs", "srs/"]
COPY ["srs/*.json", "srs/"]
COPY ["srs/Controllers/*.cs", "srs/Controllers/"]
COPY ["srs/Data/*.cs", "srs/Data/"]
COPY ["srs/Data/*.json", "srs/Data/"]
COPY ["srs/DTO/*.cs", "srs/DTO/"]
COPY ["srs/Entity/*.cs", "srs/Entity/"]
COPY ["srs/Interfaces/*.cs", "srs/Interfaces/"]
COPY ["srs/Migrations/*.cs", "srs/Migrations/"]
COPY ["srs/Repositories/*.cs", "srs/Repositories/"]
COPY ["srs/Services/*.cs", "srs/Services/"]
WORKDIR "/src/srs"
RUN dotnet build "./srs.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./srs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "srs.dll"]
