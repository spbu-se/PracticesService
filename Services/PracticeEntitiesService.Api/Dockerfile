FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/PracticeEntities/PracticeEntities.csproj", "Services/PracticeEntities/"]
RUN dotnet restore "Services/PracticeEntities/PracticeEntities.csproj"
COPY . .
WORKDIR "/src/Services/PracticeEntities"
RUN dotnet build "PracticeEntities.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PracticeEntities.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PracticeEntities.dll"]
