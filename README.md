<div align="center">

# OpsDesk

> A lightweight IT service management platform built with Angular and ASP.NET Core.
> It models a realistic incident-management workflow, role-based access, SLA tracking,
> audit history, notifications and real-time updates.

<p align="center">

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512bd4.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Angular](https://img.shields.io/badge/Angular-latest-dd0031.svg)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178c6.svg)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10-8a2be2.svg)](https://learn.microsoft.com/en-us/ef/core/)
[![CI](https://img.shields.io/badge/CI-GitHub%20Actions-2088ff.svg)](.github/workflows/ci.yml)

</p>
</div>

---

## What is OpsDesk?

Small organizations frequently manage IT issues through email, chat messages or spreadsheets, requests get lost, nobody knows who owns an issue, users can't see status, and there's no audit trail. OpsDesk centralizes this into a single application: **every IT request has an owner, a priority, a status, a deadline and a history.**

Employees report incidents and requests. Technicians claim tickets, work them through a controlled lifecycle, and communicate with users via public comments while keeping internal notes private. Administrators manage users, categories, SLA rules and have full visibility over the operation.

The project was built as a focused demonstration of production-oriented full-stack development: authentication, authorization, business rules, relational data modeling, a REST API, background processing, real-time updates, automated tests, CI/CD and a production deployment, not as a generic CRUD tutorial.

---

## Domain Model

```
User
 ├── TicketsCreated
 ├── TicketsAssigned
 ├── Comments
 ├── Notifications
 └── AuditEntries

Ticket
 ├── Category
 ├── Creator
 ├── Assignee
 ├── Comments (Public / Internal)
 └── AuditEntries
```

Deactivating a user never removes their historical tickets or audit records.

### Ticket lifecycle

```
NEW → ASSIGNED → IN_PROGRESS → RESOLVED → CLOSED
                                   │
                                   ▼
                               REOPENED → IN_PROGRESS
```

Invalid transitions (e.g. `CLOSED → IN_PROGRESS` without an explicit reopen) are rejected server-side. Every priority (`Low`/`Medium`/`High`/`Critical`) carries a configurable SLA; each open ticket is continuously evaluated as `ON_TRACK`, `AT_RISK` or `BREACHED`.

---

## Architecture

### Backend

```
Angular
   │  HTTPS
   ▼
ASP.NET Core Middleware   (routing, auth, exception handling)
   │
   ▼
Endpoint / Controller       (binds request → DTO, validates)
   │
   ▼
Application                 (use cases: CreateTicket, ChangeTicketStatus, AssignTicket...)
   │
   ▼
Domain                      (Ticket, Comment, AuditEntry — the business rules live here)
   │
   ▼
Infrastructure (EF Core)    (PostgreSQL, background jobs, SignalR hubs)
   │
   ▼
PostgreSQL
```

```
OpsDesk/
│
├── src/
│   ├── OpsDesk.Api/              # Endpoints, auth, DI, middleware, OpenAPI
│   ├── OpsDesk.Application/      # Use cases / application services
│   ├── OpsDesk.Domain/           # Entities + business rules — no framework dependencies
│   └── OpsDesk.Infrastructure/   # EF Core, PostgreSQL, background jobs, SignalR
│
├── tests/
│   ├── OpsDesk.UnitTests/        # Domain + Application business rules
│   └── OpsDesk.IntegrationTests/ # Test host + Testcontainers + real PostgreSQL
│
├── frontend/
│   └── opsdesk-web/              # Angular app (core/, features/, shared/)
│
├── .github/workflows/            # CI + deployment pipelines
├── docker-compose.yml
└── README.md
```

A lightweight layered architecture is used deliberately, no Repository Pattern on top of EF Core, no CQRS/MediatR, no microservices. The domain and business rules (state machine, SLA calculation, authorization policies) are what this project is meant to demonstrate, not infrastructure ceremony.

### Frontend

```
src/app/
│
├── core/          # auth/, http/ (interceptors), guards/ (UX convenience only)
├── features/       # auth/, dashboard/, tickets/, notifications/, admin/
└── shared/         # components/, models/, ui/
```

State is handled with Angular Signals + RxJS + services — no NgRx unless the application genuinely needs it. Route guards mirror backend policies for UX, but **authorization is always enforced server-side**; the frontend boundary is never trusted as the real security boundary.

---

## API Overview

Base path: `/api`

### Auth

| Method | Route            | Description                    |
| ------ | ---------------- | ------------------------------ |
| POST   | `/auth/register` | Register (Employee by default) |
| POST   | `/auth/login`    | Login                          |
| POST   | `/auth/logout`   | Logout                         |
| GET    | `/me`            | Current user                   |

### Tickets

| Method | Route                    | Description                          | Notes                          |
| ------ | ------------------------ | ------------------------------------ | ------------------------------ |
| GET    | `/tickets`               | List (filters + search + pagination) | server-side pagination         |
| POST   | `/tickets`               | Create ticket                        | calculates SLA deadline        |
| GET    | `/tickets/{id}`          | Get ticket                           | ownership-checked              |
| PATCH  | `/tickets/{id}`          | Update fields / status               | rejects invalid transitions    |
| POST   | `/tickets/{id}/assign`   | Assign to a technician               |                                |
| POST   | `/tickets/{id}/claim`    | Self-assign (Technician)             |                                |
| POST   | `/tickets/{id}/comments` | Add public or internal comment       | internal hidden from Employees |
| POST   | `/tickets/{id}/resolve`  | Resolve ticket                       |                                |
| POST   | `/tickets/{id}/reopen`   | Reopen a resolved ticket             |                                |

**Supported filters on `GET /tickets` (combinable):**

```
?status=InProgress
?priority=Critical
?categoryId=3
?assigneeId=7
?from=2026-08-01&to=2026-08-31
?slaState=AtRisk
```

### Dashboard, categories, notifications, audit

| Method | Route            | Description                                                    |
| ------ | ---------------- | -------------------------------------------------------------- |
| GET    | `/dashboard`     | Open/critical/unassigned counts, SLA breaches, avg. resolution |
| GET    | `/categories`    | List categories                                                |
| POST   | `/categories`    | Create category (Administrator)                                |
| GET    | `/notifications` | Current user's notifications                                   |
| GET    | `/audit`         | Audit log (Administrator)                                      |

Example `GET /api/dashboard` response:

```json
{
  "openTickets": 24,
  "critical": 2,
  "unassigned": 5,
  "slaBreached": 3,
  "resolvedToday": 9,
  "averageResolutionHours": 6.4
}
```

**Error contract (standard, via middleware):**

```json
{
  "type": "validation_error",
  "message": "The ticket could not be created.",
  "errors": { "title": ["Title is required."] },
  "traceId": "a0f27d..."
}
```

---

## Getting Started (development)

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) + [Angular CLI](https://angular.dev/tools/cli)
- PostgreSQL 16 (local install or Docker)
- Docker & Docker Compose (optional, for local Postgres)

### Setup

```bash
git clone https://github.com/vvasconceloss/opsdesk.git
cd opsdesk
```

Create `.env` from example (used by `docker-compose.yml`):

```bash
# .env
DB_NAME=opsdesk
DB_USER=opsdesk_admin
DB_PASSWORD=your_secure_password
```

Start PostgreSQL (Docker):

```bash
docker compose up -d
```

#### Backend

```bash
cd src/OpsDesk.Api
dotnet restore
dotnet build
```

Configure local secrets (outside version control):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5433;Database=opsdesk;Username=opsdesk_admin;Password=your_secure_password"
dotnet user-secrets set "Auth:Secret" "a-long-random-development-only-value"
```

Apply migrations and run:

```bash
dotnet ef database update
dotnet run
```

The API will be available at `http://localhost:5099` (and `https://localhost:7115`), with the OpenAPI UI at `/scalar/v1` (or `/swagger`) in Development.

#### Frontend

```bash
cd frontend/opsdesk-web
npm install
ng serve
```

The Angular app will be available at `http://localhost:4200`, configured to talk to the local API.

### Required environment variables (production)

No secrets are committed to Git. The deployed environment must define:

```
DATABASE_CONNECTION_STRING
AUTH_SECRET
CORS_ORIGINS
```

---

## Stack

```
Backend       C# 14, ASP.NET Core 10 Web API, EF Core 10, layered architecture
              (Domain/Application/Infrastructure/Api), policy-based authorization,
              FluentValidation, Serilog, SignalR, background hosted services

Frontend      Angular, TypeScript, Signals, RxJS, Angular Router, Angular Forms

Database      PostgreSQL 16 — relational modeling, FKs with explicit delete
              behavior, GroupBy/Count aggregations, migrations

Engineering   Authentication, server-side authorization, state machines, SLA
              calculation, audit logging, real-time updates (SignalR), xUnit,
              Testcontainers, GitHub Actions CI/CD, Docker
```

---

## Project Philosophy

- **Small product, real problem**: scope is deliberately constrained.
- **Business rules over CRUD**: the value is in workflows, authorization, SLA handling and state transitions.
- **Boring infrastructure**: simple technologies that solve the actual problem.
- **Production-shaped, not production-sized**: professional practices, without pretending to be an enterprise platform.

OpsDesk isn't trying to compete with Jira, ServiceNow or Zendesk. It answers one question: _can this be built as a complete, secure and deployable business application, end to end?_

---

## License

This project is licensed under the MIT License.
