# Airline Transaction Monitor — Claude Code Conventions

## Project Overview

Distributed real-time payment monitoring system for aviation FinTech. Ingests airline transactions via REST, processes through analytics pipeline, raises alerts when error thresholds are crossed, and pushes state to a real-time Angular dashboard via SignalR.

**Stack:** .NET 10 + ASP.NET Core + Entity Framework Core + PostgreSQL 16 + NATS JetStream + SignalR + Angular 21 (Signals + Zoneless) + Tailwind CSS + OpenTelemetry + Jaeger + Prometheus + Grafana + SonarQube + Jenkins

## Project Structure

```
airline-transaction-monitor/
├── apps/
│   ├── gateway/                    # API Gateway (.NET 10)
│   │   ├── Controllers/            # Auth, routing endpoints
│   │   ├── Hubs/                   # SignalR hub (TransactionHub)
│   │   ├── Auth/                   # JWT configuration + token issuing
│   │   ├── Nats/                   # NATS subscribers → SignalR push
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── ingestion/                  # Ingestion Service (.NET 10)
│   │   ├── Domain/                 # Entities, value objects, rules
│   │   ├── Application/            # Use cases, port interfaces
│   │   ├── Infrastructure/         # EF Core DbContext, NATS publisher
│   │   ├── Api/                    # Controllers, DTOs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── analytics/                  # Analytics Service (.NET 10)
│   │   ├── Domain/                 # Metrics, alerts, rules
│   │   ├── Application/            # Use cases, port interfaces
│   │   ├── Infrastructure/         # EF Core DbContext, NATS subscriber/publisher
│   │   ├── Api/                    # Controllers, DTOs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── simulator/                  # TransactionSimulator (.NET 10, demo-only)
│   │   ├── Generators/             # Bogus-based transaction generators
│   │   ├── Controllers/            # /generate, /burst, /health
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── common/                     # Shared library (.NET class library)
│   │   ├── Events/                 # NATS event contracts
│   │   ├── Auth/                   # JWT shared config
│   │   └── Telemetry/              # OpenTelemetry shared setup
│   └── web/                        # Angular 21 dashboard
│       ├── src/
│       │   ├── app/
│       │   │   ├── dashboard/      # Main dashboard page
│       │   │   ├── transactions/   # Transaction list + detail
│       │   │   ├── alerts/         # Alerts page
│       │   │   ├── auth/           # Login page
│       │   │   ├── services/       # SignalR + HTTP services
│       │   │   └── shared/         # Shared components, pipes, guards
│       │   └── environments/
│       ├── Dockerfile
│       └── vitest.config.ts
├── infra/
│   ├── grafana/                    # Dashboard JSON provisioning
│   ├── prometheus/                 # prometheus.yml
│   └── sonarqube/                  # sonar-project.properties
├── n8n/
│   └── workflows/                  # 7 JSON workflow files (auto-imported)
├── tests/
│   ├── Ingestion.UnitTests/        # xUnit unit tests
│   ├── Ingestion.IntegrationTests/ # Testcontainers (Postgres + NATS)
│   ├── Analytics.UnitTests/
│   ├── Analytics.IntegrationTests/
│   ├── Gateway.UnitTests/
│   └── Gateway.IntegrationTests/
├── docker-compose.yml              # Full 12-container stack
├── docker-compose.infra.yml        # Infra-only (Postgres, NATS, n8n, Jaeger, Prometheus, Grafana, SonarQube)
├── Jenkinsfile                     # Declarative pipeline as code
├── AirlineTransactionMonitor.sln   # .NET solution file
├── global.json                     # Pins .NET SDK to 10.x
├── CLAUDE.md                       # This file
└── README.md
```

## Conventions

### C# / .NET Backend

- .NET 10 (LTS) with C# 14
- ASP.NET Core Web API for all services
- Entity Framework Core with code-first migrations
- Two PostgreSQL databases: `ingestion_db` and `analytics_db` (same Postgres instance)
- UUID primary keys (`Guid`) for all tables
- Auto-generate sequential codes (e.g., `TXN-2026-04-000001`) for human-readable identifiers
- Use `CreatedAt` / `UpdatedAt` timestamps on all entities (PascalCase for C#)
- Amounts stored as integers in minor units (cents) to avoid floating-point issues
- Nullable `DisabledAt` for soft deletes where applicable

### Hexagonal Architecture (Ingestion + Analytics)

- **Domain layer**: Entities, value objects, domain rules — NO external dependencies (no EF Core, no NATS, no HTTP)
- **Application layer**: Use cases, port interfaces (e.g., `ITransactionRepository`, `IEventPublisher`)
- **Infrastructure layer**: Adapters implementing ports (EF Core DbContext, NATS publisher/subscriber)
- **Api layer**: Controllers, DTOs, request/response mapping
- Domain core must be testable without any infrastructure

### NATS JetStream Events

- Stream: `TRANSACTIONS` → subjects: `transaction.created`
- Stream: `METRICS` → subjects: `metrics.updated`
- Stream: `ALERTS` → subjects: `alert.raised`
- Events are published by one service, consumed by another — no direct HTTP calls between services

### Authentication

- JWT bearer tokens issued by the Gateway (`POST /api/auth/login`)
- In-memory user store (assessment scope)
- All `/api/*` routes require valid JWT except `/api/auth/login`
- TransactionSimulator obtains JWT automatically at startup

### Angular Dashboard

- Angular 21 with Signals + Zoneless change detection (no Zone.js)
- Tailwind CSS for styling
- SignalR for real-time updates (no polling)
- Vitest for testing

### Observability

- OpenTelemetry on every .NET service
- Traces → Jaeger (connected traces across Gateway → Ingestion → NATS → Analytics → Gateway)
- Metrics → Prometheus → Grafana
- Health checks on `/health` for every service
- Swagger/OpenAPI on every .NET service

### Testing

- xUnit for .NET: unit tests (mock adapters) + integration tests (Testcontainers for Postgres + NATS)
- Vitest for Angular: component tests + fake SignalR connection
- All tests must pass in the Jenkinsfile pipeline

### Docker

- `docker-compose.infra.yml` — 7 infra containers for local development
- `docker-compose.yml` — Full 12-container stack (infra + all services)
- TransactionSimulator and n8n are demo-only, disableable via `SIMULATOR_ENABLED=false`

## Common Commands

### .NET Services (from project root)

```bash
dotnet build                              # Build entire solution
dotnet test                               # Run all tests
dotnet run --project apps/gateway         # Run Gateway
dotnet run --project apps/ingestion       # Run Ingestion Service
dotnet run --project apps/analytics       # Run Analytics Service
dotnet run --project apps/simulator       # Run TransactionSimulator
```

### Entity Framework Migrations

```bash
dotnet ef migrations add <Name> --project apps/ingestion    # Add migration
dotnet ef database update --project apps/ingestion          # Apply migrations
```

### Angular Dashboard (from `apps/web/`)

```bash
npm install                   # Install dependencies
npm start                     # Start dev server (ng serve)
npm test                      # Run Vitest tests
npm run build                 # Production build
```

### Docker Compose

```bash
docker compose -f docker-compose.infra.yml up -d    # Start infra only
docker compose -f docker-compose.infra.yml down      # Stop infra
docker compose up -d                                  # Start full stack (12 containers)
docker compose down                                   # Stop full stack
```

## Service Ports

| Service | Port | URL |
|---------|------|-----|
| API Gateway | 5000 | http://localhost:5000 |
| Ingestion Service | 5001 | http://localhost:5001 |
| Analytics Service | 5002 | http://localhost:5002 |
| TransactionSimulator | 5003 | http://localhost:5003 |
| Angular Dashboard | 4200 | http://localhost:4200 |
| PostgreSQL | 5432 | localhost:5432 |
| NATS | 4222 | localhost:4222 |
| NATS Monitor | 8222 | http://localhost:8222 |
| Jaeger UI | 16686 | http://localhost:16686 |
| Prometheus | 9090 | http://localhost:9090 |
| Grafana | 3000 | http://localhost:3000 |
| SonarQube | 9000 | http://localhost:9000 |
| n8n | 5678 | http://localhost:5678 |
