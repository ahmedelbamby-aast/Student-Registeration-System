# Tasks: Eligibility and Subject Discovery

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-011 in specs/011-eligibility-subject-discovery/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-002] Validate the consumed upstream requirements, plan, data model, and API contract at specs/002-aastmt-policy-rulebook/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-009] Validate the consumed upstream requirements, plan, data model, and API contract at specs/009-catalog-prerequisites-policy-admin/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T007 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/011-eligibility-subject-discovery/dependency-baseline.md.
- [ ] T008 [GATE] Freeze SPEC-011 requirements, API, data-model, policy approvals, and dependency versions in specs/011-eligibility-subject-discovery/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T009 [P] [ENTITY-OfferingEligibility] [OWNER-SPEC-011] Create the future failing invariant/schema/serialization checks for canonical OfferingEligibility ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec011/OfferingEligibilityModelTests.cs.
- [ ] T010 [ENTITY-OfferingEligibility] [OWNER-SPEC-011] Deliver the canonical OfferingEligibility model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/OfferingEligibility.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [P] [ENTITY-EligibilityReason] [OWNER-SPEC-011] Create the future failing invariant/schema/serialization checks for canonical EligibilityReason ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EligibilityReasonModelTests.cs.
- [ ] T012 [ENTITY-EligibilityReason] [OWNER-SPEC-011] Deliver the canonical EligibilityReason model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/EligibilityReason.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [P] [ENTITY-GroupSummary] [OWNER-SPEC-011] Create the future failing invariant/schema/serialization checks for canonical GroupSummary ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec011/GroupSummaryModelTests.cs.
- [ ] T014 [ENTITY-GroupSummary] [OWNER-SPEC-011] Deliver the canonical GroupSummary model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/GroupSummary.cs after T013 fails for the expected reason (depends on T013).
- [ ] T015 [P] [ENTITY-PolicyVersion] [OWNER-SPEC-011] Create the future failing invariant/schema/serialization checks for canonical PolicyVersion ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec011/PolicyVersionModelTests.cs.
- [ ] T016 [ENTITY-PolicyVersion] [OWNER-SPEC-011] Deliver the canonical PolicyVersion model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/PolicyVersion.cs after T015 fails for the expected reason (depends on T015).
- [ ] T017 [API-Endpoint01] [OWNER-SPEC-011] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/student/terms/{termId}/offerings in specs/011-eligibility-subject-discovery/contracts/api.md.
- [ ] T018 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/student/terms/{termId}/offerings in tests/StudentRegistration.ContractTests/Specs/Spec011/Endpoint01ContractTests.cs.
- [ ] T019 [API-Endpoint01] [OWNER-SPEC-011] Deliver the sole canonical GET /api/student/terms/{termId}/offerings handler at src/StudentRegistration.Server/Modules/Registration/Endpoints/Spec011Endpoints.cs after T018 fails for the expected reason (depends on T018).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Eligible offering (FR-1, FR-2, FR-5) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Eligibility and Subject Discovery.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T019.
- [ ] T020 [P] [AC-1] [FR-1] [FR-2] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-1Tests.cs for AC-1: Eligible offering (FR-1, FR-2, FR-5): Given a student meets prerequisites/load rules and one group is open When discovery loads Then the offering is listed with credits and all group staff/location/time details.
### US2 - Explain unavailable (FR-4, FR-6) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Eligibility and Subject Discovery.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T019.
- [ ] T021 [P] [AC-2] [FR-4] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-2Tests.cs for AC-2: Explain unavailable (FR-4, FR-6): Given a course requires 96 earned credits and the student has 95 When unavailable offerings are viewed Then the course is shown with the earned-credit reason, required/current values, policy version, and source explanation.
### US3 - Full groups (FR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Eligibility and Subject Discovery.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T019.
- [ ] T022 [P] [AC-3] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-3Tests.cs for AC-3: Full groups (FR-2): Given every published group for an otherwise eligible offering is full When default available discovery loads Then the offering is not selectable And its unavailable view states that no group currently has a seat.
### US4 - Bounded server-authoritative search (FR-3, FR-7, FR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Eligibility and Subject Discovery.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T019.
- [ ] T023 [P] [AC-4] [FR-3] [FR-7] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-4Tests.cs for AC-4: Bounded server-authoritative search (FR-3, FR-7, FR-8): Given a student requests title/day filters with an oversized page When discovery is evaluated Then the server applies eligibility before returning bounded, stably sorted results And client manipulation cannot make an ineligible offering selectable.
### US5 - Discovery quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Eligibility and Subject Discovery.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T019.
- [ ] T024 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec011/AC-5Tests.cs for AC-5: Discovery quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given the approved 300-read-per-second fixture, fixed input/version, malicious search strings, and color-vision/accessibility checks When discovery quality tests execute Then response time is at most 300 ms p95 And search is length-bounded and parameterized And eligibility is deterministic And every status has text/icon meaning independent of color.
- [ ] T025 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-1Tests.cs and assert: Policy/profile data unavailable -> safe unavailable result and support reference, never accidental eligibility.
- [ ] T026 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-2Tests.cs and assert: Group becomes full after results load -> details/submit revalidate.
- [ ] T027 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-3Tests.cs and assert: Search contains SQL metacharacters -> treated as literal parameterized text.
- [ ] T028 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec011/EdgeCases/EC-4Tests.cs and assert: No eligible offerings -> show the evaluated policy version and reason codes, reset-filter action, current window state, and the configured Registrar/advisor support path.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T029 [P] [FR-1] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the future failing FR-1 checks in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs. Test focus: all approved rules, eligible/default/unavailable reasons, policy version and no client override. Prove the requirement against its linked AC/EC fixtures: The system MUST evaluate every relevant approved rule on the server.
- [ ] T030 [FR-1] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver FR-1 through the bounded Server-authoritative eligibility workstream at src/StudentRegistration.Server/Modules/Registration/EligibilityService.cs only after T029 fails for the expected reason (depends on T029): The system MUST evaluate every relevant approved rule on the server.
- [ ] T031 [P] [FR-2] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the future failing FR-2 checks in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs. Test focus: all approved rules, eligible/default/unavailable reasons, policy version and no client override. Prove the requirement against its linked AC/EC fixtures: Default discovery MUST list eligible offerings having at least one published selectable group.
- [ ] T032 [FR-2] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver FR-2 through the bounded Server-authoritative eligibility workstream at src/StudentRegistration.Server/Modules/Registration/EligibilityService.cs only after T031 fails for the expected reason (depends on T031): Default discovery MUST list eligible offerings having at least one published selectable group.
- [ ] T033 [P] [FR-3] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Registration/OfferingSearchTests.cs. Test focus: code/title filters, parameterization, stable sort and bounded pagination. Prove the requirement against its linked AC/EC fixtures: Students MUST be able to search by code/title and filter by eligibility, credits, day, and availability.
- [ ] T034 [FR-3] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Deliver FR-3 through the bounded Bounded offering search workstream at src/StudentRegistration.Server/Modules/Registration/OfferingSearchQuery.cs only after T033 fails for the expected reason (depends on T033): Students MUST be able to search by code/title and filter by eligibility, credits, day, and availability.
- [ ] T035 [P] [FR-4] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the future failing FR-4 checks in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs. Test focus: all approved rules, eligible/default/unavailable reasons, policy version and no client override. Prove the requirement against its linked AC/EC fixtures: Students MUST be able to inspect unavailable offerings and every blocking reason.
- [ ] T036 [FR-4] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver FR-4 through the bounded Server-authoritative eligibility workstream at src/StudentRegistration.Server/Modules/Registration/EligibilityService.cs only after T035 fails for the expected reason (depends on T035): Students MUST be able to inspect unavailable offerings and every blocking reason.
- [ ] T037 [P] [FR-5] [WORKSTREAM-DISCOVERY-PROJECTION] Create the future failing FR-5 checks in tests/StudentRegistration.ContractTests/Registration/GroupSummaryProjectionTests.cs. Test focus: credits, capacity, staff, room, day/time and state fields. Prove the requirement against its linked AC/EC fixtures: Results MUST show course code/title/credits and group capacity, staff, location, day and time.
- [ ] T038 [FR-5] [WORKSTREAM-DISCOVERY-PROJECTION] Deliver FR-5 through the bounded Discovery projection workstream at src/StudentRegistration.Server/Modules/Registration/GroupSummaryProjection.cs only after T037 fails for the expected reason (depends on T037): Results MUST show course code/title/credits and group capacity, staff, location, day and time.
- [ ] T039 [P] [FR-6] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the future failing FR-6 checks in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs. Test focus: all approved rules, eligible/default/unavailable reasons, policy version and no client override. Prove the requirement against its linked AC/EC fixtures: Each decision MUST include policy version and stable reasons.
- [ ] T040 [FR-6] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver FR-6 through the bounded Server-authoritative eligibility workstream at src/StudentRegistration.Server/Modules/Registration/EligibilityService.cs only after T039 fails for the expected reason (depends on T039): Each decision MUST include policy version and stable reasons.
- [ ] T041 [P] [FR-7] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Create the future failing FR-7 checks in tests/StudentRegistration.ApplicationTests/Registration/EligibilityDecisionTests.cs. Test focus: all approved rules, eligible/default/unavailable reasons, policy version and no client override. Prove the requirement against its linked AC/EC fixtures: Client filtering MUST NOT substitute for server eligibility.
- [ ] T042 [FR-7] [WORKSTREAM-SERVER-AUTHORITATIVE-ELIGIBILITY] Deliver FR-7 through the bounded Server-authoritative eligibility workstream at src/StudentRegistration.Server/Modules/Registration/EligibilityService.cs only after T041 fails for the expected reason (depends on T041): Client filtering MUST NOT substitute for server eligibility.
- [ ] T043 [P] [FR-8] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Registration/OfferingSearchTests.cs. Test focus: code/title filters, parameterization, stable sort and bounded pagination. Prove the requirement against its linked AC/EC fixtures: Stable sorting and bounded pagination MUST be supported.
- [ ] T044 [FR-8] [WORKSTREAM-BOUNDED-OFFERING-SEARCH] Deliver FR-8 through the bounded Bounded offering search workstream at src/StudentRegistration.Server/Modules/Registration/OfferingSearchQuery.cs only after T043 fails for the expected reason (depends on T043): Stable sorting and bounded pagination MUST be supported.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T045 [P] [STU-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-02 in tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDiscoveryPageFeatureTests.cs.
- [ ] T046 [STU-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Deliver the sole canonical Blazor implementation for STU-02 at src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor after T045 and the SPEC-003 contract/component checks fail for expected reasons (depends on T045).
- [ ] T047 [P] [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-6] [AC-1] [AC-2] [AC-3] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-03 in tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDetailsPageFeatureTests.cs.
- [ ] T048 [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-6] [AC-1] [AC-2] [AC-3] Deliver the sole canonical Blazor implementation for STU-03 at src/StudentRegistration.Client/Pages/SubjectDetailsPage.razor after T047 and the SPEC-003 contract/component checks fail for expected reasons (depends on T047).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T049 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-1.md: Discovery SHOULD respond within 300 ms p95 at 300 read requests/s.
- [ ] T050 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-2.md: Search input MUST be parameterized and limited in length.
- [ ] T051 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-3.md: Eligibility MUST be deterministic for a fixed input/version.
- [ ] T052 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec011/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-011-NFR-4.md: Eligibility/status MUST not rely on color alone.

## Phase 7 - Scope and Release Evidence

- [ ] T053 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-011-scope-review.md that OS-1 remains excluded: Recommendations before a student selects courses.
- [ ] T054 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-011-scope-review.md that OS-2 remains excluded: Search across other colleges/terms unless approved.
- [ ] T055 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-011-scope-review.md that OS-3 remains excluded: Client-authoritative eligibility.
- [ ] T056 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-011-scope-review.md that OS-4 remains excluded: Advisor approval workflow.
- [ ] T057 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-011-traceability.md and reject release if any row lacks passing evidence.
- [ ] T058 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-011 in docs/release-evidence/SPEC-011-release-approval.md.

No task is complete and no implementation file has been created.
