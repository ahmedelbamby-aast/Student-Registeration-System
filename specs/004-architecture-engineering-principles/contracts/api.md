# API Contract: Architecture and Engineering Principles

## Feature Contract

This spec establishes dependency/deployment constraints. Its minimal
composition boundary includes GET /api/health; public feature shapes belong to
SPEC-006 onward.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
