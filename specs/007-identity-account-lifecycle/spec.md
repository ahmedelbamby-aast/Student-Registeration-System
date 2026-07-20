# Feature Specification: Identity and Account Lifecycle

**Feature Branch**: 007-identity-account-lifecycle
**Created**: 2026-07-12
**Status**: APPROVED
**Owner**: Security Lead
**Normative detail**: [requirements.md](requirements.md)

**Owner-approved demo amendment (2026-07-20):** Ahmed ELbamby froze the
combined Lecturer/Teaching Assistant account path. Every enabled demo account
has exactly one of Student, Admin, Lecturer, or TeachingAssistant. Legacy
multi-role session shapes remain compatibility-only and are not provisioned,
accepted for staff login, or included in manual testing.

## Context

Students require University-ID login and controlled first-time activation.
Admin, Lecturer, and TA need one staff login without a pre-authentication or
self-asserted role selector. Each enabled staff account has exactly one staff
role derived by the server after authentication. Blazor client state is not a security
boundary, so identity and authorization are enforced by ASP.NET Core.

For this non-production demo, explicit Development and Testing database
bootstrap generates synthetic pre-provisioned users, unique University IDs,
and initial PIN/password credentials. SQL Server stores only ASP.NET Core
Identity password hashes; plaintext credentials never enter the database,
source control, migrations, logs, telemetry, snapshots, or test reports.

## User Scenarios and Testing

### User Story 1 - Student login (FR-1, FR-4) (P1)

As a Student or staff user, I need the Student login (FR-1, FR-4) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an activated active student with University ID and password<br>
When valid credentials are submitted on /student/login<br>
Then a secure authenticated session is established<br>
And the server routes only to the student's own context.
### User Story 2 - Student activation safety (FR-2) (P1)

As a Student or staff user, I need the Student activation safety (FR-2) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given no pre-imported Identity-owned institutional student identity matches an entered University ID<br>
When activation is submitted<br>
Then no account is created or linked<br>
And a generic safe response is returned.
### User Story 3 - Shared staff login (FR-3, FR-4, FR-6) (P2)

As a Student or staff user, I need the Shared staff login (FR-3, FR-4, FR-6) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a pre-provisioned staff account with TA claim and valid generated demo
credentials<br>
When staff login succeeds<br>
Then the server supplies TA context<br>
And no client parameter can add Lecturer or Admin permissions.
### User Story 4 - Antiforgery (FR-7) (P2)

As a Student or staff user, I need the Antiforgery (FR-7) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given an authenticated cookie without a valid antiforgery token<br>
When a state-changing request is submitted<br>
Then the request is rejected and no state changes.
### User Story 5 - Secure lifecycle and abuse control (FR-5, FR-8, FR-9) (P3)

As a Student or staff user, I need the Secure lifecycle and abuse control (FR-5, FR-8, FR-9) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given repeated failed login/recovery attempts for an account<br>
When the approved threshold is reached<br>
Then lockout/rate limiting and safe audit occur<br>
And no long-lived credential is written to browser local storage.
### User Story 6 - Parallel activation is single-use (FR-2, FR-10, FR-11) (P3)

As a Student or staff user, I need the Parallel activation is single-use (FR-2, FR-10, FR-11) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given one pre-provisioned inactive University ID and its generated initial
password<br>
When ten first-use requests submit that credential and one new password
concurrently through two application replicas<br>
Then exactly one conditional transition verifies the initial credential,
replaces its hash, marks activation complete, and rotates security state<br>
And every other request receives the same safe already-used result<br>
And no duplicate University ID claim or second identity exists.
### User Story 7 - Replica-wide invalidation (FR-5, FR-12) (P3)

As a Student or staff user, I need the Replica-wide invalidation (FR-5, FR-12) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given a user has sessions routed to two application replicas<br>
When recovery changes the password and security stamp<br>
Then both replicas reject every earlier session<br>
And lockout/rate-limit counters remain consistent across replicas.
### User Story 8 - Identity route contract (FR-13) (P3)

As a Student or staff user, I need the Identity route contract (FR-13) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given AUTH-02 through AUTH-05 and STU-08 Page Design Records<br>
When their component, contract, E2E, accessibility, and visual plans are
reviewed<br>
Then every route/state maps to SPEC-003 and the owning identity FR/AC IDs.
### User Story 9 - Authentication quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Student or staff user, I need the Authentication quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-9 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-9)**

Given the SPEC-007 pinned login profile and positive/negative role matrix<br>
When authentication performance, enumeration, configuration, and authorization
tests execute<br>
Then login is at most 500 ms p95<br>
And errors do not reveal account existence<br>
And credential configuration passes the pinned NFR-3 demo baseline while the
unverified AASTMT production boundary stays fail closed<br>
And every protected endpoint permits and denies exactly the documented roles.

### User Story 10 - Governed Admin user lifecycle (FR-3, FR-4, FR-12, FR-14) (P2)

As an authorized identity Admin, I need user import/list/status/role commands
so that ADM-03 is functional and concurrent role changes cannot remove the
final enabled Admin.

**Independent Test**: Execute AC-10 with an import fixture, two enabled Admins,
two concurrent role-removal commands, and audit/idempotency doubles.

**Acceptance Scenario (AC-10)**

Given current versions, reason, permission, and pre-provisioned identities<br>
When Admin user commands execute<br>
Then valid changes are idempotent/audited/replica-visible and invalid, stale,
or final-enabled-Admin removal attempts change nothing.

## Edge Cases

- EC-1: University ID already activated -> direct to login/recovery, no second
  account.
- EC-2: Disabled/locked account -> safe generic denial and audit.
- EC-3: User has Lecturer and TA claims -> explicit authorized context switch,
  never privilege union beyond claims.
- EC-4: Session expires during plan edit -> reauthenticate then revalidate plan.
- EC-5: Repeated recovery request -> rate limit while returning generic result.
- EC-6: Activation commits but its response is lost -> retry returns the
  already-used safe result and MUST NOT create another user or role assignment.

## Requirements

### Functional Requirements

- FR-1: Student login MUST accept normalized University ID and password.
- FR-2: Student activation MUST only claim a pre-imported Identity-owned
  ApplicationUser identity with normalized unique University ID. For the demo,
  the request supplies the system-generated initial PIN/password and a new
  password. First use verifies the initial credential against its ASP.NET Core
  Identity hash, replaces that hash with the new-password hash, and atomically
  marks the pre-provisioned identity active; password confirmation remains a
  client-only validation field. The browser cannot create an identity, choose
  a University ID, or write a plaintext credential to SQL.
- FR-3: Staff MUST use one login and MUST NOT self-register.
- FR-4: The server MUST issue role and exact governed permission claims and
  enforce endpoint/resource policies for
  Student/Admin/Lecturer/TeachingAssistant. For the approved demo, one
  server-side allow-list maps the effective active role to permissions:
  Student receives `Context.Read` and `AcademicProfile.ReadOwn`; Admin receives
  `IdentityAccess.Manage`, `Context.Read`, `AcademicTerms.Manage`, and
  `AcademicProfiles.Manage`; Lecturer and TeachingAssistant receive only
  `Context.Read`. A role without its exact claim is denied, and a context
  switch rebuilds rather than unions claims.
- FR-5: The system MUST support generic request-and-complete recovery,
  password change, logout, and revoke-all-sessions with security-stamp
  rotation across replicas. Recovery proof delivery MUST use the narrow
  Identity-owned `IAccountRecoveryProofDelivery` port and MUST NOT return the
  proof from the request endpoint. Testing uses an injected in-memory adapter;
  Development may use only a Git-ignored, seven-day-bounded local adapter.
  Production remains unavailable until an approved institutional adapter is
  configured and MUST fail closed without one.
- FR-6: Demo staff MUST authenticate on the shared staff page with their
  pre-provisioned local username and generated password. No MFA, 2FA, role
  selector before authentication, self-asserted role, or public staff
  registration is used; the server derives roles and issues the session only
  after password verification and account-state checks. A post-authentication
  context choice is allowed only from that server-returned role set.
- FR-7: Authentication MUST use a same-origin Secure, HttpOnly, SameSite cookie
  plus antiforgery for every state-changing endpoint, including anonymous
  login, activation, and recovery commands. The host issues a Secure, SameSite
  `XSRF-TOKEN` request-token cookie for echo only in `X-XSRF-TOKEN`; it is not
  an authentication credential, while the framework antiforgery cookie remains
  HttpOnly.
- FR-8: Long-lived tokens MUST NOT be stored in browser local storage.
- FR-9: Login/activation/recovery MUST be rate-limited and safely audited.
- FR-10: First-use activation and recovery proofs MUST be attempt-bounded and
  atomically single-use; recovery proofs MUST also be time-bounded.
- FR-11: Claiming an institutional University ID MUST be protected by a unique
  database constraint so parallel activation requests cannot link it twice.
- FR-12: Lockout/rate-limit state, challenges, security stamps, role state,
  session invalidation, and cookie-key access MUST be replica-independent and
  shared; sticky sessions are not a correctness mechanism.
- FR-13: AUTH-02 through AUTH-05 and STU-08 MUST consume the SPEC-003 page,
  state, accessibility, and functional-test contracts.
- FR-14: Explicitly authorized Admins MUST have idempotent pre-provisioned-user
  import/status, bounded user/import reads, versioned account status and role
  commands, audit, and an Identity-owned AdminSecurityGuard that makes
  IdentityAccess the sole concurrency-safe final-enabled-Admin mutation owner.
  Validated imports stage only bounded normalized candidate rows. Publication
  prepares an import-ID credential handoff, commits SQL, then completes it;
  rollback aborts it, retry is idempotent, Production fails closed without an
  approved delivery adapter, and Endpoint 14 exposes no credential or path.

### Non-Functional Requirements

- NFR-1: Login SHOULD respond within 500 ms p95 during a 10-minute profile at
  25 password-login attempts/second across at least two stateless replicas and
  25,000 synthetic accounts: 80% valid, 15% invalid-credential, and 5%
  already-locked requests. Expected generic denials are not errors; unexpected
  errors MUST remain below 1%, with no account-enumeration or shared-state
  inconsistency.
- NFR-2: Authentication errors MUST NOT reveal whether an account exists.
- NFR-3: The demo MUST pin ASP.NET Core Identity 10 password hashing to
  IdentityV3/PBKDF2 with at least 100,000 iterations; require 15-128 character
  passwords with no character-class composition rule; reject versioned common
  and context-specific blocked values; allow password managers, paste, spaces,
  and Unicode; and lock password authentication after five failed attempts for
  five minutes. Activation/recovery proofs expire after 15 minutes and allow at
  most five failed verifications. An official AASTMT credential policy remains
  unverified, so Production MUST fail closed until an approved, versioned
  policy reconciles or supersedes this demo baseline.
- NFR-4: Every protected endpoint MUST have positive/negative authorization
  tests.

### Key Entities and References

- **ApplicationUser**, **Staff**, **StudentActivation**,
  **AccountRecoveryChallenge**, **RoleAssignment**, and
  **AuthenticationAbuseState**, **IdentityImportBatch**,
  **IdentityImportCandidateRow**, **SecurityEvent**, and **AdminSecurityGuard**
  are owned by SPEC-007. Candidate rows are immutable batch-owned normalized
  staging data and contain no credential or raw upload.
- SPEC-017 may consume/query append-only SecurityEvent records because it
  depends on SPEC-007; Identity does not depend on downstream SPEC-017.
- The shared Data Protection key ring is SPEC-004 infrastructure governed in
  security/operations by SPEC-018, not an Identity domain entity.

## Success Criteria

- **SC-1**: Authorized students and staff reach only their permitted context.
- **SC-2**: Account activation cannot claim an unknown or already-claimed institutional identity.
- **SC-3**: Authentication and recovery failures reveal no account-existence information.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| AUTH-02 | /student/login | StudentLoginPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| AUTH-03 | /student/activate | StudentActivationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| AUTH-04 | /staff/login | StaffLoginPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| AUTH-05 | /account/recovery | AccountRecoveryPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| STU-08 | /student/account | StudentAccountPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| ADM-03 | /admin/users | UserAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-007 |
| SYS-01 | /status/{code} | SystemStatusPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-003 |

## Out of Scope

- OS-1: Social login and public staff registration.
- OS-2: Student-created identity without institutional pre-provisioning.
- OS-3: Authorization based only on Blazor route/component visibility.
- OS-4: Final identity-provider integration until AASTMT confirms provider.

## Approval State

DEC-01 and DEC-02 are resolved for this non-production demo through generated,
pre-provisioned local credentials and password-only student/staff
authentication. DEC-13 remains a future production secret/key-custody
decision and does not block local demo implementation. Ahmed ELbamby approved
Gate A demo implementation on 2026-07-13. This approval does not authorize
production deployment, official AASTMT go-live, or later Gate B-D/release
decisions.
