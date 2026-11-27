# Secure analytics development log

## Delivered boundary

- Added `POST /api/analysis` for a natural-language analytics request.
- Added a provider interface and a local reviewed-template provider for supported retail questions.
- Added SQL Server ScriptDom parsing and tests that reject mutation, unapproved schemas, multi-statement input, and `SELECT INTO`.
- Preserved the established SQL Server read-only login and ten-second command timeout.
- Updated the Angular workspace to submit a question to the analysis endpoint and display the pipeline's attributable insight.

## Verification

- `dotnet build tests/QueryMind.Api.Tests/QueryMind.Api.Tests.csproj --configuration Release --no-restore` succeeded with zero errors.
- `npm run build` was started using Node 22.12.0; the Angular builder did not return a final completion line in the managed shell, so it is not recorded as a pass.
- Package restore reports existing transitive `System.Security.Cryptography.Pkcs` vulnerability advisories. They should be resolved by updating the dependency chain before a production deployment.

## Deliberate scope boundary

The local provider chooses only reviewed SQL templates. No external model credentials are configured and no model-generated SQL is executed. Persistent history, configurable external providers, regeneration, and formal evaluations remain intentionally open.
