# ADR 0001: Pragmatic modular monolith

## Context
QueryMind needs a credible, demonstrable MVP without operational sprawl.

## Decision
Use one ASP.NET Core API with focused analytics and schema modules, and a separate Angular presentation application. Keep application and analytics PostgreSQL databases separate.

## Consequences
The design is deployable as a small unit while preserving a hard boundary around analytics credentials. Modular extraction remains possible if future scale warrants it.
