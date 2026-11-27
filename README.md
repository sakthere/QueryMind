# QueryMind

**From natural language to actionable intelligence.** QueryMind is a portfolio-grade analytics workspace for relational data exploration.

## Day 1 status

The foundation milestone provides a polished Angular workspace, deterministic predefined analytics, API contracts, database schema discovery, and a reproducible PostgreSQL retail dataset. The UI labels these analyses as demo fixtures; LLM-generated SQL, secure execution, insights, and history are intentionally deferred to Day 2.

## Run locally

1. `docker compose up -d` starts the two PostgreSQL databases and seeds 10,000 deterministic retail orders.
2. `dotnet run --project src/QueryMind.Api --urls http://localhost:5080` starts the API.
3. In `web`, run `npm install` then `npm start`, and open `http://localhost:4200`.

See [development setup](docs/development-setup.md) and [architecture](docs/architecture.md).
