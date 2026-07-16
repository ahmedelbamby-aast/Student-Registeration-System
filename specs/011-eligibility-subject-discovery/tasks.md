# Tasks: Eligibility and Subject Discovery

**Status**: COMPLETE for the bounded Gate A demo implementation approved by Ahmed ELbamby on 2026-07-13; all tasks are evidence-backed.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Approval is the final planning gate. Tests precede projection, query, endpoint, and page delivery; parallel tasks use distinct files.

## Phase 1 - Planning Readiness and Recorded Gate A Approval

- [x] T001 [DEP-SPEC-002] Baseline `DEMO-POC-2026.1`, its 18-credit normal target/maximum, 12-credit probation maximum, reason codes, provenance, and remaining non-demo decisions from specs/002-aastmt-policy-rulebook/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T002 [DEP-SPEC-003] Baseline STU-02/STU-03 states, explanations, accessibility, and functional tests from specs/003-ux-storyboard-accessibility/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T003 [DEP-SPEC-006] [DEP-SPEC-008] Baseline the canonical Page<T> response with applied-sort echo from specs/006-domain-class-api-contracts/ plus authenticated academic context, profile, holds, and StudentTermAcademicState versions from specs/008-academic-term-student-profile/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T004 [DEP-SPEC-009] Baseline canonical catalogue and approved PolicySet versions from specs/009-catalog-prerequisites-policy-admin/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T005 [DEP-SPEC-010] Baseline offering/group/activity/staff/room/capacity/version projections from specs/010-offerings-groups-resources/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T006 [DEP-SPEC-018] Baseline read-load, security, accessibility, and operations gates from specs/018-quality-security-scalability-operations/ in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [x] T007 [GATE] Analyze the approved demo policy/curriculum, 18/12 load projection, fail-closed behavior, projection ownership, explanation completeness, pagination/sort protocol, endpoints, routes, and task traces and freeze the result in specs/011-eligibility-subject-discovery/checklists/implementation-readiness.md.
- [x] T008 [GATE] Record Ahmed ELbamby's 2026-07-13 Gate A demo approval while fulfilling the Policy SME review perspective in specs/011-eligibility-subject-discovery/checklists/approval.md; T009 and later remain blocked until T001-T007 pass.

## Phase 2 - Failing Projection and Contract Tests

- [x] T009 [P] [ENTITY-OfferingEligibility] [OWNER-SPEC-011] Create failing deterministic/fail-closed academic/catalogue/policy/offering/current-plan version-context projection checks in tests/StudentRegistration.IntegrationTests/Specs/Spec011/OfferingEligibilityModelTests.cs.
- [x] T010 [P] [ENTITY-EligibilityReason] [OWNER-SPEC-011] Create failing governed code/order/pass/block/value/policy/source-access/approval/effective/override/support explanation checks in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EligibilityReasonModelTests.cs.
- [x] T011 [P] [ENTITY-GroupSummary] [OWNER-SPEC-011] Create failing lifecycle-only state/selectable/capacity/non-selectable-reason/nested-meeting-staff/activity/room/time/version projection checks in tests/StudentRegistration.IntegrationTests/Specs/Spec011/GroupSummaryModelTests.cs.
- [x] T012 [API-Endpoint01] [OWNER-SPEC-011] Finalize the T007-frozen authorization, allow-listed query, Page<T>, stable-sort, success, and error matrix for GET /api/student/terms/{termId}/offerings in specs/011-eligibility-subject-discovery/contracts/api.md.
- [x] T013 [P] [API-Endpoint01] Create failing self-scope, server eligibility, search/filter, page limits, stable tie-break, canonical Page<OfferingEligibilityDto>, and applied-sort echo checks in tests/StudentRegistration.ContractTests/Specs/Spec011/Endpoint01ContractTests.cs for GET /api/student/terms/{termId}/offerings.
- [x] T014 [API-Endpoint02] [OWNER-SPEC-011] Finalize the T007-frozen authorized-context, complete detail/version, refresh-only EC-2, success, and error matrix for GET /api/student/offerings/{offeringId}/eligibility in specs/011-eligibility-subject-discovery/contracts/api.md.
- [x] T015 [P] [API-Endpoint02] Create failing authorized-context, complete reason/group detail, unavailable, and stale-advisory checks in tests/StudentRegistration.ContractTests/Specs/Spec011/Endpoint02ContractTests.cs for GET /api/student/offerings/{offeringId}/eligibility.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [x] T016 [P] [AC-1] [FR-1] [FR-2] [FR-5] Create 15-plus-3-to-default-18 eligible offering, load projection, and complete activity/group-detail coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-1Tests.cs.
- [x] T017 [P] [AC-2] [FR-4] [FR-6] Create unavailable required/current/policy/source explanation coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-2Tests.cs.
- [x] T018 [P] [AC-3] [FR-2] Create all-groups-full unavailable coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-3Tests.cs.
- [x] T019 [P] [AC-4] [FR-3] [FR-7] [FR-8] Create oversized-page rejection, valid stable page, and client-bypass coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-4Tests.cs.
- [x] T020 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create discovery quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-5Tests.cs; keep this task unchecked until T045-T048 measured evidence passes.
- [x] T021 [P] [EC-1] Create decision-data-unavailable fail-closed/support coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-1Tests.cs.
- [x] T022 [P] [EC-2] Create group-full-after-read detail-refresh coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-2Tests.cs; assert refreshed unavailable/version state only, while submission revalidation remains SPEC-014.
- [x] T023 [P] [EC-3] Create literal parameterized metacharacter search coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-3Tests.cs.
- [x] T024 [P] [EC-4] Create no-eligible reset/window/support state coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-4Tests.cs.
- [x] T025 [P] [SC-1] Create understandable available/unavailable reason outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/SC-1OutcomeTests.cs.
- [x] T026 [P] [SC-2] Create bounded relevant-offering discovery outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/SC-2OutcomeTests.cs; keep this task unchecked until T045-T048 measured evidence passes.
- [x] T027 [P] [SC-3] Create no-client-eligibility-escalation outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [x] T028 [FR-1] [FR-2] [FR-4] [FR-6] [FR-7] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the failing consolidated provider-port/current-plan demo window/standing/hold/prerequisite/GPA/earned-credit/repeat/load/capacity/conflict, empty-live-plan, injected-15-credit, 18/19 and 12/13 boundaries, governed reasons, fail-closed, and no-client-override suite in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs.
- [x] T029 [FR-3] [FR-8] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Create the failing consolidated query normalization/filter/page-limit/stable-sort suite in tests/StudentRegistration.IntegrationTests/Registration/OfferingSearchTests.cs.
- [x] T030 [FR-5] [WORKSTREAM-DISCOVERY-PROJECTION] Create the failing complete course/group/lifecycle/non-selectable-reason/nested-activity-staff/room/time/capacity/version suite in tests/StudentRegistration.ContractTests/Registration/GroupSummaryProjectionTests.cs.
- [x] T031 [ENTITY-OfferingEligibility] [OWNER-SPEC-011] Deliver the canonical OfferingEligibility projection at src/StudentRegistration.Registration/Domain/OfferingEligibility.cs after T009 fails.
- [x] T032 [ENTITY-EligibilityReason] [OWNER-SPEC-011] Deliver the canonical EligibilityReason at src/StudentRegistration.Registration/Domain/EligibilityReason.cs after T010 fails.
- [x] T033 [ENTITY-GroupSummary] [OWNER-SPEC-011] Deliver the canonical GroupSummary at src/StudentRegistration.Registration/Domain/GroupSummary.cs after T011 fails.
- [x] T034 [FR-1] [FR-2] [FR-4] [FR-6] [FR-7] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver server-authoritative eligibility at src/StudentRegistration.Registration/Application/EligibilityService.cs plus the Registration-owned ICurrentPlanReader and versioned empty-plan adapter after T028 fails.
- [x] T035 [FR-3] [FR-8] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Deliver bounded offering search at src/StudentRegistration.Registration/Application/OfferingSearchQuery.cs after T029 fails.
- [x] T036 [FR-5] [WORKSTREAM-DISCOVERY-PROJECTION] Deliver complete group projection at src/StudentRegistration.Registration/Application/GroupSummaryProjection.cs plus the narrow Academics/Scheduling eligibility read-port value contracts after T030 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [x] T037 [API-Endpoint01] Deliver GET /api/student/terms/{termId}/offerings at src/StudentRegistration.Registration/Endpoints/Spec011Endpoints.cs after T013, T028, T029, and T030 fail.
- [x] T038 [API-Endpoint02] Deliver GET /api/student/offerings/{offeringId}/eligibility at src/StudentRegistration.Registration/Endpoints/Spec011Endpoints.cs after T015, T028, and T030 fail.

## Phase 6 - Frontend Functional Tests and Pages

- [x] T039 [P] [STU-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Create failing discovery load-projection/search/filter/page/empty/error/reason journeys in tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDiscoveryPageFeatureTests.cs.
- [x] T040 [STU-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Deliver SubjectDiscoveryPage at src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor after T039 fails.
- [x] T041 [P] [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-6] [AC-1] [AC-2] [AC-3] Create failing complete offering/group/reason/full/stale journeys in tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDetailsPageFeatureTests.cs.
- [x] T042 [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-6] [AC-1] [AC-2] [AC-3] Deliver SubjectDetailsPage at src/StudentRegistration.Client/Pages/SubjectDetailsPage.razor after T041 and the SPEC-010 contribution tests fail.

## Phase 7 - Quality, Scope, and Release Evidence

- [x] T043 [ENTITY-OfferingEligibility] [ENTITY-EligibilityReason] [ENTITY-GroupSummary] [PERSISTENCE-MAPPING] Create the failing real-SQL read-model/provider-adapter contribution suite in tests/StudentRegistration.IntegrationTests/Specs/Spec011/RegistrationDiscoveryModelConfigurationTests.cs, proving keyless/read-only projections, complete Academics/Scheduling port values, required source indexes, parameterized predicates, and that SPEC-011 owns no writable eligibility or plan table.
- [x] T044 [ENTITY-OfferingEligibility] [ENTITY-EligibilityReason] [ENTITY-GroupSummary] [PERSISTENCE-MAPPING] Deliver the read-only discovery EF Core mapping/query adapter contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationDiscoveryModelConfiguration.cs and src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationDiscoveryQueryAdapter.cs after T043 fails; it implements the narrow provider ports, creates no writable eligibility/plan entity, and SPEC-004 remains the sole StudentRegistrationDbContext writer.
- [x] T045 [P] [NFR-1] Produce 300-read/s discovery evidence in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-1.md.
- [x] T046 [P] [NFR-2] Produce search length/parameterization evidence in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-2.md.
- [x] T047 [P] [NFR-3] Produce fixed-input/version determinism evidence in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-3.md.
- [x] T048 [P] [NFR-4] Produce text/icon/non-color status evidence in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-4.md.
- [x] T049 [OS-1] [OS-2] [OS-3] [OS-4] Record verified discovery scope exclusions in docs/release-evidence/SPEC-011-scope-review.md.
- [x] T050 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-011-traceability.md.
- [x] T051 [GATE] Record product, Policy SME, UX, backend, QA, security, accessibility, and operations release approvals in docs/release-evidence/SPEC-011-release-approval.md; record pending SPEC-003 product-wide visual governance honestly without treating it as a blocker to the bounded SPEC-011 feature evidence.

All 51 tasks are complete and backed by the linked automated or recorded evidence.
