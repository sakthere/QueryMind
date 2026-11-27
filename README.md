# QueryMind

**From natural language to actionable intelligence.** QueryMind is a portfolio-grade analytics workspace for relational data exploration.

## Day 1 status

The foundation milestone provides a polished Angular workspace, deterministic predefined analytics, API contracts, database schema discovery, and a reproducible SQL Server retail dataset. The UI labels these analyses as demo fixtures; LLM-generated SQL, secure execution, insights, and history are intentionally deferred to Day 2.

## Run locally

1. Copy `.env.example` to `.env` and replace both passwords with strong local-development values.
2. `docker compose up -d` starts SQL Server, creates the separate `QueryMindAnalytics` and `QueryMindApp` databases, and seeds 10,000 deterministic retail orders.
2. `dotnet run --project src/QueryMind.Api` starts the API at `http://localhost:5205`.
3. In `web`, run `npm install` then `npm start`, and open `http://localhost:4200`.

See [development setup](docs/development-setup.md) and [architecture](docs/architecture.md).
