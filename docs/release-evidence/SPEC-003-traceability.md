# SPEC-003 Release Traceability Matrix

**Evidence date:** 2026-07-19  
**Scope:** non-production design-capability demo  
**Release decision:** WITHHELD while any required, non-waived row below is not PASS

**2026-07-21 amendment:** the canonical scope is now exactly 30 routes. The
historical 27-route PASS statements below apply only to the v1 baseline.
STU-09, ADM-10, and STF-05 have approved design records but their five-layer
executable evidence remains PENDING; release therefore remains withheld.

Status is fail-closed: a created test, an approval to execute work, or an
automated persona does not count as passing evidence unless the normative
clause it maps to is actually satisfied.

## Functional requirements

| Requirement | Primary executable / governed evidence | Status |
|---|---|---|
| FR-1 | 27 Page Design Records; route contract/component/E2E suites; route matrix below | PASS |
| FR-2 | `design/pages/*.md`, implementation-readiness hash manifest, Gate A and contributor baselines | PASS |
| FR-3 | `design-tokens.json`, generated CSS, schema tests, visual baseline manifest | PASS |
| FR-4 | Client unit suites for AppShell, navigation, forms, data, scheduling, feedback, overlay, status | PASS |
| FR-5 | Per-route contract/component/E2E state matrices | PASS |
| FR-6 | `ConflictPanelTests`, STU-04/STU-05 component and browser conflict journeys | PASS |
| FR-7 | Registration review component/browser suites: blockers, pending state, dialog, duplicate prevention | PASS |
| FR-8 | ScheduleCalendar/ScheduleList tests and STU-04/STF-02 parity browser assertions | PASS |
| FR-9 | AUTH-02/03/04/05 contract, component, and E2E suites | PASS |
| FR-10 | AppShell/client context projections and stale/server-time route journeys | PASS |
| FR-11 | 27 Page Design Records plus six-profile accessibility matrices | PASS |
| FR-12 | Contract, component, E2E, accessibility, and visual route families | PASS |
| FR-13 | Page Design Record test IDs, this matrix, and route matrix below | PASS |
| FR-14 | Version-pinned dependency contracts, typed API clients, server-authority and stale-response tests | PASS |

## Non-functional requirements

| Requirement | Evidence | Status |
|---|---|---|
| NFR-1 | `NFR-1EvidenceTests.cs`, 27 route accessibility suites, `SPEC-003-NFR-1.md` | PASS |
| NFR-2 | `NFR-2EvidenceTests.cs`, keyboard/focus/dialog assertions, `SPEC-003-NFR-2.md` | PASS |
| NFR-3 | 8 calculated token contrast theories, `SPEC-003-NFR-3.md` | PASS |
| NFR-4 | 44px deployed token and per-route target evidence, `SPEC-003-NFR-4.md` | PASS |
| NFR-5 | six-profile route matrices and semantic overflow alternatives, `SPEC-003-NFR-5.md` | PASS |
| NFR-6 | Release published-host cold-cache LCP/API/compression harness and `SPEC-003-NFR-6-results.json` | WAIVED-DEMO — measured p75 exceeds 2,500ms; Ahmed-approved non-production exception, production go-live withheld |
| NFR-7 | Four-engine POC manifest, exact-build verification, and current-stable Firefox smoke | PASS |
| NFR-8 | `SPEC-003-NFR-8.md`, `SPEC-003-NFR-8-results.json`; representative human cohort protocol retained | WAIVED-DEMO — Ahmed-approved non-production exception; no human result claimed and production withheld |
| NFR-9 | 27-route, 432-artifact versioned baseline manifest; 467/467 visual assembly | PASS |
| NFR-10 | Resource-backed UI, culture audit, direction-safe CSS and full-source quality gate | PASS — 5/5 focused gate |

## Acceptance criteria

The primary executable files are under
`tests/StudentRegistration.AcceptanceTests/Specs/Spec003/` unless stated.

| Criterion | Primary evidence | Status |
|---|---|---|
| AC-1 | `AC-1Tests.cs`, 27 governed Page Design Records | PASS |
| AC-2 | `AC-2Tests.cs`, token/schema/component tests | PASS |
| AC-3 | `AC-3Tests.cs`, per-route state suites | PASS |
| AC-4 | `AC-4Tests.cs`, AUTH-02/03/04/05 route suites | PASS |
| AC-5 | `AC-5Tests.cs`, STU-02/STU-03 route suites | PASS |
| AC-6 | `AC-6Tests.cs`, STU-04/STU-05 conflict and accessibility suites | PASS |
| AC-7 | `AC-7Tests.cs`, ADM-02 through ADM-09 route suites | PASS |
| AC-8 | `AC-8Tests.cs`, STF-01 through STF-04 route suites | PASS |
| AC-9 | `AC-9Tests.cs`, NFR-1/2/5 evidence and accessibility matrices | PASS |
| AC-10 | `AC-10Tests.cs`, NFR-7/NFR-9 evidence | PASS |
| AC-11 | `AC-11Tests.cs`, stale-response component/E2E suites | PASS |
| AC-12 | `AC-12Tests.cs`, NFR-8 and NFR-10 evidence | WAIVED-DEMO — explicit machine-checked owner disposition; production cohort still required |
| AC-13 | `AC-13Tests.cs`, NFR-6 Release measurement | WAIVED-DEMO — evidence is complete; the 2,500ms threshold is not passed and the demo waiver is explicit |
| AC-14 | `AC-14Tests.cs`, AUTH-01 and STU-01 route suites | PASS |
| AC-15 | `AC-15Tests.cs`, STU-06/STU-07/STU-08 route suites | PASS |
| AC-16 | `AC-16Tests.cs`, ADM-01 route suites | PASS |
| AC-17 | `AC-17Tests.cs`, SYS-01 route suites | PASS |

## Edge cases

All primary executable files are under
`tests/StudentRegistration.IntegrationTests/Specs/Spec003/EdgeCases/`.

| Edge case | Executable evidence | Status |
|---|---|---|
| EC-1 | `EC-1Tests.cs` — polite update without focus theft | PASS |
| EC-2 | `EC-2Tests.cs` — safe identifier, reauthentication, server refetch/revalidation | PASS |
| EC-3 | `EC-3Tests.cs` plus six-profile browser reflow matrices | PASS |
| EC-4 | `EC-4Tests.cs` — reduced-motion equivalent state | PASS |
| EC-5 | `EC-5Tests.cs` — startup/offline recoverable state | PASS |
| EC-6 | `EC-6Tests.cs` — unknown reason safe fallback/reference | PASS |
| EC-7 | `EC-7Tests.cs` plus content-addressed approval manifests | PASS |
| EC-8 | `EC-8Tests.cs` plus STF-04 internal-scroll/semantic-alternative matrix | PASS |
| EC-9 | `EC-9Tests.cs` — autofill labels/reviewability/secret safety | PASS |
| EC-10 | `EC-10Tests.cs` — pending disablement/idempotent presentation | PASS |

## Success criteria

| Criterion | Evidence | Status |
|---|---|---|
| SC-1 | 27 Page Design Records and complete route-to-test matrix below | PASS |
| SC-2 | NFR-1 through NFR-5 executable WCAG evidence | PASS |
| SC-3 | NFR-8 controlled usability cohort and demo-waiver record | WAIVED-DEMO — representative humans remain required before production |

## Route-to-test matrix

For every row, the test family is the same-named file in `Client.ContractTests`,
`Client.UnitTests`, `E2ETests`, `AccessibilityTests`, and `VisualTests`; the
implementation owner is hash-pinned in
`design/implementation-readiness-2026-07-19.json` where applicable.

| Route | Path / page | Test families | Status |
|---|---|---|---|
| AUTH-01 | `/` — RoleGatewayPage | contract, component, E2E, accessibility, visual | PASS |
| AUTH-02 | `/student/login` — StudentLoginPage | contract, component, E2E, accessibility, visual | PASS |
| AUTH-03 | `/student/activate` — StudentActivationPage | contract, component, E2E, accessibility, visual | PASS |
| AUTH-04 | `/staff/login` — StaffLoginPage | contract, component, E2E, accessibility, visual | PASS |
| AUTH-05 | `/account/recovery` — AccountRecoveryPage | contract, component, E2E, accessibility, visual | PASS |
| STU-01 | `/student` — StudentDashboardPage | contract, component, E2E, accessibility, visual | PASS |
| STU-02 | `/student/subjects` — SubjectDiscoveryPage | contract, component, E2E, accessibility, visual | PASS |
| STU-03 | `/student/subjects/{offeringId}` — SubjectDetailsPage | contract, component, E2E, accessibility, visual | PASS |
| STU-04 | `/student/schedule` — ScheduleBuilderPage | contract, component, E2E, accessibility, visual | PASS |
| STU-05 | `/student/review` — RegistrationReviewPage | contract, component, E2E, accessibility, visual | PASS |
| STU-06 | `/student/registration/result/{id}` — RegistrationResultPage | contract, component, E2E, accessibility, visual | PASS |
| STU-07 | `/student/registrations` — RegistrationHistoryPage | contract, component, E2E, accessibility, visual | PASS |
| STU-08 | `/student/account` — StudentAccountPage | contract, component, E2E, accessibility, visual | PASS |
| STU-09 | `/student/roadmap` — StudentRoadmapPage | contract, component, E2E, accessibility, visual | PENDING — T289 |
| ADM-01 | `/admin` — AdminDashboardPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-02 | `/admin/terms` — TermAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-03 | `/admin/users` — UserAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-04 | `/admin/students` — StudentAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-05 | `/admin/catalogue` — CatalogueAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-06 | `/admin/offerings` — OfferingAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-07 | `/admin/resources` — ResourceAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-08 | `/admin/registrations` — RegistrationAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-09 | `/admin/audit` — AuditAdministrationPage | contract, component, E2E, accessibility, visual | PASS |
| ADM-10 | `/admin/approvals` — ApprovalAdministrationPage | contract, component, E2E, accessibility, visual | PASS — shared composition covers paged pending/overload, held capacity, terminal removal, denial, stale focus recovery, and double-action suppression; complete cross-browser visual expansion remains the separate T301 gate |
| STF-01 | `/staff` — StaffDashboardPage | contract, component, E2E, accessibility, visual | PASS |
| STF-02 | `/staff/timetable` — StaffTimetablePage | contract, component, E2E, accessibility, visual | PASS |
| STF-03 | `/staff/groups/{groupId}/roster` — StaffRosterPage | contract, component, E2E, accessibility, visual | PASS |
| STF-04 | `/staff/availability` — StaffAvailabilityPage | contract, component, E2E, accessibility, visual | PASS |
| STF-05 | `/staff/approvals` — StaffApprovalInboxPage | contract, component, E2E, accessibility, visual | PASS — Lecturer/TA scopes cover paged pending/overload, held capacity, terminal removal, denial, stale focus recovery, and double-action suppression; complete cross-browser visual expansion remains the separate T301 gate |
| SYS-01 | `/status/{code}` — SystemStatusPage | contract, component, E2E, accessibility, visual | PASS |

## Release rejection

This matrix approves only the non-production demo because NFR-8/AC-12/SC-3
carry Ahmed's explicit `WAIVED-DEMO` disposition. Automated browser
personas and Ahmed ELbamby's execution approval are valuable but do not become
eight student, three Admin, three Lecturer, and three TA participants. The
NFR-6 row is an explicit approved demo waiver, not a threshold pass. All
automatable rows have completed; the human cohort remains a production gate.
