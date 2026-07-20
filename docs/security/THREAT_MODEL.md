# STRIDE Threat Model

**Artifact version:** 1.0.0

**Status:** Reviewed

**Scope:** Non-production Student Registration System demo

**Owner:** Ahmed ELbamby

**Reviewer:** Ahmed ELbamby

**Reviewed on:** 2026-07-14
**Next review due:** 2026-10-14
**Production authority:** Not granted

## Purpose and scope

This model governs the modular-monolith demo, its Blazor WebAssembly browser
client, ASP.NET Core API replicas, SQL Server state, local POC secret inputs,
operational telemetry, and release evidence. It applies STRIDE to the
server-authoritative identity, registration, administration, and operations
boundaries. It does not authorize production use or claim an approved
production secret provider, certificate custodian, hosting topology, or AASTMT
go-live.

## Assets

- Account identity and session state, role assignments, security stamps, and
  authorization and data scope.
- Student academic context, registration plans, active enrollments, group
  capacity, and the atomicity and idempotency invariants around them.
- Protected option tokens, the shared Data Protection key ring, and its
  generated external local certificate.
- Admin commands, append-only audit facts, bounded exports, and release
  evidence.
- SQL persistence, migrations, synthetic fixtures, backups, and recovery
  evidence.
- Privacy-safe telemetry, correlation identifiers, configuration, and secrets.

## Trust boundaries

| Boundary | Data crossing the boundary | Required control |
|---|---|---|
| Browser to API | Credentials, antiforgery values, commands, queries, and protected option tokens | TLS, secure same-origin cookie, antiforgery, validation, rate limits, and server-derived authorization and data scope |
| API to SQL Server | Identity state, academic data, registration transactions, audit facts, and shared keys | Least-privilege connection, EF parameterization, constraints, transactions, concurrency controls, and encrypted transport/storage |
| API replicas to shared key ring | Data Protection key reads/writes used by sessions and tokens | One application name, SQL-backed repository, generated external certificate, startup failure when key inputs are unavailable, and no sticky-session dependency |
| API to telemetry and exports | Allow-listed aggregate signals, safe references, bounded reports, and evidence | Redaction, controlled cardinality, access control, retention, and no credentials or full profiles |
| Deployment operator to runtime configuration | Connection inputs, external certificate path/password, environment, and release configuration | User Secrets or environment variables for POC secrets, certificate outside Git, reviewed deployment inputs, and fail-closed production authority |

## Threat register

| ID | Boundary or asset | STRIDE | Threat | Mitigation | Validation evidence | Residual risk | Owner | Review state |
|---|---|---|---|---|---|---|---|---|
| TM-01 | Browser to API; identity and session | Spoofing | Credential guessing, account enumeration, or stolen session impersonates a student or staff user | Generic failures, ASP.NET Identity hashing, lockout and rate limiting, Secure HttpOnly SameSite cookie, antiforgery, and shared security-stamp state | SPEC-007 negative authentication, abuse, cookie, antiforgery, and replica invalidation tests when its runtime slice is delivered | Medium until SPEC-007 executable identity and two-replica evidence exists; release blocked | Ahmed ELbamby | Reviewed |
| TM-02 | API authorization and data scope | Elevation of privilege | A client-supplied role or identifier exposes another user’s data or grants an Admin, Lecturer, or Teaching Assistant action | Server-derived roles, endpoint policy checks, resource ownership checks, bounded DTOs, and no role selector | SPEC-001 RBAC matrix plus future SPEC-007, SPEC-016, and SPEC-017 positive and negative authorization suites | Medium until all protected endpoints have executable matrix evidence; release blocked | Ahmed ELbamby | Reviewed |
| TM-03 | API replicas to shared key ring; protected option tokens | Tampering | A token is altered, replayed, or cannot be read after a request moves to another replica | Shared SQL Data Protection keys, one application name, external certificate protection, expiry, purpose isolation, and server revalidation | SPEC-004 DataProtection registration tests; SPEC-013 option-token and SPEC-007 cross-replica session tests activate when those runtimes exist | Medium until downstream cross-replica token and session tests execute; no production-provider claim | Ahmed ELbamby | Reviewed |
| TM-04 | API to SQL Server; registration races | Tampering | Concurrent submissions overbook a group, duplicate an active enrollment, or partially commit a plan | One server-authoritative transaction, unique and check constraints, idempotency, optimistic/database concurrency, and final revalidation | SPEC-014 real-SQL contention and invariant tests plus target, spike, and failover evidence | Low for the non-production demo after the mandatory runtime, contention, load, failover, and invariant evidence passed; production remains unauthorized | Ahmed ELbamby | Reviewed |
| TM-05 | Admin, audit, and export | Repudiation | A privileged mutation is denied later, an audit trail is changed, or an export exceeds the actor’s scope | Explicit permission, reason, correlation, server time, atomic append-only audit, version checks, bounded scoped export, and no override path | SPEC-004 atomic audit tests and future SPEC-017 authorization, concurrency, audit, and export tests | Medium until SPEC-017 executable evidence exists; release blocked for unresolved High findings | Ahmed ELbamby | Reviewed |
| TM-06 | SQL persistence | Information disclosure | Injection, excessive query scope, backup exposure, or a plaintext credential/full profile leaks durable data | EF/LINQ parameterization, narrow repositories, least privilege, encryption controls, hash-only credentials, synthetic-only POC data, and protected backup handling | Architecture dependency tests, migration/constraint tests, credential leakage scans, and recovery rehearsal | Medium; production grants, encryption authority, backup custody, and retention remain undecided and fail closed | Ahmed ELbamby | Reviewed |
| TM-07 | API to telemetry and exports; telemetry | Information disclosure | Logs, metrics, traces, health output, or evidence expose credentials, student profiles, topology, or unbounded user input | Allow-listed fields and dimensions, redaction, aggregate metrics, safe correlation, bounded fallback, restricted operations access, and seven-day local cleanup | SPEC-018 observability, health, cardinality, authorization, redaction, and artifact-retention tests | Medium until the observability runtime and evidence pipeline execute; release blocked on leakage | Ahmed ELbamby | Reviewed |
| TM-08 | Deployment configuration; secrets | Information disclosure | A connection secret or certificate password is committed, logged, or stored beside the certificate | POC secrets only from .NET User Secrets or environment variables, generated certificate outside Git, ephemeral certificate loading, Git-ignore, and no credential logging path | SPEC-004 Data Protection tests and SPEC-018 tracked-file, configuration, and secret-input tests | Low for the local demo after automated scans; production provider and custody remain unresolved | Ahmed ELbamby | Reviewed |
| TM-09 | Deployment operator to runtime configuration; deployment | Elevation of privilege | A wrong environment, unapproved key authority, unsafe migration, or missing dependency starts with production-like authority | Explicit environment/configuration validation, migration-before-traffic, health gating, immutable evidence, and fail-closed production repository/encryption approval | Architecture production-authority tests, CI order checks, migration tests, recovery rehearsal, and release manifest | Medium because final hosting, secret provider, and certificate custody are outside POC scope | Ahmed ELbamby | Reviewed |
| TM-10 | Browser to API and API to SQL Server | Denial of service | Login abuse, expensive search/optimizer input, or registration spikes exhaust API or SQL capacity | Bounded payloads/pages, rate limits, query indexes, optimizer budgets, connection limits, back pressure, health signals, and two stateless replicas | Boundary tests and mandatory 10-minute target plus 200-per-second spike across two replicas | Medium until measured load and failover evidence passes; correctness may never be relaxed | Ahmed ELbamby | Reviewed |
| TM-11 | Audit facts and release evidence | Tampering | Evidence or audit history is edited to hide a failed gate or privileged operation | Append-only runtime audit, immutable versioned evidence after sign-off, superseding corrections with rationale, hashes/version pins, and review ownership | SPEC-004 audit transaction tests and SPEC-018 evidence-schema, freshness, and release-gate tests | Medium until the release-evidence pipeline and signed Gate B-D records exist | Ahmed ELbamby | Reviewed |
| TM-12 | Browser assets and API errors | Information disclosure | Blazor state, local storage, validation errors, or health details reveal protected state, long-lived tokens, or topology | No SQL access from client, no long-lived browser token storage, privacy-safe ApiError, public safe health summary, and server revalidation | Architecture boundary tests, API contract tests, and future browser security/E2E tests | Medium until identity pages and executable browser tests exist; no production claim | Ahmed ELbamby | Reviewed |

Residual Medium or High entries above describe bounded work that has not yet
received its downstream executable evidence. They are not accepted production
risks. A release cannot treat a planned or skipped test as passing evidence.

## Release blockers

The release gate fails when any of these conditions is present:

- an unreviewed or stale threat model;
- an unresolved Critical or High security finding;
- a capacity, duplicate-enrollment, or atomicity invariant failure;
- a Critical or Major core-usability defect;
- missing negative authorization, credential-leakage, cross-replica,
  accessibility, load, or recovery evidence required for the release; or
- a configuration or document that implies production security authority,
  hosting approval, or an approved production secret provider.

Skipped downstream checks remain visible and blocking until their owning
feature runtime exists and the activation condition is satisfied.

## Review and change control

Ahmed ELbamby reviews this artifact as the sole developer and demo approval
authority. Review is required by the next-review date and immediately after a
trust-boundary, authentication, authorization, registration transaction,
secret/key, topology, telemetry, export, or deployment change. A correction
creates a new semantic artifact version with its rationale and cannot erase
earlier release evidence. Gate A approval covers only non-production demo
implementation; Gate B-D, release, production, and official AASTMT approvals
remain separate.
