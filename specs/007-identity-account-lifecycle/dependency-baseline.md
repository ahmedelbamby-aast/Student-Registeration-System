# SPEC-007 Dependency Baseline

**Recorded:** 2026-07-14<br>
**Feature:** SPEC-007 Identity and Account Lifecycle<br>
**Result:** PASS FOR DEPENDENCY-ORDERED NON-PRODUCTION DEMO WORK

## SPEC-003 identity-route design baseline

- Accepted design-governance commit:
  `8ee8f724af3b60bbbc464532bc68de6f173247e5`.
- Accepted versions: page-design governance `1.1`, route manifest `2.1.0`,
  page/API manifest `1.1.0`, component manifest `1.1.0`, and immutable Page
  Design Records version `1.0`.
- Consumed records: AUTH-02, AUTH-03, AUTH-04, AUTH-05, STU-08, ADM-03, and
  the bounded SPEC-007 contribution to SYS-01. AUTH-03 requires University ID,
  issued initial PIN/password, new password, and client-side confirmation.
- Readiness boundary: the records are approved in `design-only` state and
  their SPEC-007 contributor versions remain `not-pinned`. They govern the
  test-first route work but do not by themselves claim route source, browser,
  accessibility, visual, or release evidence. ADM-03 also remains dependent
  on SPEC-017's later contributor contract; SPEC-007 owns its identity side.

## SPEC-004 architecture baseline

- Accepted completion commit:
  `6b087936c0d8b6341c656ade698247561caa3996`.
- Accepted versions: module-boundary schema `1.0`, architecture-decision
  schema `1.0`, persistence manifest `2.1.0`, and approval boundary
  `Gate-A-2026-07-13`.
- Consumed boundary: the exact nine-project modular monolith, composition-only
  API, IdentityAccess `Application.Ports`/`Endpoints` surface, one
  Infrastructure.SqlServer-owned DbContext, module-owned EF contributions,
  and no generic repository or additional deployment component.
- Shared audit boundary: identity mutations use the upstream
  `IAuditEventWriter.AppendAsync(AuditEventDraft, CancellationToken)` port
  inside the caller's active EF transaction; write failure rolls back the
  business change. SPEC-017 owns later read/export experiences, not a second
  identity writer.
- Shared-key boundary: replicas use one SQL-backed Data Protection key ring
  protected by an external local certificate. Missing or unapproved production
  repository/encryption authority fails closed.

## SPEC-005 identity-persistence baseline

- Accepted design-contract commit:
  `d88892f97e2164fc3ee60b530187c1f4a658ceb4`. This is not a runtime migration,
  restoration, performance, or release-evidence completion claim.
- Accepted versions: persistence manifest `2.1.0`, unique invariants `1.0`,
  check constraints `1.0`, concurrency tokens `1.0`, immutable history `1.0`,
  and relational invariants `1.0`.
- Consumed boundary: SPEC-007 owns ApplicationUser, Staff, StudentActivation,
  AccountRecoveryChallenge, AuthenticationAbuseState, IdentityImportBatch,
  IdentityImportCandidateRow,
  RoleAssignment, SecurityEvent, and AdminSecurityGuard. StudentActivation
  references the Identity-owned ApplicationUser, never SPEC-008's Student.
  University ID is filtered-unique, credentials are hash-only, and activation,
  recovery, role, guard, and mutable identity state use database-enforced
  uniqueness, conditional transitions, or rowversion as declared.
- Additive owner reconciliation: approved contract
  `identity-persistence-owner-extension/1.1` and the canonical ERD add the
  identity security/lockout fields, recovery attempts/delivery reference,
  assigning actor, operation-scoped abuse HMAC, import idempotency/result
  fields, and singleton guard precision required by SPEC-007. These additions
  preserve the accepted SPEC-005 design invariants and do not create a
  migration or second owner.
- Current manifest reconciliation: persistence manifest `2.1.1` adds the
  batch-owned `IdentityImportCandidateRow` to SPEC-007's writable mapping
  contribution without changing the accepted migration owner or sequence.
- Data lifecycle: Development and Testing are synthetic-only. Development
  persists until guarded reset; Testing is isolated per run. Plaintext
  credentials, proofs, profiles, and secrets are excluded from SQL,
  migrations, source, logs, telemetry, snapshots, and evidence.
- Migration boundary: SPEC-007 contributes
  `IdentityAccessModelConfiguration`; SPEC-008 exclusively owns initial
  migration `S1IdentityAcademicFoundation` after both owner mappings exist.

## SPEC-006 shared-contract baseline

- Accepted shared-foundation commit:
  `df6774ce57157b05b4af21380ce8afd2412ce422`.
- Accepted versions: ApiError schema `1.0`, Page schema `1.0`, specification
  manifest `2.0.2`, and workstream manifest `2.0.1`. SPEC-007 reconciles the
  current specification and entity-ownership manifests to `2.0.3` by adding
  the canonical `IdentityImportCandidateRow -> 007` declaration while
  retaining `StudentActivation -> 007`; the current workstream manifest is
  `2.0.2` for the approved activation/recovery/security refinements. Earlier
  immutable dependency pins remain unchanged.
- Consumed boundary: privacy-safe `ApiError`, DTO/credential isolation,
  deterministic JSON, default page size `20` and maximum `100`, deterministic
  sorting, request-body concurrency, owner-scoped idempotency, injected
  TimeProvider, and complete AppContext composition. SPEC-007 refines the DTO
  rule only for the eight explicitly tested transient authentication and
  account-lifecycle request members; responses, raw persistence, logs, audit
  payloads, and diagnostics remain secret-free.
- Context boundary: SPEC-007 supplies authenticated identity/session/authorized
  roles; SPEC-008 supplies authoritative time/term/window/service and owns the
  public/authenticated context handlers. Missing contributors return
  `CONTEXT_UNAVAILABLE`; the browser never fills partial context.
- Deferred-runtime boundary: generated OpenAPI, real endpoint proof, exact
  downstream context pins, and release evidence are later tasks and are not
  inherited through this baseline.

## SPEC-018 security and operations baseline

- Accepted implementation-foundation commit:
  `4ac311825bf2c82ec382e488db1a4de145650c79`.
- Accepted boundary: safe health/metrics contracts, correlation and redaction,
  fail-closed security configuration, SQL-backed shared Data Protection keys,
  generated local certificate outside Git, environment/User-Secrets inputs,
  synthetic non-production database lifecycle, and repository-bounded
  seven-day cleanup.
- Identity state remains shared and durable: security stamps, sessions, roles,
  challenges, and abuse state cannot depend on sticky sessions or process
  memory. ASP.NET Identity salted hashes are verified rather than compared as
  deterministic bytes.
- Load/release boundary: the 10-minute target, spike, authenticated-session,
  cross-replica, accessibility, recovery, and release evidence that depend on
  SPEC-007 runtime remain later fail-closed tasks. This baseline does not claim
  those measurements passed.

## Reconciled SPEC-007 decisions

- AUTH-03 sends `universityId`, `initialPassword`, and `newPassword`; password
  confirmation stays in the Blazor form. Verification, password-hash
  replacement, and activation consumption occur in one transaction.
- Recovery proof delivery crosses the narrow Identity-owned
  `IAccountRecoveryProofDelivery` port. The request response is always the same
  202 envelope and never includes the proof. Testing injects an in-memory
  adapter; Development may use a Git-ignored local adapter subject to the
  seven-day cleanup rule. Production fails closed until an approved
  institutional adapter is configured.
- The dependency graph `003/004/005/006/018 -> 007` is acyclic. No dependency
  grants production deployment, official AASTMT authority, initial migration
  ownership, downstream route completion, or Gate B-D/release approval.

## 2026-07-14 downstream permission amendment

- SPEC-001's governed tokens are consumed as exact claim values. Identity owns
  the effective-role allow-list and cookie claim issuance for
  `IdentityAccess.Manage`, `Context.Read`, `AcademicProfile.ReadOwn`,
  `AcademicTerms.Manage`, and `AcademicProfiles.Manage`.
- Independent policies require the exact role and permission claim. Selecting
  a role context replaces claims from that effective role and never unions
  privileges from other available roles.
- This additive demo amendment does not authorize production grant sources,
  institutional identity integration, Gate B-D, or official AASTMT go-live.
