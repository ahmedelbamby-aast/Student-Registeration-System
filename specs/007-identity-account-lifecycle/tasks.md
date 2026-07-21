# Tasks: Identity and Account Lifecycle

**Status**: COMPLETE for the approved Development/Testing design-capability demo; T001-T133 are verified on 2026-07-14. Production and downstream feature gates remain separate.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Planning verification and approval precede implementation. Tests precede every model, service, endpoint, and page delivery. `[P]` is used only for different target files.

## Phase 1 - Planning Readiness and Recorded Gate A Approval

- [x] T001 [DEP-SPEC-003] Validate the consumed page/state/accessibility contracts in specs/003-ux-storyboard-accessibility/ and record the accepted version in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [x] T002 [DEP-SPEC-004] Validate the modular-monolith and external-port contracts in specs/004-architecture-engineering-principles/ and record the accepted version in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [x] T003 [DEP-SPEC-005] Validate the identity persistence, single-use, uniqueness, and retention mappings in specs/005-erd-data-lifecycle/ and record the accepted version in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [x] T004 [DEP-SPEC-006] Validate the safe error/DTO, pagination, concurrency, idempotency, and AppContext conventions in specs/006-domain-class-api-contracts/ and record the accepted version in specs/007-identity-account-lifecycle/dependency-baseline.md; SPEC-007 FR-7 owns cookie/antiforgery behavior under SPEC-018 security governance.
- [x] T005 [DEP-SPEC-018] Validate the shared key-ring, secret-provider, security, load, and operations contracts in specs/018-quality-security-scalability-operations/ and record the accepted version in specs/007-identity-account-lifecycle/dependency-baseline.md.
- [x] T006 [GATE] Run consistency analysis across requirements.md, data-model.md, contracts/api.md, the demo-only generated-credential decision, DEC-13 production boundary, route contribution, and task trace; record the frozen result in specs/007-identity-account-lifecycle/checklists/implementation-readiness.md.
- [x] T007 [GATE] Record Ahmed ELbamby's 2026-07-13 Gate A demo approval of the generated Development/Testing credential model and password-only staff login in specs/007-identity-account-lifecycle/checklists/approval.md; no later task may start before T001-T006 pass.

## Phase 2 - Failing Model and Contract Tests

- [x] T008 [P] [ENTITY-ApplicationUser] [OWNER-SPEC-007] Create failing invariant and shared-security-stamp checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/ApplicationUserModelTests.cs.
- [x] T009 [P] [ENTITY-Staff] [OWNER-SPEC-007] Create failing provisioned-staff and user-link checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/StaffModelTests.cs.
- [x] T010 [P] [ENTITY-StudentActivation] [OWNER-SPEC-007] Create failing provisioned/activated timestamp, failed-attempt, rowversion, no-plaintext-credential, and atomically single-use activation checks proving the activation FK targets the Identity-owned pre-provisioned ApplicationUser rather than downstream Student in tests/StudentRegistration.IntegrationTests/Specs/Spec007/StudentActivationModelTests.cs.
- [x] T011 [P] [ENTITY-AccountRecoveryChallenge] [OWNER-SPEC-007] Create failing hashed, attempt-bounded, atomically single-use recovery checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/AccountRecoveryChallengeModelTests.cs.
- [x] T012 [P] [FR-2] [FR-3] Create failing Development/Testing-only synthetic identity, unique University-ID, ASP.NET password-hash, idempotent bootstrap, and production-environment rejection checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/DemoIdentitySeedTests.cs.
- [x] T013 [P] [ENTITY-RoleAssignment] [OWNER-SPEC-007] Create failing effective-role and duplicate-scope checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/RoleAssignmentModelTests.cs.
- [x] T014 [P] [ENTITY-AuthenticationAbuseState] [ENTITY-SecurityEvent] [ENTITY-AdminSecurityGuard] [OWNER-SPEC-007] Create failing shared rate-limit/lockout rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/AuthenticationAbuseStateModelTests.cs, append-only safe event checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/SecurityEventModelTests.cs, and singleton final-Admin serialization checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/AdminSecurityGuardModelTests.cs.
- [x] T015 [API-Endpoint01] [OWNER-SPEC-007] Finalize POST /api/auth/student/login in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T016 [P] [API-Endpoint01] Create failing POST /api/auth/student/login contract, antiforgery, rate-limit, and authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint01ContractTests.cs.
- [x] T017 [API-Endpoint02] [OWNER-SPEC-007] Finalize POST /api/auth/student/activate in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T018 [P] [API-Endpoint02] Create failing POST /api/auth/student/activate initial-credential/new-password contract, antiforgery, rate-limit, generic-error, and replay checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint02ContractTests.cs.
- [x] T019 [API-Endpoint03] [OWNER-SPEC-007] Finalize POST /api/auth/staff/login in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T020 [P] [API-Endpoint03] Create failing POST /api/auth/staff/login direct-password, antiforgery, rate-limit, disabled/locked, generic-error, and server-derived-role checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint03ContractTests.cs.
- [x] T021 [P] [FR-2] [FR-3] [FR-5] Create failing one-time Development credential-sheet and local recovery-proof delivery, no-plaintext-SQL/source/log/report, Git-ignore, seven-day cleanup, and Production-environment rejection checks in tests/StudentRegistration.SecurityTests/DemoCredentialStorageTests.cs.
- [x] T022 [FR-2] [FR-3] Publish the future demo identity provisioning and credential-handling contract at docs/data/demo-identity-provisioning.md only after T021 fails for the expected reason.
- [x] T023 [API-Endpoint04] [OWNER-SPEC-007] Finalize POST /api/auth/logout in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T024 [P] [API-Endpoint04] Create failing POST /api/auth/logout antiforgery and cookie-expiry checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint04ContractTests.cs.
- [x] T025 [API-Endpoint05] [OWNER-SPEC-007] Finalize POST /api/auth/recovery/request in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T026 [P] [API-Endpoint05] Create failing POST /api/auth/recovery/request antiforgery, indistinguishable-response, and rate-limit checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint05ContractTests.cs.
- [x] T027 [API-Endpoint06] [OWNER-SPEC-007] Finalize POST /api/auth/recovery/complete in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T028 [P] [API-Endpoint06] Create failing POST /api/auth/recovery/complete antiforgery, rate-limit, single-use, and security-stamp-rotation checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint06ContractTests.cs.
- [x] T029 [API-Endpoint07] [OWNER-SPEC-007] Finalize POST /api/auth/password/change in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T030 [P] [API-Endpoint07] Create failing POST /api/auth/password/change reauthentication, antiforgery, and rotation checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint07ContractTests.cs.
- [x] T031 [API-Endpoint08] [OWNER-SPEC-007] Finalize POST /api/auth/sessions/revoke-all in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T032 [P] [API-Endpoint08] Create failing POST /api/auth/sessions/revoke-all antiforgery and two-replica invalidation checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint08ContractTests.cs.
- [x] T033 [API-Endpoint09] [OWNER-SPEC-007] Finalize GET /api/auth/session in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T034 [P] [API-Endpoint09] Create failing GET /api/auth/session active-role, role-set, expiry, and security-internal non-disclosure checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint09ContractTests.cs.
- [x] T035 [API-Endpoint10-RETIRED] Record removal of the former role-context endpoint; single-role sessions require no context mutation.
- [x] T036 [P] [API-Endpoint10-RETIRED] Verify the request type and endpoint remain absent and multiple roles fail closed.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [x] T037 [P] [AC-1] [FR-1] [FR-4] Create failing student-login acceptance coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-1Tests.cs.
- [x] T038 [P] [AC-2] [FR-2] Create failing unknown/pre-imported identity activation coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-2Tests.cs.
- [x] T039 [P] [AC-3] [FR-3] [FR-4] [FR-6] Create failing shared staff password-login, no-second-factor, and server-role coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-3Tests.cs.
- [x] T040 [P] [AC-4] [FR-7] Create the failing every-state-changing-endpoint antiforgery matrix in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-4Tests.cs.
- [x] T041 [P] [AC-5] [FR-5] [FR-8] [FR-9] Create failing generic recovery, abuse-control, and browser-storage coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-5Tests.cs.
- [x] T042 [P] [AC-6] [FR-2] [FR-10] [FR-11] Create failing two-replica activation one-winner coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-6Tests.cs.
- [x] T043 [P] [AC-7] [FR-5] [FR-12] Create failing recovery/password/revoke-all replica invalidation coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-7Tests.cs.
- [x] T044 [P] [AC-8] [FR-13] Create failing identity route-contract coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-8Tests.cs.
- [x] T045 [P] [AC-9] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create failing authentication quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-9Tests.cs.
- [x] T046 [P] [EC-1] Create already-activated identity coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-1Tests.cs.
- [x] T047 [P] [EC-2] Create disabled/locked-account coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-2Tests.cs.
- [x] T048 [P] [EC-3] Create invalid combined-role fail-closed coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-3Tests.cs.
- [x] T049 [P] [EC-4] Create expired-session and plan-revalidation coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-4Tests.cs.
- [x] T050 [P] [EC-5] Create repeated generic recovery coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-5Tests.cs.
- [x] T051 [P] [EC-6] Create lost activation-response replay coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec007/EdgeCases/EC-6Tests.cs.
- [x] T052 [P] [SC-1] Create permitted-context outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/SC-1OutcomeTests.cs.
- [x] T053 [P] [SC-2] Create unknown/already-claimed activation outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/SC-2OutcomeTests.cs.
- [x] T054 [P] [SC-3] Create account-enumeration resistance outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/SC-3OutcomeTests.cs.

## Phase 4 - Behavior Tests, Models, and Application Delivery

- [x] T055 [FR-1] [WORKSTREAM-STUDENT-AUTHENTICATION] Create the failing consolidated student-authentication behavior suite in tests/StudentRegistration.IntegrationTests/Identity/StudentLoginTests.cs.
- [x] T056 [FR-2] [FR-10] [FR-11] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Create the failing consolidated initial-password verification, new-hash replacement, security-state rotation, single conditional transition race, and University-ID uniqueness suite in tests/StudentRegistration.IntegrationTests/Identity/StudentActivationConcurrencyTests.cs.
- [x] T057 [FR-3] [FR-6] [WORKSTREAM-STAFF-PASSWORD-AUTHENTICATION] Create the failing consolidated provisioned-staff, direct-password, disabled/locked, no-role, no-second-factor, and server-role suite in tests/StudentRegistration.IntegrationTests/Identity/StaffPasswordLoginTests.cs.
- [x] T058 [FR-4] [WORKSTREAM-AUTHORIZATION-POLICIES] Create the failing role, exact-permission, effective-role claim-issuance, and resource-scope matrix in tests/StudentRegistration.AuthorizationTests/IdentityResourceScopeTests.cs, tests/StudentRegistration.AuthorizationTests/AcademicPermissionPolicyTests.cs, and tests/StudentRegistration.SecurityTests/IdentityRuntimeCompositionTests.cs.
- [x] T059 [FR-5] [FR-7] [FR-8] [FR-12] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Create the failing consolidated cookie, recovery-proof delivery-port, generic-202/no-proof response, password, revoke-all, and two-replica stamp suite in tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs.
- [x] T060 [FR-7] [FR-9] [NFR-3] [WORKSTREAM-IDENTITY-ABUSE-CONTROLS] Create the failing pinned cookie/antiforgery, password/hash/blocklist, shared five-attempt lockout, 15-minute proof/rate-limit, generic bounded telemetry, and enumeration suite in tests/StudentRegistration.SecurityTests/IdentityRateLimitTests.cs.
- [x] T061 [FR-13] [WORKSTREAM-IDENTITY-ROUTE-STATES] Create the failing AUTH-02..05 and STU-08 state-mapping suite in tests/StudentRegistration.Client.UnitTests/Identity/IdentityRouteStateTests.cs.
- [x] T062 [ENTITY-ApplicationUser] [OWNER-SPEC-007] Deliver the canonical ApplicationUser at src/StudentRegistration.IdentityAccess/Domain/ApplicationUser.cs after T008 fails.
- [x] T063 [ENTITY-Staff] [OWNER-SPEC-007] Deliver the canonical Staff at src/StudentRegistration.IdentityAccess/Domain/Staff.cs after T009 fails.
- [x] T064 [ENTITY-StudentActivation] [OWNER-SPEC-007] Deliver the canonical StudentActivation at src/StudentRegistration.IdentityAccess/Domain/StudentActivation.cs after T010 fails.
- [x] T065 [ENTITY-AccountRecoveryChallenge] [OWNER-SPEC-007] Deliver the canonical AccountRecoveryChallenge at src/StudentRegistration.IdentityAccess/Domain/AccountRecoveryChallenge.cs after T011 fails.
- [x] T066 [FR-2] [FR-3] Deliver the Development/Testing-only identity seed contributor at src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs after T012 and T021 fail; use ASP.NET Core Identity hashing and reject every other environment.
- [x] T067 [ENTITY-RoleAssignment] [OWNER-SPEC-007] Deliver the canonical RoleAssignment at src/StudentRegistration.IdentityAccess/Domain/RoleAssignment.cs after T013 fails.
- [x] T068 [ENTITY-AuthenticationAbuseState] [ENTITY-SecurityEvent] [ENTITY-AdminSecurityGuard] [OWNER-SPEC-007] Deliver the canonical AuthenticationAbuseState at src/StudentRegistration.IdentityAccess/Domain/AuthenticationAbuseState.cs, canonical SecurityEvent at src/StudentRegistration.IdentityAccess/Domain/SecurityEvent.cs, and canonical AdminSecurityGuard at src/StudentRegistration.IdentityAccess/Domain/AdminSecurityGuard.cs after T014 fails.
- [x] T069 [FR-1] [WORKSTREAM-STUDENT-AUTHENTICATION] Deliver student authentication at src/StudentRegistration.IdentityAccess/Application/StudentAuthenticationService.cs after T055 fails.
- [x] T070 [FR-2] [FR-10] [FR-11] [WORKSTREAM-ATOMIC-STUDENT-ACTIVATION] Deliver initial-credential verification, new-hash replacement, security-state rotation, and conditional first-use activation at src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs after T056 fails.
- [x] T071 [FR-3] [FR-6] [WORKSTREAM-STAFF-PASSWORD-AUTHENTICATION] Deliver direct staff password authentication and server-derived role routing at src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs after T057 fails.
- [x] T072 [FR-4] [WORKSTREAM-AUTHORIZATION-POLICIES] Deliver Identity-owned exact-permission policies and the effective-role allow-list at src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs plus cookie claim issuance/replacement at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T058 fails; Admin role membership alone does not satisfy an academic or identity-management policy.
- [x] T073 [FR-5] [FR-7] [FR-8] [FR-12] [WORKSTREAM-SESSION-AND-RECOVERY-LIFECYCLE] Deliver the recovery delivery port at src/StudentRegistration.IdentityAccess/Application/Ports/IAccountRecoveryProofDelivery.cs and session/recovery lifecycle at src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs after T059 fails.
- [x] T074 [FR-7] [FR-9] [NFR-3] [WORKSTREAM-IDENTITY-ABUSE-CONTROLS] Deliver the pinned credential configuration at src/StudentRegistration.IdentityAccess/Application/IdentitySecurityOptions.cs, shared abuse controls at src/StudentRegistration.IdentityAccess/Application/IdentityRateLimitPolicies.cs, and cookie/antiforgery composition at src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs after T060 fails; reuse SPEC-018's generic bounded identity/request telemetry without editing its allow-list.
- [x] T075 [FR-13] [WORKSTREAM-IDENTITY-ROUTE-STATES] Deliver identity route-state mapping at src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs after T061 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [x] T076 [API-Endpoint01] Deliver the shared authentication DTOs at src/StudentRegistration.Contracts/Identity/AuthenticationContracts.cs and POST /api/auth/student/login handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T016 and T055 fail for expected reasons.
- [x] T077 [API-Endpoint02] Deliver the POST /api/auth/student/activate handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T018 and T056 fail for expected reasons.
- [x] T078 [API-Endpoint03] Deliver the POST /api/auth/staff/login handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T020 and T057 fail for expected reasons.
- [x] T079 [FR-2] [FR-3] [FR-5] Deliver the Development-only one-time ignored credential-sheet writer at src/StudentRegistration.Api/Development/DemoCredentialSheetWriter.cs and bounded local recovery adapter at src/StudentRegistration.Api/Development/DevelopmentRecoveryProofDelivery.cs after T021, T022, and T059 fail; Testing injects an in-memory adapter and Production startup rejects both components.
- [x] T080 [API-Endpoint04] Deliver the POST /api/auth/logout handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T024 and T059 fail for expected reasons.
- [x] T081 [API-Endpoint05] Deliver the POST /api/auth/recovery/request handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T026, T059, T060, and T079 fail for expected reasons.
- [x] T082 [API-Endpoint06] Deliver the POST /api/auth/recovery/complete handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T028 and T059 fail for expected reasons.
- [x] T083 [API-Endpoint07] Deliver the POST /api/auth/password/change handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T030 and T059 fail for expected reasons.
- [x] T084 [API-Endpoint08] Deliver the POST /api/auth/sessions/revoke-all handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T032 and T059 fail for expected reasons.
- [x] T085 [API-Endpoint09] Deliver the GET /api/auth/session handler at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T034 and T059 fail for expected reasons.
- [x] T086 [API-Endpoint10-RETIRED] Remove the former role-context handler and keep single-role session issuance server-derived.

## Phase 6 - Frontend Functional Tests and Pages

- [x] T087 [P] [AUTH-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [AC-1] [AC-9] Create failing Student Login journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentLoginPageFeatureTests.cs.
- [x] T088 [AUTH-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [AC-1] [AC-9] Deliver StudentLoginPage at src/StudentRegistration.Client/Pages/StudentLoginPage.razor after T087 fails.
- [x] T089 [P] [AUTH-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-10] [FR-11] [AC-2] [AC-6] Create failing issued-initial-password, new-password/confirmation, validation, and concurrency-result Student Activation journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentActivationPageFeatureTests.cs.
- [x] T090 [AUTH-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-10] [FR-11] [AC-2] [AC-6] Deliver the issued-initial-password plus new-password/confirmation StudentActivationPage at src/StudentRegistration.Client/Pages/StudentActivationPage.razor after T089 fails.
- [x] T091 [P] [AUTH-04] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-6] [AC-3] Create failing shared Staff Login password-only, no-second-factor, and server-role journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/StaffLoginPageFeatureTests.cs.
- [x] T092 [AUTH-04] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-6] [AC-3] Deliver StaffLoginPage at src/StudentRegistration.Client/Pages/StaffLoginPage.razor after T091 fails.
- [x] T093 [P] [AUTH-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-10] [FR-12] [AC-5] [AC-7] Create failing recovery request/complete journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/AccountRecoveryPageFeatureTests.cs.
- [x] T094 [AUTH-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-10] [FR-12] [AC-5] [AC-7] Deliver AccountRecoveryPage at src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor after T093 fails.
- [x] T095 [P] [STU-08] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-12] [AC-4] [AC-5] [AC-7] Create failing password-change and revoke-all journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/StudentAccountPageFeatureTests.cs.
- [x] T096 [STU-08] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [FR-12] [AC-4] [AC-5] [AC-7] Deliver StudentAccountPage at src/StudentRegistration.Client/Pages/StudentAccountPage.razor after T095 fails.
- [x] T097 [P] [ADM-03] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-12] [FR-14] [AC-3] [AC-7] [AC-10] Create failing import/list/status/role User Administration journeys in tests/StudentRegistration.E2ETests/Specs/Spec007/UserAdministrationPageFeatureTests.cs.
- [x] T098 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-12] [FR-14] [AC-3] [AC-7] [AC-10] Finalize the Identity-side data/actions/states consumed by ADM-03 in specs/007-identity-account-lifecycle/contracts/routes/ADM-03.md after T097 fails.
- [x] T099 [SYS-01] [UI-CONTRACT-SPEC-003] [FR-5] [FR-12] [AC-5] [AC-7] Finalize the Identity contribution to specs/007-identity-account-lifecycle/contracts/routes/SYS-01.md without writing the canonical status page.
- [x] T100 [P] [SYS-01] [UI-CONTRACT-SPEC-003] [FR-5] [FR-12] [AC-5] [AC-7] Verify that contribution in tests/StudentRegistration.E2ETests/Specs/Spec007/SystemStatusPageContributorTests.cs.

## Phase 7 - Admin User Lifecycle Contracts and Delivery

- [x] T101 [ENTITY-IdentityImportBatch] [ENTITY-IdentityImportCandidateRow] [OWNER-SPEC-007] Create failing source/hash/state/error/rowversion/idempotent-result batch checks plus immutable normalized batch-owned candidate-row checks in tests/StudentRegistration.IntegrationTests/Specs/Spec007/IdentityImportBatchModelTests.cs.
- [x] T102 [API-Endpoint11] [OWNER-SPEC-007] Finalize GET /api/admin/users in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T103 [P] [API-Endpoint11] Create failing bounded search/minimized user list and authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint11ContractTests.cs for GET /api/admin/users.
- [x] T104 [API-Endpoint12] [OWNER-SPEC-007] Finalize POST /api/admin/users/imports in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T105 [P] [API-Endpoint12] Create failing antiforgery, source/hash/idempotency, and pre-provision validation checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint12ContractTests.cs for POST /api/admin/users/imports.
- [x] T106 [API-Endpoint13] [OWNER-SPEC-007] Finalize GET /api/admin/users/imports/{importId} in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T107 [P] [API-Endpoint13] Create failing import state/row-error/direct-object authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint13ContractTests.cs for GET /api/admin/users/imports/{importId}.
- [x] T108 [API-Endpoint14] [OWNER-SPEC-007] Finalize POST /api/admin/users/imports/{importId}/publish in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T109 [P] [API-Endpoint14] Create failing antiforgery, all-or-nothing, import-version, and idempotent publication checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint14ContractTests.cs for POST /api/admin/users/imports/{importId}/publish.
- [x] T110 [API-Endpoint15] [OWNER-SPEC-007] Finalize PATCH /api/admin/users/{userId}/status in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T111 [P] [API-Endpoint15] Create failing antiforgery, aggregate expectedRowVersion, reason/audit, session-invalidation, and final-enabled-Admin disable-race checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint15ContractTests.cs for PATCH /api/admin/users/{userId}/status.
- [x] T112 [API-Endpoint16] [OWNER-SPEC-007] Finalize PUT /api/admin/users/{userId}/roles in specs/007-identity-account-lifecycle/contracts/api.md.
- [x] T113 [P] [API-Endpoint16] Create failing antiforgery, single aggregate expectedRowVersion, reason/audit, claim-refresh, and final-enabled-Admin race checks in tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint16ContractTests.cs for PUT /api/admin/users/{userId}/roles.
- [x] T114 [AC-10] [FR-3] [FR-4] [FR-12] [FR-14] Create failing governed Admin user lifecycle acceptance coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec007/AC-10Tests.cs.
- [x] T115 [FR-14] [WORKSTREAM-GOVERNED-ADMIN-USER-LIFECYCLE] Create the failing consolidated import/list/status/role/idempotency suite in tests/StudentRegistration.IntegrationTests/Identity/AdminUserLifecycleTests.cs, including two-replica AdminSecurityGuard locking for disable-versus-disable and disable-versus-role-removal races, one FINAL_ADMIN_REQUIRED loser, shared AuditEvent/SecurityEvent atomicity, and proof that SPEC-017 has no competing role writer.
- [x] T116 [ENTITY-IdentityImportBatch] [ENTITY-IdentityImportCandidateRow] [OWNER-SPEC-007] Deliver the canonical IdentityImportBatch at src/StudentRegistration.IdentityAccess/Domain/IdentityImportBatch.cs and the canonical immutable batch-owned IdentityImportCandidateRow at src/StudentRegistration.IdentityAccess/Domain/IdentityImportCandidateRow.cs after T101 fails.
- [x] T117 [FR-14] [WORKSTREAM-GOVERNED-ADMIN-USER-LIFECYCLE] Deliver governed Admin user lifecycle behavior at src/StudentRegistration.IdentityAccess/Application/AdminUserLifecycleService.cs after T115 fails.
- [x] T118 [API-Endpoint11] Deliver the shared administration DTOs at src/StudentRegistration.Contracts/Identity/AdministrationContracts.cs and GET /api/admin/users at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T103 and T115 fail.
- [x] T119 [API-Endpoint12] Deliver POST /api/admin/users/imports at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T105 and T115 fail.
- [x] T120 [API-Endpoint13] Deliver GET /api/admin/users/imports/{importId} at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T107 and T115 fail.
- [x] T121 [API-Endpoint14] Deliver POST /api/admin/users/imports/{importId}/publish at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T109 and T115 fail.
- [x] T122 [API-Endpoint15] Deliver PATCH /api/admin/users/{userId}/status at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T111 and T115 fail.
- [x] T123 [API-Endpoint16] Deliver PUT /api/admin/users/{userId}/roles at src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs after T113 and T115 fail.
- [x] T124 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-12] [FR-14] [AC-3] [AC-7] [AC-10] Deliver the SPEC-007 identity-owned import/list/status/role slice of UserAdministrationPage at src/StudentRegistration.Client/Pages/UserAdministrationPage.razor after T097-T123 pass. Preserve the documented contribution seam for SPEC-017 T077-T078; its later audit/reporting extension and contributor verification remain deferred and do not block this Identity-owned slice.

## Phase 8 - Quality, Scope, and Release Evidence

- [x] T125 [ENTITY-ApplicationUser] [ENTITY-Staff] [ENTITY-StudentActivation] [ENTITY-AccountRecoveryChallenge] [ENTITY-AuthenticationAbuseState] [ENTITY-IdentityImportBatch] [ENTITY-IdentityImportCandidateRow] [ENTITY-RoleAssignment] [ENTITY-SecurityEvent] [ENTITY-AdminSecurityGuard] [PERSISTENCE-MAPPING] Create the failing real-SQL relational-model contribution suite in tests/StudentRegistration.IntegrationTests/Specs/Spec007/IdentityAccessModelConfigurationTests.cs for every listed entity, including immutable batch-owned candidate-row staging with unique batch/ordinal and cascade ownership, unique University ID, password-hash-only persistence, activation/recovery single-use, role scope, singleton guard, rowversion/indexes, and append-only SecurityEvent; do not require or claim SPEC-008's not-yet-owned initial migration.
- [x] T126 [ENTITY-ApplicationUser] [ENTITY-Staff] [ENTITY-StudentActivation] [ENTITY-AccountRecoveryChallenge] [ENTITY-AuthenticationAbuseState] [ENTITY-IdentityImportBatch] [ENTITY-IdentityImportCandidateRow] [ENTITY-RoleAssignment] [ENTITY-SecurityEvent] [ENTITY-AdminSecurityGuard] [PERSISTENCE-MAPPING] Deliver the complete ten-entity IdentityAccess EF Core mapping contribution, including immutable batch-owned IdentityImportCandidateRow staging, at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs after T125 fails; SPEC-004 remains the sole StudentRegistrationDbContext writer and SPEC-008 remains the sole S1IdentityAcademicFoundation migration writer.
- [x] T127 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce the 10-minute, 25-login/s, 25,000-account, two-replica, 80/15/5-mix p95/error/invariant evidence in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-1.md.
- [x] T128 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce enumeration-resistance evidence in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-2.md.
- [x] T129 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce evidence for the pinned IdentityV3/100,000-iteration, 15-128 length, blocklist, no-composition, five-attempt lockout, 15-minute proof, and unverified-AASTMT-production-fail-closed baseline in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-3.md.
- [x] T130 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce endpoint authorization-matrix evidence in tests/StudentRegistration.QualityTests/Specs/Spec007/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-007-NFR-4.md.
- [x] T131 [OS-1] [OS-2] [OS-3] [OS-4] Record the verified scope exclusions and demo-versus-production identity boundary in docs/release-evidence/SPEC-007-scope-review.md.
- [x] T132 [TRACE] [SC-1] [SC-2] [SC-3] Generate the completed FR/NFR/AC/EC/SC/route/entity/endpoint evidence matrix at docs/release-evidence/SPEC-007-traceability.md and reject release for any missing row.
- [x] T133 [GATE] Record product, security, identity-owner, QA, accessibility, data/concurrency, and operations release approvals in docs/release-evidence/SPEC-007-release-approval.md.

All tasks T001-T133 are complete for the approved Development/Testing demo.
Production and downstream feature gates remain outside this SPEC-007 approval.
