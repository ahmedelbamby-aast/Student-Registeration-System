# SPEC-001 Complete Traceability Evidence

**Feature:** Product Charter and RBAC
**Evidence date:** 2026-07-19
**Evidence scope:** bounded charter/RBAC contract and downstream aggregate
runtime evidence
**Production authority:** Not granted

## Admission rule

Every SPEC-001 FR, NFR, AC, EC, SC, and route-boundary row below must name an
authoritative artifact and executable passing evidence. The automated
`TraceabilityEvidenceTests` requires the exact row inventory; a missing row or
non-PASS row rejects SPEC-001 release. Passing SPEC-001 does not waive the
separate manual or production gates owned by another specification; the
broader system release remains independently governed by SPEC-018.

## Functional requirements

| ID | Requirement | Authoritative artifact/runtime | Executable evidence | Status |
|---|---|---|---|---|
| FR-1 | Student, Admin, Lecturer, TeachingAssistant roles | `contracts/roles.md`; SPEC-007 `RolePolicies` | `RoleDefinitionTests`; `RolePolicyContractTests`; identity policy tests | PASS |
| FR-2 | Separate student and shared staff entry | `contracts/entry-point-boundary.md`; identity pages/endpoints | `EntryPointBoundaryTests`; AC-1/AC-2; login page tests | PASS |
| FR-3 | Server-derived role, permission, and resource scope | `contracts/server-derived-scope.md`; API authorization policies | `ServerDerivedScopeTests`; `IdentityEndpointAuthorizationMatrixTests`; `StaffContextAuthorizationTests` | PASS |
| FR-4 | Login-to-atomic-registration-receipt journey | `SPEC-001-charter-traceability.md`; AUTH-02 and STU-02/04/05/06 runtime | `CharterJourneyTests.Fr_4...`; route E2E suites; SQL load/invariant suites | PASS |
| FR-5 | Role-scoped staff/admin workspaces | `contracts/rbac-matrix.md`; staff/admin pages and endpoints | `WorkspaceScopeTests`; `StaffWorkspaceScopeTests`; staff/admin page suites | PASS |
| FR-6 | MVP scope and non-goals match project plan | requirements; `docs/PROJECT_PLAN.md`; `SPEC-001-scope-review.md` | `CharterJourneyTests.Fr_6...`; NFR-3 scope review test | PASS |
| FR-7 | Every implementation story traces to approved FR and AC | readiness checklist; task trace tags | `CharterJourneyTests.Fr_7...`; AC-3 | PASS |

## Non-functional requirements

| ID | Requirement | Measured/automated evidence | Result | Status |
|---|---|---|---|---|
| NFR-1 | Critical flows meet WCAG 2.2 AA | `NFR-1EvidenceTests.cs`; `SPEC-001-NFR-1.md`; 36/36 route/browser artifact | Automated charter evidence passes; manual system-release gate remains SPEC-018-owned | PASS |
| NFR-2 | Approved scale without domain change | `NFR-2EvidenceTests.cs`; `SPEC-001-NFR-2.md`; machine load evidence | Exact 75 write/300 read target and 200 write spike pass on two replicas with zero invariant drift | PASS |
| NFR-3 | API authorization for every protected action | `NFR-3EvidenceTests.cs`; `SPEC-001-NFR-3.md` | 21 protected operations plus positive/negative policy suites | PASS |
| NFR-4 | One deployable modular monolith | `NFR-4EvidenceTests.cs`; `SPEC-001-NFR-4.md` | Nine governed projects, one web deployable, one composition root, acyclic graph | PASS |

## Acceptance criteria

| ID | Scenario | Executable evidence | Status |
|---|---|---|---|
| AC-1 | Student entry exposes no staff-role selection | `AC-1Tests`; student entry and route contract tests | PASS |
| AC-2 | Lecturer claims select only Lecturer context; route change grants no Admin data | `AC-2Tests`; `StaffContextAuthorizationTests`; endpoint authorization matrix | PASS |
| AC-3 | Unapproved or untraced story is rejected | `AC-3Tests`; `CharterJourneyTests.Fr_7...` | PASS |
| AC-4 | Gate C atomic student journey and scoped Admin, Lecturer, and TeachingAssistant workspaces | `AC-4Tests`; FR-4 aggregate; staff/admin E2E and authorization suites | PASS |
| AC-5 | Accessibility, authorization, architecture, and scale quality boundary | `AC-5Tests`; all four SPEC-001 NFR evidence suites and artifacts | PASS |

## Edge cases

| ID | Boundary | Executable evidence | Status |
|---|---|---|---|
| EC-1 | Dual Lecturer/TA receives only existing selectable contexts | `IntegrationTests/Specs/Spec001/EdgeCases/EC-1Tests.cs` | PASS |
| EC-2 | No supported staff role denies safely | `IntegrationTests/Specs/Spec001/EdgeCases/EC-2Tests.cs` | PASS |
| EC-3 | Outside-MVP request requires a new approved spec | `IntegrationTests/Specs/Spec001/EdgeCases/EC-3Tests.cs`; scope review | PASS |

## Success criteria

| ID | Criterion | Evidence | Status |
|---|---|---|---|
| SC-1 | Every MVP capability has owner and acceptance scenario | project-plan exact spec inventory; FR/AC matrix above; owner-spec route chain | PASS |
| SC-2 | Every protected role has an explicit permission boundary | role, permission, and RBAC contracts; policy/authorization tests | PASS |
| SC-3 | No delivery task is admitted without approved requirement and acceptance reference | readiness checklist; `CharterJourneyTests.Fr_7...`; AC-3 | PASS |

## Frontend route boundary

| ID | Boundary | Evidence | Status |
|---|---|---|---|
| ROUTE-BOUNDARY | SPEC-001 directly owns no route; downstream routes remain governed by SPEC-003 plus the implementing owner spec | `spec.md` Frontend Route Ownership; owner-spec E2E suites listed in FR-4/FR-5 | PASS |

## Release record

The applicable review decisions are recorded in
`SPEC-001-release-approval.md`. This matrix is a SPEC-001 conformance and
aggregate-evidence release, not authorization for production deployment or an
official AASTMT release.
