# API Contract: Product Charter and RBAC

## Feature Contract

Detailed contracts belong to SPEC-006 and feature specs. The charter's
role/context boundary is observed through `GET /api/context`, whose canonical
handler owner is SPEC-008; SPEC-001 owns no endpoint.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Versioned updates/deletes follow SPEC-006 request-body `expectedRowVersion` and 409 `STALE_VERSION`; retryable commands follow their owner-spec idempotency contract.
- Dates use ISO 8601 and the server-configured academic term.
- Lists follow the exact SPEC-006 default-20/maximum-100 pagination and deterministic unique-ID tie-break sorting protocol.
