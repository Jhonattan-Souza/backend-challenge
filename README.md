# CNAB Processor

Financial transaction processor for CNAB files with REST API and web interface.

## Quick Start

```bash
# Clone and configure
cp .env.example .env

# Start all services
docker compose up -d

# Access
# - Frontend: http://localhost:8080
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
# - Jaeger UI: http://localhost:16686
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/cnab-files` | Upload CNAB file (multipart/form-data) |
| `POST` | `/api/v1/cnab-text` | Process CNAB as text (text/plain) |
| `GET` | `/api/v1/stores` | List stores with pagination |

### Examples

```bash
# Upload file
curl -X POST http://localhost:5000/api/v1/cnab-files \
  -H "X-Client-Id: $(uuidgen)" \
  -F "file=@sample.txt"

# Get stores with pagination
curl "http://localhost:5000/api/v1/stores?page=1&pageSize=10"

# Filter by CPF
curl "http://localhost:5000/api/v1/stores?cpf=12345678901"
```

## Project Structure

```
src/
├── Api/          # FastEndpoints, Swagger, Health Checks
├── Application/  # Use cases, Commands, Queries, DTOs
├── Domain/       # Entities, Value Objects, Repository interfaces
├── Infrastructure/ # EF Core, Repository implementations
└── Web/          # Frontend (HTML/CSS/JS)
tests/
├── Tests.Unit/        # Unit tests (Shouldly, NSubstitute)
└── Tests.Integration/ # E2E tests (Testcontainers)
```

## Tech Stack

- **.NET 10** with FastEndpoints
- **SQL Server 2022** 
- **OpenTelemetry** + Jaeger (observability)
- **Docker Compose** (orchestration)

## Development

```bash
# Run tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build API only
dotnet build src/Api
```

## Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Full health status with DB check |
| `/health/live` | Liveness probe |
| `/health/ready` | Readiness probe |

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| **Clean Architecture** | Separates Domain, Application, Infrastructure and API layers for testability and maintainability |
| **Repository + Unit of Work** | Abstracts data access, enables unit testing with mocks, coordinates transactions |
| **Result Pattern (FluentResults)** | Explicit error handling without exceptions, cleaner control flow |
| **FluentValidation in Domain** | Business rules enforced at entity creation, fail-fast validation |
| **FastEndpoints** | Minimal API alternative with built-in validation, throttling, and Swagger support |
| **CQRS-lite (Commands/Queries)** | Separates read/write operations, vertical slice organization per feature |
| **Testcontainers for E2E** | Real SQL Server in tests, no mocking infrastructure |
| **LineHash for idempotency** | SHA256 hash prevents duplicate transaction processing on re-upload |
