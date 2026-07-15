# Tasks: Domain Classes and API Contracts

**Status**: T001-T024, T029-T048, and T052-T053 are complete and verified on 2026-07-14 (46 of 63). Runtime, OpenAPI, measurable NFR, and production-release gates remain pending as listed below.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Checked tasks have verified artifacts; unchecked tasks remain pending or dependency-gated. Every task names an exact file and traces to a requirement, criterion, edge case, route, contract type, endpoint, dependency, or gate. `[ENTITY-*]` labels are canonical governance/ownership identifiers required by the manifests; for SPEC-006 they identify non-persisted transport/value contracts, not SQL entities or aggregates.

**Execution boundaries**:
- `[DEFERRED-DOWNSTREAM]` acceptance fixtures may be created only as compiled, explicitly skipped tests whose reason names the missing owner/spec and activation condition; a skip is never passing acceptance or release evidence.
- `[RUNTIME-GATE]` and `[OPENAPI-RUNTIME-GATE]` tasks remain unchecked until their real application host, endpoint handlers, contributors, and version pins exist. An empty host, mock-only endpoint surface, or hand-authored OpenAPI document cannot satisfy them.
- `[PRODUCTION-RELEASE-GATE]` tasks remain unchecked during Gate A work and execute only after the applicable downstream runtime is complete and frozen for review.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [x] T001 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [x] T002 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/006-domain-class-api-contracts/dependency-baseline.md.
- [x] T003 [GATE] Run cross-spec consistency analysis for SPEC-006; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and approved Gate A demo-implementation status; record findings and resolutions in specs/006-domain-class-api-contracts/checklists/consistency-analysis.md.
- [x] T004 [GATE] After T003 passes, freeze the SPEC-006 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/006-domain-class-api-contracts/checklists/implementation-readiness.md.
- [x] T005 [GATE] After T004 passes, verify the accountable owner and Ahmed ELbamby's 2026-07-13 Gate A human approval for SPEC-006 in specs/006-domain-class-api-contracts/checklists/approval.md from the Technical Lead review perspective as the final planning gate; Ahmed is the sole human approver, and this record does not pre-approve the separate production-release review perspectives in T063. No test, source, migration, or other implementation task may execute without this approval record.

## Phase 2 - Models and API Contracts

- [x] T006 [ENTITY-ApiError] [TYPE-ApiError] [SCHEMA-TEST] [OWNER-SPEC-006] Create the future failing JSON Schema ownership/version/required-field/optional-field checks for the canonical ApiError schema artifact in tests/StudentRegistration.ContractTests/Shared/ApiErrorSchemaTests.cs; this task tests the schema artifact only and does not define or publish the C# type. The 2026-07-14 SPEC-008 dependency revalidation first proves at most 20 field keys, 5 messages per key, and 256 characters per message.
- [x] T007 [ENTITY-ApiError] [TYPE-ApiError] [SCHEMA-WRITER] [OWNER-SPEC-006] Record the approved ApiError JSON Schema/version at specs/006-domain-class-api-contracts/schemas/api-error.schema.json after T006 fails for the expected missing-schema reason (depends on T006); T007 is the sole JSON Schema writer, while T013 is the sole C# contract writer, including the test-first bounded field-error amendment revalidated on 2026-07-14.
- [x] T008 [ENTITY-Page] [TYPE-Page] [SCHEMA-TEST] [OWNER-SPEC-006] Create the future failing default-20/maximum-100/sort-echo JSON Schema checks for canonical Page ownership in tests/StudentRegistration.ContractTests/Shared/PageModelTests.cs.
- [x] T009 [ENTITY-Page] [TYPE-Page] [SCHEMA-WRITER] [OWNER-SPEC-006] Record the approved Page JSON Schema/version at specs/006-domain-class-api-contracts/schemas/page.schema.json after T008 fails for the expected reason (depends on T008); canonical C# source publication remains T038-owned and deferred until FR-5 behavior tests fail as expected.
- [x] T010 [FR-10] [ENTITY-AppContext] [ENTITY-TermSummaryDto] [ENTITY-RegistrationWindowSummaryDto] [TYPE-AppContextDto] [TYPE-TermSummaryDto] [TYPE-RegistrationWindowSummaryDto] [OWNER-SPEC-006] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Create the future failing field/state/serialization/owner checks in tests/StudentRegistration.ContractTests/Shared/AppContextModelTests.cs. Test focus: TermSummaryDto and RegistrationWindowSummaryDto are separate shared types; teachingTerm and registrationTerm are distinct and independently nullable when the authoritative contributor reports no applicable term; a non-none registrationWindowState requires one matched registrationWindow with the same computed state and a non-null registration term, while none requires a null window; the matched-window summary contains only ID, computed state, UTC interval, and row version and never expands the six-field public DTO; activeRole is non-null for active/expiring sessions and null only for a multi-authorized-role `role-selection-required` session; SPEC-007 supplies identity/session/authorized-role values, while SPEC-008 supplies authoritative time/term/window values and owns both handlers.
- [x] T011 [FR-10] [ENTITY-AppContext] [ENTITY-TermSummaryDto] [ENTITY-RegistrationWindowSummaryDto] [TYPE-AppContextDto] [TYPE-TermSummaryDto] [TYPE-RegistrationWindowSummaryDto] [OWNER-SPEC-006] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Deliver the bounded composed application-context schema and publish the canonical AppContext contract at src/StudentRegistration.Contracts/AppContextDto.cs with the canonical separate TermSummaryDto at src/StudentRegistration.Contracts/TermSummaryDto.cs and RegistrationWindowSummaryDto at src/StudentRegistration.Contracts/RegistrationWindowSummaryDto.cs only after T010 fails for the expected reason (depends on T010); T011 is the sole shared source writer for all three types, preserves the contributor split, authoritative-null and matched-window consistency/privacy semantics from T010, publishes no handler, and does not synthesize missing contributor values.
- [x] T012 [FR-3] [ENTITY-ApiError] [TYPE-ApiError] [SOURCE-TEST] [OWNER-SPEC-006] Create the future failing C# shape, constructor/invariant, nullability, and web-serialization checks for canonical ApiError ownership in tests/StudentRegistration.ContractTests/Shared/ApiErrorModelTests.cs; require code, safe message, and correlationId, with only fieldErrors and currentVersion optional. The 2026-07-14 SPEC-008 dependency revalidation first proves at most 20 field keys, 5 messages per key, and 256 characters per message.
- [x] T013 [FR-3] [ENTITY-ApiError] [TYPE-ApiError] [SOURCE-WRITER] [OWNER-SPEC-006] Publish the canonical C# ApiError contract at src/StudentRegistration.Contracts/ApiError.cs after T012 fails for the expected missing-type reason (depends on T012); T013 owns only the transport type, including the test-first bounded field-error amendment revalidated on 2026-07-14, while T034 owns runtime exception-to-response handling.
- [x] T014 [NFR-4] [API-JSONPOLICY] [JSON-POLICY-TEST] [OWNER-SPEC-006] Create the future failing shared HTTP JSON-policy checks in tests/StudentRegistration.ContractTests/Shared/JsonContractPolicyTests.cs for web camelCase names, documented enum strings, ISO-8601 UTC timestamps, invariant decimal numbers, stable timezone identifiers, and the contract-specific nullable behavior defined by T010 and T012.
- [x] T015 [NFR-4] [API-JSONPOLICY] [JSON-POLICY-WRITER] [OWNER-SPEC-006] Deliver the single API-composition JSON policy at src/StudentRegistration.Api/Composition/JsonContractRegistration.cs after T014 fails for the expected missing-registration reason (depends on T014); configure the existing ASP.NET Core serializer once without adding a generic serialization framework or changing feature-owned DTO shapes.

## Phase 3 - User-Story Acceptance and Edge Tests

T016-T024 are contract-first placeholders, not current runtime proof: create each as a compiled, explicitly skipped fixture with its missing downstream owner and activation condition in the skip reason. T025-T028 are real boundary tests and remain unchecked until the stated runtime exists; source inspection, mocks that bypass ASP.NET Core, and skipped tests cannot satisfy them.

### US1 - Safe error (FR-3, NFR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T016 [AC-1] [FR-3] [NFR-3] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-1Tests.cs for an unexpected database exception producing generic code, safe message, and correlation ID with no stack trace, SQL, connection, or secret data; activate and pass it only after T034 is registered in the real API host and a version-pinned downstream EF-backed endpoint exists.
### US2 - EF isolation (FR-2) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T017 [AC-2] [FR-2] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-2Tests.cs proving an EF entity's internal rowversion/navigation graph never crosses a real endpoint DTO boundary; activate and pass it only after an approved, version-pinned downstream EF-backed endpoint and its feature-owned DTO exist.
### US3 - Contract verification (NFR-1, NFR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T018 [AC-3] [NFR-1] [NFR-2] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-3Tests.cs proving approved endpoint success/error shapes match integration responses and generated OpenAPI; activate and pass it only after real version-pinned downstream handlers exist and the T049-T051 OpenAPI runtime gate is complete.
### US4 - Mutation time and concurrency contract (FR-4, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T019 [AC-4] [FR-4] [FR-7] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-4Tests.cs for request-body expectedRowVersion, authorized 409 STALE_VERSION without unauthorized disclosure, injected TimeProvider, idempotency, and before/after-commit cancellation behavior; activate and pass it only against an approved versioned downstream mutation with real SQL commit behavior.
### US5 - Bounded compatible listing (FR-5, FR-6) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T020 [AC-5] [FR-5] [FR-6] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-5Tests.cs for default page 1/size 20, maximum 100, 400 PAGE_SIZE_INVALID without silent capping, canonical sort echo with a stable unique-ID tie-breaker, and breaking-change governance; activate and pass it only against an approved version-pinned downstream listing endpoint.
### US6 - Complete idempotency contract (FR-4, FR-8) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T021 [AC-6] [FR-4] [FR-8] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-6Tests.cs for atomic first claim, same-key/same-payload processing and replay, 409 IDEMPOTENCY_KEY_REUSED for a different canonical payload, replayable final-result policy, cancellation before commit, and response loss after commit; activate and pass it only against an approved retryable downstream command with real persistence and generated OpenAPI.
### US7 - Privacy-safe public context (FR-9, NFR-3) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T022 [AC-7] [FR-9] [NFR-3] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-7Tests.cs proving unauthenticated GET /api/public/context returns only FR-9 fields and excludes authenticated context, internal health, capacity, and personal data; activate and pass it only after SPEC-008's handler and contributor runtime are approved, implemented, and version-pinned.
### US8 - Application-service and serialization consistency (FR-1, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Domain Classes and API Contracts.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T015.
- [x] T023 [AC-8] [FR-1] [NFR-4] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped future Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-8Tests.cs proving representative real endpoints delegate business decisions to focused module services and use the T015 JSON policy consistently; activate and pass it only after representative approved command/query handlers from downstream feature specs are implemented and version-pinned.
### US9 - Complete authenticated context (FR-10) (P3)

**Goal**: Prove AC-9 with approved SPEC-007 session/role and SPEC-008 academic-context contributor fixtures.

**Independent Test**: Execute only AC-9; passing integration is deferred until both contributor specs are approved and version-pinned.

**Dependencies**: Approval/dependency/model/API baseline through T015 plus approved contributor fixtures.
- [x] T024 [AC-9] [FR-10] [DEFERRED-DOWNSTREAM] Create compiled, explicitly skipped complete-context, authorized-role filtering, activeRole null-state, independently nullable teaching/registration term, and missing-contributor fail-safe coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec006/AC-9Tests.cs; activate and pass it only after SPEC-007 and SPEC-008 contributor contracts, fixtures, and SPEC-008's real handler are approved, implemented, and version-pinned.
- [ ] T025 [EC-1] [RUNTIME-GATE] After a real request-body command endpoint is implemented, exercise malformed JSON through ASP.NET Core in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-1Tests.cs and assert 400 VALIDATION_ERROR with no command construction, application-service call, or durable effect; keep this task unchecked until that endpoint exists.
- [ ] T026 [EC-2] [RUNTIME-GATE] After a real body-consuming endpoint is implemented, exercise an unsupported media type through ASP.NET Core in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-2Tests.cs and assert 415 before command execution; keep this task unchecked until that endpoint exists.
- [ ] T027 [EC-3] [RUNTIME-GATE] After a real idempotent SQL-backed mutation is implemented, inject cancellation before and response loss after commit in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-3Tests.cs and assert safe pre-commit cancellation plus stored-result replay without a duplicate effect after commit; keep this task unchecked until that runtime exists.
- [ ] T028 [EC-4] [RUNTIME-GATE] After a real protected endpoint and SPEC-007 authorization pipeline are implemented, exercise unauthenticated and unauthorized requests in tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-4Tests.cs and assert 401/403 with no protected resource, identifier, rowversion, or currentVersion disclosure; keep this task unchecked until that runtime exists.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T029 [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Create the future failing bounded composition-boundary/design-guard checks in tests/StudentRegistration.ArchitectureTests/ApplicationServiceBoundaryTests.cs. Test focus: the endpoint contract requires delegation to module-owned use cases, domain code stays infrastructure independent, and generic framework layers remain prohibited; real endpoint delegation remains deferred to T023 and its downstream activation condition.
- [x] T030 [FR-1] [WORKSTREAM-FOCUSED-APPLICATION-SERVICE-BOUNDARY] Deliver the bounded Focused application service boundary workstream at docs/architecture/application-service-boundary.md only after T029 fails for the expected reason (depends on T029); do not create a generic Application project or speculative IUseCase abstraction.
- [x] T031 [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-2 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Contracts MUST use DTOs/value identifiers, MUST NOT serialize EF entities or persistence/security internals, and MUST limit transient secret-bearing fields to explicitly approved authentication and account-lifecycle request DTOs.
- [x] T032 [FR-2] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Publish the DTO/EF/security-internal isolation rules at specs/006-domain-class-api-contracts/contracts/dto-isolation.md only after T031 fails for the expected reason (depends on T031); T007 is the sole design-time JSON Schema writer and T013 is the sole C# ApiError source writer.
- [x] T033 [FR-3] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Create the future failing FR-3 checks in tests/StudentRegistration.ContractTests/Shared/ApiErrorAndDtoTests.cs. Test focus: declared DTO fields, stable errors, correlation and no EF/security leakage. Prove the requirement against its linked AC/EC fixtures: Errors MUST use stable machine code, safe message, correlation ID, and optional field details.
- [x] T034 [FR-2] [FR-3] [WORKSTREAM-SAFE-DTO-AND-ERROR-CONTRACTS] Deliver the bounded Safe DTO and error contracts workstream at src/StudentRegistration.Api/Composition/ApiErrorHandlingExtensions.cs only after T031 and T033 fail for their expected reasons (depends on T031, T033); map unexpected exceptions to a generic ApiError with a correlation ID, never expose exception/SQL/secret details, expose registration/use seams for the existing composition root, and do not add a second ApiError type.
- [x] T035 [FR-4] [WORKSTREAM-MUTATION-METADATA] Create the future failing request-body concurrency checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Reject If-Match/412 as outside MVP.
- [x] T036 [FR-4] [WORKSTREAM-MUTATION-METADATA] Deliver the expected-rowversion shared value contract at src/StudentRegistration.Contracts/ExpectedRowVersion.cs only after T035 fails for the expected reason (depends on T035); feature request DTOs embed it and own their cancellation/idempotency details.
- [x] T037 [FR-5] [WORKSTREAM-PAGINATION-CONTRACT] Create the future failing page 1/default 20/maximum 100, invalid-value 400 PAGE_SIZE_INVALID, no-silent-cap, applied-sort echo, and unique-ID tie-break checks in tests/StudentRegistration.ContractTests/Shared/PaginationContractTests.cs. Test focus: bounded page size, stable order and total metadata.
- [x] T038 [FR-5] [ENTITY-Page] [WORKSTREAM-PAGINATION-CONTRACT] Deliver the bounded Pagination contract workstream and publish the canonical Page contract at src/StudentRegistration.Contracts/Page.cs only after T008 and T037 fail for their expected reasons (depends on T008, T037).
- [x] T039 [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Create the future failing FR-6 checks in tests/StudentRegistration.ContractTests/Shared/ApiVersionPolicyTests.cs. Test focus: breaking-change detection and approved version process. Prove the requirement against its linked AC/EC fixtures: API versioning policy MUST be defined before the first breaking change.
- [x] T040 [FR-6] [WORKSTREAM-API-VERSION-GOVERNANCE] Deliver FR-6 through the bounded API version governance workstream at docs/API_VERSIONING.md only after T039 fails for the expected reason (depends on T039): API versioning policy MUST be defined before the first breaking change.
- [x] T041 [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Create the future failing cross-module checks in tests/StudentRegistration.ArchitectureTests/TimeProviderUsageTests.cs. Test focus: all module application/domain time comes from injected TimeProvider. Reject direct browser/system-clock reads.
- [x] T042 [FR-7] [WORKSTREAM-AUTHORITATIVE-TIME-CONTRACT] Deliver the bounded Authoritative time contract workstream at src/StudentRegistration.Api/Composition/TimeProviderRegistration.cs only after T041 fails for the expected reason (depends on T041); business modules consume the abstraction without a generic Application project.
- [x] T043 [FR-8] [WORKSTREAM-MUTATION-METADATA] Create the future failing FR-8 checks in tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs. Test focus: rowversion, idempotency owner/payload/replay/mismatch and cancellation semantics. Prove the requirement against its linked AC/EC fixtures: Idempotency contracts MUST define owner/scope, server-canonical payload, atomic first claim, same-payload processing/replay, different-payload IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- [x] T044 [FR-4] [FR-8] [WORKSTREAM-MUTATION-METADATA] Deliver the bounded Mutation metadata workstream at src/StudentRegistration.Contracts/CommandMetadata.cs only after T035 and T043 fail for their expected reasons (depends on T035, T043); compose the T036-owned ExpectedRowVersion contract with the IdempotencyKey value delivered here while each feature owns scope, canonical payload, persistence, and replay rules.
- [x] T045 [FR-9] [ENTITY-PublicContextDto] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Create the future failing FR-9 checks in tests/StudentRegistration.ContractTests/Shared/PublicContextPrivacyTests.cs. Test focus: only public time, term labels, window and service state; no personal/internal data. Prove the requirement against its linked AC/EC fixtures: Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.
- [x] T046 [FR-9] [ENTITY-PublicContextDto] [WORKSTREAM-PUBLIC-CONTEXT-DTO] Deliver FR-9 through the bounded Public context DTO workstream and publish the canonical PublicContextDto contract at src/StudentRegistration.Contracts/PublicContextDto.cs only after T045 fails for the expected reason (depends on T045): Public AUTH-01 status MUST use GET /api/public/context, returning only server time/timezone, public teaching/registration term labels, window state, maintenance state, and no user, role, student, capacity, or internal-health data.

- [x] T047 [FR-10] [ENTITY-AppContext] [ENTITY-TermSummaryDto] [ENTITY-RegistrationWindowSummaryDto] [TYPE-RegistrationWindowSummaryDto] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Create the future failing composition-only AppContextDto/TermSummaryDto/RegistrationWindowSummaryDto completeness, matched-window state consistency and privacy, service/support fields, dual-role selection state, contributor ownership, authorized-role filtering, authoritative-null, and missing-contributor failure checks in tests/StudentRegistration.ContractTests/Shared/AppContextCompositionContractTests.cs.
- [x] T048 [FR-10] [ENTITY-AppContext] [ENTITY-TermSummaryDto] [ENTITY-RegistrationWindowSummaryDto] [TYPE-RegistrationWindowSummaryDto] [WORKSTREAM-COMPOSED-APPLICATION-CONTEXT] Publish the complete contributor, authorization filtering, matched-window state consistency/privacy, authoritative-null, missing-contributor failure, and handler-ownership contract at specs/006-domain-class-api-contracts/contracts/app-context-composition.md only after T047 fails for the expected reason (depends on T047); T011 remains the sole AppContextDto/TermSummaryDto/RegistrationWindowSummaryDto shared source writer, while SPEC-008 consumes those contracts and alone writes both handlers.
- [ ] T049 [NFR-1] [OPENAPI-BASELINE] [OPENAPI-RUNTIME-GATE] After approved version-pinned downstream handlers and a real OpenAPI generator exist, create the failing deterministic-generation and semantic-drift checks in tests/StudentRegistration.ContractTests/OpenApi/OpenApiBaselineTests.cs; keep this task unchecked until then.
- [ ] T050 [NFR-1] [OPENAPI-BASELINE] [OPENAPI-RUNTIME-GATE] Generate and approve the deterministic baseline at specs/006-domain-class-api-contracts/contracts/openapi/student-registration-v1.json only after T049 fails against the real generated surface (depends on T049); never hand-author or approve an empty/speculative baseline.
- [ ] T051 [NFR-1] [OPENAPI-CI] [OPENAPI-RUNTIME-GATE] Wire the semantic operation/schema/status/security drift gate at .github/scripts/Verify-OpenApi.ps1 only after T049 and T050 establish the real baseline behavior (depends on T049, T050); formatting/order-only drift is ignored and SPEC-018 retains CI workflow ownership.

## Phase 5 - Frontend Route Tests and Integration

- [x] T052 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Create the future failing design-contract checks for SPEC-006 safe data, actions, stable reasons, authorization, and stale/concurrent contribution in tests/StudentRegistration.ContractTests/Routes/SystemStatusContributorContractTests.cs against the pinned SPEC-003 design-only baseline; this is not Razor, browser, component, or E2E runtime evidence.
- [x] T053 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-3] [AC-1] Publish the SPEC-006 SYS-01 contributor contract at specs/006-domain-class-api-contracts/contracts/routes/SYS-01.md only after T052 fails for the expected missing-contract reason (depends on T052); do not edit or claim the canonical Razor page, and leave runtime/component pins `not-pinned`.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T054 [NFR-1] [AUTOMATED-EVIDENCE] [PRODUCTION-RELEASE-GATE] After T049-T051 and the approved downstream API surface pass, produce measurable evidence in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-1.md that deterministic OpenAPI generation and semantic operation/schema/status/security comparison reject every unapproved drift while ignoring formatting/order-only differences.
- [ ] T055 [NFR-2] [AUTOMATED-EVIDENCE] [PRODUCTION-RELEASE-GATE] After all approved downstream handlers and integration suites pass, produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-2.md: Every success/error response in approved feature specs MUST have a contract/integration test.
- [ ] T056 [NFR-3] [AUTOMATED-EVIDENCE] [PRODUCTION-RELEASE-GATE] After real endpoint and security integration exists, produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-3.md: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or unauthorized identifiers.
- [ ] T057 [NFR-4] [AUTOMATED-EVIDENCE] [PRODUCTION-RELEASE-GATE] After real HTTP serialization coverage exists, produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec006/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-006-NFR-4.md: JSON field naming and date/decimal formats MUST be consistent.

## Phase 7 - Scope and Release Evidence

- [ ] T058 [OS-1] [PRODUCTION-RELEASE-GATE] Inspect the complete approved source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-1 remains excluded: GraphQL, gRPC, and public third-party API.
- [ ] T059 [OS-2] [PRODUCTION-RELEASE-GATE] Inspect the complete approved source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-2 remains excluded: Generic CRUD endpoints for every entity.
- [ ] T060 [OS-3] [PRODUCTION-RELEASE-GATE] Inspect the complete approved source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-3 remains excluded: A mediator library unless approved handler volume justifies it.
- [ ] T061 [OS-4] [PRODUCTION-RELEASE-GATE] Inspect the complete approved source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-006-scope-review.md that OS-4 remains excluded: Breaking-change version until an actual breaking change is proposed.
- [ ] T062 [TRACE] [SC-1] [SC-2] [SC-3] [PRODUCTION-RELEASE-GATE] Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-006-traceability.md only after every applicable runtime task passes; reject release if any row lacks passing evidence.
- [ ] T063 [GATE] [PRODUCTION-RELEASE-GATE] Record Ahmed ELbamby's product-owner, domain-owner, QA, security, accessibility, data/concurrency, and operations review perspectives applicable to SPEC-006 in docs/release-evidence/SPEC-006-release-approval.md only after T062 passes; Ahmed remains the sole human approver for this demo.

Verified progress: T001-T024, T029-T048, and T052-T053 are complete (46 of
63). T016-T024 deliver only compiled, explicitly skipped downstream acceptance
fixtures and do not claim passing runtime acceptance. T025-T028, T049-T051,
and T054-T063 remain explicitly runtime, OpenAPI, measurable NFR, or
production-release gated.
