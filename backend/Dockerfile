# Render.com ve diğer container ortamları (.NET 9 API)
# Panel: Runtime = Docker, Root Directory = backend, Dockerfile path = Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GameTracker.Api.csproj", "./"]
RUN dotnet restore "GameTracker.Api.csproj"
COPY . .
RUN dotnet publish "GameTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GameTracker.Api.dll"]
