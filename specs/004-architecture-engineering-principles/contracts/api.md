# API Contract: Architecture and Engineering Principles

## Feature Contract

This spec establishes dependency/deployment constraints. Its minimal
composition boundary references `GET /api/health`, whose canonical behavior and
handler are owned by SPEC-018; public feature shapes belong to SPEC-006 onward.
SPEC-004 owns no endpoint.

The internal `IAuditEventWriter.AppendAsync(AuditEventDraft,
CancellationToken)` contract joins the caller's current EF transaction and
never starts or commits an independent transaction. `AuditEventDraft` requires
safe actor/subject references, action, reason, redacted before/after summaries,
correlation ID, and server time. A write failure propagates so the business
transaction rolls back. SPEC-017 consumes the read side only.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Versioned updates/deletes follow SPEC-006 request-body `expectedRowVersion` and 409 `STALE_VERSION`; retryable commands follow their owner-spec idempotency contract.
- Dates use ISO 8601 and the server-configured academic term.
- Lists follow the exact SPEC-006 default-20/maximum-100 pagination and deterministic unique-ID tie-break sorting protocol.
