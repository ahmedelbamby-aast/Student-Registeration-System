# Policy Versioning and Academic Scope Contract

**Contract:** `policy-versioning/1.0`
**Governance owner:** SPEC-002
**Runtime owner:** SPEC-009

## Version identity and effective period

Every rulebook has an immutable identifier, version label, `EffectiveFromUtc`,
optional exclusive `EffectiveToUtc`, state, priority, approval actor/time, and
source-register version. Server time selects the effective period; a browser
clock is never authoritative.

Allowed lifecycle:

`Draft -> Approved -> Published -> Superseded`

Only `Published` and currently effective rulebooks may govern a decision.
Draft, merely Approved, expired, future, Superseded, missing, or inconsistent
versions fail closed.

## Academic scope

Scope fields are optional restrictions over term, campus, college, program,
cohort, and standing. Every populated field must match the authoritative
student/term context. An absent field means “not restricted on this dimension”
and never means a client may supply the value.

`DEMO-POC-2026.1` is restricted to the synthetic POC population, College of
Artificial Intelligence, Data Science program snapshot, and explicitly
configured demo terms. It is not a global production default.

## Deterministic selection

1. Filter to Published versions effective at server time.
2. Require every populated Academic scope field to match.
3. Select the matching rulebook with the highest priority.
4. If no rulebook matches, fail closed with `POLICY_UNAVAILABLE` and alert
   Admin.
5. If more than one match has equal highest priority, reject publication when
   detected; evaluation also must fail closed with `POLICY_SCOPE_AMBIGUOUS`.

The selected rulebook identifier/version is included in every result. A
published version is immutable; correction creates a later approved version
that supersedes it without changing historical decisions.
