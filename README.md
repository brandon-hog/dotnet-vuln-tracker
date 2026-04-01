# DotnetVulnTracker

A fullstack .NET portfolio project showcasing a layered web application end-to-end.

DotnetVulnTracker helps teams track network assets and correlate them with vulnerability data so they can prioritize remediation by risk. This project is intentionally structured to show practical backend architecture, API design, authentication, and frontend UX in one cohesive solution.

## Why I chose to create this Project

- Practicing fullstack ownership in the .NET framework: Blazor WebAssembly frontend + ASP.NET Core API + PostgreSQL.
- Practicing Clean Architecture boundaries with Domain/Application/Infrastructure separation.
- Learning and implementing real-world backend patterns: MediatR CQRS handlers, EF Core repositories, hosted background worker, and Identity-based auth.
- Learning Unitx tests to include tests for domain behavior and invariants.

## Tech Stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Blazor WebAssembly
- Entity Framework Core + Npgsql (PostgreSQL)
- ASP.NET Core Identity (Minimal API endpoints)
- MediatR
- xUnit
- Docker (for local PostgreSQL)

## Solution Layout

- `Client`: Blazor WebAssembly frontend (UI, auth state, token handler).
- `Api`: HTTP API, DI composition root, Identity endpoints, background CVE sync worker.
- `Application`: Use cases (commands/queries), interfaces/abstractions.
- `Domain`: Core entities and business rules.
- `Infrastructure`: EF Core DbContext, migrations, repository implementations.
- `Shared`: DTOs shared between client and API.
- `Tests`: Unit tests (currently domain-focused).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Optional but recommended: EF Core CLI tools
  - `dotnet tool install --global dotnet-ef`

## Running Locally

### 1) Start PostgreSQL in Docker

The API development connection string uses:
- Host: `localhost`
- Database: `postgres`
- Username: `postgres`
- Password: `password`

Run:

```powershell
docker run --name vulntracker-postgres `
  -e POSTGRES_PASSWORD=password `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_DB=postgres `
  -p 5432:5432 `
  -d postgres:16
```

To restart later:

```powershell
docker start vulntracker-postgres
```

### 2) Apply EF Core migrations

From the repo root:

```powershell
cd Api
dotnet ef database update --project ../Infrastructure --startup-project .
```

### 3) Run the API

In one terminal:

```powershell
dotnet run --project Api
```

API base URL (dev): `http://localhost:5286`  
Swagger UI: `http://localhost:5286/swagger`

### 4) Run the Client

In a second terminal:

```powershell
dotnet run --project Client
```

Client URL (dev): `http://localhost:5199`

### 5) First-use flow

1. Open the client in the browser.
2. Register a user account.
3. Log in.
4. Navigate to assets and start creating records.

## Implemented Features

- **Authentication and authorization**
  - User registration/login via ASP.NET Core Identity endpoints.
  - Token-based authenticated API access from Blazor.
  - Automatic refresh-token retry flow in the client HTTP pipeline.
- **Asset management**
  - Create, read, update, and delete assets.
  - Paginated asset listing.
  - Search by hostname/IP address.
- **Vulnerability visibility**
  - Asset detail page with vulnerability list, severity badges, and CVSS scores.
  - Risk score calculation encapsulated in the domain entity.
- **Background CVE ingestion**
  - Hosted worker fetches vulnerability data from the NVD API on a schedule.
  - Demo correlation logic flags assets when hostname keywords appear in CVE descriptions.
- **Developer experience**
  - Swagger for API exploration.
  - Unit tests for domain model behaviors.

## Clean Architecture in This Project

This solution uses Clean Architecture to keep business logic independent of frameworks and infrastructure concerns.

### Layer Responsibilities

- **Domain (`Domain`)**
  - Holds enterprise business rules and invariants.
  - `Asset` and `Vulnerability` enforce valid state and behavior (e.g., CVSS bounds, duplicate CVE prevention, risk scoring).
- **Application (`Application`)**
  - Defines use cases (commands/queries) and interfaces (`IAssetRepository`).
  - Orchestrates actions through MediatR handlers without depending on concrete data access.
- **Infrastructure (`Infrastructure`)**
  - Implements application abstractions (repository + EF Core persistence).
  - Contains `AppDbContext`, migrations, and query/paging implementation details.
- **Presentation (`Api` + `Client`)**
  - `Api`: controllers and Identity endpoints that expose application capabilities over HTTP.
  - `Client`: Blazor pages and auth-aware HTTP client that consume the API.

### Dependency Direction

Dependencies point inward:

- `Client` and `Api` depend on application contracts.
- `Infrastructure` depends on `Application` and `Domain`.
- `Application` depends on `Domain`.
- `Domain` depends on nothing external.

### Request Flow Example

`Client` page action -> `Api` controller endpoint -> MediatR command/query handler in `Application` -> `IAssetRepository` abstraction -> `Infrastructure` repository implementation -> PostgreSQL via EF Core -> response mapped to DTO -> returned to `Client`.

This keeps business rules testable and resilient to framework changes.

## Testing

Run all tests:

```powershell
dotnet test
```

Current tests focus on domain behavior (asset creation, vulnerability handling, and risk score calculation).

## Notes

- This is a portfolio demo with intentionally simplified CVE correlation logic in the background worker.
- For production hardening, next steps would include richer matching heuristics, deeper test coverage across application/infrastructure layers, and deployment automation.