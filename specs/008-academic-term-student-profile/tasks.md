# Tasks: Academic Term and Student Profile

**Status**: Planned only. Do not execute until all readiness checks and Ahmed ELbamby's human approval pass.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Approval is the final planning gate. Every delivery is preceded by a failing exact-file test, and `[P]` never targets the same file twice.

## Phase 1 - Planning Readiness and Final Approval

- [ ] T001 [DEP-SPEC-002] Baseline approved term/policy semantics from specs/002-aastmt-policy-rulebook/ in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T002 [DEP-SPEC-003] Baseline AUTH-01, STU-01, ADM-02, and ADM-04 page/state contracts from specs/003-ux-storyboard-accessibility/ in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-005] [DEP-SPEC-006] Baseline persistence, UTC, rowversion, and provenance contracts from specs/005-erd-data-lifecycle/ plus the canonical TermSummaryDto, AppContextDto, pagination, and error contracts from specs/006-domain-class-api-contracts/ in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-007] Baseline session/AppContext identity fields and role scope from specs/007-identity-account-lifecycle/ in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-018] Baseline performance, authorization, two-replica, and operations gates from specs/018-quality-security-scalability-operations/ in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T006 [GATE] Analyze requirements, AppContext composition, ownership, APIs, window state/version, student-term locking, routes, and tasks and freeze the result in specs/008-academic-term-student-profile/checklists/implementation-readiness.md.
- [ ] T007 [GATE] As the final planning action, record Ahmed ELbamby's human approval in specs/008-academic-term-student-profile/checklists/approval.md; do not start T008 or later before T001-T007 pass.

## Phase 2 - Failing Model and Contract Tests

- [ ] T008 [P] [ENTITY-AcademicTerm] [OWNER-SPEC-008] Create failing lifecycle, timezone, uniqueness, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/AcademicTermModelTests.cs.
- [ ] T009 [P] [ENTITY-RegistrationWindow] [OWNER-SPEC-008] Create failing lifecycle, scope, interval, computed-state, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/RegistrationWindowModelTests.cs.
- [ ] T010 [P] [ENTITY-Student] [OWNER-SPEC-008] Create failing University-ID, provenance, active-state, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/StudentModelTests.cs.
- [ ] T011 [P] [ENTITY-TranscriptAttempt] [OWNER-SPEC-008] Create failing immutable attempt/provenance checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/TranscriptAttemptModelTests.cs.
- [ ] T012 [P] [ENTITY-StudentHold] [OWNER-SPEC-008] Create failing effective-period, blocking, source, and term checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/StudentHoldModelTests.cs.
- [ ] T013 [P] [ENTITY-StudentTermAcademicState] [OWNER-SPEC-008] Create failing unique student-term and shared rowversion-lock checks in tests/StudentRegistration.IntegrationTests/Specs/Spec008/StudentTermAcademicStateModelTests.cs.
- [ ] T014 [API-Endpoint01] [OWNER-SPEC-008] Finalize GET /api/public/context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T015 [P] [API-Endpoint01] Create failing public-data-minimization and server-time checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint01ContractTests.cs for GET /api/public/context.
- [ ] T016 [API-Endpoint02] [OWNER-SPEC-008] Finalize GET /api/context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T017 [P] [API-Endpoint02] Create failing composed identity/academic AppContext and replica-consistency checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint02ContractTests.cs for GET /api/context.
- [ ] T018 [API-Endpoint03] [OWNER-SPEC-008] Finalize GET /api/students/me/academic-context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T019 [P] [API-Endpoint03] Create failing self-scope, transcript summary/attempts, all active holds with blocking flags, data-version/as-of, and provenance checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint03ContractTests.cs for GET /api/students/me/academic-context.
- [ ] T020 [API-Endpoint04] [OWNER-SPEC-008] Finalize GET /api/admin/terms in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T021 [P] [API-Endpoint04] Create failing bounded paging/filtering and Admin authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint04ContractTests.cs for GET /api/admin/terms.
- [ ] T022 [API-Endpoint05] [OWNER-SPEC-008] Finalize POST /api/admin/terms in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T023 [P] [API-Endpoint05] Create failing create-term provenance, validation, and authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint05ContractTests.cs for POST /api/admin/terms.
- [ ] T024 [API-Endpoint06] [OWNER-SPEC-008] Finalize PUT /api/admin/terms/{termId} in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T025 [P] [API-Endpoint06] Create failing term/window expected-version and no-partial-edit checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint06ContractTests.cs for PUT /api/admin/terms/{termId}.
- [ ] T026 [API-Endpoint07] [OWNER-SPEC-008] Finalize POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T027 [P] [API-Endpoint07] Create failing publish overlap, stale-version, stable-lock, and audit checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint07ContractTests.cs for POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish.
- [ ] T028 [API-Endpoint08] [OWNER-SPEC-008] Finalize GET /api/admin/students in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T029 [P] [API-Endpoint08] Create failing bounded search, field minimization, and Admin authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint08ContractTests.cs for GET /api/admin/students.
- [ ] T030 [API-Endpoint09] [OWNER-SPEC-008] Finalize GET /api/admin/students/{studentId}/academic-context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T031 [P] [API-Endpoint09] Create failing authorized detail, direct-object denial, and provenance checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint09ContractTests.cs for GET /api/admin/students/{studentId}/academic-context.
- [ ] T032 [API-Endpoint10] [OWNER-SPEC-008] Finalize PATCH /api/admin/students/{studentId}/academic-profile in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T033 [P] [API-Endpoint10] Create failing reason/source/version/guard/audit and unauthorized/stale checks in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint10ContractTests.cs for PATCH /api/admin/students/{studentId}/academic-profile.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [ ] T034 [P] [AC-1] [FR-1] [FR-6] Create device-clock independence coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-1Tests.cs.
- [ ] T035 [P] [AC-2] [FR-2] [FR-4] Create no-active-context coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-2Tests.cs.
- [ ] T036 [P] [AC-3] [FR-5] [FR-6] Create hold-before-submit fail-closed coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-3Tests.cs.
- [ ] T037 [P] [AC-4] [FR-3] [FR-7] Create governed term/profile edit coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-4Tests.cs.
- [ ] T038 [P] [AC-5] [FR-6] [FR-8] Create real-SQL hold-versus-submit serial-order coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-5Tests.cs.
- [ ] T039 [P] [AC-6] [FR-3] [FR-4] [FR-9] Create concurrent overlapping-window publication one-winner coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-6Tests.cs.
- [ ] T040 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create term/profile quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-7Tests.cs.
- [ ] T041 [P] [AC-8] [FR-1] [FR-2] [FR-4] [FR-10] Create composed AppContext coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-8Tests.cs.
- [ ] T042 [P] [AC-9] [FR-3] [FR-7] [FR-9] [FR-11] Create complete ADM-02/ADM-04 API journey coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-9Tests.cs.
- [ ] T043 [P] [EC-1] Create overlapping-window coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-1Tests.cs.
- [ ] T044 [P] [EC-2] Create missing GPA/provenance fail-closed coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-2Tests.cs.
- [ ] T045 [P] [EC-3] Create UTC/timezone rule-change coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-3Tests.cs.
- [ ] T046 [P] [EC-4] Create stale Admin edit coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-4Tests.cs.
- [ ] T047 [P] [EC-5] Create scheduled-cutoff versus emergency-close coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-5Tests.cs.
- [ ] T048 [P] [SC-1] Create authoritative time/window outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/SC-1OutcomeTests.cs.
- [ ] T049 [P] [SC-2] Create complete sourced-profile outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/SC-2OutcomeTests.cs.
- [ ] T050 [P] [SC-3] Create non-overlapping context outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [ ] T051 [FR-1] [FR-2] [FR-4] [FR-6] [FR-10] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Create the failing consolidated canonical SPEC-006 TermSummaryDto consumption, time/term/window/AppContext/supportReferencePath/re-resolution suite in tests/StudentRegistration.ApplicationTests/Academics/AcademicContextBoundaryTests.cs.
- [ ] T052 [FR-3] [FR-9] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Create the failing consolidated lifecycle/version/overlap/two-admin publication suite in tests/StudentRegistration.IntegrationTests/Academics/RegistrationWindowConcurrencyTests.cs.
- [ ] T053 [FR-5] [FR-7] [FR-8] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Create the failing consolidated profile/transcript/all-active-holds/provenance/version/correction/hold-submit suite in tests/StudentRegistration.IntegrationTests/Academics/ProfileHoldConcurrencyTests.cs.
- [ ] T054 [FR-11] [WORKSTREAM-ADMIN-ACADEMIC-JOURNEYS] Create the failing bounded Admin list and owner-command suite in tests/StudentRegistration.IntegrationTests/Academics/AdminAcademicJourneyTests.cs.
- [ ] T055 [ENTITY-AcademicTerm] [OWNER-SPEC-008] Deliver the canonical AcademicTerm at src/StudentRegistration.Academics/Domain/AcademicTerm.cs after T008 fails.
- [ ] T056 [ENTITY-RegistrationWindow] [OWNER-SPEC-008] Deliver the canonical RegistrationWindow at src/StudentRegistration.Academics/Domain/RegistrationWindow.cs after T009 fails.
- [ ] T057 [ENTITY-Student] [OWNER-SPEC-008] Deliver the canonical Student at src/StudentRegistration.Academics/Domain/Student.cs after T010 fails.
- [ ] T058 [ENTITY-TranscriptAttempt] [OWNER-SPEC-008] Deliver the canonical TranscriptAttempt at src/StudentRegistration.Academics/Domain/TranscriptAttempt.cs after T011 fails.
- [ ] T059 [ENTITY-StudentHold] [OWNER-SPEC-008] Deliver the canonical StudentHold at src/StudentRegistration.Academics/Domain/StudentHold.cs after T012 fails.
- [ ] T060 [ENTITY-StudentTermAcademicState] [OWNER-SPEC-008] Deliver the canonical StudentTermAcademicState at src/StudentRegistration.Academics/Domain/StudentTermAcademicState.cs after T013 fails.
- [ ] T061 [FR-1] [FR-2] [FR-4] [FR-6] [FR-10] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Deliver academic context composition at src/StudentRegistration.Academics/Application/AcademicContextResolver.cs after T051 fails.
- [ ] T062 [FR-3] [FR-9] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Deliver term/window publication at src/StudentRegistration.Academics/Application/RegistrationWindowService.cs after T052 fails.
- [ ] T063 [FR-5] [FR-7] [FR-8] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Deliver academic profile and student-term guarding at src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs after T053 fails.
- [ ] T064 [FR-11] [WORKSTREAM-ADMIN-ACADEMIC-JOURNEYS] Deliver bounded Admin queries/commands at src/StudentRegistration.Academics/Application/AdminAcademicManagementService.cs after T054 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [ ] T065 [API-Endpoint01] Deliver GET /api/public/context at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T015 and T051 fail.
- [ ] T066 [API-Endpoint02] Deliver GET /api/context at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T017 and T051 fail.
- [ ] T067 [API-Endpoint03] Deliver GET /api/students/me/academic-context at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T019 and T053 fail.
- [ ] T068 [API-Endpoint04] Deliver GET /api/admin/terms at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T021 and T054 fail.
- [ ] T069 [API-Endpoint05] Deliver POST /api/admin/terms at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T023 and T054 fail.
- [ ] T070 [API-Endpoint06] Deliver PUT /api/admin/terms/{termId} at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T025, T052, and T054 fail.
- [ ] T071 [API-Endpoint07] Deliver POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T027 and T052 fail.
- [ ] T072 [API-Endpoint08] Deliver GET /api/admin/students at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T029 and T054 fail.
- [ ] T073 [API-Endpoint09] Deliver GET /api/admin/students/{studentId}/academic-context at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T031, T053, and T054 fail.
- [ ] T074 [API-Endpoint10] Deliver PATCH /api/admin/students/{studentId}/academic-profile at src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs after T033, T053, and T054 fail.

## Phase 6 - Frontend Functional Tests and Pages

- [ ] T075 [P] [AUTH-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-10] [AC-1] [AC-2] [AC-8] Create failing role-gateway/composed-context journeys in tests/StudentRegistration.E2ETests/Specs/Spec008/RoleGatewayPageFeatureTests.cs.
- [ ] T076 [AUTH-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-10] [AC-1] [AC-2] [AC-8] Deliver RoleGatewayPage at src/StudentRegistration.Client/Pages/RoleGatewayPage.razor after T075 fails.
- [ ] T077 [P] [STU-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-5] [FR-10] [AC-1] [AC-2] [AC-7] [AC-8] Create failing student-dashboard composed-context states in tests/StudentRegistration.E2ETests/Specs/Spec008/StudentDashboardPageFeatureTests.cs.
- [ ] T078 [STU-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-5] [FR-10] [AC-1] [AC-2] [AC-7] [AC-8] Deliver StudentDashboardPage at src/StudentRegistration.Client/Pages/StudentDashboardPage.razor after T077 fails.
- [ ] T079 [P] [ADM-02] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-7] [FR-9] [FR-11] [AC-4] [AC-6] [AC-9] Create failing term-list/edit/publish owner-API journeys in tests/StudentRegistration.E2ETests/Specs/Spec008/TermAdministrationPageFeatureTests.cs.
- [ ] T080 [ADM-02] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-7] [FR-9] [FR-11] [AC-4] [AC-6] [AC-9] Deliver TermAdministrationPage at src/StudentRegistration.Client/Pages/TermAdministrationPage.razor after T079 and SPEC-017 contributor tests fail.
- [ ] T081 [P] [ADM-04] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-11] [AC-4] [AC-5] [AC-9] Create failing student-search/detail/correction owner-API journeys in tests/StudentRegistration.E2ETests/Specs/Spec008/StudentAdministrationPageFeatureTests.cs.
- [ ] T082 [ADM-04] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-11] [AC-4] [AC-5] [AC-9] Deliver StudentAdministrationPage at src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor after T081 and SPEC-017 contributor tests fail.

## Phase 7 - Quality, Scope, and Release Evidence

- [ ] T083 [ENTITY-AcademicTerm] [ENTITY-RegistrationWindow] [ENTITY-Student] [ENTITY-StudentTermAcademicState] [ENTITY-TranscriptAttempt] [ENTITY-StudentHold] [PERSISTENCE-MAPPING] [MIGRATION-S1IdentityAcademicFoundation] Create the failing real-SQL Academics mapping suite in tests/StudentRegistration.IntegrationTests/Specs/Spec008/AcademicContextModelConfigurationTests.cs and initial-migration empty-database/update/rollback/snapshot parity suite in tests/StudentRegistration.IntegrationTests/Persistence/S1IdentityAcademicFoundationMigrationTests.cs, including unique ApplicationUserId, stable ProgramCode/CourseCode source references with no downstream Program/Course FK, scope/window constraints, provenance, rowversion, holds, transcripts, and student-term guard.
- [ ] T084 [ENTITY-AcademicTerm] [ENTITY-RegistrationWindow] [ENTITY-Student] [ENTITY-StudentTermAcademicState] [ENTITY-TranscriptAttempt] [ENTITY-StudentHold] [PERSISTENCE-MAPPING] [MIGRATION-S1IdentityAcademicFoundation] Deliver the Academics EF Core mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs, generate the initial migration at src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T083 and prerequisite SPEC-004/SPEC-007 mappings pass; SPEC-004 remains the sole DbContext writer.
- [ ] T085 [P] [NFR-1] Produce fake-clock boundary evidence in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-1.md.
- [ ] T086 [P] [NFR-2] Produce 300-read/s latency evidence in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-2.md.
- [ ] T087 [P] [NFR-3] Produce UTC/IANA persistence evidence in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-3.md.
- [ ] T088 [P] [NFR-4] Produce self/staff/Admin authorization evidence in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-4.md.
- [ ] T089 [OS-1] [OS-2] [OS-3] [OS-4] Record verified scope exclusions in docs/release-evidence/SPEC-008-scope-review.md.
- [ ] T090 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-008-traceability.md.
- [ ] T091 [GATE] Record product, Registrar, Identity, QA, accessibility, data/concurrency, security, and operations release approvals in docs/release-evidence/SPEC-008-release-approval.md.

No task is complete and no implementation file has been created.
