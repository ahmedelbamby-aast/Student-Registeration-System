# SPEC-018 Traceability and Evidence Matrix

**Artifact version:** 1.0.0

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** BLOCKED

**Production authority:** Not granted

This matrix is complete for the current working tree. A `BLOCKED` row rejects
release; automated evidence never substitutes for the manual NFR-8 record.

## Functional and non-functional requirements

| Requirement | Passing evidence | Status |
|---|---|---|
| FR-1 | `.github/workflows/ci.yml`; release and migration gate tests | PASS |
| FR-2 | 133/133 critical behavior tests; `SPEC-018-NFR-9-coverage.json` | PASS |
| FR-3 | operations health/log/metric/trace tests and NFR-5 evidence | PASS |
| FR-4 | `SPEC-018-load-results.json`; NFR-2 through NFR-6 tests | PASS |
| FR-5 | `SPEC-018-NFR-7-recovery.json`; recovery rehearsal 8/8 | PASS |
| FR-6 | secret, privacy, certificate, and shared-key security tests | PASS |
| FR-7 | two-replica failover/restart and shared Data Protection key tests | PASS |
| FR-8 | 36/36 browser-route matrix; automated NVDA probe; manual record unsigned | BLOCKED |
| FR-9 | reviewed `docs/security/THREAT_MODEL.md` and negative authorization tests | PASS |
| NFR-1 | `SPEC-018-NFR-1.md` | PASS |
| NFR-2 | `SPEC-018-NFR-2.md`; exact 10-minute target and 60-second spike | PASS |
| NFR-3 | `SPEC-018-NFR-3.md`; catalogue 35.9788 ms, commit 27.5353 ms | PASS |
| NFR-4 | `SPEC-018-NFR-4.md`; zero target/spike/failover invariants | PASS |
| NFR-5 | `SPEC-018-NFR-5.md`; two stateless replicas and restart | PASS |
| NFR-6 | `SPEC-018-NFR-6.md`; 0/225,000 unexpected failures | PASS |
| NFR-7 | `SPEC-018-NFR-7.md`; RPO 1 second, RTO 2 seconds | PASS |
| NFR-8 | `SPEC-018-NFR-8-browser-matrix.json`; `SPEC-018-NFR-8-nvda-probe.json`; manual record unsigned | BLOCKED |
| NFR-9 | `SPEC-018-NFR-9.md`; owner branch rates 93.60%, 90.32%, 92.86% | PASS |

## Success and acceptance criteria

| Criterion | Evidence | Status |
|---|---|---|
| SC-1 | exact target, spike, collision, failover, and invariant artifacts | PASS |
| SC-2 | automated browser/NVDA and security/privacy evidence; human UX/QA sign-off absent | BLOCKED |
| SC-3 | recovery rehearsal and integrity/reconciliation evidence | PASS |
| AC-1 | `AC-1Tests`; NFR-2/3/4/5/6 artifacts | PASS |
| AC-2 | `AC-2Tests`; spike degradation and invariant evidence | PASS |
| AC-3 | `AC-3Tests`; NFR-7 recovery artifact | PASS |
| AC-4 | `AC-4Tests`; `SPEC-018-screen-reader-manual.md` | BLOCKED |
| AC-5 | reviewed threat model and security tests; final approval record pending | BLOCKED |
| AC-6 | ordered CI gates and ready isolated Testing lifecycle contracts | PASS |
| AC-7 | operational NFR evidence passes; final traceability and approvals blocked | BLOCKED |

## Edge cases

| Edge case | Executable evidence | Status |
|---|---|---|
| EC-1 | `EC-1Tests` exporter failure and fallback behavior | PASS |
| EC-2 | `EC-2Tests` replica removal/shared auth/database continuity | PASS |
| EC-3 | `EC-3Tests` SQL failure, health, safe reference, no partial result | PASS |
| EC-4 | `EC-4Tests` compatibility and environment guard rejection | PASS |
| EC-5 | `EC-5Tests` permission removal invalidates existing session | PASS |

## Critical route matrix

Every route below passed automated axe, keyboard/focus, 44 CSS pixel, and
responsive checks in Chrome, Edge, Firefox, and Playwright WebKit. Each remains
blocked for release until the representative manual Windows/NVDA record is
completed and signed.

| Route | Automated test | Automated | Manual NVDA/keyboard | Status |
|---|---|---|---|---|
| AUTH-02 | `StudentLoginPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| AUTH-04 | `StaffLoginPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STU-02 | `SubjectDiscoveryPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STU-04 | `ScheduleBuilderPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STU-05 | `RegistrationReviewPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STU-06 | `RegistrationRecordsPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STF-01 | `StaffDashboardPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STF-03 | `StaffRosterPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |
| STF-04 | `StaffAvailabilityPageAccessibilityTests` | PASS | NOT EXECUTED | BLOCKED |

## Release rejection

Release is rejected until the manual NFR-8 evidence is executed and signed,
the repository-wide CI sweep is green, and every approval in
`SPEC-018-release-approval.md` is approved.
