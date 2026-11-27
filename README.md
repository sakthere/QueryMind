# QueryMind

> **Intelligence, grounded in data.**

QueryMind is an AI-powered analytics workspace being built to help users explore relational data through natural-language questions. It is a focused portfolio project demonstrating .NET, Angular, SQL Server, secure analytics boundaries, and deliberate product design.

> **Current capability:** A reviewed local provider maps supported questions to approved SQL templates. An optional OpenAI-compatible provider can be configured locally. Every candidate statement is parsed through a SQL AST safety gate before a read-only SQL Server query runs.

## Why QueryMind?

Business questions should not require hand-written SQL for every answer. QueryMind's intended execution path is:

```mermaid
flowchart LR
    Q[Natural-language question] --> I[Interpret intent]
    I --> S[Retrieve relevant schema]
    S --> G[Generate SQL]
    G --> V[Validate query safety]
    V --> E[Execute read-only query]
    E --> R[Results, visualization, insights]
```

The application, rather than an LLM, remains in control of schema access, SQL validation, resource limits, and database execution.

## What works today

- Interactive Angular analytics workspace with responsive navigation and light/dark themes.
- Suggested revenue, category, customer, region, and cancellation analyses.
- A bounded local natural-language provider for the five curated analyses, with provider attribution.
- Curated schema context and a single bounded repair attempt for an explicitly configured external provider.
- A runnable local-provider regression suite for expected query shape and SQL safety; it never calls an external model.
- SQL Server ScriptDom AST validation: one `SELECT`, approved `analytics` tables only, no `SELECT INTO`, external rowsets, or multi-statement scripts.
- Results canvas with a visualization, tabular data, SQL preview, and execution metadata.
- Schema Explorer backed by the API's curated schema contract.
- Persistent query history in the separate `QueryMindApp` database when `QUERYMIND_APP_PASSWORD` is configured locally; session history remains available otherwise.
- SQL Server 2022 Docker environment with separate `QueryMindAnalytics` and `QueryMindApp` databases.
- Reproducible retail dataset: 2,000 customers, 100 products, and 10,000 deterministic orders.
- SQL Server `analytics_reader` login restricted to `SELECT` access on the analytics schema.

## Screenshots

Screenshots will be added from the running application after the current local preview is started. They will show actual UI states rather than mocked marketing imagery.

## Architecture

QueryMind uses a pragmatic modular monolith: one ASP.NET Core API and one Angular client, with deliberately separate application and analytics databases on SQL Server.

```mermaid
flowchart TB
    UI[Angular workspace] --> API[ASP.NET Core API]
    API --> D[Predefined SQL analytics]
    API --> SC[Schema discovery]
    API -. Next capability .-> SQ[Restricted SQL execution]
    SQ --> A[(QueryMindAnalytics)]
    API --> APP[(QueryMindApp)]
```

See the full [architecture notes](docs/architecture.md) and the [architecture decision record](docs/adr/0001-architecture-selection.md).

## Technology stack

| Area | Technology |
| --- | --- |
| API | .NET 10, ASP.NET Core Web API, C# |
| Frontend | Angular 20, TypeScript, Signals, SCSS |
| Data | SQL Server 2022, Entity Framework Core SQL Server provider |
| Local infrastructure | Docker Compose |
| Testing | xUnit foundation, Angular build/type checks |
| CI | GitHub Actions |

## Local development

### Prerequisites

- .NET 10 SDK
- Node.js 22 LTS
- Docker Desktop with Linux containers enabled

### Start the data environment

```powershell
Copy-Item .env.example .env
# Edit .env and set strong local passwords before continuing.
docker compose up -d
docker compose ps
```

This starts SQL Server on `localhost:1433`, creates both databases, seeds the analytics dataset, creates the read-only analytics login, and provisions the application-only history login. All passwords remain local to `.env`.

### Start the API and frontend

Open two terminals:

```powershell
# Terminal 1
dotnet run --project src/QueryMind.Api
```

```powershell
# Terminal 2
cd web
npm ci
npm start
```

Open `http://localhost:4200`.

## Example questions

- How has monthly revenue changed?
- Compare revenue across product categories.
- Which customers have the highest lifetime value?
- Which regions are performing best?
- What percentage of orders were cancelled?

## Security model

The current database foundation is deliberately structured for restricted analytics access:

- Application and analytics data are isolated into separate SQL Server databases.
- A dedicated `analytics_reader` login receives `SELECT` access only to the analytics schema.
- Secrets are supplied through the ignored local `.env` file, never committed.
- Model-proposed SQL is treated as untrusted input and passes through a strict AST validation boundary before execution.

## Roadmap

### Foundation

- [x] API and Angular workspace
- [x] SQL Server Docker topology and deterministic retail seed data
- [x] Schema contract and predefined read-only SQL analytics
- [x] CI and initial architecture documentation

### Secure AI analytics

- [x] Provider abstraction and bounded local question interpretation
- [x] SQL AST validation and reviewed-template execution boundary
- [ ] Configurable external provider and schema-aware SQL generation
- [ ] Bounded regeneration, grounded insights, and persistent history

### Evaluation and portfolio polish

- [ ] AI evaluation dataset and runner
- [ ] Observability and execution metrics
- [ ] End-to-end workflow coverage and deployment guide

## Known limitations

- Natural-language input maps to a bounded reviewed analysis set, not arbitrary SQL.
- The local provider only selects application-owned templates. An external provider is opt-in through local environment variables and is never enabled by committed configuration.
- External-provider evaluation and production telemetry are planned work; the built-in local-provider regression checks are available from the Evaluations screen.
- The app is an MVP foundation, not a production-ready analytics service.

## Documentation

- [Architecture](docs/architecture.md)
- [Development setup](docs/development-setup.md)
- [Roadmap](docs/roadmap.md)
- [Secure AI analytics boundary](docs/secure-ai-analytics.md)
- [Foundation development log](docs/development-log/foundation.md)
