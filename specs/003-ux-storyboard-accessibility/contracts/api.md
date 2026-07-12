# API Contract: UX Storyboard and Accessibility

## Feature Contract

UI consumes contracts in SPEC-006 through SPEC-017, including GET
/api/context. UI state mapping from each stable error/reason code is mandatory
in those specs.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
