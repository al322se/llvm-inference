FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InferenceLlvm.csproj", "."]
RUN dotnet restore "./InferenceLlvm.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./InferenceLlvm.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InferenceLlvm.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InferenceLlvm.dll"]