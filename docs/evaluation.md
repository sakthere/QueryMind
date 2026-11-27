# Evaluation approach

The Evaluations screen runs five deterministic regression cases against the local reviewed provider. Each case verifies both the expected query shape and SQL AST validation. The run is intentionally local and does not contact an external provider, so it cannot consume API credits.

Current checks cover revenue trend, category comparison, customer value, regional performance, and order status. They are a regression guard for the reviewed provider, not a claim of broad model-quality measurement.

Future work: isolated evaluation credentials, a versioned benchmark dataset, execution correctness checks, latency distribution, failure taxonomy, and external-provider comparison.

## Runtime metrics

The Settings screen displays an in-memory summary of analysis outcomes, attempts, duration, and row counts. The API does not record question text, SQL, result values, credentials, or provider request payloads in these metrics. Metrics reset when the API restarts.
