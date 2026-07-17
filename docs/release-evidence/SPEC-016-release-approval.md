# SPEC-016 Bounded Demo Release Approval

**Date:** 2026-07-17
**Approved by:** Ahmed Elbamby
**Decision:** APPROVED for the non-production design-capability demo

Ahmed Elbamby is the sole developer and human approval authority for this
demo. He reviewed each accountability below as a distinct perspective. This
approval completes SPEC-016 only; it is not Gate D, production deployment,
official AASTMT authorization, or a claim that pending SPEC-018 manual/load/
recovery work has passed.

| Perspective | Decision and evidence |
|---|---|
| Product owner | APPROVED — FR-1..FR-10, AC-1..AC-8, SC-1..SC-3 and four routes trace to passing evidence. |
| Domain owner | APPROVED — Scheduling remains the canonical availability/assignment/alert owner; StaffAdministration adds projections and facades only. |
| QA | APPROVED — focused build and automated behavior, contract, integration, concurrency, client, E2E and quality suites pass. |
| Security/privacy | APPROVED — server-derived role/resource scope, privacy-safe 404, exact three-field roster, row-free audit metadata, no Admin availability mutation. |
| Accessibility | APPROVED for automated demo evidence — keyboard controls and calendar/list alternatives pass automated checks; manual NVDA remains SPEC-018/Gate-D work. |
| Data/concurrency | APPROVED — complete-range rowversion replacement, server deadline, serializable locking, one-winner stale handling, atomic alert/audit transaction. |
| Operations | APPROVED for bounded demo — clean build, safe errors/correlation, no new migration/service/queue; production load/failover/recovery remains unapproved. |

## Verified evidence

- Solution build: 0 warnings, 0 errors.
- SPEC-016 Acceptance: 8/8; edge cases: 5/5.
- Authorization: 7/7; application: 9/9; contract: 6/6;
  architecture: 3/3; integration: 18/18.
- Client unit: 10/10; client route contracts: 6/6; accessibility: 12/12;
  Playwright E2E: 13/13; quality: 7/7.
- Scope review and complete traceability: PASS.

## Accepted bounded limitation

The current demo identity schema has no distinct persisted Student display-name
column. Roster `DisplayName` therefore uses the persisted username label and
may equal UniversityId in synthetic data. This does not expand PII or alter the
three-field contract. A production identity revision requires its own approved
specification and migration.
