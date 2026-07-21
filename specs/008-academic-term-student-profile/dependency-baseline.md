# SPEC-008 Dependency Baseline

**Recorded:** 2026-07-14<br>
**Feature:** SPEC-008 Academic Term and Student Profile<br>
**Result:** PASS FOR T001-T005 DEPENDENCY BASELINING; T006 MUST VERIFY THE
RECONCILED OWNER EXTENSIONS BELOW BEFORE T008 STARTS

This baseline freezes the approved upstream contracts consumed by SPEC-008.
It authorizes dependency-ordered work for the non-production demo only. It
does not grant Gate B-D, production, official AASTMT, route-release, or
downstream-feature approval.

## T001 - SPEC-002 term and policy baseline

- Accepted completion commit:
  `bf3e2e34f0134428b199b6d2de12114c10cd656e`.
- Accepted inputs are `requirements.md`, `policy-rules.md`,
  `policy-sources.md`, `data-model.md`, and the contracts under
  `specs/002-aastmt-policy-rulebook/`.
- Accepted artifacts are `policy-rulebook/1.0` and the published
  `DEMO-POC-2026.1` profile, effective from `2026-07-13T00:00:00Z`, with
  priority 100 inside its explicitly configured synthetic College of
  Artificial Intelligence demo scope.
- Server time, the explicitly configured AcademicTerm and registration
  window, authenticated identity, and authorization scope are authoritative.
  The browser never derives the current term or opens a registration window.
- A policy window matches only when authoritative server time is within its
  configured `OpensUtc` and `ClosesUtc`. An absent or unknown environment,
  term, window, standing, provenance, or policy match fails closed; the demo
  rulebook is never a global fallback.
- SPEC-008 supplies sourced term, standing, and hold facts. `Active` is the
  allowed demo standing, an unknown standing fails closed, and an active hold
  with `blocksRegistration = true` blocks registration. Hold codes remain
  data and are not guessed into a financial or academic taxonomy.
- The stable relevant policy reasons include
  `REGISTRATION_WINDOW_CLOSED`, `ACADEMIC_STANDING_UNAVAILABLE`, and
  `REGISTRATION_HOLD`. Policy decisions retain version, effective period,
  source/access metadata, approval actor, explanation, and a privacy-safe
  input summary.
- Published policy versions are immutable and are superseded rather than
  edited. Public AASTMT sources establish provenance only; Ahmed Elbamby's
  approval authorizes this bounded demo profile, not production policy.
- SPEC-009 remains the runtime PolicySet/PolicyRule and evaluation owner.
  SPEC-008 owns authoritative academic inputs and MUST NOT implement a second
  policy evaluator.

## T002 - SPEC-003 page and state baseline

- Accepted design-governance commit:
  `8ee8f724af3b60bbbc464532bc68de6f173247e5`.
- Accepted versions are page-design governance `1.1`, route manifest `2.1.0`,
  page/API manifest `1.1.0`, component manifest `1.1.0`, responsive-layout
  contract `1.0`, authenticated-shell contract `1.0`, and immutable Page
  Design Records version `1.0`.
- The exact consumed records are:

| Route | Page/API boundary | Required states | SPEC-008 boundary |
|---|---|---|---|
| AUTH-01 `/` | `RoleGatewayPage.razor`; `GET /api/public/context` | loading, success, service-error, stale, offline; empty, validation-error, unauthorized, and session-expired are not applicable | Render only privacy-safe server time/timezone, public teaching and registration term labels, window/service state, named Student/Staff destinations, retry, and safe support. |
| STU-01 `/student` | `StudentDashboardPage.razor`; `GET /api/context`, `GET /api/students/me/academic-context`, and the downstream current-timetable read | loading, empty, success, validation-error, service-error, unauthorized, session-expired, stale, offline | Render authoritative context/profile data; place every hold or incomplete-profile blocker before and programmatically associate it with a disabled Start/Resume action. |
| ADM-02 `/admin/terms` | `TermAdministrationPage.razor`; bounded term list/create/update/window-publish APIs | loading, empty, success, validation-error, service-error, unauthorized, session-expired, stale, offline | Provide the versioned editor, validation summary, publish confirmation, server-accepted result, and refresh/review behavior after a concurrent change. |
| ADM-04 `/admin/students` | `StudentAdministrationPage.razor`; bounded Student search/detail/profile-correction APIs | loading, empty, success, validation-error, service-error, unauthorized, session-expired, stale, offline | Show only authorized sourced academic detail, provenance and versions; require a reasoned correction and disclose no protected data in denied states. |

- All records retain the same semantic/DOM/focus order at 320, 375, 768,
  1024, 1280, and 1920 CSS pixels. Targets are at least 44 CSS pixels,
  reasons do not truncate, state changes use the specified live regions, and
  viewport changes never alter authorization or outcome meaning.
- These records were accepted in `design-only` state. Their SPEC-008
  contributor version was `not-pinned`; STU-01 also has a future SPEC-015
  contribution and ADM-02/ADM-04 have future SPEC-017 contributions.
  SPEC-008 implements and tests only its owned contribution now. It does not
  fabricate the SPEC-015 timetable or SPEC-017 reporting/operations result,
  create a cyclic dependency, or claim final browser, visual, accessibility,
  or route-release evidence. An unavailable downstream region uses the
  approved explicit safe state and action; it never presents cached or sample
  data as a live success.

## T003 - SPEC-005 persistence and SPEC-006 contract baseline

### Persistence and data lifecycle

- Accepted SPEC-005 design-contract commit:
  `d88892f97e2164fc3ee60b530187c1f4a658ceb4`. This is a design/invariant pin,
  not runtime migration or release evidence.
- Accepted contracts are `unique-invariants/1.0`,
  `check-constraints/1.0`, `concurrency-tokens/1.0`,
  `immutable-history/1.0`, and the entity references under
  `specs/005-erd-data-lifecycle/contracts/`, together with
  `docs/diagrams/ERD.md`.
- Current persistence manifest `2.1.1` assigns SPEC-008 the writable
  `AcademicContextModelConfiguration` contribution for `AcademicTerm`,
  `RegistrationWindow`, `Student`, `StudentTermAcademicState`,
  `TranscriptAttempt`, and `StudentHold`. SPEC-008 owns initial migration
  `S1IdentityAcademicFoundation` after the SPEC-004, SPEC-007, and SPEC-008
  model contributions exist; Infrastructure.SqlServer remains the sole
  DbContext and migration composer.
- `AcademicTerm.Code`, `Student.ApplicationUserId`, and
  `(StudentId, TermId)` on `StudentTermAcademicState` are unique. Teaching and
  registration-window ends must be after their starts. Mutable term, window,
  Student, and student-term state use SQL Server rowversion.
- An authorized stale mutation returns `409 STALE_VERSION`, may include only
  the safe current version, and performs no lost update. Authorization runs
  before existence/version disclosure, so an unauthorized caller receives
  `403` without resource or rowversion data.
- Transcript attempts retain historical meaning and are never overwritten.
  Imported and seeded academic facts retain source provenance.
- Development and per-run Testing use SQL Server 2022 Developer compatibility
  level 160. Migrations complete before the synthetic seed. Testing is
  isolated and disposed per run; Development persists until an explicit
  environment-guarded reset. The same seed version and stable fixture ordinal
  reproduce the same logical identity/profile graph. Seed data is not embedded
  in a migration, and production or real student data is never loaded.
- All instants are UTC `datetime2`; recurring class times remain local
  `DayOfWeek`/`TimeOnly` values interpreted with the term's IANA timezone.
  Server time, not a database/client default or browser clock, supplies audit
  and decision timestamps.

### Shared response, pagination, and error contracts

- Accepted SPEC-006 shared-foundation commit:
  `df6774ce57157b05b4af21380ce8afd2412ce422`; the approved current contract
  refinements through SPEC-007 completion commit
  `99f1db3358ec66c948d8f404b28891784880cfc1` preserve this boundary.
- Canonical runtime sources are
  `src/StudentRegistration.Contracts/TermSummaryDto.cs`,
  `AppContextDto.cs`, `PublicContextDto.cs`, `Page.cs`, and `ApiError.cs`.
  EF entities and persistence/security internals never cross the HTTP
  boundary.
- `TermSummaryDto` remains exactly `{ id, code, label, state, rowVersion }`.
  Its states are `draft`, `registrationOpen`, `registrationClosed`,
  `teaching`, `completed`, and `archived`.
- `PublicContextDto` remains the exact six-field public response: UTC server
  time, IANA timezone ID, nullable public teaching/registration term labels,
  computed window state, and service state. It contains no identity, role,
  session, window identifier/version, capacity, profile, or topology data.
- `AppContextDto` remains the single authenticated context response. SPEC-008
  owns an approved additive shared-contract extension that adds one nullable
  canonical `RegistrationWindowSummaryDto` containing `id`, computed `state`,
  `opensAtUtc`, `closesAtUtc`, and `rowVersion`. Service state remains the
  canonical `available`/`maintenance`/`unavailable` vocabulary. No second
  `AcademicAppContextDto` response is created. The summary is absent only with
  authoritative `registrationWindowState = none`; a missing contributor is
  instead `503 CONTEXT_UNAVAILABLE` with no partial success body. SPEC-006
  retains shared-schema ownership, SPEC-007 supplies identity/session fields,
  and SPEC-008 supplies and composes the academic fields.
- Lists use `Page<T>`, default request page 1 and size 20, maximum size 100,
  and an endpoint-declared allow-listed canonical sort ending with the unique
  ID. Invalid pagination returns `400 PAGE_SIZE_INVALID`; servers never
  silently cap it.
- Errors remain the canonical `ApiError` containing code, safe message,
  correlation ID, optional field errors, and optional current version.
  `WINDOW_OVERLAP` uses no second error type and returns no conflicting-window
  collection or version map; the authorized client receives the stable code
  and refetches the bounded term aggregate before review/retry. A directly
  contested stale aggregate may use `currentVersion` only for that aggregate.
- Versioned mutations keep concurrency metadata in the request body. A
  multi-window term command may use required
  `expectedTermRowVersion` plus a bounded map of required expected window
  versions because it atomically contests multiple aggregates; this is the
  feature-owned explicit refinement of the single-aggregate
  `expectedRowVersion` protocol, not an `If-Match`/412 protocol.

## T004 - SPEC-007 session and RBAC baseline

- Accepted completion commit:
  `99f1db3358ec66c948d8f404b28891784880cfc1`.
- Accepted inputs are `requirements.md`, `contracts/api.md`, `data-model.md`,
  and the runtime contracts/policies in
  `src/StudentRegistration.Contracts/Identity/AuthenticationContracts.cs`
  and
  `src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs`.
- SPEC-007 supplies the session fields `displayName`, server-derived effective
  roles, `activeRole`, `sessionState`, and `expiresAtUtc`. The only role tokens
  are `Student`, `Admin`, `Lecturer`, and `TeachingAssistant`.
- `activeRole` equals the single effective server role for an active or
  expiring session. Zero or multiple roles fail closed as invalid account
  configuration; no alternate-role mutation exists. A Student remains in the self-scoped Student
  context.
- `GET /api/context` requires `Context.Read`. Student profile reads require
  `AcademicProfile.ReadOwn` plus a resource check that compares the
  authenticated ApplicationUser identifier to `Student.ApplicationUserId`.
  Term/window Admin APIs require `AcademicTerms.Manage`. Named Student academic
  Admin reads/corrections require the approved
  `AcademicProfiles.Manage` permission and institutional-admin scope. These
  named claims/policies are added to the SPEC-001/SPEC-007 governed vocabulary
  before the endpoints activate; `Admin` role membership alone is never an
  implicit superuser grant.
- Protected mutations retain the same-origin Secure/HttpOnly/SameSite cookie
  and antiforgery contract. Shared SQL role, security-stamp, session, and abuse
  state is visible to every replica; no sticky session or process-memory state
  is a correctness requirement.
- Every protected SPEC-008 endpoint receives positive role/permission/scope
  coverage and negative unauthenticated, wrong-role, missing-permission, and
  direct-object coverage. Denial occurs before resource existence, profile,
  or version disclosure.

## T005 - SPEC-018 quality, security, replica, and operations baseline

- Accepted implementation-foundation commit:
  `4ac311825bf2c82ec382e488db1a4de145650c79`.
- Accepted inputs are `requirements.md`, `contracts/api.md`, `data-model.md`,
  `docs/operations/telemetry-contract.md`, `docs/security/THREAT_MODEL.md`, and
  the SPEC-018 release-evidence schemas/results.
- CI runs restore, format verification, warnings-as-errors build, unit and
  architecture tests, real-SQL integration/migration tests, E2E/accessibility,
  and security checks in the governed order. A required failure blocks the
  corresponding merge/release gate.
- The validation fixture supports 25,000 wholly synthetic accounts and 5,000
  concurrent authenticated sessions. The full blocking SPEC-018 target remains
  10 minutes at 75 registration submissions/s plus 300 mixed reads/s, followed
  by a 60-second 200-submissions/s spike, across at least two stateless API
  replicas. Optional 2x, 5x, and soak diagnostics never replace those runs or
  relax correctness.
- Both replicas use the shared SQL database and one SQL-backed Data Protection
  key repository protected by the approved generated local certificate
  outside Git. Cross-replica context/session behavior must work without sticky
  sessions; one-replica failover must leave the other replica functional.
- SPEC-008 emits only allow-listed aggregate request latency/throughput,
  unexpected-error, business-rejection, SQL-duration, lock-wait, and deadlock
  signals with server correlation. Logs, metrics, traces, snapshots, and
  evidence contain no credential, raw request/response, query string,
  identifier, full student profile, connection value, or topology.
- SQL unavailability returns a safe no-partial result with a correlation
  reference, changes health to unhealthy, and commits no mutation. Exporter
  failure uses the bounded fallback without taking down the business flow.
- Every protected resource requires negative role/ownership testing. A stale
  threat model, unresolved Critical/High finding, sensitive-data leak,
  invariant failure, or missing authorization, cross-replica, accessibility,
  load, or recovery evidence remains release-blocking.
- SPEC-008 NFR-2 is a distinct feature measurement: execute 300 authenticated
  `GET /api/context` requests/second for 10 minutes across two
  independently addressable replicas and the 25,000-account fixture; require
  p95 at or below 300 ms and unexpected server failures below 0.1%. Record
  duration, achieved rate, replica count, p95, failures, and configuration.
  This evidence proves only the SPEC-008 context-read target. It does not
  replace or claim the later SPEC-018 mixed-read/submission target, spike,
  failover, or production release gate, all of which remain pending until
  their downstream runtime activation conditions exist.

## Reconciled SPEC-008 owner decisions

- SPEC-008-specific browser/API DTOs are logically Academics-owned but have one
  physical dependency-neutral definition under
  `src/StudentRegistration.Contracts/Academics/`. This follows the established
  SPEC-007 shared-contract pattern and preserves the approved Client-to-
  Contracts dependency without a Client-to-Academics reference or duplicate
  browser models.

- The canonical persisted AcademicTerm lifecycle is the SPEC-006 vocabulary:
  Draft, RegistrationOpen, RegistrationClosed, Teaching, Completed, and
  Archived. The canonical persisted RegistrationWindow lifecycle is Draft,
  Published, EmergencyClosed, and Superseded. Upcoming/Open/Closed/None is
  computed from that lifecycle plus authoritative server time and is never a
  browser-written or separately persisted lifecycle value. This owner
  refinement supersedes the older design-only ERD wording
  Draft/Open/Closed/Cancelled; the ERD and executable mapping must be
  reconciled before T008/T009 can pass.
- `TranscriptAttempt` and `StudentHold` are canonical SPEC-008 entities. Their
  entries are reconciled in entity-ownership manifest `2.0.5` with persistence
  manifest `2.1.1` before model delivery.
- The SPEC-008 persistence contribution is an additive owner refinement of the
  design ERD: it records the already-approved cohort/active/profile
  provenance, term-owned hold message/source, append-only transcript
  supersession reference, and normalized window scope required by SPEC-008.
  It introduces no new demographic/contact field and no foreign key to
  downstream Program/Course runtime entities; program/course references remain
  stable sourced codes until their owner contracts activate.
- The shared AppContext is extended once and remains the only authenticated
  context DTO. Public context remains minimal. Browsers never compose a
  partial context or infer a window/version.
- `AcademicTerms.Manage` protects term/window administration and the newly
  governed `AcademicProfiles.Manage` protects named Student academic reads and
  corrections. No endpoint relies on Admin role alone.
- Admin lists, transcript attempts, and provenance use the canonical default
  20/maximum 100 `Page<T>` contract and deterministic unique-ID tie-break
  sorts. Every active hold is returned together, with a hard maximum of 100;
  a profile above that limit fails closed as `PROFILE_NOT_READY` rather than
  returning a partial hold set. Transcript correction appends an immutable
  superseding attempt instead of overwriting history. Multi-aggregate version
  inputs and correction operations are explicitly bounded and tested, while
  overlap/stale errors require a bounded aggregate refetch rather than an
  unbounded diagnostic payload.
- SPEC-008 delivers only its route regions and safe downstream-unavailable
  states. SPEC-015 and SPEC-017 remain later contributors, not reverse
  dependencies and not fabricated successes.
- The SPEC-008 300-context-read/s measurement is independent evidence and does
  not redefine the later SPEC-018 workload.

The dependency graph `002/003/005/006/007/018 -> 008` remains acyclic. T006
must verify that the shared-contract, authorization-vocabulary, ERD, ownership,
endpoint, route, and task artifacts reflect these decisions before any T008 or
later test or implementation work starts.
