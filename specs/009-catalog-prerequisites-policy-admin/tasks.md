# Tasks: Catalogue, Prerequisites, and Policy Administration

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-009 in specs/009-catalog-prerequisites-policy-admin/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-002] Validate the consumed upstream requirements, plan, data model, and API contract at specs/002-aastmt-policy-rulebook/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T007 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [ ] T008 [GATE] Freeze SPEC-009 requirements, API, data-model, policy approvals, and dependency versions in specs/009-catalog-prerequisites-policy-admin/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T009 [P] [ENTITY-Program] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical Program ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/ProgramModelTests.cs.
- [ ] T010 [ENTITY-Program] [OWNER-SPEC-009] Deliver the canonical Program model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/Program.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [P] [ENTITY-Course] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical Course ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CourseModelTests.cs.
- [ ] T012 [ENTITY-Course] [OWNER-SPEC-009] Deliver the canonical Course model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/Course.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [P] [ENTITY-CurriculumCourse] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical CurriculumCourse ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CurriculumCourseModelTests.cs.
- [ ] T014 [ENTITY-CurriculumCourse] [OWNER-SPEC-009] Deliver the canonical CurriculumCourse model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/CurriculumCourse.cs after T013 fails for the expected reason (depends on T013).
- [ ] T015 [P] [ENTITY-CoursePrerequisite] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical CoursePrerequisite ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CoursePrerequisiteModelTests.cs.
- [ ] T016 [ENTITY-CoursePrerequisite] [OWNER-SPEC-009] Deliver the canonical CoursePrerequisite model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/CoursePrerequisite.cs after T015 fails for the expected reason (depends on T015).
- [ ] T017 [P] [ENTITY-PolicySet] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical PolicySet ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/PolicySetModelTests.cs.
- [ ] T018 [ENTITY-PolicySet] [OWNER-SPEC-009] Deliver the canonical PolicySet model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/PolicySet.cs after T017 fails for the expected reason (depends on T017).
- [ ] T019 [P] [ENTITY-PolicyRule] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical PolicyRule ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/PolicyRuleModelTests.cs.
- [ ] T020 [ENTITY-PolicyRule] [OWNER-SPEC-009] Deliver the canonical PolicyRule model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/PolicyRule.cs after T019 fails for the expected reason (depends on T019).
- [ ] T021 [P] [ENTITY-ImportBatch] [OWNER-SPEC-009] Create the future failing invariant/schema/serialization checks for canonical ImportBatch ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec009/ImportBatchModelTests.cs.
- [ ] T022 [ENTITY-ImportBatch] [OWNER-SPEC-009] Deliver the canonical ImportBatch model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/ImportBatch.cs after T021 fails for the expected reason (depends on T021).
- [ ] T023 [API-Endpoint01] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/admin/programs in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T024 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/admin/programs in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint01ContractTests.cs.
- [ ] T025 [API-Endpoint01] [OWNER-SPEC-009] Deliver the sole canonical GET /api/admin/programs handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T024 fails for the expected reason (depends on T024).
- [ ] T026 [API-Endpoint02] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/courses in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T027 [P] [API-Endpoint02] Verify every documented response and authorization outcome for POST /api/admin/courses in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint02ContractTests.cs.
- [ ] T028 [API-Endpoint02] [OWNER-SPEC-009] Deliver the sole canonical POST /api/admin/courses handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T027 fails for the expected reason (depends on T027).
- [ ] T029 [API-Endpoint03] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/admin/curricula/{id} in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T030 [P] [API-Endpoint03] Verify every documented response and authorization outcome for PUT /api/admin/curricula/{id} in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint03ContractTests.cs.
- [ ] T031 [API-Endpoint03] [OWNER-SPEC-009] Deliver the sole canonical PUT /api/admin/curricula/{id} handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T030 fails for the expected reason (depends on T030).
- [ ] T032 [API-Endpoint04] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/policies/{id}/validate in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T033 [P] [API-Endpoint04] Verify every documented response and authorization outcome for POST /api/admin/policies/{id}/validate in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint04ContractTests.cs.
- [ ] T034 [API-Endpoint04] [OWNER-SPEC-009] Deliver the sole canonical POST /api/admin/policies/{id}/validate handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T033 fails for the expected reason (depends on T033).
- [ ] T035 [API-Endpoint05] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/policies/{id}/simulate in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T036 [P] [API-Endpoint05] Verify every documented response and authorization outcome for POST /api/admin/policies/{id}/simulate in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint05ContractTests.cs.
- [ ] T037 [API-Endpoint05] [OWNER-SPEC-009] Deliver the sole canonical POST /api/admin/policies/{id}/simulate handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T036 fails for the expected reason (depends on T036).
- [ ] T038 [API-Endpoint06] [OWNER-SPEC-009] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/policies/{id}/publish in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T039 [P] [API-Endpoint06] Verify every documented response and authorization outcome for POST /api/admin/policies/{id}/publish in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint06ContractTests.cs.
- [ ] T040 [API-Endpoint06] [OWNER-SPEC-009] Deliver the sole canonical POST /api/admin/policies/{id}/publish handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec009Endpoints.cs after T039 fails for the expected reason (depends on T039).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Missing prerequisite (FR-2, FR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T041 [P] [AC-1] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-1Tests.cs for AC-1: Missing prerequisite (FR-2, FR-3): Given an import references course IN321 that does not exist When validation runs Then the row is rejected with source row and missing code And no part of that import is published.
### US2 - Prerequisite cycle (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T042 [P] [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-2Tests.cs for AC-2: Prerequisite cycle (FR-3): Given Course A requires B and B requires A When the curriculum is validated Then publication is blocked with the cycle path.
### US3 - Policy simulation (FR-4, FR-5) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T043 [P] [AC-3] [FR-4] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-3Tests.cs for AC-3: Policy simulation (FR-4, FR-5): Given a draft Project I rule requiring GPA 2.0 and 96 credits When simulated with GPA 2.1 and 95 credits Then it fails with the earned-credit reason And identifies the draft policy version/source.
### US4 - Governed catalogue publish (FR-1, FR-6, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T044 [P] [AC-4] [FR-1] [FR-6] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-4Tests.cs for AC-4: Governed catalogue publish (FR-1, FR-6, FR-7): Given an authorized Admin has a valid draft course/curriculum/policy version When publication is confirmed Then the prior published version remains immutable and is superseded And an unauthorized user cannot publish it.
### US5 - Concurrent publication has one winner (FR-6, FR-8, FR-9) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T045 [P] [AC-5] [FR-6] [FR-8] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-5Tests.cs for AC-5: Concurrent publication has one winner (FR-6, FR-8, FR-9): Given two admins preview conflicting changes for the same policy scope and expected version When both confirmations execute concurrently Then exactly one immutable version is published And the other receives 409 STALE_PREVIEW with the current version.
### US6 - Edited draft invalidates preview (FR-8, FR-10) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T046 [P] [AC-6] [FR-8] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-6Tests.cs for AC-6: Edited draft invalidates preview (FR-8, FR-10): Given an admin receives a preview token and then edits the draft When the old token is confirmed twice with one idempotency key Then confirmation is rejected as STALE_PREVIEW And retry returns the same rejection And no publication or duplicate audit event is created.
### US7 - Catalogue and policy quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Catalogue, Prerequisites, and Policy Administration.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T040.
- [ ] T047 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-7Tests.cs for AC-7: Catalogue and policy quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given a 10,000-row staging import, fixed simulation input/version, and fault-injected publication fixture When the feature quality gate executes Then import validation finishes within 30 seconds And simulations are deterministic And publication is all-or-nothing And each published change records actor, reason, source, and timestamp.
- [ ] T048 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-1Tests.cs and assert: Duplicate course code differs only by case/spacing -> normalize and reject duplicate.
- [ ] T049 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-2Tests.cs and assert: Published course is referenced by history -> deactivate/supersede, do not delete.
- [ ] T050 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-3Tests.cs and assert: Concurrent policy publish -> one succeeds; stale version gets 409.
- [ ] T051 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-4Tests.cs and assert: Unknown rule type/config -> reject draft validation.
- [ ] T052 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-5Tests.cs and assert: Audit persistence fails during publish -> the policy/catalogue version and activation change roll back in the same local SQL transaction.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T053 [P] [FR-1] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Academics/CataloguePublicationTests.cs. Test focus: normalized codes, missing references, credits, cycles, preview provenance and all-or-nothing publish. Prove the requirement against its linked AC/EC fixtures: Admin MUST manage programs, curricula, courses, credit values, status, prerequisites, minimum grades/GPA/earned credits, and cohort scope.
- [ ] T054 [FR-1] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Deliver FR-1 through the bounded Catalogue validation and publication workstream at src/StudentRegistration.Server/Modules/Academics/CataloguePublicationService.cs only after T053 fails for the expected reason (depends on T053): Admin MUST manage programs, curricula, courses, credit values, status, prerequisites, minimum grades/GPA/earned credits, and cohort scope.
- [ ] T055 [P] [FR-2] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Academics/CataloguePublicationTests.cs. Test focus: normalized codes, missing references, credits, cycles, preview provenance and all-or-nothing publish. Prove the requirement against its linked AC/EC fixtures: Imports MUST provide preview, row-level validation, provenance, and all-or-nothing publication.
- [ ] T056 [FR-2] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Deliver FR-2 through the bounded Catalogue validation and publication workstream at src/StudentRegistration.Server/Modules/Academics/CataloguePublicationService.cs only after T055 fails for the expected reason (depends on T055): Imports MUST provide preview, row-level validation, provenance, and all-or-nothing publication.
- [ ] T057 [P] [FR-3] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Academics/CataloguePublicationTests.cs. Test focus: normalized codes, missing references, credits, cycles, preview provenance and all-or-nothing publish. Prove the requirement against its linked AC/EC fixtures: The system MUST detect missing references, duplicate codes, invalid credits, and prerequisite cycles before publish.
- [ ] T058 [FR-3] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Deliver FR-3 through the bounded Catalogue validation and publication workstream at src/StudentRegistration.Server/Modules/Academics/CataloguePublicationService.cs only after T057 fails for the expected reason (depends on T057): The system MUST detect missing references, duplicate codes, invalid credits, and prerequisite cycles before publish.
- [ ] T059 [P] [FR-4] [WORKSTREAM-POLICY-ADMINISTRATION] Create the future failing FR-4 checks in tests/StudentRegistration.IntegrationTests/Academics/PolicyAdministrationTests.cs. Test focus: typed values, simulation, immutable versions, authorization and audit. Prove the requirement against its linked AC/EC fixtures: Admin MUST manage typed effective-dated PolicySet/PolicyRule values.
- [ ] T060 [FR-4] [WORKSTREAM-POLICY-ADMINISTRATION] Deliver FR-4 through the bounded Policy administration workstream at src/StudentRegistration.Server/Modules/Academics/PolicyAdministrationService.cs only after T059 fails for the expected reason (depends on T059): Admin MUST manage typed effective-dated PolicySet/PolicyRule values.
- [ ] T061 [P] [FR-5] [WORKSTREAM-POLICY-ADMINISTRATION] Create the future failing FR-5 checks in tests/StudentRegistration.IntegrationTests/Academics/PolicyAdministrationTests.cs. Test focus: typed values, simulation, immutable versions, authorization and audit. Prove the requirement against its linked AC/EC fixtures: Admin MUST simulate a policy decision against test student inputs before publication.
- [ ] T062 [FR-5] [WORKSTREAM-POLICY-ADMINISTRATION] Deliver FR-5 through the bounded Policy administration workstream at src/StudentRegistration.Server/Modules/Academics/PolicyAdministrationService.cs only after T061 fails for the expected reason (depends on T061): Admin MUST simulate a policy decision against test student inputs before publication.
- [ ] T063 [P] [FR-6] [WORKSTREAM-POLICY-ADMINISTRATION] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Academics/PolicyAdministrationTests.cs. Test focus: typed values, simulation, immutable versions, authorization and audit. Prove the requirement against its linked AC/EC fixtures: Published catalogue/policy versions MUST be immutable and superseded.
- [ ] T064 [FR-6] [WORKSTREAM-POLICY-ADMINISTRATION] Deliver FR-6 through the bounded Policy administration workstream at src/StudentRegistration.Server/Modules/Academics/PolicyAdministrationService.cs only after T063 fails for the expected reason (depends on T063): Published catalogue/policy versions MUST be immutable and superseded.
- [ ] T065 [P] [FR-7] [WORKSTREAM-POLICY-ADMINISTRATION] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Academics/PolicyAdministrationTests.cs. Test focus: typed values, simulation, immutable versions, authorization and audit. Prove the requirement against its linked AC/EC fixtures: Only approved Admin/Registrar permissions MAY publish.
- [ ] T066 [FR-7] [WORKSTREAM-POLICY-ADMINISTRATION] Deliver FR-7 through the bounded Policy administration workstream at src/StudentRegistration.Server/Modules/Academics/PolicyAdministrationService.cs only after T065 fails for the expected reason (depends on T065): Only approved Admin/Registrar permissions MAY publish.
- [ ] T067 [P] [FR-8] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Academics/PublicationRaceTests.cs. Test focus: bound preview token, scope lock, dependency recheck, one winner, replay and payload mismatch. Prove the requirement against its linked AC/EC fixtures: Every update/publish confirmation MUST include the expected draft version and a preview token bound to actor, scope, canonical draft content, dependency versions, and expiry.
- [ ] T068 [FR-8] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Deliver FR-8 through the bounded Preview, concurrency and idempotency workstream at src/StudentRegistration.Server/Modules/Academics/PublicationConfirmationService.cs only after T067 fails for the expected reason (depends on T067): Every update/publish confirmation MUST include the expected draft version and a preview token bound to actor, scope, canonical draft content, dependency versions, and expiry.
- [ ] T069 [P] [FR-9] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Academics/PublicationRaceTests.cs. Test focus: bound preview token, scope lock, dependency recheck, one winner, replay and payload mismatch. Prove the requirement against its linked AC/EC fixtures: Catalogue/policy publication MUST lock the affected publication scope, revalidate references and conflicts inside one transaction, and atomically create its immutable version and audit event.
- [ ] T070 [FR-9] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Deliver FR-9 through the bounded Preview, concurrency and idempotency workstream at src/StudentRegistration.Server/Modules/Academics/PublicationConfirmationService.cs only after T069 fails for the expected reason (depends on T069): Catalogue/policy publication MUST lock the affected publication scope, revalidate references and conflicts inside one transaction, and atomically create its immutable version and audit event.
- [ ] T071 [P] [FR-10] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Academics/PublicationRaceTests.cs. Test focus: bound preview token, scope lock, dependency recheck, one winner, replay and payload mismatch. Prove the requirement against its linked AC/EC fixtures: Retryable import/publish commands MUST use an idempotency key; replay of the same key/payload returns its stored result and reuse with a different payload returns 409 IDEMPOTENCY_KEY_REUSED.
- [ ] T072 [FR-10] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Deliver FR-10 through the bounded Preview, concurrency and idempotency workstream at src/StudentRegistration.Server/Modules/Academics/PublicationConfirmationService.cs only after T071 fails for the expected reason (depends on T071): Retryable import/publish commands MUST use an idempotency key; replay of the same key/payload returns its stored result and reuse with a different payload returns 409 IDEMPOTENCY_KEY_REUSED.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T073 [P] [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-5] [AC-6] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-05 in tests/StudentRegistration.E2ETests/Specs/Spec009/CatalogueAdministrationPageFeatureTests.cs.
- [ ] T074 [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-5] [AC-6] Deliver the sole canonical Blazor implementation for ADM-05 at src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor after T073 and the SPEC-003 contract/component checks fail for expected reasons (depends on T073).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T075 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-1.md: Import validation for 10,000 rows SHOULD finish within 30 seconds in staging.
- [ ] T076 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-2.md: Simulation MUST be deterministic for the same version/input.
- [ ] T077 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-3.md: Publication MUST be transactional.
- [ ] T078 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-4.md: Every published change MUST have actor, reason, source, and timestamp.

## Phase 7 - Scope and Release Evidence

- [ ] T079 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-009-scope-review.md that OS-1 remains excluded: Scraping public web pages as production catalogue source.
- [ ] T080 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-009-scope-review.md that OS-2 remains excluded: Arbitrary policy scripting.
- [ ] T081 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-009-scope-review.md that OS-3 remains excluded: Silent auto-correction of referential errors.
- [ ] T082 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-009-scope-review.md that OS-4 remains excluded: Deleting historical course/policy records.
- [ ] T083 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-009-traceability.md and reject release if any row lacks passing evidence.
- [ ] T084 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-009 in docs/release-evidence/SPEC-009-release-approval.md.

No task is complete and no implementation file has been created.
