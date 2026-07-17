# SPEC-016 Implementation Readiness

**Reviewed:** 2026-07-17
**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED NON-PRODUCTION DEMO WORK
**Gate A approval:** Ahmed Elbamby, 2026-07-13; revalidated 2026-07-17
**Result:** PASS

- [x] Constitution compliance is recorded against version 1.1.0 and preserves
  the simple modular monolith without exception.
- [x] SPEC-003, SPEC-007, SPEC-010, SPEC-015, and SPEC-018 accepted statuses,
  exact artifact hashes, consumed contracts, and deferred boundaries are
  recorded in `dependency-baseline.md`.
- [x] Every direct dependency resolves and the graph
  `003/007/010/015/018 -> 016` is acyclic.
- [x] Scheduling remains sole owner of GroupStaffAssignment,
  StaffTermAvailability, StaffAvailability, ScheduleImpactAlert, persistence,
  and transactional mutation. StaffAdministration owns projections/facades and
  five staff endpoints only.
- [x] The missing narrow production Scheduling mutation port is explicit and
  bounded by T012, T056-T057, and T060-T065; no duplicate aggregate or direct
  StaffAdministration persistence is permitted.
- [x] Shared staff login and server-derived Lecturer/TeachingAssistant context
  remain canonical; no role picker or client-authoritative scope is accepted.
- [x] Every assignment, timetable, and roster access is authorized from current
  GroupStaffAssignment data before querying the object.
- [x] Roster pages default to 20, cap at 100, stable-sort by DisplayName then
  UniversityId, expose only UniversityId/DisplayName/EnrollmentState, and audit
  metadata without row content.
- [x] Availability updates use complete-range replacement, expected aggregate
  rowversion, server-time deadline checks, current published assignment state,
  and atomic durable impact alerts with no automatic class movement.
- [x] Admin may select an aggregate ID and rowversion as an immutable planning
  dependency but has no range-copy or
  availability mutation/correction/override route, permission, editable
  control, notification workflow, or correction-audit flow.
- [x] STF-01 through STF-04 inherit their immutable SPEC-003 design records;
  downstream route pins and release claims wait for actual passing evidence.
- [x] SPEC-018 pending manual/load/failover/recovery/release work is not claimed
  as passing SPEC-016 evidence.
- [x] Grades, attendance, messaging, capacity/policy/term administration,
  unrelated records, automatic rescheduling, distributed infrastructure, and
  duplicate persistence remain excluded.

## Automated validation evidence

- The Spec Kit prerequisite script resolves the feature directory to
  `specs/016-lecturer-ta-workspace` and finds the required task/context files.
- All three SPEC-016 readiness checklists had zero incomplete items before
  implementation.
- `.specify/scripts/powershell/Test-AllSpecs.ps1 -Phase Implementation`
  reports SPEC-016 at requirements score 100 with automated gates PASS and
  human approval APPROVED.
- The repository-wide result remains FAIL for pre-existing findings outside
  SPEC-016. For SPEC-010, the unregistered Admin availability mutation literals
  are intentional negative-contract examples; its separate API declaration
  drift is not represented as a global pass. The exact consumed ownership and
  read-only contracts are frozen here, and the real narrow-port runtime gap is
  explicitly assigned to the test-first SPEC-016 workstream.

## Scoped dependency exception

**Owner:** Ahmed Elbamby
**Recorded:** 2026-07-17
**Expires:** before T083 release traceability is checked
**Scope:** SPEC-010 documentation declaration lint only

SPEC-010's `requirements.md` retains an older abbreviated TypeScript API block
while its approved `contracts/api.md`, endpoint manifest, completed task ledger,
and focused passing contract/integration suites contain the canonical expanded
contract. SPEC-016 pins the exact `contracts/api.md` hash recorded in
`dependency-baseline.md`; it does not copy or reinterpret the abbreviated
block. The four reported Admin availability mutation literals are deliberate
negative examples and remain unmapped. This exception permits dependency-
ordered implementation but not release: T083 must confirm the upstream drift
is resolved or record a renewed constitutional review before it can pass.

**Closure (2026-07-17):** The SPEC-016 traceability audit confirmed that all
runtime contracts and tests consume the pinned canonical SPEC-010 contract,
not the abbreviated declaration. Ahmed Elbamby completed the required renewed
constitutional review and accepted the bounded demo closure recorded in
`docs/release-evidence/SPEC-016-traceability.md`. The upstream documentation
finding is not represented as globally fixed and remains a production/Gate-D
non-claim.

## Approval currency

The 2026-07-13 Gate A record remains current and Ahmed Elbamby's 2026-07-17
instruction revalidates this non-production demo baseline. Any later accepted
dependency hash, route, authorization, persistence, concurrency, privacy, or
scope change returns affected work to In Review.

Dependency-ordered SPEC-016 work may begin only after T001-T008 are checked
from their named evidence. Later tasks remain unchecked until their artifacts
and expected-red or passing evidence exist.
