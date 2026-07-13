# Tasks: Domain Classes and API Contracts

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [ ] T001 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [ ] T002 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [ ] T003 [GATE] Run cross-spec consistency analysis for SPEC-006; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and planning-only status; record findings and resolutions in specs/006-domain-class-api-contracts/checklists/consistency-analysis.md.
- [ ] T004 [GATE] After T003 passes, freeze the SPEC-006 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/006-domain-class-api-contracts/checklists/implementation-readiness.md.
- [ ] T005 [GATE] After T004 passes, record the accountable owner and Ahmed ELbamby's human approval for SPEC-006 in specs/006-domain-class-api-contracts/checklists/approval.md as the final planning gate; no test, source, migration, or other implementation task may execute before this approval.

## Phase 2 - Models and API Contracts

- [ ] T006 [ENTITY-ApiError] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical ApiError ownership in tests/StudentRegistration.ContractTests/Shared/ApiErrorModelTests.cs.
- [ ] T007 [ENTITY-ApiError] [OWNER-SPEC-006] Record the approved ApiError schema/version at specs/006-domain-class-api-contracts/schemas/api-error.schema.json after T006 fails for the expected reason (depends on T006); canonical source publication remains deferred until FR-2/FR-3 behavior tests fail as expected.
- [ ] T008 [ENTITY-Page] [OWNER-SPEC-006] Create the future failing default-20/maximum-100/sort-echo schema checks for canonical Page ownership in tests/StudentRegistration.ContractTests/Shared/PageModelTests.cs.
- [ ] T009 [ENTITY-Page] [OWNER-SPEC-006] Record the approved Page schema/version at specs/006-domain-class-api-contracts/schemas/page.schema.json after T008 fails for the expected reason (depends on T008); canonical source publication remains deferred until FR-5 behavior tests fail as expected.
- [ ] T010 [FR-10] [ENTITY-AppContext] [TYPE-TermSummaryDto] [OWNER-SPEC-006] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Create the future failing field/state/serialization/owner checks in tests/StudentRegistration.ContractTests/Shared/AppContextModelTests.cs. Test focus: complete AppContextDto and TermSummaryDto fields, contributor ownership, authorized role selection and fail-safe composition.
- [ ] T011 [FR-10] [ENTITY-AppContext] [TYPE-TermSummaryDto] [OWNER-SPEC-006] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Deliver the bounded Composed application context workstream and publish the canonical AppContext contract at src/StudentRegistration.Contracts/AppContextDto.cs with TermSummaryDto at src/StudentRegistration.Contracts/TermSummaryDto.cs only after T010 fails for the expected reason (depends on T010); SPEC-007/SPEC-008 supply values and SPEC-008 owns both context handlers.
- [ ] T012 [ENTITY-CommandResult] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical CommandResult ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/CommandResultModelTests.cs.
- [ ] T013 [ENTITY-CommandResult] [OWNER-SPEC-006] Deliver the canonical CommandResult model or governed artifact at src/StudentRegistration.Contracts/CommandResult.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [ENTITY-DomainValue] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical DomainValue ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/DomainValueModelTests.cs.
- [ ] T015 [ENTITY-DomainValue] [OWNER-SPEC-006] Deliver the canonical DomainValue model or governed artifact at src/StudentRegistration.Contracts/DomainValue.cs after T014 fails for the expected reason (depends on T014).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Safe error (FR-3, NFR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T016 [AC-1] [FR-3] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-1Tests.cs for AC-1: Safe error (FR-3, NFR-3): Given an unexpected database exception When an API request fails Then the response contains generic code, safe message and correlation ID And contains no stack trace, SQL, or connection information.
### US2 - EF isolation (FR-2) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T017 [AC-2] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-2Tests.cs for AC-2: EF isolation (FR-2): Given an EF entity has an internal rowversion and navigation graph When an endpoint returns the resource Then only fields declared by the DTO contract are serialized.
### US3 - Contract verification (NFR-1, NFR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T018 [AC-3] [NFR-1] [NFR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-3Tests.cs for AC-3: Contract verification (NFR-1, NFR-2): Given an approved endpoint contract When CI runs integration and OpenAPI checks Then success and documented error shapes match exactly.
### US4 - Mutation time and concurrency contract (FR-4, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T019 [AC-4] [FR-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-4Tests.cs for request-body expectedRowVersion, 409 STALE_VERSION, TimeProvider, idempotency, and before/after-commit cancellation behavior.
### US5 - Bounded compatible listing (FR-5, FR-6) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T020 [AC-5] [FR-5] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-5Tests.cs for default page 1/size 20, maximum 100, 400 PAGE_SIZE_INVALID without silent capping, stable unique-ID tie-break sorting, and breaking-change governance.
### US6 - Complete idempotency contract (FR-4, FR-8) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T021 [AC-6] [FR-4] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-6Tests.cs for AC-6: Complete idempotency contract (FR-4, FR-8): Given a retryable command contract is reviewed When its OpenAPI and integration cases are inspected Then same-key/same-payload processing and replay are explicit And same-key/different-payload returns 409 IDEMPOTENCY_KEY_REUSED And cancellation before commit versus response loss after commit has distinct documented behavior.
### US7 - Privacy-safe public context (FR-9, NFR-3) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T022 [AC-7] [FR-9] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-7Tests.cs for AC-7: Privacy-safe public context (FR-9, NFR-3): Given an unauthenticated visitor opens AUTH-01 When GET /api/public/context succeeds Then only the FR-9 fields are returned And authenticated context, internal health, capacity, and personal data are absent.
### US8 - Application-service and serialization consistency (FR-1, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [ ] T023 [AC-8] [FR-1] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-8Tests.cs for AC-8: Application-service and serialization consistency (FR-1, NFR-4): Given every approved endpoint contract and representative command/query When architecture and JSON contract tests execute Then endpoints delegate business decisions to focused application services And field naming, UTC dates, timezone identifiers, and invariant decimal formats are identical across responses.
### US9 - Complete authenticated context (FR-10) (P3)

**Goal**: Prove AC-9 with approved SPEC-007 session/role and SPEC-008 academic-context contributor fixtures.

**Independent Test**: Execute only AC-9; passing integration is deferred until both contributor specs are approved and version-pinned.

**Dependencies**: Approval/dependency/model/API baseline through T015 plus approved contributor fixtures.
- [ ] T024 [AC-9] [FR-10] Create the future failing complete-context, authorized-role filtering, and missing-contributor fail-safe coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-9Tests.cs.
- [ ] T025 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-1Tests.cs and assert: Malformed JSON -> 400 VALIDATION_ERROR with no command execution.
- [ ] T026 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-2Tests.cs and assert: Unsupported media type -> 415.
- [ ] T027 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-3Tests.cs and assert: Canceled request before commit -> cancel safely; after commit, idempotent retry returns stored result.
- [ ] T028 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-4Tests.cs and assert: Unauthenticated/unauthorized -> 401/403 with no protected data.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T029 [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Create the future failing FR-1 checks in tests/StudentRegistration.ArchitectureTests/ApplicationServiceBoundaryTests.cs. Test focus: endpoints delegate to module-owned use cases and domain code stays infrastructure independent.
- [ ] T030 [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Deliver the bounded Focused application service boundary workstream at docs/architecture/application-service-boundary.md only after T029 fails for the expected reason (depends on T029); do not create a generic Application project or speculative IUseCase abstraction.
- [ ] T031 [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-2 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Contracts MUST use DTOs/value identifiers and MUST NOT serialize EF entities or password/security internals.
- [ ] T032 [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Publish the DTO/EF/security-internal isolation rules at specs/006-domain-class-api-contracts/contracts/dto-isolation.md only after T031 fails for the expected reason (depends on T031); T007 is the single ApiError contract source writer.
- [ ] T033 [FR-3] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-3 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Errors MUST use stable machine code, safe message, correlation ID, and optional field details.
- [ ] T034 [FR-2] [FR-3] [ENTITY-ApiError] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Deliver the bounded Safe DTO and error contracts workstream and publish the canonical ApiError contract at src/StudentRegistration.Contracts/ApiError.cs only after T031 and T033 fail for their expected reasons (depends on T031, T033).
- [ ] T035 [FR-4] [WORKSTREAM-MUTATION-METADATA] Create the future failing request-body concurrency checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Reject If-Match/412 as outside MVP.
- [ ] T036 [FR-4] [WORKSTREAM-MUTATION-METADATA] Deliver the expected-rowversion shared value contract at src/StudentRegistration.Contracts/ExpectedRowVersion.cs only after T035 fails for the expected reason (depends on T035); feature request DTOs embed it and own their cancellation/idempotency details.
- [ ] T037 [FR-5] [WORKSTREAM-PAGINATION-CONTRACT] Create the future failing page 1/default 20/maximum 100, invalid-value 400 PAGE_SIZE_INVALID, no-silent-cap, applied-sort echo, and unique-ID tie-break checks in tests/StudentRegistration.ContractTests/Shared/PaginationContractTests.cs. Test focus: bounded page size, stable order and total metadata.
- [ ] T038 [FR-5] [ENTITY-Page] [WORKSTREAM-PAGINATION-CONTRACT] Deliver the bounded Pagination contract workstream and publish the canonical Page contract at src/StudentRegistration.Contracts/Page.cs only after T008 and T037 fail for their expected reasons (depends on T008, T037).
- [ ] T039 [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Create the future failing FR-6 checks in tests/StudentRegistration.ContractTests/Shared/ApiVersionPolicyTests.cs. Test focus: breaking-change detection and approved version process. Prove the requirement against its linked AC/EC fixtures: API versioning policy MUST be defined before the first breaking change.
- [ ] T040 [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Deliver FR-6 through the bounded API version governance workstream at docs/API_VERSIONING.md only after T039 fails for the expected reason (depends on T039): API versioning policy MUST be defined before the first breaking change.
- [ ] T041 [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Create the future failing cross-module checks in tests/StudentRegistration.ArchitectureTests/TimeProviderUsageTests.cs. Test focus: all module application/domain time comes from injected TimeProvider. Reject direct browser/system-clock reads.
- [ ] T042 [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Deliver the bounded Authoritative time contract workstream at src/StudentRegistration.Api/Composition/TimeProviderRegistration.cs only after T041 fails for the expected reason (depends on T041); business modules consume the abstraction without a generic Application project.
- [ ] T043 [FR-8] [WORKSTREAM-MUTATION-METADATA] Create the future failing FR-8 checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Prove the requirement against its linked AC/EC fixtures: Idempotency contracts MUST define owner/scope, server-canonical payload, atomic first claim, same-payload processing/replay, different-payload IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- [ ] T044 [FR-4] [FR-8] [WORKSTREAM-MUTATION-METADATA] Deliver the bounded Mutation metadata workstream at src/StudentRegistration.Contracts/CommandMetadata.cs only after T035 and T043 fail for their expected reasons (depends on T035, T043); include ExpectedRowVersion and IdempotencyKey value contracts while each feature owns scope, canonical payload, persistence, and replay rules.
- [ ] T045 [FR-9] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Create the future failing FR-9 checks in tests/StudentRegistration.ContractTests/Shared/PublicContextPrivacyTests.cs. Test focus: only public time, term labels, window and service state; no personal/internal data. Prove the requirement against its linked AC/EC fixtures: Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.
- [ ] T046 [FR-9] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Deliver FR-9 through the bounded Public context DTO workstream at src/StudentRegistration.Contracts/PublicContextDto.cs only after T045 fails for the expected reason (depends on T045): Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.

- [ ] T047 [FR-10] [TYPE-TermSummaryDto] [WORKSTREAM-COMPOSED-APP-CONTEXT] Create the future failing AppContextDto/TermSummaryDto completeness, service/support fields, dual-role selection state, contributor ownership, authorized-role filtering, and missing-contributor fail-safe checks in tests/StudentRegistration.ContractTests/Shared/AppContextContractTests.cs.
- [ ] T048 [FR-10] [WORKSTREAM-COMPOSED-APP-CONTEXT] Publish the complete contributor, authorization filtering, missing-contributor failure, and handler-ownership contract at specs/006-domain-class-api-contracts/contracts/app-context-composition.md only after T047 fails for the expected reason (depends on T047); T011 is the single AppContextDto source writer and SPEC-008 alone writes handlers.
- [ ] T049 [NFR-1] [OPENAPI-BASELINE] Create the future failing deterministic-generation and semantic-drift checks in tests/StudentRegistration.ContractTests/OpenApi/OpenApiBaselineTests.cs.
- [ ] T050 [NFR-1] [OPENAPI-BASELINE] Generate and approve the deterministic baseline at specs/006-domain-class-api-contracts/contracts/openapi/student-registration-v1.json only after T049 fails for the expected missing-baseline reason (depends on T049).
- [ ] T051 [NFR-1] [OPENAPI-CI] Wire the semantic operation/schema/status/security drift gate at .github/scripts/Verify-OpenApi.ps1 only after T049 and T050 establish the expected baseline behavior (depends on T049, T050); formatting/order-only drift is ignored.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T052 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Finalize SPEC-006 data, actions, stable reasons, authorization, and stale/concurrent contribution for SYS-01 at specs/006-domain-class-api-contracts/contracts/routes/SYS-01.md without editing the canonical Razor page.
- [ ] T053 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Verify the SPEC-006 contribution consumed by SYS-01 in tests/StudentRegistration.E2ETests/Specs/Spec006/SystemStatusPageContributorTests.cs.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T054 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable evidence in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-1.md that deterministic OpenAPI generation and semantic operation/schema/status/security comparison reject every unapproved drift while ignoring formatting/order-only differences.
- [ ] T055 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-2.md: Every success/error response in approved feature specs MUST have a contract/integration test.
- [ ] T056 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-3.md: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or unauthorized identifiers.
- [ ] T057 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-4.md: JSON field naming and date/decimal formats MUST be consistent.

## Phase 7 - Scope and Release Evidence

- [ ] T058 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-1 remains excluded: GraphQL, gRPC, and public third-party API.
- [ ] T059 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-2 remains excluded: Generic CRUD endpoints for every entity.
- [ ] T060 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-3 remains excluded: A mediator library unless approved handler volume justifies it.
- [ ] T061 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-4 remains excluded: Breaking-change version until an actual breaking change is proposed.
- [ ] T062 [TRACE] [SC-1] [SC-2] [SC-3] Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-006-traceability.md and reject release if any row lacks passing evidence.
- [ ] T063 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-006 in docs/release-evidence/SPEC-006-release-approval.md.

No task is complete and no implementation file has been created.
