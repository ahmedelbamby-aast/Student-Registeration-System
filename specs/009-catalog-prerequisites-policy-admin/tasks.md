# Tasks: Catalogue, Prerequisites, and Policy Administration

**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13. Execute remaining readiness and test-first tasks in dependency order.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Readiness and approval precede implementation. A single test task may cover a coherent workstream, but parallel tasks never write the same file.

## Phase 1 - Planning Readiness and Recorded Gate A Approval

- [x] T001 [DEP-SPEC-002] Baseline approved typed rules, official-versus-demo source classifications, the 18-credit normal and 12-credit GPA-below-2.0 demo limits, and remaining unpublished policy decisions from specs/002-aastmt-policy-rulebook/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T002 [DEP-SPEC-003] Baseline ADM-05 states, components, accessibility, and functional tests from specs/003-ux-storyboard-accessibility/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T003 [DEP-SPEC-005] Baseline immutable-history, mapping, index, transaction, and audit persistence from specs/005-erd-data-lifecycle/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T004 [DEP-SPEC-006] Baseline errors, bounded pages, rowversion, idempotency, and preview conventions from specs/006-domain-class-api-contracts/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T005 [DEP-SPEC-008] Baseline program/cohort/term identifiers and academic test inputs from specs/008-academic-term-student-profile/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T006 [DEP-SPEC-018] Baseline security, load, audit, and operations gates from specs/018-quality-security-scalability-operations/ in specs/009-catalog-prerequisites-policy-admin/dependency-baseline.md.
- [x] T007 [GATE] Analyze and freeze the 19-course `docs/DEMO_CURRICULUM.md` official-source snapshot, URL/access-date and synthetic-field provenance, simple demo policy boundaries, ownership, draft/version/import states, contracts, publication races, route links, and task traces in specs/009-catalog-prerequisites-policy-admin/checklists/implementation-readiness.md.
- [x] T008 [GATE] Record Ahmed ELbamby's 2026-07-13 Gate A demo approval while fulfilling the Registrar/Policy SME review perspective in specs/009-catalog-prerequisites-policy-admin/checklists/approval.md; T009 and later remain blocked until T001-T007 pass.

## Phase 2 - Failing Model and Contract Tests

- [ ] T009 [P] [ENTITY-Program] [OWNER-SPEC-009] Create failing scoped/versioned Program checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/ProgramModelTests.cs.
- [ ] T010 [P] [ENTITY-Course] [OWNER-SPEC-009] Create failing normalized code, credit, status, version ownership, and official-versus-synthetic field-provenance checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CourseModelTests.cs.
- [ ] T011 [P] [ENTITY-CurriculumCourse] [OWNER-SPEC-009] Create failing curriculum/course/version, cohort-scope, term, and field-provenance checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CurriculumCourseModelTests.cs.
- [ ] T012 [P] [ENTITY-CoursePrerequisite] [OWNER-SPEC-009] Create failing self-edge, missing-reference, cycle, and prerequisite-provenance checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CoursePrerequisiteModelTests.cs.
- [ ] T013 [P] [ENTITY-PolicySet] [OWNER-SPEC-009] Create failing typed effective-date, scope, lifecycle, and immutable-publication checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/PolicySetModelTests.cs.
- [ ] T014 [P] [ENTITY-PolicyRule] [OWNER-SPEC-009] Create failing typed rule/value/source checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/PolicyRuleModelTests.cs.
- [ ] T015 [P] [ENTITY-ImportBatch] [OWNER-SPEC-009] Create failing target/source/access-date/hash/synthetic-field-count/state/error/version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/ImportBatchModelTests.cs.
- [ ] T016 [P] [ENTITY-CatalogueDraft] [OWNER-SPEC-009] Create failing editable lifecycle/content-hash/rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CatalogueDraftModelTests.cs.
- [ ] T017 [P] [ENTITY-CatalogueVersion] [OWNER-SPEC-009] Create failing immutable scope/version/supersession checks in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CatalogueVersionModelTests.cs.
- [ ] T018 [API-Endpoint01] [OWNER-SPEC-009] Finalize GET /api/admin/programs in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T019 [P] [API-Endpoint01] Create failing bounded program list and authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint01ContractTests.cs for GET /api/admin/programs.
- [ ] T020 [API-Endpoint02] [OWNER-SPEC-009] Finalize GET /api/admin/catalogue/versions in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T021 [P] [API-Endpoint02] Create failing immutable version list/filter checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint02ContractTests.cs for GET /api/admin/catalogue/versions.
- [ ] T022 [API-Endpoint03] [OWNER-SPEC-009] Finalize GET /api/admin/catalogue/drafts/{draftId} in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T023 [P] [API-Endpoint03] Create failing draft detail/direct-object authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint03ContractTests.cs for GET /api/admin/catalogue/drafts/{draftId}.
- [ ] T024 [API-Endpoint04] [OWNER-SPEC-009] Finalize PUT /api/admin/catalogue/drafts/{draftId} in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T025 [P] [API-Endpoint04] Create failing expected-version, URL/access-date/field-classification provenance, preview-invalidation, and no-partial-edit checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint04ContractTests.cs for PUT /api/admin/catalogue/drafts/{draftId}.
- [ ] T026 [API-Endpoint05] [OWNER-SPEC-009] Finalize POST /api/admin/catalogue/imports in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T027 [P] [API-Endpoint05] Create failing source/access-date/hash/synthetic-field-manifest/idempotent import-creation checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint05ContractTests.cs for POST /api/admin/catalogue/imports.
- [ ] T028 [API-Endpoint06] [OWNER-SPEC-009] Finalize GET /api/admin/catalogue/imports/{importId} in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T029 [P] [API-Endpoint06] Create failing import status/row-error/minimization checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint06ContractTests.cs for GET /api/admin/catalogue/imports/{importId}.
- [ ] T030 [API-Endpoint07] [OWNER-SPEC-009] Finalize POST /api/admin/catalogue/imports/{importId}/validate in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T031 [P] [API-Endpoint07] Create failing complete-graph, official/synthetic provenance validation, and bound-preview checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint07ContractTests.cs for POST /api/admin/catalogue/imports/{importId}/validate.
- [ ] T032 [API-Endpoint08] [OWNER-SPEC-009] Finalize POST /api/admin/catalogue/imports/{importId}/publish in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T033 [P] [API-Endpoint08] Create failing stale-preview/idempotency/atomic-publication checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint08ContractTests.cs for POST /api/admin/catalogue/imports/{importId}/publish.
- [ ] T034 [API-Endpoint09] [OWNER-SPEC-009] Finalize GET /api/admin/policies in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T035 [P] [API-Endpoint09] Create failing bounded policy list and source/status checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint09ContractTests.cs for GET /api/admin/policies.
- [ ] T036 [API-Endpoint10] [OWNER-SPEC-009] Finalize POST /api/admin/policies in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T037 [P] [API-Endpoint10] Create failing typed policy-draft create/idempotency checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint10ContractTests.cs for POST /api/admin/policies.
- [ ] T038 [API-Endpoint11] [OWNER-SPEC-009] Finalize PUT /api/admin/policies/{policySetId} in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T039 [P] [API-Endpoint11] Create failing expected-version, typed-value, source, and preview-invalidation checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint11ContractTests.cs for PUT /api/admin/policies/{policySetId}.
- [ ] T040 [API-Endpoint12] [OWNER-SPEC-009] Finalize POST /api/admin/policies/{policySetId}/validate in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T041 [P] [API-Endpoint12] Create failing typed validation and preview-binding checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint12ContractTests.cs for POST /api/admin/policies/{policySetId}/validate.
- [ ] T042 [API-Endpoint13] [OWNER-SPEC-009] Finalize POST /api/admin/policies/{policySetId}/simulate in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T043 [P] [API-Endpoint13] Create failing deterministic simulation/explanation checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint13ContractTests.cs for POST /api/admin/policies/{policySetId}/simulate.
- [ ] T044 [API-Endpoint14] [OWNER-SPEC-009] Finalize POST /api/admin/policies/{policySetId}/publish in specs/009-catalog-prerequisites-policy-admin/contracts/api.md.
- [ ] T045 [P] [API-Endpoint14] Create failing one-winner, immutable version, audit, and replay checks in tests/StudentRegistration.ContractTests/Specs/Spec009/Endpoint14ContractTests.cs for POST /api/admin/policies/{policySetId}/publish.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [ ] T046 [P] [AC-1] [FR-2] [FR-3] Create missing-prerequisite import coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-1Tests.cs.
- [ ] T047 [P] [AC-2] [FR-3] Create prerequisite-cycle path coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-2Tests.cs.
- [ ] T048 [P] [AC-3] [FR-4] [FR-5] Create source-backed DS413 plus normal 18/19-credit and GPA-below-2.0 12/13-credit typed simulation/explanation coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-3Tests.cs.
- [ ] T049 [P] [AC-4] [FR-1] [FR-6] [FR-7] Create immutable governed publication coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-4Tests.cs.
- [ ] T050 [P] [AC-5] [FR-6] [FR-8] [FR-9] Create two-admin publication one-winner coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-5Tests.cs.
- [ ] T051 [P] [AC-6] [FR-8] [FR-10] Create edited-draft stale-preview deterministic-replay coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-6Tests.cs.
- [ ] T052 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create catalogue/policy quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/AC-7Tests.cs.
- [ ] T053 [P] [EC-1] Create normalized duplicate-code coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-1Tests.cs.
- [ ] T054 [P] [EC-2] Create historical-reference deactivation coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-2Tests.cs.
- [ ] T055 [P] [EC-3] Create stale concurrent policy publication coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-3Tests.cs.
- [ ] T056 [P] [EC-4] Create unknown typed-rule rejection coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-4Tests.cs.
- [ ] T057 [P] [EC-5] Create audit-failure full-rollback coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec009/EdgeCases/EC-5Tests.cs.
- [ ] T058 [P] [SC-1] Create invalid/cyclic publication-block outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/SC-1OutcomeTests.cs.
- [ ] T059 [P] [SC-2] Create immutable/auditable version outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/SC-2OutcomeTests.cs.
- [ ] T060 [P] [SC-3] Create preview/explanation outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec009/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [ ] T061 [FR-1] [FR-2] [FR-3] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Create the failing consolidated 19-course snapshot/provenance/draft/import/graph/version/publication suite in tests/StudentRegistration.IntegrationTests/Academics/CataloguePublicationTests.cs.
- [ ] T062 [FR-4] [FR-5] [FR-6] [FR-7] [WORKSTREAM-POLICY-ADMINISTRATION] Create the failing consolidated simple demo rule boundaries/typed-policy/simulation/version/authorization suite in tests/StudentRegistration.IntegrationTests/Academics/PolicyAdministrationTests.cs.
- [ ] T063 [FR-8] [FR-9] [FR-10] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Create the failing consolidated preview/scope-lock/dependency/idempotency race suite in tests/StudentRegistration.IntegrationTests/Academics/PublicationRaceTests.cs.
- [ ] T064 [ENTITY-Program] [OWNER-SPEC-009] Deliver the canonical Program at src/StudentRegistration.Academics/Domain/Program.cs after T009 fails.
- [ ] T065 [ENTITY-Course] [OWNER-SPEC-009] Deliver the canonical Course at src/StudentRegistration.Academics/Domain/Course.cs after T010 fails.
- [ ] T066 [ENTITY-CurriculumCourse] [OWNER-SPEC-009] Deliver the canonical CurriculumCourse at src/StudentRegistration.Academics/Domain/CurriculumCourse.cs after T011 fails.
- [ ] T067 [ENTITY-CoursePrerequisite] [OWNER-SPEC-009] Deliver the canonical CoursePrerequisite at src/StudentRegistration.Academics/Domain/CoursePrerequisite.cs after T012 fails.
- [ ] T068 [ENTITY-PolicySet] [OWNER-SPEC-009] Deliver the canonical PolicySet at src/StudentRegistration.Academics/Domain/PolicySet.cs after T013 fails.
- [ ] T069 [ENTITY-PolicyRule] [OWNER-SPEC-009] Deliver the canonical PolicyRule at src/StudentRegistration.Academics/Domain/PolicyRule.cs after T014 fails.
- [ ] T070 [ENTITY-ImportBatch] [OWNER-SPEC-009] Deliver the canonical ImportBatch at src/StudentRegistration.Academics/Domain/ImportBatch.cs after T015 fails.
- [ ] T071 [ENTITY-CatalogueDraft] [OWNER-SPEC-009] Deliver the canonical CatalogueDraft at src/StudentRegistration.Academics/Domain/CatalogueDraft.cs after T016 fails.
- [ ] T072 [ENTITY-CatalogueVersion] [OWNER-SPEC-009] Deliver the canonical CatalogueVersion at src/StudentRegistration.Academics/Domain/CatalogueVersion.cs after T017 fails.
- [ ] T073 [FR-1] [FR-2] [FR-3] [WORKSTREAM-CATALOGUE-VALIDATION-AND-PUBLICATION] Deliver curated snapshot/provenance plus catalogue draft/import/publication behavior at src/StudentRegistration.Academics/Application/CataloguePublicationService.cs after T061 fails.
- [ ] T074 [FR-4] [FR-5] [FR-6] [FR-7] [WORKSTREAM-POLICY-ADMINISTRATION] Deliver the typed simple demo rule set and policy administration at src/StudentRegistration.Academics/Application/PolicyAdministrationService.cs after T062 fails.
- [ ] T075 [FR-8] [FR-9] [FR-10] [WORKSTREAM-PREVIEW-CONCURRENCY-AND-IDEMPOTENCY] Deliver preview, transaction, and idempotency confirmation at src/StudentRegistration.Academics/Application/PublicationConfirmationService.cs after T063 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [ ] T076 [API-Endpoint01] Deliver GET /api/admin/programs at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T019 and T061 fail.
- [ ] T077 [API-Endpoint02] Deliver GET /api/admin/catalogue/versions at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T021 and T061 fail.
- [ ] T078 [API-Endpoint03] Deliver GET /api/admin/catalogue/drafts/{draftId} at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T023 and T061 fail.
- [ ] T079 [API-Endpoint04] Deliver PUT /api/admin/catalogue/drafts/{draftId} at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T025, T061, and T063 fail.
- [ ] T080 [API-Endpoint05] Deliver POST /api/admin/catalogue/imports at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T027, T061, and T063 fail.
- [ ] T081 [API-Endpoint06] Deliver GET /api/admin/catalogue/imports/{importId} at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T029 and T061 fail.
- [ ] T082 [API-Endpoint07] Deliver POST /api/admin/catalogue/imports/{importId}/validate at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T031 and T061 fail.
- [ ] T083 [API-Endpoint08] Deliver POST /api/admin/catalogue/imports/{importId}/publish at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T033, T061, and T063 fail.
- [ ] T084 [API-Endpoint09] Deliver GET /api/admin/policies at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T035 and T062 fail.
- [ ] T085 [API-Endpoint10] Deliver POST /api/admin/policies at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T037, T062, and T063 fail.
- [ ] T086 [API-Endpoint11] Deliver PUT /api/admin/policies/{policySetId} at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T039, T062, and T063 fail.
- [ ] T087 [API-Endpoint12] Deliver POST /api/admin/policies/{policySetId}/validate at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T041 and T062 fail.
- [ ] T088 [API-Endpoint13] Deliver POST /api/admin/policies/{policySetId}/simulate at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T043 and T062 fail.
- [ ] T089 [API-Endpoint14] Deliver POST /api/admin/policies/{policySetId}/publish at src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs after T045, T062, and T063 fail.

## Phase 6 - Frontend Functional Tests and Page

- [ ] T090 [P] [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-5] [AC-6] Create failing catalogue snapshot provenance/synthetic-label/draft/import/version/simple-policy journeys in tests/StudentRegistration.E2ETests/Specs/Spec009/CatalogueAdministrationPageFeatureTests.cs.
- [ ] T091 [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-5] [AC-6] Deliver CatalogueAdministrationPage at src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor after T090 and SPEC-017 contributor tests fail.

## Phase 7 - Quality, Scope, and Release Evidence

- [ ] T092 [ENTITY-Program] [ENTITY-Course] [ENTITY-CurriculumCourse] [ENTITY-CoursePrerequisite] [ENTITY-PolicySet] [ENTITY-PolicyRule] [ENTITY-CatalogueDraft] [ENTITY-CatalogueVersion] [ENTITY-ImportBatch] [PERSISTENCE-MAPPING] Create the failing real-SQL catalogue/policy mapping, provenance classification, normalized uniqueness, immutable version, graph FK, lifecycle, rowversion, and index suite in tests/StudentRegistration.IntegrationTests/Specs/Spec009/CatalogueModelConfigurationTests.cs.
- [ ] T093 [ENTITY-Program] [ENTITY-Course] [ENTITY-CurriculumCourse] [ENTITY-CoursePrerequisite] [ENTITY-PolicySet] [ENTITY-PolicyRule] [ENTITY-CatalogueDraft] [ENTITY-CatalogueVersion] [ENTITY-ImportBatch] [PERSISTENCE-MAPPING] Deliver the complete catalogue/policy EF Core mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/CatalogueModelConfiguration.cs after T092 fails; SPEC-004 remains the sole StudentRegistrationDbContext writer.
- [ ] T094 [P] [NFR-1] Produce 10,000-row validation evidence in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-1.md.
- [ ] T095 [P] [NFR-2] Produce deterministic simulation evidence in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-2.md.
- [ ] T096 [P] [NFR-3] Produce transactional publication/fault evidence in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-3.md.
- [ ] T097 [P] [NFR-4] Produce actor/reason/source/access-date/value-classification/time audit evidence in tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-009-NFR-4.md.
- [ ] T098 [OS-1] [OS-2] [OS-3] [OS-4] Record verified no-live-scrape/no-official-claim/no-advisor-or-exception catalogue/policy scope exclusions in docs/release-evidence/SPEC-009-scope-review.md.
- [ ] T099 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-009-traceability.md.
- [ ] T100 [GATE] Record Registrar, product, Admin, data, QA, security, accessibility, and operations release approvals in docs/release-evidence/SPEC-009-release-approval.md.

No task is complete and no implementation file has been created.
