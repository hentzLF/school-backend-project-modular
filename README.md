# AgriMarket Backend

ASP.NET Core backend for the AgriMarket agricultural services platform, built with .NET 10 and PostgreSQL.

## Solution Structure

```
AgriMarket.slnx
├── AgriMarket.Api          # RESTful Web API (controllers, auth, Swagger)
├── AgriMarket.Web          # ASP.NET Core MVC Web UI
├── AgriMarket.BLL          # Business Logic Layer (services, DTOs, validation)
├── AgriMarket.DAL          # Data Access Layer (EF Core, migrations, seeding)
├── AgriMarket.Domain       # Domain entities, enums (no dependencies on other layers)
├── AgriMarket.Resources    # Shared localization resources (en, et)
├── AgriMarket.Tests        # Integration/unit tests
└── openspec/               # API design specs and proposals
```

The solution follows a Clean Architecture / N-Tier approach.

## Features

- **RESTful API** with camelCase JSON serialization and RFC 7807 ProblemDetails error handling
- **MVC Web UI** with server-rendered views
- **OpenAPI/Swagger** documentation at `/swagger`
- **Entity Framework Core** with PostgreSQL (Npgsql)
- **Localization** — English and Estonian via shared `.resx` resources
- **Role-based access** — Admin, Provider, Customer
- **Domains** — Users, Service Listings, Bookings, Reviews, Payments, Categories

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

## Getting Started

1. **Configure the database** — update `ConnectionStrings:DefaultConnection` in `AgriMarket.Api/appsettings.json` and `AgriMarket.Web/appsettings.json`:

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=AgriMarketDb;Username=postgres;Password=yourpassword"
    }
    ```

2. **Apply migrations**:

    ```bash
    dotnet ef database update --project AgriMarket.DAL --startup-project AgriMarket.Api
    ```

3. **Run the API**:

    ```bash
    cd AgriMarket.Api
    dotnet run
    ```

4. **Run the MVC web app**:

    ```bash
    cd AgriMarket.Web
    dotnet run
    ```

5. **Explore the API** at `http://localhost:<port>/swagger`

## Running Tests

```bash
dotnet test
```

## Docker

```bash
docker compose up --build
# Optional overrides:
# AGRI_POSTGRES_HOST_PORT=5434 AGRI_JWT_KEY="change-me-to-32-chars-min" docker compose up --build
# Use external PostgreSQL (skip local db container):
# AGRI_POSTGRES_HOST=your-db-host AGRI_POSTGRES_DB_PORT=5432 docker compose up --build --scale agrimarket-db=0
```

## Development Guidelines

- **API standards** — endpoints must match the specs in the `openspec/specs` directory
- **Error handling** — all endpoints use RFC 7807 ProblemDetails
- **Null handling** — null values are omitted from JSON responses
- **Specs** — review the `openspec` directory when making architectural changes or extending API functionality
