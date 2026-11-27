# Architecture

QueryMind is a pragmatic modular monolith. The API separates analytics contracts and deterministic execution from schema discovery. The Angular client consumes HTTP contracts and owns presentation state. PostgreSQL has separate application and analytics databases; the analytics schema has an explicit read-only role.

```mermaid
flowchart LR
 UI[Angular workspace] --> API[ASP.NET Core API]
 API --> D[Day 1 deterministic analytics]
 API --> S[Schema discovery]
 D -. Day 2 .-> Q[Validated read-only SQL]
 Q --> A[(Analytics PostgreSQL)]
 API --> APP[(Application PostgreSQL)]
```
