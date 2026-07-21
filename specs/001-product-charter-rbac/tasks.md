# Tasks: Product Charter and RBAC

**Status**: Approved for Gate A demo implementation on 2026-07-13. Execute in dependency order; later release and production gates remain required.
**Implementation progress**: 42/42 tasks complete. Governed contracts,
the bounded runtime journey, accessibility/scale measurements, traceability,
and applicable non-production demo approvals are verified. Broader manual and
production release gates remain owned by SPEC-018.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task names an exact artifact and traces to a requirement,
criterion, edge case, route, entity, endpoint, dependency, or gate. A checked
task has passing evidence and an existing artifact; unchecked tasks remain
pending their owning dependency or release gate.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [x] T001 [DEP-ROOT] Confirm SPEC-001 is the dependency root, has no upstream feature dependency, and records the constitution/project-plan baseline in specs/001-product-charter-rbac/dependency-baseline.md.
- [x] T002 [GATE] Run cross-spec consistency analysis for SPEC-001; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and approved Gate A demo-implementation status; record findings and resolutions in specs/001-product-charter-rbac/checklists/consistency-analysis.md.
- [x] T003 [GATE] After T002 passes, freeze the SPEC-001 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/001-product-charter-rbac/checklists/implementation-readiness.md.
- [x] T004 [GATE] After T003 passes, verify the accountable owner and Ahmed ELbamby's 2026-07-13 Gate A human approval for SPEC-001 in specs/001-product-charter-rbac/checklists/approval.md as the final planning gate; no test, source, migration, or other implementation task may execute without this approval record.

## Phase 2 - Models and API Contracts

- [x] T005 [ENTITY-RoleDefinition] [ARTIFACT-OWNER-SPEC-001] Create the future failing vocabulary/ownership checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/RoleDefinitionTests.cs.
- [x] T006 [ENTITY-RoleDefinition] [ARTIFACT-OWNER-SPEC-001] Publish the canonical governed role vocabulary at specs/001-product-charter-rbac/contracts/roles.md only after T005 fails for the expected reason (depends on T005); do not create a runtime identity model.
- [x] T007 [ENTITY-PermissionDefinition] [ARTIFACT-OWNER-SPEC-001] Create the future failing permission-definition completeness and ownership checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/PermissionDefinitionTests.cs.
- [x] T008 [ENTITY-PermissionDefinition] [ARTIFACT-OWNER-SPEC-001] Publish the canonical capability/data-scope definitions at specs/001-product-charter-rbac/contracts/permissions.md only after T007 fails for the expected reason (depends on T007); executable policies remain owned by SPEC-007.
- [x] T009 [ENTITY-RbacMatrix] [ARTIFACT-OWNER-SPEC-001] Create the future failing role-to-permission matrix, denial, and SPEC-007 runtime-owner checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/RbacMatrixTests.cs.

- [x] T010 [ENTITY-RbacMatrix] [ARTIFACT-OWNER-SPEC-001] Record the approved RbacMatrix schema/version target in specs/001-product-charter-rbac/checklists/rbac-matrix-schema.md after T009 fails for the expected reason (depends on T009); canonical publication remains deferred until all role-boundary behavior tests fail as expected.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Student boundary (FR-1, FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T009.
- [x] T011 [AC-1] [FR-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-1Tests.cs for AC-1: Student boundary (FR-1, FR-2): Given a user opens the public landing page When the user selects Student Then only student activation/login actions are presented And no staff role can be selected.
### US2 - Staff role derivation (FR-2, FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T009.
- [x] T012 [AC-2] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-2Tests.cs for AC-2: Staff role derivation (FR-2, FR-3): Given a valid staff account with Lecturer claims When the user authenticates through the shared staff page Then the server routes the user to the Lecturer context And changing a client route does not grant Admin data.
### US3 - Scope traceability (FR-6, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T009.
- [x] T013 [AC-3] [FR-6] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-3Tests.cs for AC-3: Scope traceability (FR-6, FR-7): Given a proposed implementation story When it is evaluated for Sprint readiness Then it references an approved SPEC-NNN/FR-N and SPEC-NNN/AC-N And work is rejected when no approved contract exists.
### US4 - End-to-end role coverage (FR-4, FR-5) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T009.
- [x] T014 [AC-4] [FR-4] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-4Tests.cs for AC-4: End-to-end role coverage (FR-4, FR-5): Given the Gate C staging release When representatives execute the approved Student, Admin, Lecturer and TA journeys Then the student can complete the atomic registration flow And every staff role reaches only its approved workspace.
### US5 - Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T009.
- [x] T015 [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-5Tests.cs for AC-5: Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4): Given the MVP release candidate and SPEC-018 production-like load profile When accessibility, authorization, architecture, and scale gates execute Then critical flows meet WCAG 2.2 AA And every protected request is API-authorized And the modular monolith meets SPEC-018 targets without distributed services.
- [x] T016 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-1Tests.cs and assert: User holds Lecturer and TA roles -> offer only the authorized contexts.
- [x] T017 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-2Tests.cs and assert: Staff has no supported role -> deny access with safe no-role message.
- [x] T018 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-3Tests.cs and assert: A requested enhancement is outside MVP -> create/review a new spec rather than adding it silently.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T019 [FR-1] [FR-2] [FR-3] [FR-5] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create consolidated role-boundary checks in tests/StudentRegistration.SpecificationTests/Spec001/RolePolicyContractTests.cs. Test focus: student/staff entry separation, server-derived claims, direct-route denials, invalid combined-role accounts, and no-role contexts.
- [x] T020 [FR-1] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Publish the four-role boundary at specs/001-product-charter-rbac/contracts/role-boundary.md only after T019 fails for the expected reason (depends on T019); SPEC-007 alone writes runtime authorization source.
- [x] T021 [FR-2] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing student-versus-shared-staff entry contract checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/EntryPointBoundaryTests.cs.
- [x] T022 [FR-2] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Publish the distinct student/shared-staff entry boundary at specs/001-product-charter-rbac/contracts/entry-point-boundary.md only after T021 fails for the expected reason (depends on T021).
- [x] T023 [FR-3] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing server-derived role/data-scope contract checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/ServerDerivedScopeTests.cs.
- [x] T024 [FR-3] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Publish the server-derived scope and client-distrust rules at specs/001-product-charter-rbac/contracts/server-derived-scope.md only after T023 fails for the expected reason (depends on T023).
- [x] T025 [FR-4] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-4 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: The system MUST support the end-to-end student flow from login through an atomic registration receipt.
- [x] T026 [FR-4] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-4 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T025 fails for the expected reason (depends on T025): The system MUST support the end-to-end student flow from login through an atomic registration receipt.
- [x] T027 [FR-5] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing role-workspace/data-scope contract checks in tests/StudentRegistration.SpecificationTests/Specs/Spec001/WorkspaceScopeTests.cs.
- [x] T028 [FR-1] [FR-2] [FR-3] [FR-5] [ENTITY-RbacMatrix] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Deliver the bounded Role and authorization boundaries workstream and publish the canonical RbacMatrix at specs/001-product-charter-rbac/contracts/rbac-matrix.md only after T019, T021, T023, and T027 fail for their expected reasons (depends on T019, T021, T023, T027); runtime policies remain SPEC-007 owned.
- [x] T029 [FR-6] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-6 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- [x] T030 [FR-6] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-6 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T029 fails for the expected reason (depends on T029): MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- [x] T031 [FR-7] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-7 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: Every implementation story MUST trace to an approved spec and acceptance criterion.
- [x] T032 [FR-7] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-7 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T031 fails for the expected reason (depends on T031): Every implementation story MUST trace to an approved spec and acceptance criterion.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [x] T033 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-1.md: Critical flows MUST meet WCAG 2.2 AA.
- [x] T034 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-2.md: The design MUST support the approved SPEC-018 scale targets without changing domain behavior.
- [x] T035 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-3.md: Authorization MUST be enforced by the API for every protected action.
- [x] T036 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-4.md: The initial solution MUST remain one deployable modular monolith.

## Phase 7 - Scope and Release Evidence

- [x] T037 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-1 remains excluded: Payment, grade entry, attendance, waitlist, advisor workflow, and notifications.
- [x] T038 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-2 remains excluded: Public staff registration.
- [x] T039 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-3 remains excluded: Client-side-only authorization.
- [x] T040 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-4 remains excluded: Multi-tenancy and native mobile applications.
- [x] T041 [TRACE] [SC-1] [SC-2] [SC-3] Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-001-traceability.md and reject release if any row lacks passing evidence.
- [x] T042 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-001 in docs/release-evidence/SPEC-001-release-approval.md.

All forty-two tasks are complete with executable evidence. The SPEC-001
approval is limited to its non-production charter/RBAC and bounded aggregate
scope; it does not waive broader SPEC-018 manual or production release gates.

## Phase 8 - Owner-approved 2026-07-20 approval RBAC amendment

- [ ] T043 [P] [RBAC] Add `RegistrationApproval.DecideAll` and `RegistrationApproval.DecideAssigned` to the governed permission vocabulary and role matrix in specs/001-product-charter-rbac/contracts/role-boundary.md and the canonical permission artifacts.
- [ ] T044 [RBAC] Add positive Admin, assigned Lecturer/TA, unassigned staff, wrong-role, missing-permission, and authorization-before-disclosure tests under tests/StudentRegistration.AuthorizationTests and tests/StudentRegistration.ContractTests.
- [ ] T045 [TRACE] Regenerate SPEC-001 RBAC, scope, traceability, and release evidence after the executable SPEC-007/SPEC-014 policies pass.
