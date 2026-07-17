# SPEC-017 Implementation Readiness

**Reviewed:** 2026-07-17  
**Repository baseline:** `6bf65e9c230443d6fa365ec0a0168d8a7ef5766c`  
**Normative approval baseline:** `025479c100b83e726c777b2311015470481a7515`  
**Gate A approval:** Ahmed Elbamby, 2026-07-13; revalidated 2026-07-17  
**Result:** SCOPED PASS FOR DEPENDENCY-ORDERED NON-PRODUCTION DEMO WORK

- [x] Constitution 1.1.0 compliance is recorded in `approval.md` with no
  exception and preserves the simple modular monolith.
- [x] SPEC-003, SPEC-004, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-014,
  SPEC-015, SPEC-016, and SPEC-018 statuses, exact artifact hashes, ownership,
  consumed contracts, and deferred boundaries are recorded in
  `dependency-baseline.md`.
- [x] The direct dependency graph
  `003/004/007/008/009/010/014/015/016/018 -> 017` is valid and acyclic.
- [x] StaffAdministration keeps its current module references. Cross-module
  reads use narrow application ports implemented by Infrastructure.SqlServer;
  feature-owner commands are called directly without a generic facade.
- [x] SPEC-004 remains the sole audit transaction writer and shared DbContext
  owner; SPEC-017 owns only ExportJob mapping and read/query/export behavior.
- [x] SPEC-007 remains the sole SecurityEvent, RoleAssignment, and
  AdminSecurityGuard writer. Every Admin-role change preserves the Identity
  command and FINAL_ADMIN_REQUIRED result.
- [x] SPEC-010 remains the sole StaffTermAvailability and ScheduleImpactAlert
  writer. Admin view/import is bounded and read-only with no correction path.
- [x] Registration repair, enrollment correction, drop, withdrawal, seat
  decrement, break-glass override, super-admin, warehouse, report replica,
  broker, and duplicate writer remain excluded.
- [x] Every FR-1 through FR-13, NFR-1 through NFR-4, AC-1 through AC-9, EC-1
  through EC-6, OS-1 through OS-4, entity, endpoint, workstream, and ADM route
  has an evidence-producing task. Tasks remain contiguous T001-T101.
- [x] The task plan now distinguishes red-first delivery tests from upstream
  conformance checks, restores the complete availability boundary, names
  runtime composition artifacts, and makes release-document ordering explicit.
- [x] SPEC-009/010 documentation-lint exceptions are pinned, bounded to this
  demo, owned by Ahmed Elbamby, and expire before T100; they do not relax
  runtime behavior or permit a global/release PASS claim.
- [x] Pending SPEC-003 route evidence and SPEC-018 Gate-D/load/recovery work are
  not claimed as complete. SPEC-017 owns only its named downstream evidence.

## Demo-only NFR evidence profile

Ahmed Elbamby approves this bounded profile for SPEC-017 demo evidence only.
It is not an institutional retention rule, production size, or AASTMT SLA.

| Evidence | Approved demo profile |
|---|---|
| Database | SQL Server 2022 Developer, compatibility level 160, isolated `StudentRegistration_Test_{runId}` database |
| Audit search | 100,000 synthetic, allow-listed AuditEvent/SecurityEvent rows; page 1 size 20; five warm-ups plus 30 measured queries; p95 <= 1,000 ms |
| Metrics freshness | all required metric names present; observed timestamp age <= 60 seconds; unavailable source reports stale/degraded rather than zero |
| Export | 25,000 synthetic scoped rows; bounded streaming batches of at most 1,000 rows; two application replicas/workers; exactly one artifact |
| Lease/retry | 60-second renewable SQL lease; at most three attempts; process-loss reclaim only after expiry |
| Artifact retention | 15 minutes in the isolated demo test; authorized download before expiry and 410/inaccessible after expiry |
| Environment record | commit, OS/runtime, database image/version, replica count, fixture seed, exact command, raw samples, and threshold calculation stored with each T092-T095 report |

Unknown institutional audit/export retention continues to fail closed. The
15-minute value exists only to make secure expiry measurable in isolated tests.

## Automated validation evidence

- `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks
  -IncludeTasks` resolved the absolute SPEC-017 feature directory and found
  research, model, contracts, quickstart, and tasks.
- All SPEC-017 readiness checklists had zero incomplete planning items before
  implementation.
- `.specify/scripts/powershell/Test-AllSpecs.ps1 -Phase Implementation`
  reported SPEC-017 with requirements score 100, automated gates PASS, and
  human approval APPROVED.
- The repository-wide result remains FAIL for disclosed findings outside
  SPEC-017, including SPEC-009/010 declaration lint and unrelated specs. This
  record claims only a scoped SPEC-017 PASS and preserves those findings.
- `.gitignore` and `.dockerignore` already contain the required .NET, Docker,
  secrets, logs, test-result, export, and generated-artifact exclusions. No
  additional ignore technology is present.

## Gate A currency (T012)

The original approved normative SPEC-017 files changed together in commit
`025479c100b83e726c777b2311015470481a7515` on 2026-07-13. During Phase 2,
`requirements.md` and `contracts/api.md` were synchronized to make the existing
endpoint DTO and response details testable. The recorded Phase 2 clarification
review in `approval.md` confirms no endpoint, actor, mutation, ownership, or
scope expansion and records Ahmed Elbamby's 2026-07-17 demo reapproval. The
clarified files are pinned at
`7f29e7e836dd3f5970395531830a5e74a53eb15e`; Gate A therefore remains current
for this baseline.

Any accepted change to those normative files, dependency hashes, ownership,
routes, permissions, persistence, concurrency, privacy, or scope returns the
affected work to In Review. Gate B-D, production deployment, and official
AASTMT authorization remain separate.
