# Architecture

QueryMind is a pragmatic modular monolith. The API separates analytics contracts and predefined read-only execution from schema discovery. The Angular client consumes HTTP contracts and owns presentation state. SQL Server has separate application and analytics databases; the analytics schema has an explicit read-only login.

```mermaid
flowchart LR
 UI[Angular workspace] --> API[ASP.NET Core API]
 API --> D[Day 1 predefined SQL analytics]
 API --> S[Schema discovery]
 D -. Day 2 .-> Q[Validated read-only SQL]
 Q --> A[(Analytics SQL Server)]
 API --> APP[(Application SQL Server)]
```
