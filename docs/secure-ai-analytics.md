# Secure AI analytics boundary

QueryMind treats model output as untrusted input. The currently shipped local provider is deliberately bounded: it maps familiar analytics questions to reviewed query templates. It does not execute text supplied by a user or model directly.

Each natural-language request follows this path:

1. A provider selects a candidate SQL statement and identifies itself.
2. SQL Server ScriptDom parses the statement into an AST.
3. The safety gate accepts exactly one `SELECT`, rejects `SELECT INTO`, external rowsets, multi-statement scripts, and unapproved tables, and only permits the `analytics` schema's curated tables.
4. The AST-approved statement is executed through the restricted reader connection. The local provider selects reviewed templates; an external provider must pass the identical gate.
5. The existing `analytics_reader` SQL Server login runs the statement with `SELECT` access only and a ten-second command timeout.
6. The response labels its provider and grounds its summary in the returned rows.

When an external provider's first SQL proposal is rejected by the safety gate, QueryMind permits at most one repair request. That request includes the rejected SQL and the safety reason, then the replacement must pass the identical AST validation and restricted execution path. A second failure is returned as a blocked result; it is never executed.

Only the curated table-and-column contract is supplied as schema context to an external provider. Database connection values, local environment values, and application history are never included in the prompt.

This boundary is intentionally more restrictive than the future configurable model-provider interface. An external provider must use the same parser gate, restricted executor, row limit, and audit trail before it can be enabled.

## Current limitations

- The local provider understands the five curated retail analyses only.
- An OpenAI-compatible provider is optional and only activates when all three local settings (`AI__BASE_URL`, `AI__MODEL`, and `AI__API_KEY`) are supplied. No default setting makes a model API call.
- Query history is saved in the separate application database when its local password is configured. Regeneration and evaluation measurements remain upcoming work.
