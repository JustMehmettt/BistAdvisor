FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BistAdvisor.Worker/BistAdvisor.Worker.csproj", "BistAdvisor.Worker/"]
COPY ["BistAdvisor.Application/BistAdvisor.Application.csproj", "BistAdvisor.Application/"]
COPY ["BistAdvisor.Domain/BistAdvisor.Domain.csproj", "BistAdvisor.Domain/"]
COPY ["BistAdvisor.Infrastructure/BistAdvisor.Infrastructure.csproj", "BistAdvisor.Infrastructure/"]
RUN dotnet restore "BistAdvisor.Worker/BistAdvisor.Worker.csproj"

COPY . .
WORKDIR "/src/BistAdvisor.Worker"
RUN dotnet build "./BistAdvisor.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BistAdvisor.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BistAdvisor.Worker.dll"]
