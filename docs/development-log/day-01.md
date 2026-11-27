# Day 01 development log

Implemented the initial API, Angular workspace source, Docker SQL Server topology, deterministic retail schema/seed script, and architectural docs. Node 22 LTS is installed and the frontend production bundle builds successfully. The Day 1 API executes bounded, application-owned read-only SQL Server statements through the dedicated analytics login. Natural-language-to-SQL remains deferred to Day 2.

## Final verification

- Docker Compose reported SQL Server healthy; the initialization service seeded 10,000 orders.
- The restricted `analytics_reader` login returned the expected order count and real monthly revenue aggregates.
- `POST /api/analytics/execute/monthly-revenue` returned twelve live SQL Server rows with `SQL Server read-only query` metadata.
- API Release build passed.
- xUnit passed: 2 tests, 0 failures.
- Angular production build passed under Node 22 LTS.

The README screenshots section remains reserved for genuine application captures. The local preview was run and inspected, but the repository capture mechanism is isolated from desktop localhost and cannot write browser screenshots into the working tree.
