# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["company/Company.API/Company.API.csproj", "company/Company.API/"]
COPY ["Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "Inventory/Inventory.Infrastructure/"]
COPY ["Inventory/Inventory.Domain/Inventory.Domain.csproj", "Inventory/Inventory.Domain/"]
COPY ["Inventory/Inventory.Application/Inventory.Application.csproj", "Inventory/Inventory.Application/"]
COPY ["company/Company.Application/Company.Application.csproj", "company/Company.Application/"]
COPY ["company/Company.Domain/Company.Domain.csproj", "company/Company.Domain/"]
COPY ["company/Company.Infrastructure/Company.Infrastructure.csproj", "company/Company.Infrastructure/"]
RUN dotnet restore "./company/Company.API/Company.API.csproj"
COPY . .
WORKDIR "/src/company/Company.API"
RUN dotnet build "./Company.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Company.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Company.API.dll"]