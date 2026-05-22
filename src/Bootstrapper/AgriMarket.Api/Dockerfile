FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AgriMarket.slnx .
COPY Directory.Build.props .
COPY src/Bootstrapper/AgriMarket.Api/AgriMarket.Api.csproj src/Bootstrapper/AgriMarket.Api/
COPY src/Shared/AgriMarket.Shared/AgriMarket.Shared.csproj src/Shared/AgriMarket.Shared/
COPY src/Modules/Users/AgriMarket.Modules.Users/AgriMarket.Modules.Users.csproj src/Modules/Users/AgriMarket.Modules.Users/
COPY src/Modules/Users/AgriMarket.Modules.Users.Contracts/AgriMarket.Modules.Users.Contracts.csproj src/Modules/Users/AgriMarket.Modules.Users.Contracts/
COPY src/Modules/Marketplace/AgriMarket.Modules.Marketplace/AgriMarket.Modules.Marketplace.csproj src/Modules/Marketplace/AgriMarket.Modules.Marketplace/
COPY src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/AgriMarket.Modules.Marketplace.Contracts.csproj src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/
COPY src/Modules/Bookings/AgriMarket.Modules.Bookings/AgriMarket.Modules.Bookings.csproj src/Modules/Bookings/AgriMarket.Modules.Bookings/
COPY src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/AgriMarket.Modules.Bookings.Contracts.csproj src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/
COPY src/Modules/Messaging/AgriMarket.Modules.Messaging/AgriMarket.Modules.Messaging.csproj src/Modules/Messaging/AgriMarket.Modules.Messaging/
COPY src/Modules/Messaging/AgriMarket.Modules.Messaging.Contracts/AgriMarket.Modules.Messaging.Contracts.csproj src/Modules/Messaging/AgriMarket.Modules.Messaging.Contracts/
COPY AgriMarket.Resources/AgriMarket.Resources.csproj AgriMarket.Resources/

RUN dotnet restore src/Bootstrapper/AgriMarket.Api/AgriMarket.Api.csproj

COPY . .
RUN dotnet publish src/Bootstrapper/AgriMarket.Api/AgriMarket.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgriMarket.Api.dll"]
