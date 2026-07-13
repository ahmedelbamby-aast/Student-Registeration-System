# PolicyRulebook Schema and Version Target

**Target schema:** `policy-rulebook/1.0`
**Profile:** `DEMO-POC-2026.1`
**Recorded:** 2026-07-13 by Ahmed ELbamby

- [x] Version, effective period, academic scope, and deterministic priority are
  required.
- [x] Approval state, actor, time, and demo-only authority are explicit.
- [x] Rule categories reference the governed typed-rule registry.
- [x] Source facts, demo approvals, synthetic fields, and unresolved
  institutional values have distinct provenance classifications.
- [x] Unknown/missing policy, equal applicable priority, and unresolved source
  conflict fail closed.
- [x] Published rulebooks are immutable and superseded by another version.
- [x] SPEC-009 owns runtime PolicySet/PolicyRule evaluation and SPEC-015 owns
  durable decision snapshots.

Canonical rulebook publication remains deferred until the rulebook behavior
tests required by T020, T022, T024, T026, T028, and T032 have failed for their
expected missing-artifact reasons.
