# Tasks: Product Charter and RBAC

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-001 in specs/001-product-charter-rbac/checklists/approval.md before executing any later task.
- [ ] T002 [GATE] Freeze SPEC-001 requirements, API, data-model, policy approvals, and dependency versions in specs/001-product-charter-rbac/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T003 [P] [ENTITY-Role] [OWNER-SPEC-001] Create the future failing invariant/schema/serialization checks for canonical Role ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec001/RoleModelTests.cs.
- [ ] T004 [ENTITY-Role] [OWNER-SPEC-001] Deliver the canonical Role model or governed artifact at src/StudentRegistration.Domain/Modules/Governance/Role.cs after T003 fails for the expected reason (depends on T003).
- [ ] T005 [P] [ENTITY-Permission] [OWNER-SPEC-001] Create the future failing invariant/schema/serialization checks for canonical Permission ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec001/PermissionModelTests.cs.
- [ ] T006 [ENTITY-Permission] [OWNER-SPEC-001] Deliver the canonical Permission model or governed artifact at src/StudentRegistration.Domain/Modules/Governance/Permission.cs after T005 fails for the expected reason (depends on T005).
- [ ] T007 [P] [ENTITY-RoleAssignment] [CONSUMER-SPEC-007] Verify SPEC-001 consumes the canonical RoleAssignment at src/StudentRegistration.Domain/Modules/IdentityAccess/RoleAssignment.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec001/RoleAssignmentModelTests.cs.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Student boundary (FR-1, FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T008 [P] [AC-1] [FR-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-1Tests.cs for AC-1: Student boundary (FR-1, FR-2): Given a user opens the public landing page When the user selects Student Then only student activation/login actions are presented And no staff role can be selected.
### US2 - Staff role derivation (FR-2, FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T009 [P] [AC-2] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-2Tests.cs for AC-2: Staff role derivation (FR-2, FR-3): Given a valid staff account with Lecturer claims When the user authenticates through the shared staff page Then the server routes the user to the Lecturer context And changing a client route does not grant Admin data.
### US3 - Scope traceability (FR-6, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T010 [P] [AC-3] [FR-6] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-3Tests.cs for AC-3: Scope traceability (FR-6, FR-7): Given a proposed implementation story When it is evaluated for Sprint readiness Then it references an approved SPEC-NNN/FR-N and SPEC-NNN/AC-N And work is rejected when no approved contract exists.
### US4 - End-to-end role coverage (FR-4, FR-5) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T011 [P] [AC-4] [FR-4] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-4Tests.cs for AC-4: End-to-end role coverage (FR-4, FR-5): Given the Gate C staging release When representatives execute the approved Student, Admin, Lecturer and TA journeys Then the student can complete the atomic registration flow And every staff role reaches only its approved workspace.
### US5 - Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Product Charter and RBAC.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T012 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/AC-5Tests.cs for AC-5: Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4): Given the MVP release candidate and SPEC-018 production-like load profile When accessibility, authorization, architecture, and scale gates execute Then critical flows meet WCAG 2.2 AA And every protected request is API-authorized And the modular monolith meets SPEC-018 targets without distributed services.
- [ ] T013 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-1Tests.cs and assert: User holds Lecturer and TA roles -> offer only the authorized contexts.
- [ ] T014 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-2Tests.cs and assert: Staff has no supported role -> deny access with safe no-role message.
- [ ] T015 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec001/EdgeCases/EC-3Tests.cs and assert: A requested enhancement is outside MVP -> create/review a new spec rather than adding it silently.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T016 [P] [FR-1] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing FR-1 checks in tests/StudentRegistration.AuthorizationTests/Spec001/RolePolicyTests.cs. Test focus: student/staff entry separation, server-derived claims, direct-route denials, dual-role and no-role contexts. Prove the requirement against its linked AC/EC fixtures: The system MUST support Student, Admin, Lecturer, and TeachingAssistant roles.
- [ ] T017 [FR-1] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Deliver FR-1 through the bounded Role and authorization boundaries workstream at src/StudentRegistration.Server/Authorization/RolePolicies.cs only after T016 fails for the expected reason (depends on T016): The system MUST support Student, Admin, Lecturer, and TeachingAssistant roles.
- [ ] T018 [P] [FR-2] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing FR-2 checks in tests/StudentRegistration.AuthorizationTests/Spec001/RolePolicyTests.cs. Test focus: student/staff entry separation, server-derived claims, direct-route denials, dual-role and no-role contexts. Prove the requirement against its linked AC/EC fixtures: Students MUST use a student entry point; Admin/Lecturer/TA MUST share a staff entry point.
- [ ] T019 [FR-2] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Deliver FR-2 through the bounded Role and authorization boundaries workstream at src/StudentRegistration.Server/Authorization/RolePolicies.cs only after T018 fails for the expected reason (depends on T018): Students MUST use a student entry point; Admin/Lecturer/TA MUST share a staff entry point.
- [ ] T020 [P] [FR-3] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing FR-3 checks in tests/StudentRegistration.AuthorizationTests/Spec001/RolePolicyTests.cs. Test focus: student/staff entry separation, server-derived claims, direct-route denials, dual-role and no-role contexts. Prove the requirement against its linked AC/EC fixtures: The server MUST derive role and data scope and MUST NOT trust a client-selected role.
- [ ] T021 [FR-3] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Deliver FR-3 through the bounded Role and authorization boundaries workstream at src/StudentRegistration.Server/Authorization/RolePolicies.cs only after T020 fails for the expected reason (depends on T020): The server MUST derive role and data scope and MUST NOT trust a client-selected role.
- [ ] T022 [P] [FR-4] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-4 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: The system MUST support the end-to-end student flow from login through an atomic registration receipt.
- [ ] T023 [FR-4] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-4 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T022 fails for the expected reason (depends on T022): The system MUST support the end-to-end student flow from login through an atomic registration receipt.
- [ ] T024 [P] [FR-5] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/Spec001/RolePolicyTests.cs. Test focus: student/staff entry separation, server-derived claims, direct-route denials, dual-role and no-role contexts. Prove the requirement against its linked AC/EC fixtures: The system MUST expose role-scoped staff/admin workspaces.
- [ ] T025 [FR-5] [WORKSTREAM-ROLE-AND-AUTHORIZATION-BOUNDARIES] Deliver FR-5 through the bounded Role and authorization boundaries workstream at src/StudentRegistration.Server/Authorization/RolePolicies.cs only after T024 fails for the expected reason (depends on T024): The system MUST expose role-scoped staff/admin workspaces.
- [ ] T026 [P] [FR-6] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-6 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- [ ] T027 [FR-6] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-6 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T026 fails for the expected reason (depends on T026): MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- [ ] T028 [P] [FR-7] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Create the future failing FR-7 checks in tests/StudentRegistration.AcceptanceTests/Specs/Spec001/CharterJourneyTests.cs. Test focus: student atomic journey, role workspaces, approved scope admission and FR/AC traceability. Prove the requirement against its linked AC/EC fixtures: Every implementation story MUST trace to an approved spec and acceptance criterion.
- [ ] T029 [FR-7] [WORKSTREAM-MVP-JOURNEY-AND-TRACEABILITY-GOVERNANCE] Deliver FR-7 through the bounded MVP journey and traceability governance workstream at docs/release-evidence/SPEC-001-charter-traceability.md only after T028 fails for the expected reason (depends on T028): Every implementation story MUST trace to an approved spec and acceptance criterion.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T030 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-1.md: Critical flows MUST meet WCAG 2.2 AA.
- [ ] T031 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-2.md: The design MUST support the approved SPEC-018 scale targets without changing domain behavior.
- [ ] T032 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-3.md: Authorization MUST be enforced by the API for every protected action.
- [ ] T033 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-001-NFR-4.md: The initial solution MUST remain one deployable modular monolith.

## Phase 7 - Scope and Release Evidence

- [ ] T034 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-1 remains excluded: Payment, grade entry, attendance, waitlist, advisor workflow, and notifications.
- [ ] T035 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-2 remains excluded: Public staff registration.
- [ ] T036 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-3 remains excluded: Client-side-only authorization.
- [ ] T037 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-001-scope-review.md that OS-4 remains excluded: Multi-tenancy and native mobile applications.
- [ ] T038 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-001-traceability.md and reject release if any row lacks passing evidence.
- [ ] T039 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-001 in docs/release-evidence/SPEC-001-release-approval.md.

No task is complete and no implementation file has been created.
