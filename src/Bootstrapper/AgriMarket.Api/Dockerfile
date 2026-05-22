FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AgriMarket.slnx .
COPY Directory.Build.props .
COPY AgriMarket.Api/AgriMarket.Api.csproj AgriMarket.Api/
COPY AgriMarket.BLL/AgriMarket.BLL.csproj AgriMarket.BLL/
COPY AgriMarket.DAL/AgriMarket.DAL.csproj AgriMarket.DAL/
COPY AgriMarket.Domain/AgriMarket.Domain.csproj AgriMarket.Domain/
COPY AgriMarket.Resources/AgriMarket.Resources.csproj AgriMarket.Resources/

RUN dotnet restore AgriMarket.Api/AgriMarket.Api.csproj

COPY . .
RUN dotnet publish AgriMarket.Api/AgriMarket.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgriMarket.Api.dll"]
