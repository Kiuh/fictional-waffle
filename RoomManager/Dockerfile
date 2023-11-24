FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RoomManager/RoomManager.csproj", "RoomManager/"]
COPY ["RoomManagerApi/RoomManagerApi.csproj", "RoomManagerApi/"]
RUN dotnet restore "./RoomManager/./RoomManager.csproj"
COPY . .
WORKDIR "/src/RoomManager"
RUN dotnet build "./RoomManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RoomManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoomManager.dll"]