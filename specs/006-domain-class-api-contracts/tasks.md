# Tasks: Domain Classes and API Contracts

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-006 in specs/006-domain-class-api-contracts/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [ ] T004 [GATE] Freeze SPEC-006 requirements, API, data-model, policy approvals, and dependency versions in specs/006-domain-class-api-contracts/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T005 [P] [ENTITY-ApiError] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical ApiError ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/ApiErrorModelTests.cs.
- [ ] T006 [ENTITY-ApiError] [OWNER-SPEC-006] Deliver the canonical ApiError model or governed artifact at src/StudentRegistration.Contracts/ApiError.cs after T005 fails for the expected reason (depends on T005).
- [ ] T007 [P] [ENTITY-Page] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical Page ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/PageModelTests.cs.
- [ ] T008 [ENTITY-Page] [OWNER-SPEC-006] Deliver the canonical Page model or governed artifact at src/StudentRegistration.Contracts/Page.cs after T007 fails for the expected reason (depends on T007).
- [ ] T009 [P] [ENTITY-AppContext] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical AppContext ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/AppContextModelTests.cs.
- [ ] T010 [ENTITY-AppContext] [OWNER-SPEC-006] Deliver the canonical AppContext model or governed artifact at src/StudentRegistration.Contracts/AppContext.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [P] [ENTITY-CommandResult] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical CommandResult ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/CommandResultModelTests.cs.
- [ ] T012 [ENTITY-CommandResult] [OWNER-SPEC-006] Deliver the canonical CommandResult model or governed artifact at src/StudentRegistration.Contracts/CommandResult.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [P] [ENTITY-DomainValue] [OWNER-SPEC-006] Create the future failing invariant/schema/serialization checks for canonical DomainValue ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec006/DomainValueModelTests.cs.
- [ ] T014 [ENTITY-DomainValue] [OWNER-SPEC-006] Deliver the canonical DomainValue model or governed artifact at src/StudentRegistration.Contracts/DomainValue.cs after T013 fails for the expected reason (depends on T013).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Safe error (FR-3, NFR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T015 [P] [AC-1] [FR-3] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-1Tests.cs for AC-1: Safe error (FR-3, NFR-3): Given an unexpected database exception When an API request fails Then the response contains generic code, safe message and correlation ID And contains no stack trace, SQL, or connection information.
### US2 - EF isolation (FR-2) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T016 [P] [AC-2] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-2Tests.cs for AC-2: EF isolation (FR-2): Given an EF entity has an internal rowversion and navigation graph When an endpoint returns the resource Then only fields declared by the DTO contract are serialized.
### US3 - Contract verification (NFR-1, NFR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T017 [P] [AC-3] [NFR-1] [NFR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-3Tests.cs for AC-3: Contract verification (NFR-1, NFR-2): Given an approved endpoint contract When CI runs integration and OpenAPI checks Then success and documented error shapes match exactly.
### US4 - Mutation time and concurrency contract (FR-4, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T018 [P] [AC-4] [FR-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-4Tests.cs for AC-4: Mutation time and concurrency contract (FR-4, FR-7): Given a time-dependent mutation with an idempotency/concurrency token When the request is canceled or retried Then the application service uses TimeProvider and the declared token behavior And produces no duplicate committed effect.
### US5 - Bounded compatible listing (FR-5, FR-6) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T019 [P] [AC-5] [FR-5] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-5Tests.cs for AC-5: Bounded compatible listing (FR-5, FR-6): Given a client requests an oversized page from an existing API version When the list endpoint validates the request Then it caps/rejects the size according to contract And a breaking shape change requires the approved versioning process.
### US6 - Complete idempotency contract (FR-4, FR-8) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T020 [P] [AC-6] [FR-4] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-6Tests.cs for AC-6: Complete idempotency contract (FR-4, FR-8): Given a retryable command contract is reviewed When its OpenAPI and integration cases are inspected Then same-key/same-payload processing and replay are explicit And same-key/different-payload returns 409 IDEMPOTENCY_KEY_REUSED And cancellation before commit versus response loss after commit has distinct documented behavior.
### US7 - Privacy-safe public context (FR-9, NFR-3) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T021 [P] [AC-7] [FR-9] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-7Tests.cs for AC-7: Privacy-safe public context (FR-9, NFR-3): Given an unauthenticated visitor opens AUTH-01 When GET /api/public/context succeeds Then only the FR-9 fields are returned And authenticated context, internal health, capacity, and personal data are absent.
### US8 - Application-service and serialization consistency (FR-1, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T014.
- [ ] T022 [P] [AC-8] [FR-1] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-8Tests.cs for AC-8: Application-service and serialization consistency (FR-1, NFR-4): Given every approved endpoint contract and representative command/query When architecture and JSON contract tests execute Then endpoints delegate business decisions to focused application services And field naming, UTC dates, timezone identifiers, and invariant decimal formats are identical across responses.
- [ ] T023 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-1Tests.cs and assert: Malformed JSON -> 400 VALIDATION_ERROR with no command execution.
- [ ] T024 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-2Tests.cs and assert: Unsupported media type -> 415.
- [ ] T025 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-3Tests.cs and assert: Canceled request before commit -> cancel safely; after commit, idempotent retry returns stored result.
- [ ] T026 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-4Tests.cs and assert: Unauthenticated/unauthorized -> 401/403 with no protected data.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T027 [P] [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Create the future failing FR-1 checks in tests/StudentRegistration.ArchitectureTests/ApplicationServiceBoundaryTests.cs. Test focus: endpoints delegate decisions and domain code stays infrastructure independent. Prove the requirement against its linked AC/EC fixtures: Endpoints MUST delegate business decisions to focused application services.
- [ ] T028 [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Deliver FR-1 through the bounded Focused application service boundary workstream at src/StudentRegistration.Application/Abstractions/IUseCase.cs only after T027 fails for the expected reason (depends on T027): Endpoints MUST delegate business decisions to focused application services.
- [ ] T029 [P] [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-2 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Contracts MUST use DTOs/value identifiers and MUST NOT serialize EF entities or password/security internals.
- [ ] T030 [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Deliver FR-2 through the bounded Safe DTO and error contracts workstream at src/StudentRegistration.Contracts/ApiError.cs only after T029 fails for the expected reason (depends on T029): Contracts MUST use DTOs/value identifiers and MUST NOT serialize EF entities or password/security internals.
- [ ] T031 [P] [FR-3] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-3 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Errors MUST use stable machine code, safe message, correlation ID, and optional field details.
- [ ] T032 [FR-3] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Deliver FR-3 through the bounded Safe DTO and error contracts workstream at src/StudentRegistration.Contracts/ApiError.cs only after T031 fails for the expected reason (depends on T031): Errors MUST use stable machine code, safe message, correlation ID, and optional field details.
- [ ] T033 [P] [FR-4] [WORKSTREAM-MUTATION-METADATA] Create the future failing FR-4 checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Prove the requirement against its linked AC/EC fixtures: Update/delete endpoints MUST require an expected rowversion or If-Match value; retryable create/confirm/submit commands MUST require an idempotency key; cancellation behavior before and after commit MUST be documented per endpoint.
- [ ] T034 [FR-4] [WORKSTREAM-MUTATION-METADATA] Deliver FR-4 through the bounded Mutation metadata workstream at src/StudentRegistration.Contracts/CommandMetadata.cs only after T033 fails for the expected reason (depends on T033): Update/delete endpoints MUST require an expected rowversion or If-Match value; retryable create/confirm/submit commands MUST require an idempotency key; cancellation behavior before and after commit MUST be documented per endpoint.
- [ ] T035 [P] [FR-5] [WORKSTREAM-PAGINATION-CONTRACT] Create the future failing FR-5 checks in tests/StudentRegistration.ContractTests/Shared/PaginationContractTests.cs. Test focus: bounded page size, stable order and total metadata. Prove the requirement against its linked AC/EC fixtures: Listing endpoints MUST use bounded pagination.
- [ ] T036 [FR-5] [WORKSTREAM-PAGINATION-CONTRACT] Deliver FR-5 through the bounded Pagination contract workstream at src/StudentRegistration.Contracts/Page.cs only after T035 fails for the expected reason (depends on T035): Listing endpoints MUST use bounded pagination.
- [ ] T037 [P] [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Create the future failing FR-6 checks in tests/StudentRegistration.ContractTests/Shared/ApiVersionPolicyTests.cs. Test focus: breaking-change detection and approved version process. Prove the requirement against its linked AC/EC fixtures: API versioning policy MUST be defined before the first breaking change.
- [ ] T038 [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Deliver FR-6 through the bounded API version governance workstream at docs/API_VERSIONING.md only after T037 fails for the expected reason (depends on T037): API versioning policy MUST be defined before the first breaking change.
- [ ] T039 [P] [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Create the future failing FR-7 checks in tests/StudentRegistration.ApplicationTests/Time/TimeProviderUsageTests.cs. Test focus: domain/application time comes from injected TimeProvider. Prove the requirement against its linked AC/EC fixtures: Domain code MUST use TimeProvider abstraction for current time.
- [ ] T040 [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Deliver FR-7 through the bounded Authoritative time contract workstream at src/StudentRegistration.Application/Time/TimeProviderRegistration.cs only after T039 fails for the expected reason (depends on T039): Domain code MUST use TimeProvider abstraction for current time.
- [ ] T041 [P] [FR-8] [WORKSTREAM-MUTATION-METADATA] Create the future failing FR-8 checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Prove the requirement against its linked AC/EC fixtures: Idempotency contracts MUST define owner/scope, server-canonical payload, atomic first claim, same-payload processing/replay, different-payload IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- [ ] T042 [FR-8] [WORKSTREAM-MUTATION-METADATA] Deliver FR-8 through the bounded Mutation metadata workstream at src/StudentRegistration.Contracts/CommandMetadata.cs only after T041 fails for the expected reason (depends on T041): Idempotency contracts MUST define owner/scope, server-canonical payload, atomic first claim, same-payload processing/replay, different-payload IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- [ ] T043 [P] [FR-9] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Create the future failing FR-9 checks in tests/StudentRegistration.ContractTests/Shared/PublicContextPrivacyTests.cs. Test focus: only public time, term labels, window and service state; no personal/internal data. Prove the requirement against its linked AC/EC fixtures: Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.
- [ ] T044 [FR-9] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Deliver FR-9 through the bounded Public context DTO workstream at src/StudentRegistration.Contracts/PublicContextDto.cs only after T043 fails for the expected reason (depends on T043): Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T045 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Finalize SPEC-006 data, actions, stable reasons, authorization, and stale/concurrent contribution for SYS-01 at specs/006-domain-class-api-contracts/contracts/routes/SYS-01.md without editing the canonical Razor page.
- [ ] T046 [P] [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Verify the SPEC-006 contribution consumed by SYS-01 in tests/StudentRegistration.E2ETests/Specs/Spec006/SystemStatusPageContributorTests.cs.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T047 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-1.md: OpenAPI output MUST match implementation in CI.
- [ ] T048 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-2.md: Every success/error response in approved feature specs MUST have a contract/integration test.
- [ ] T049 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-3.md: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or unauthorized identifiers.
- [ ] T050 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-4.md: JSON field naming and date/decimal formats MUST be consistent.

## Phase 7 - Scope and Release Evidence

- [ ] T051 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-1 remains excluded: GraphQL, gRPC, and public third-party API.
- [ ] T052 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-2 remains excluded: Generic CRUD endpoints for every entity.
- [ ] T053 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-3 remains excluded: A mediator library unless approved handler volume justifies it.
- [ ] T054 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-4 remains excluded: Breaking-change version until an actual breaking change is proposed.
- [ ] T055 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-006-traceability.md and reject release if any row lacks passing evidence.
- [ ] T056 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-006 in docs/release-evidence/SPEC-006-release-approval.md.

No task is complete and no implementation file has been created.
