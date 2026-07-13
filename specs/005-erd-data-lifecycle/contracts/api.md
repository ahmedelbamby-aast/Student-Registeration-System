# API Contract: ERD and Data Lifecycle

## Feature Contract

Database design is exposed only through approved feature endpoints. SPEC-015
owns the student registration-record resource, and SPEC-006 owns shared DTO
rules. SPEC-005 owns no route or handler.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Versioned updates/deletes follow SPEC-006 request-body `expectedRowVersion` and 409 `STALE_VERSION`; retryable commands follow their owner-spec idempotency contract.
- Dates use ISO 8601 and the server-configured academic term.
- Lists follow the exact SPEC-006 default-20/maximum-100 pagination and deterministic unique-ID tie-break sorting protocol.
