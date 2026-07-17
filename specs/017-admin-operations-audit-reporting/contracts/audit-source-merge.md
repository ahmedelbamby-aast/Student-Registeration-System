# Audit Source Merge and Redaction Contract

## Ownership and direction

SPEC-017 reads the canonical SPEC-004 `AuditEvent` and SPEC-007
`SecurityEvent` streams through one narrow `IAdminAuditReader`. It owns no
audit entity, security entity, EF mapping, append method, update/delete path,
or transaction writer. Sensitive owner-feature commands continue to append
through SPEC-004's `IAuditEventWriter` in the caller's SQL transaction.

## Scope before paging

The authenticated endpoint supplies an explicit administrative row scope.
The SQL reader applies that subject-reference scope to both streams before
counting, ordering, skipping, or taking rows. Access to the Identity security
stream is a separate scope flag; a caller without that flag receives no
security rows and no indication that omitted rows exist. The reader never
accepts a caller-provided unrestricted default.

Optional occurred-time, actor, action, and source-stream filters are applied
as parameterized LINQ predicates. Results order by `OccurredAtUtc DESC`, then
the canonical event `Id DESC`, before page boundaries are applied. Page 1 and
size 20 are defaults; size 100 is the hard maximum.

## Bounded field projection

Only actor reference, safe actor display, action, entity type/identifier,
reason, bounded before/after summaries, correlation ID, event ID/time, and
source stream leave the reader. `SecurityEvent.MetadataJson`, credentials,
tokens, secrets, raw exceptions, and unbounded JSON never enter the result.

Before/after JSON is parsed defensively. Only these scalar fields are exposed:
`state`, `status`, `enabled`, `roles`, `capacity`, `enrolledCount`, `version`,
`lifecycle`, `termId`, `groupId`, `offeringId`, `policySetId`, `reasonCode`, and
`source`. At most 20 fields are returned; names and display values are bounded.
Malformed JSON, nested values, unknown names, and sensitive names are omitted.
Every result identifies redaction version `spec017-v1`.

## Source normalization

- SPEC-004 rows use source stream `audit`, their canonical action/entity
  fields, and their canonical before/after JSON.
- SPEC-007 rows use source stream `identity-security`, `EventType` as action,
  entity type `SecurityEvent`, and the application-user identifier when
  present, otherwise the bounded subject reference, as entity identifier.
- Both sources retain their canonical ID, reason, correlation ID, and server
  occurrence time.

## Append-only and rollback guarantees

The public port exposes `ReadAsync` only. The SQL adapter uses
`AsNoTracking`, contains no `Add`, `Update`, `Remove`, `SaveChanges`, raw SQL,
or writer dependency, and is registered later only as a query adapter.

SPEC-004's existing real-SQL fault test remains the atomicity authority: when
audit persistence fails, the enclosing business transaction rolls back both
business and audit state. SPEC-017 conformance rejects any second
`IAuditEventWriter` implementation or StaffAdministration audit writer.
