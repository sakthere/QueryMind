# Secure AI analytics boundary

QueryMind treats model output as untrusted input. The currently shipped local provider is deliberately bounded: it maps familiar analytics questions to reviewed query templates. It does not execute text supplied by a user or model directly.

Each natural-language request follows this path:

1. A provider selects a candidate SQL statement and identifies itself.
2. SQL Server ScriptDom parses the statement into an AST.
3. The safety gate accepts exactly one `SELECT`, rejects `SELECT INTO`, external rowsets, multi-statement scripts, and unapproved tables, and only permits the `analytics` schema's curated tables.
4. Only a reviewed template whose exact SQL matches the approved catalog is executed in this release.
5. The existing `analytics_reader` SQL Server login runs the statement with `SELECT` access only and a ten-second command timeout.
6. The response labels its provider and grounds its summary in the returned rows.

This boundary is intentionally more restrictive than the future configurable model-provider interface. An external provider must use the same parser gate, restricted executor, row limit, and audit trail before it can be enabled.

## Current limitations

- The local provider understands the five curated retail analyses only.
- Query history persistence, external model configuration, regeneration, and evaluation measurements remain upcoming work.
- The current API does not claim arbitrary natural-language-to-SQL support.
