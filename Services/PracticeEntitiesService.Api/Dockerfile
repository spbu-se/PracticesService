FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/PracticeEntitiesService.Api/PracticeEntitiesService.Api.csproj", "Services/PracticeEntitiesService.Api/"]
RUN dotnet restore "Services/PracticeEntitiesService.Api/PracticeEntitiesService.Api.csproj"
COPY . .
WORKDIR "/src/Services/PracticeEntitiesService.Api"
RUN dotnet build "PracticeEntitiesService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PracticeEntitiesService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PracticeEntitiesService.Api.dll"]
