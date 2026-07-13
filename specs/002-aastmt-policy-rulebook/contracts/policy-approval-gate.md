# Policy Approval and Fail-Closed Gate

**Contract:** `policy-approval-gate/1.0`
**Governance owner:** SPEC-002
**Runtime enforcement owner:** SPEC-009

## State gate

The only lifecycle is `Draft -> Approved -> Published -> Superseded`.

- Draft is editable but never executable.
- Approved records a human decision but is not yet active.
- Published is immutable and may govern only while effective and in scope.
- Superseded remains readable for history but cannot govern a new decision.

At evaluation, server time must select exactly one Published, approved,
effective, scope-matching rulebook. Missing, expired, future, conflicting, or
unapproved state must fail closed and produce a stable decision-data reason.

## Publication gate

Publication requires:

1. known typed rule definitions conforming to the closed schema;
2. one source/provenance record for every governed value/field;
3. resolved source conflicts inside the bounded profile;
4. a complete effective period and academic scope;
5. unique deterministic top priority for overlapping applicable context;
6. approved boundary examples; and
7. an approving actor, reason, time, and expected version.

Any validation failure rejects the entire publication; no partial rulebook is
made available.

## Authority boundary

`OfficialAASTMT` means a fact was transcribed from the recorded public AASTMT
source. `AhmedApprovedDemo` means Ahmed approved a product-owned value for this
non-production proof of concept. That approval is not official AASTMT production approval.
`SyntheticDemo` marks local data, and `UnresolvedInstitutional` can never govern
submission.

The runtime may not translate source availability, UI visibility, Admin role,
or a client-provided status into approval. A new allowance or interpretation
requires a separately versioned and approved amendment.
