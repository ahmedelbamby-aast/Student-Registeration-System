# Admin Confirmation Conformance

SPEC-017 preserves owner-specific concurrency protocols. It does not introduce
a generic `AdminConfirmationService`.

An owner confirmation token, where required, is bound to the actor, operation
scope, canonical payload, expected aggregate version, dependency versions, and
expiry. Confirmation rechecks current permission and current versions. Stale
input, scope, permission, dependency, or expiry is rejected. The same
idempotency key and payload replays the committed result; a different payload
returns `IDEMPOTENCY_KEY_REUSED`; concurrent current confirmations have one
winner.

Commands without a preview token still use their owner’s required rowversion
and explicit UI confirmation. Identity role reductions additionally serialize
through `AdminSecurityGuard`, preventing final-Admin write skew across replicas.

Executable evidence is in `AdminConfirmationConcurrencyTests` and the linked
SPEC-007/SPEC-009 owner tests. Focused result on 2026-07-17: 3/3 green.
