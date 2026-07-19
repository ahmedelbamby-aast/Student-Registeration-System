# SPEC-001 Charter and Journey Evidence

**Evidence state:** PASS at the bounded SPEC-001 charter scope
**Recorded:** 2026-07-19
**Production authority:** Not granted

## FR-4 — end-to-end student journey

**Status:** PASS.

The delivered modular-monolith surface supports the bounded student journey
from authenticated entry to one authoritative atomic registration receipt.
The owner-spec route and runtime suites remain authoritative for each step;
SPEC-001 aggregates them without copying their implementation.

| Step | Route/operation | Runtime owner | Executable evidence | Result |
|---|---|---|---|---|
| Student entry | AUTH-02 `/student/login` | SPEC-007 | `StudentLoginPageFeatureTests`; identity endpoint authorization matrix | PASS |
| Discover eligible subjects | STU-02 `/student/subjects` | SPEC-011 | `SubjectDiscoveryPageFeatureTests`; offering/eligibility application tests | PASS |
| Build a conflict-safe plan | STU-04 `/student/schedule` | SPEC-012 | `ScheduleBuilderPageFeatureTests`; plan concurrency and conflict tests | PASS |
| Validate and submit once | STU-05 `/student/review` | SPEC-014 | `RegistrationReviewPageFeatureTests`; submission endpoint and idempotency tests | PASS |
| Read authoritative outcome | STU-06 `/student/registration/result/{id}` | SPEC-015 | `RegistrationResultPageFeatureTests`; receipt model/projection tests | PASS |

The SQL allocator documents and executes one commit boundary for the
idempotency claim, capacity counters, enrollments, audit, and final result.
`SPEC-018-load-results.json` supplies the measured contested-write proof: the
10-minute target, 60-second spike, and 100-request/30-seat collision profiles
all report zero overbooking, zero duplicate active enrollment, zero partial
atomic submissions, and zero total invariant violations. STU-06 then projects
the accepted result from the authoritative stored submission snapshot.

This is a bounded aggregate proof across the canonical owner-spec suites. It
does not claim that route doubles are a production identity or SQL deployment,
and it grants no production release authority.

## FR-6 — MVP scope parity

**Status:** PASS.

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

**Status:** PASS.

An implementation story must name both `SPEC-NNN/FR-N` and `SPEC-NNN/AC-N`, and
the referenced feature must have human approval for implementation. Rejected
when either link is missing or unapproved. Gate, setup, and governance tasks may
instead use their explicit dependency/gate/artifact trace tag because they do
not assert product behavior.

Evidence:

- `CharterJourneyTests.Fr_7_delivery_admission_requires_approved_fr_and_ac_traceability`
- the Definition of Ready in `docs/PROJECT_PLAN.md`
- the exact trace tags and file targets in each approved `tasks.md`
