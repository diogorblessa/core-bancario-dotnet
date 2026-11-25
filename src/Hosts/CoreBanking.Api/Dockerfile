# Build context: raiz do repositório (ver docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Restaura primeiro, copiando só os arquivos de projeto, para aproveitar o cache de camadas
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Modules/Ledger/CoreBanking.Ledger.Domain/*.csproj src/Modules/Ledger/CoreBanking.Ledger.Domain/
COPY src/Modules/Ledger/CoreBanking.Ledger.Application/*.csproj src/Modules/Ledger/CoreBanking.Ledger.Application/
COPY src/Modules/Ledger/CoreBanking.Ledger.Infrastructure/*.csproj src/Modules/Ledger/CoreBanking.Ledger.Infrastructure/
COPY src/Modules/Ledger/CoreBanking.Ledger.Contracts/*.csproj src/Modules/Ledger/CoreBanking.Ledger.Contracts/
COPY src/Hosts/CoreBanking.Api/*.csproj src/Hosts/CoreBanking.Api/
RUN dotnet restore src/Hosts/CoreBanking.Api/CoreBanking.Api.csproj

COPY src/ src/
RUN dotnet publish src/Hosts/CoreBanking.Api/CoreBanking.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CoreBanking.Api.dll"]
