# Tasks: Identity and Account Lifecycle

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-007 in specs/007-identity-account-lifecycle/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [ ] T007 [GATE] Freeze SPEC-007 requirements, API, data-model, policy approvals, and dependency versions in specs/007-identity-account-lifecycle/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T008 [P] [ENTITY-ApplicationUser] [OWNER-SPEC-007] Create the future failing invariant/schema/serialization checks for canonical ApplicationUser ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec007/ApplicationUserModelTests.cs.
- [ ] T009 [ENTITY-ApplicationUser] [OWNER-SPEC-007] Deliver the canonical ApplicationUser model or governed artifact at src/StudentRegistration.Domain/Modules/IdentityAccess/ApplicationUser.cs after T008 fails for the expected reason (depends on T008).
- [ ] T010 [P] [ENTITY-Staff] [OWNER-SPEC-007] Create the future failing invariant/schema/serialization checks for canonical Staff ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec007/StaffModelTests.cs.
- [ ] T011 [ENTITY-Staff] [OWNER-SPEC-007] Deliver the canonical Staff model or governed artifact at src/StudentRegistration.Domain/Modules/IdentityAccess/Staff.cs after T010 fails for the expected reason (depends on T010).
- [ ] T012 [P] [ENTITY-StudentActivation] [OWNER-SPEC-007] Create the future failing invariant/schema/serialization checks for canonical StudentActivation ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec007/StudentActivationModelTests.cs.
- [ ] T013 [ENTITY-StudentActivation] [OWNER-SPEC-007] Deliver the canonical StudentActivation model or governed artifact at src/StudentRegistration.Domain/Modules/IdentityAccess/StudentActivation.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-RoleAssignment] [OWNER-SPEC-007] Create the future failing invariant/schema/serialization checks for canonical RoleAssignment ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec007/RoleAssignmentModelTests.cs.
- [ ] T015 [ENTITY-RoleAssignment] [OWNER-SPEC-007] Deliver the canonical RoleAssignment model or governed artifact at src/StudentRegistration.Domain/Modules/IdentityAccess/RoleAssignment.cs after T014 fails for the expected reason (depends on T014).
- [ ] T016 [P] [ENTITY-SecurityAudit] [OWNER-SPEC-007] Create the future failing invariant/schema/serialization checks for canonical SecurityAudit ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec007/SecurityAuditModelTests.cs.
- [ ] T017 [ENTITY-SecurityAudit] [OWNER-SPEC-007] Deliver the canonical SecurityAudit model or governed artifact at src/StudentRegistration.Domain/Modules/IdentityAccess/SecurityAudit.cs after T016 fails for the expected reason (depends on T016).
- [ ] T018 [API-Endpoint01] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/auth/student/login in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T019 [P] [API-Endpoint01] Verify every documented response and authorization outcome for POST /api/auth/student/login in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint01ContractTests.cs.
- [ ] T020 [API-Endpoint01] [OWNER-SPEC-007] Deliver the sole canonical POST /api/auth/student/login handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T019 fails for the expected reason (depends on T019).
- [ ] T021 [API-Endpoint02] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/auth/student/activate in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T022 [P] [API-Endpoint02] Verify every documented response and authorization outcome for POST /api/auth/student/activate in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint02ContractTests.cs.
- [ ] T023 [API-Endpoint02] [OWNER-SPEC-007] Deliver the sole canonical POST /api/auth/student/activate handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T022 fails for the expected reason (depends on T022).
- [ ] T024 [API-Endpoint03] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/auth/staff/login in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T025 [P] [API-Endpoint03] Verify every documented response and authorization outcome for POST /api/auth/staff/login in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint03ContractTests.cs.
- [ ] T026 [API-Endpoint03] [OWNER-SPEC-007] Deliver the sole canonical POST /api/auth/staff/login handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T025 fails for the expected reason (depends on T025).
- [ ] T027 [API-Endpoint04] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/auth/logout in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T028 [P] [API-Endpoint04] Verify every documented response and authorization outcome for POST /api/auth/logout in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint04ContractTests.cs.
- [ ] T029 [API-Endpoint04] [OWNER-SPEC-007] Deliver the sole canonical POST /api/auth/logout handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T028 fails for the expected reason (depends on T028).
- [ ] T030 [API-Endpoint05] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/auth/recovery in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T031 [P] [API-Endpoint05] Verify every documented response and authorization outcome for POST /api/auth/recovery in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint05ContractTests.cs.
- [ ] T032 [API-Endpoint05] [OWNER-SPEC-007] Deliver the sole canonical POST /api/auth/recovery handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T031 fails for the expected reason (depends on T031).
- [ ] T033 [API-Endpoint06] [OWNER-SPEC-007] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/auth/session in specs/007-identity-account-lifecycle/contracts/api.md.
- [ ] T034 [P] [API-Endpoint06] Verify every documented response and authorization outcome for GET /api/auth/session in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint06ContractTests.cs.
- [ ] T035 [API-Endpoint06] [OWNER-SPEC-007] Deliver the sole canonical GET /api/auth/session handler at src/StudentRegistration.Server/Modules/IdentityAccess/Endpoints/Spec007Endpoints.cs after T034 fails for the expected reason (depends on T034).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Student login (FR-1, FR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T036 [P] [AC-1] [FR-1] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-1Tests.cs for AC-1: Student login (FR-1, FR-4): Given an activated active student with University ID and password When valid credentials are submitted on /student/login Then a secure authenticated session is established And the server routes only to the student's own context.
### US2 - Student activation safety (FR-2) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T037 [P] [AC-2] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-2Tests.cs for AC-2: Student activation safety (FR-2): Given no pre-imported student record matches an entered University ID When activation is submitted Then no account is created or linked And a generic safe response is returned.
### US3 - Shared staff login (FR-3, FR-4, FR-6) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T038 [P] [AC-3] [FR-3] [FR-4] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-3Tests.cs for AC-3: Shared staff login (FR-3, FR-4, FR-6): Given a staff account with TA claim and valid MFA When staff login succeeds Then the server supplies TA context And no client parameter can add Lecturer or Admin permissions.
### US4 - Antiforgery (FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T039 [P] [AC-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-4Tests.cs for AC-4: Antiforgery (FR-7): Given an authenticated cookie without a valid antiforgery token When a state-changing request is submitted Then the request is rejected and no state changes.
### US5 - Secure lifecycle and abuse control (FR-5, FR-8, FR-9) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T040 [P] [AC-5] [FR-5] [FR-8] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-5Tests.cs for AC-5: Secure lifecycle and abuse control (FR-5, FR-8, FR-9): Given repeated failed login/recovery attempts for an account When the approved threshold is reached Then lockout/rate limiting and safe audit occur And no long-lived credential is written to browser local storage.
### US6 - Parallel activation is single-use (FR-2, FR-10, FR-11) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T041 [P] [AC-6] [FR-2] [FR-10] [FR-11] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-6Tests.cs for AC-6: Parallel activation is single-use (FR-2, FR-10, FR-11): Given one valid activation token for one unclaimed University ID When ten activation requests use that token concurrently through two application replicas Then exactly one account link is created And every other request receives the same safe already-used result And no duplicate University ID claim exists.
### US7 - Replica-wide invalidation (FR-5, FR-12) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T042 [P] [AC-7] [FR-5] [FR-12] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-7Tests.cs for AC-7: Replica-wide invalidation (FR-5, FR-12): Given a user has sessions routed to two application replicas When recovery changes the password and security stamp Then both replicas reject every earlier session And lockout/rate-limit counters remain consistent across replicas.
### US8 - Identity route contract (FR-13) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T043 [P] [AC-8] [FR-13] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-8Tests.cs for AC-8: Identity route contract (FR-13): Given AUTH-02 through AUTH-05 and STU-08 Page Design Records When their component, contract, E2E, accessibility, and visual plans are reviewed Then every route/state maps to SPEC-003 and the owning identity FR/AC IDs.
### US9 - Authentication quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-9 as an independently demonstrable slice of Identity and Account Lifecycle.

**Independent Test**: Execute only the AC-9 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T035.
- [ ] T044 [P] [AC-9] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-9Tests.cs for AC-9: Authentication quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given the SPEC-018 approved load and positive/negative role matrix When authentication performance, enumeration, configuration, and authorization tests execute Then login is at most 500 ms p95 excluding MFA-provider latency And errors do not reveal account existence And credential configuration passes the current approved ASP.NET Core security baseline And every protected endpoint permits and denies exactly the documented roles.
- [ ] T045 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-1Tests.cs and assert: University ID already activated -> direct to login/recovery, no second account.
- [ ] T046 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-2Tests.cs and assert: Disabled/locked account -> safe generic denial and audit.
- [ ] T047 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-3Tests.cs and assert: User has Lecturer and TA claims -> explicit authorized context switch, never privilege union beyond claims.
- [ ] T048 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-4Tests.cs and assert: Session expires during plan edit -> reauthenticate then revalidate plan.
- [ ] T049 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-5Tests.cs and assert: Repeated recovery request -> rate limit while returning generic result.
- [ ] T050 [P] [EC-6] Exercise EC-6 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-6Tests.cs and assert: Activation commits but its response is lost -> retry returns the already-used safe result and MUST NOT create another user or role assignment.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T051 [P] [FR-1] [WORKSTREAM-STUDENT-AUTHENTICATION] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Identity/StudentLoginTests.cs. Test focus: normalized University ID, generic failures, own context and performance. Prove the requirement against its linked AC/EC fixtures: Student login MUST accept normalized University ID and password.
- [ ] T052 [FR-1] [WORKSTREAM-STUDENT-AUTHENTICATION] Deliver FR-1 through the bounded Student authentication workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StudentAuthenticationService.cs only after T051 fails for the expected reason (depends on T051): Student login MUST accept normalized University ID and password.
- [ ] T053 [P] [FR-2] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Identity/StudentActivationConcurrencyTests.cs. Test focus: pre-imported identity proof, single-use token and one unique institutional claim under parallel requests. Prove the requirement against its linked AC/EC fixtures: Student activation MUST only claim a pre-imported student record after verification through an approved institutional factor.
- [ ] T054 [FR-2] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Deliver FR-2 through the bounded Atomic student activation workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StudentActivationService.cs only after T053 fails for the expected reason (depends on T053): Student activation MUST only claim a pre-imported student record after verification through an approved institutional factor.
- [ ] T055 [P] [FR-3] [WORKSTREAM-STAFF-AUTHENTICATION-AND-MFA] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Identity/StaffMfaTests.cs. Test focus: one staff login, no self-registration, required MFA and server role routing. Prove the requirement against its linked AC/EC fixtures: Staff MUST use one login and MUST NOT self-register.
- [ ] T056 [FR-3] [WORKSTREAM-STAFF-AUTHENTICATION-AND-MFA] Deliver FR-3 through the bounded Staff authentication and MFA workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StaffAuthenticationService.cs only after T055 fails for the expected reason (depends on T055): Staff MUST use one login and MUST NOT self-register.
- [ ] T057 [P] [FR-4] [WORKSTREAM-AUTHORIZATION-POLICIES] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/IdentityResourceScopeTests.cs. Test focus: role and resource scope for positive, negative and direct-object cases. Prove the requirement against its linked AC/EC fixtures: The server MUST issue role claims and enforce endpoint/resource policies for Student/Admin/Lecturer/TeachingAssistant.
- [ ] T058 [FR-4] [WORKSTREAM-AUTHORIZATION-POLICIES] Deliver FR-4 through the bounded Authorization policies workstream at src/StudentRegistration.Server/Authorization/RolePolicies.cs only after T057 fails for the expected reason (depends on T057): The server MUST issue role claims and enforce endpoint/resource policies for Student/Admin/Lecturer/TeachingAssistant.
- [ ] T059 [P] [FR-5] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Create the future failing FR-5 checks in tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs. Test focus: cookie/antiforgery, logout, recovery, security-stamp invalidation across replicas and no local-storage token. Prove the requirement against its linked AC/EC fixtures: The system MUST support secure recovery, lockout, logout, and invalidate-all-sessions.
- [ ] T060 [FR-5] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Deliver FR-5 through the bounded Session and recovery lifecycle workstream at src/StudentRegistration.Server/Modules/IdentityAccess/SessionLifecycleService.cs only after T059 fails for the expected reason (depends on T059): The system MUST support secure recovery, lockout, logout, and invalidate-all-sessions.
- [ ] T061 [P] [FR-6] [WORKSTREAM-STAFF-AUTHENTICATION-AND-MFA] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Identity/StaffMfaTests.cs. Test focus: one staff login, no self-registration, required MFA and server role routing. Prove the requirement against its linked AC/EC fixtures: Staff MUST use MFA before production.
- [ ] T062 [FR-6] [WORKSTREAM-STAFF-AUTHENTICATION-AND-MFA] Deliver FR-6 through the bounded Staff authentication and MFA workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StaffAuthenticationService.cs only after T061 fails for the expected reason (depends on T061): Staff MUST use MFA before production.
- [ ] T063 [P] [FR-7] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs. Test focus: cookie/antiforgery, logout, recovery, security-stamp invalidation across replicas and no local-storage token. Prove the requirement against its linked AC/EC fixtures: Authentication MUST use a same-origin Secure, HttpOnly, SameSite cookie plus antiforgery for mutations.
- [ ] T064 [FR-7] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Deliver FR-7 through the bounded Session and recovery lifecycle workstream at src/StudentRegistration.Server/Modules/IdentityAccess/SessionLifecycleService.cs only after T063 fails for the expected reason (depends on T063): Authentication MUST use a same-origin Secure, HttpOnly, SameSite cookie plus antiforgery for mutations.
- [ ] T065 [P] [FR-8] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs. Test focus: cookie/antiforgery, logout, recovery, security-stamp invalidation across replicas and no local-storage token. Prove the requirement against its linked AC/EC fixtures: Long-lived tokens MUST NOT be stored in browser local storage.
- [ ] T066 [FR-8] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Deliver FR-8 through the bounded Session and recovery lifecycle workstream at src/StudentRegistration.Server/Modules/IdentityAccess/SessionLifecycleService.cs only after T065 fails for the expected reason (depends on T065): Long-lived tokens MUST NOT be stored in browser local storage.
- [ ] T067 [P] [FR-9] [WORKSTREAM-IDENTITY-ABUSE-CONTROLS] Create the future failing FR-9 checks in tests/StudentRegistration.SecurityTests/IdentityRateLimitTests.cs. Test focus: shared lockout/rate limiting, enumeration resistance and safe audit. Prove the requirement against its linked AC/EC fixtures: Login/activation/recovery MUST be rate-limited and safely audited.
- [ ] T068 [FR-9] [WORKSTREAM-IDENTITY-ABUSE-CONTROLS] Deliver FR-9 through the bounded Identity abuse controls workstream at src/StudentRegistration.Server/Modules/IdentityAccess/IdentityRateLimitPolicies.cs only after T067 fails for the expected reason (depends on T067): Login/activation/recovery MUST be rate-limited and safely audited.
- [ ] T069 [P] [FR-10] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Identity/StudentActivationConcurrencyTests.cs. Test focus: pre-imported identity proof, single-use token and one unique institutional claim under parallel requests. Prove the requirement against its linked AC/EC fixtures: Activation and recovery tokens MUST be single-use through an atomic database transition; concurrent uses of one token MUST change at most one account state.
- [ ] T070 [FR-10] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Deliver FR-10 through the bounded Atomic student activation workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StudentActivationService.cs only after T069 fails for the expected reason (depends on T069): Activation and recovery tokens MUST be single-use through an atomic database transition; concurrent uses of one token MUST change at most one account state.
- [ ] T071 [P] [FR-11] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Create the future failing FR-11 checks in tests/StudentRegistration.IntegrationTests/Identity/StudentActivationConcurrencyTests.cs. Test focus: pre-imported identity proof, single-use token and one unique institutional claim under parallel requests. Prove the requirement against its linked AC/EC fixtures: Claiming an institutional University ID MUST be protected by a unique database constraint so parallel activation requests cannot link it twice.
- [ ] T072 [FR-11] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Deliver FR-11 through the bounded Atomic student activation workstream at src/StudentRegistration.Server/Modules/IdentityAccess/StudentActivationService.cs only after T071 fails for the expected reason (depends on T071): Claiming an institutional University ID MUST be protected by a unique database constraint so parallel activation requests cannot link it twice.
- [ ] T073 [P] [FR-12] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Create the future failing FR-12 checks in tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs. Test focus: cookie/antiforgery, logout, recovery, security-stamp invalidation across replicas and no local-storage token. Prove the requirement against its linked AC/EC fixtures: Lockout counters, rate-limit state, security stamps, and session invalidation MUST be shared across all application replicas.
- [ ] T074 [FR-12] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Deliver FR-12 through the bounded Session and recovery lifecycle workstream at src/StudentRegistration.Server/Modules/IdentityAccess/SessionLifecycleService.cs only after T073 fails for the expected reason (depends on T073): Lockout counters, rate-limit state, security stamps, and session invalidation MUST be shared across all application replicas.
- [ ] T075 [P] [FR-13] [WORKSTREAM-IDENTITY-ROUTE-STATES] Create the future failing FR-13 checks in tests/StudentRegistration.Client.UnitTests/Identity/IdentityRouteStateTests.cs. Test focus: AUTH-02 through AUTH-05 and STU-08 states map to SPEC-003. Prove the requirement against its linked AC/EC fixtures: AUTH-02 through AUTH-05 and STU-08 MUST consume the SPEC-003 page, state, accessibility, and functional-test contracts.
- [ ] T076 [FR-13] [WORKSTREAM-IDENTITY-ROUTE-STATES] Deliver FR-13 through the bounded Identity route states workstream at src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs only after T075 fails for the expected reason (depends on T075): AUTH-02 through AUTH-05 and STU-08 MUST consume the SPEC-003 page, state, accessibility, and functional-test contracts.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T077 [P] [AUTH-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [AC-1] [AC-9] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for AUTH-02 in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentLoginPageFeatureTests.cs.
- [ ] T078 [AUTH-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [AC-1] [AC-9] Deliver the sole canonical Blazor implementation for AUTH-02 at src/StudentRegistration.Client/Pages/StudentLoginPage.razor after T077 and the SPEC-003 contract/component checks fail for expected reasons (depends on T077).
- [ ] T079 [P] [AUTH-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-10] [FR-11] [AC-2] [AC-6] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for AUTH-03 in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentActivationPageFeatureTests.cs.
- [ ] T080 [AUTH-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-10] [FR-11] [AC-2] [AC-6] Deliver the sole canonical Blazor implementation for AUTH-03 at src/StudentRegistration.Client/Pages/StudentActivationPage.razor after T079 and the SPEC-003 contract/component checks fail for expected reasons (depends on T079).
- [ ] T081 [P] [AUTH-04] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-6] [AC-3] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for AUTH-04 in tests/StudentRegistration.E2ETests/Specs/Spec007/StaffLoginPageFeatureTests.cs.
- [ ] T082 [AUTH-04] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-6] [AC-3] Deliver the sole canonical Blazor implementation for AUTH-04 at src/StudentRegistration.Client/Pages/StaffLoginPage.razor after T081 and the SPEC-003 contract/component checks fail for expected reasons (depends on T081).
- [ ] T083 [P] [AUTH-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-10] [FR-12] [AC-5] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for AUTH-05 in tests/StudentRegistration.E2ETests/Specs/Spec007/AccountRecoveryPageFeatureTests.cs.
- [ ] T084 [AUTH-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-10] [FR-12] [AC-5] [AC-7] Deliver the sole canonical Blazor implementation for AUTH-05 at src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor after T083 and the SPEC-003 contract/component checks fail for expected reasons (depends on T083).
- [ ] T085 [P] [STU-08] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-12] [AC-4] [AC-5] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-08 in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentAccountPageFeatureTests.cs.
- [ ] T086 [STU-08] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-12] [AC-4] [AC-5] [AC-7] Deliver the sole canonical Blazor implementation for STU-08 at src/StudentRegistration.Client/Pages/StudentAccountPage.razor after T085 and the SPEC-003 contract/component checks fail for expected reasons (depends on T085).
- [ ] T087 [P] [ADM-03] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-12] [AC-3] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-03 in tests/StudentRegistration.E2ETests/Specs/Spec007/UserAdministrationPageFeatureTests.cs.
- [ ] T088 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-12] [AC-3] [AC-7] Deliver the sole canonical Blazor implementation for ADM-03 at src/StudentRegistration.Client/Pages/UserAdministrationPage.razor after T087 and the SPEC-003 contract/component checks fail for expected reasons (depends on T087).
- [ ] T089 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-5] [FR-12] [AC-5] [AC-7] Finalize SPEC-007 data, actions, stable reasons, authorization, and stale/concurrent contribution for SYS-01 at specs/007-identity-account-lifecycle/contracts/routes/SYS-01.md without editing the canonical Razor page.
- [ ] T090 [P] [SYS-01] [UI-CONTRACT-SPEC-003] [FR-5] [FR-12] [AC-5] [AC-7] Verify the SPEC-007 contribution consumed by SYS-01 in tests/StudentRegistration.E2ETests/Specs/Spec007/SystemStatusPageContributorTests.cs.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T091 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-1.md: Login SHOULD respond within 500 ms p95 under the SPEC-018 production-like authenticated-session load, excluding MFA-provider latency.
- [ ] T092 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-2.md: Authentication errors MUST NOT reveal whether an account exists.
- [ ] T093 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-3.md: Password/credential configuration MUST follow current ASP.NET Core Identity and AASTMT security policy.
- [ ] T094 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-4.md: Every protected endpoint MUST have positive/negative authorization tests.

## Phase 7 - Scope and Release Evidence

- [ ] T095 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-007-scope-review.md that OS-1 remains excluded: Social login and public staff registration.
- [ ] T096 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-007-scope-review.md that OS-2 remains excluded: Student-created identity without institutional pre-provisioning.
- [ ] T097 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-007-scope-review.md that OS-3 remains excluded: Authorization based only on Blazor route/component visibility.
- [ ] T098 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-007-scope-review.md that OS-4 remains excluded: Final identity-provider integration until AASTMT confirms provider.
- [ ] T099 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-007-traceability.md and reject release if any row lacks passing evidence.
- [ ] T100 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-007 in docs/release-evidence/SPEC-007-release-approval.md.

No task is complete and no implementation file has been created.
