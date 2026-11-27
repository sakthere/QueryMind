# Development setup

Prerequisites: Docker, .NET 10 SDK, Node 20.19+ or 22.12+ (Angular 20's supported release lines).

Copy `.env.example` to `.env`, set strong local-only passwords, then run `docker compose up -d`. It initializes the separate `QueryMindAnalytics` and `QueryMindApp` databases once. To recreate seeded data, remove only the `querymind_sqlserver-data` Docker volume, then run Compose again. Never use the local development database credentials outside local Docker.

The current host supplies Node 23, which Angular does not list as supported; use Node 22 LTS for frontend installation and verification.
