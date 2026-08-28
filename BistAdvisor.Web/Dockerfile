FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BistAdvisor.Web/BistAdvisor.Web.csproj", "BistAdvisor.Web/"]
COPY ["BistAdvisor.Application/BistAdvisor.Application.csproj", "BistAdvisor.Application/"]
COPY ["BistAdvisor.Domain/BistAdvisor.Domain.csproj", "BistAdvisor.Domain/"]
COPY ["BistAdvisor.Infrastructure/BistAdvisor.Infrastructure.csproj", "BistAdvisor.Infrastructure/"]
RUN dotnet restore "BistAdvisor.Web/BistAdvisor.Web.csproj"

COPY . .
WORKDIR "/src/BistAdvisor.Web"
RUN dotnet build "./BistAdvisor.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./BistAdvisor.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BistAdvisor.Web.dll"]
