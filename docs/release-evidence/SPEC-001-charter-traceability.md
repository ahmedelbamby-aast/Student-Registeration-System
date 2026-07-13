# SPEC-001 Charter and Traceability Evidence

**Evidence state:** PARTIAL — governance controls verified; downstream runtime
and Gate C evidence pending
**Recorded:** 2026-07-13

## FR-4 — end-to-end student journey

**Status:** Pending. The student login-to-atomic-registration-receipt journey is
implemented across SPEC-003 and SPEC-007 through SPEC-015 and is accepted at
Gate C. This document does not claim that downstream runtime evidence early.

## FR-6 — MVP scope parity

**Status:** Verified by conformance test.

The SPEC-001 MVP boundary matches `docs/PROJECT_PLAN.md`: student activation and
login, shared staff login, role-scoped workspaces, authoritative academic
context, policy-aware discovery, conflict-safe planning/recommendations, atomic
registration, records, administration, and operations. OS-1 through OS-4 retain
the declared exclusions: payments/grades/attendance/waitlists/advisor workflow/
notifications, public staff registration, client-only authorization, and
multi-tenancy/native mobile applications.

Evidence:

- `CharterJourneyTests.Fr_6_mvp_scope_and_non_goals_match_the_project_plan`
- `specs/001-product-charter-rbac/requirements.md`
- `docs/PROJECT_PLAN.md`

## FR-7 — delivery admission traceability

**Status:** Verified by conformance test.

An implementation story must name both `SPEC-NNN/FR-N` and `SPEC-NNN/AC-N`, and
the referenced feature must have human approval for implementation.
Rejected when either link is missing or unapproved. Gate, setup, and governance tasks may
instead use their explicit dependency/gate/artifact trace tag because they do
not assert product behavior.

Evidence:

- `CharterJourneyTests.Fr_7_delivery_admission_requires_approved_fr_and_ac_traceability`
- the Definition of Ready in `docs/PROJECT_PLAN.md`
- the exact trace tags and file targets in each approved `tasks.md`

## Remaining evidence dependencies

- SPEC-007: runtime role/session and no-role/dual-role behavior.
- SPEC-003 and SPEC-007 through SPEC-017: AC-4/FR-4 end-to-end journeys.
- SPEC-018: accessibility, authorization, architecture, scalability, security,
  recovery, and operations measurements.
