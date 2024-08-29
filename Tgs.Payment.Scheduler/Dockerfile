FROM hub.lumigame.com/base/dotnet/core/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 5000

FROM hub.lumigame.com/base/dotnet/core/sdk:6.0-alpine AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Tgs.Payment.Scheduler/Tgs.Payment.Scheduler.csproj", "Tgs.Payment.Scheduler/"]
RUN dotnet restore "Tgs.Payment.Scheduler/Tgs.Payment.Scheduler.csproj"
COPY . .
WORKDIR "/src/Tgs.Payment.Scheduler"
RUN dotnet build "Tgs.Payment.Scheduler.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Payment.Scheduler.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Payment.Scheduler.dll"]