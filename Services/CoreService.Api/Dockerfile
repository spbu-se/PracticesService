# Use the official ASP.NET runtime image for the base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET SDK image for the build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Services/CoreService.Api/CoreService.Api.csproj", "CoreService.Api/"]
COPY ["Services/CoreService.Api/stylecop.json", "CoreService.Api/"] # Add this line
RUN dotnet restore "./CoreService.Api/CoreService.Api.csproj"

# Copy the rest of the application code and build
COPY . .
WORKDIR "/src/CoreService.Api"
RUN dotnet build "./CoreService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Run the application in the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreService.Api.dll"]
