# SPEC-008 Implementation Readiness

**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED DEMO WORK  
**Frozen:** 2026-07-14 under Ahmed ELbamby's Gate A approval

- [x] SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-007, and SPEC-018
  versions, consumed contracts, and deferred boundaries are recorded in
  `dependency-baseline.md`.
- [x] The six SPEC-008 entities, one shared
  `RegistrationWindowSummaryDto`, ten endpoints, four page contributions,
  four workstreams, and T001-T091 execution trace have one consistent owner
  and delivery path.
- [x] AcademicTerm creation uses its globally unique
  `CreationClientRequestId` plus payload hash; publication and profile
  correction use expected rowversions and no seventh idempotency entity.
- [x] Term resolution, half-open UTC window evaluation, deterministic
  open/upcoming/closed selection, ambiguity failure, stable locking, and the
  conservative same-term overlap rule are explicit and testable.
- [x] Transcript corrections are append-only, current-leaf, same-lineage,
  acyclic, and filtered-unique; summaries count current leaves only.
- [x] Paging, active-hold, window/version, correction-operation, search,
  reason/source, and field-error bounds are exact and fail closed.
- [x] `Context.Read`, `AcademicProfile.ReadOwn`, `AcademicTerms.Manage`, and
  `AcademicProfiles.Manage` are independent permission policies. Login and
  context switching derive claims only from the effective server role, and
  role-only/wrong-role/cross-substitution tests pass.
- [x] SPEC-006 remains the sole shared AppContext/window DTO writer. The
  nullable SPEC-003 frontend projection and AppShell render server-derived
  none/upcoming/open/closed state without browser-authoritative time.
- [x] SPEC-008-specific API DTOs have one dependency-neutral physical source
  boundary under `StudentRegistration.Contracts.Academics`; Client never
  references the Academics runtime and Contracts contains no business service.
- [x] The class diagram composes session and academic context at the API
  boundary, with no Academics-to-Identity dependency.
- [x] Entity-ownership manifest `2.0.5`, persistence manifest `2.1.1`, the
  ERD, entity references, relational invariants, and class diagram are aligned.
- [x] EF Core Design `10.0.9` is pinned as private tooling through the sole
  SPEC-004 Infrastructure project writer.
- [x] Development and Testing use synthetic, versioned, complete academic
  profiles and migration-before-seed orchestration; production data,
  auto-migration, and go-live authority remain excluded.
- [x] The exact NFR-2 evidence contract remains 25,000 authenticated synthetic
  students, one shared SQL Server database, two stateless API replicas, ten
  continuous minutes, 300 context reads/second, 180,000 requests, p95 no more
  than 300 ms, and unexpected failures below 0.1%.
- [x] Every delivery task is preceded by its exact model, contract, behavior,
  acceptance, edge, accessibility, SQL, or quality test; cross-spec source
  writer collisions are absent.
- [x] Initial analysis findings—shared DTO duplication, ambiguous window
  selection, unbounded inputs, generic idempotency, transcript forks,
  permission substitution, stale shell shape, source-writer collisions, and
  client transport placement—were resolved before implementation.
- [x] Independent final semantic audit returned PASS with no concrete blocker.

## Executable evidence

- `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks
  -IncludeTasks`: PASS.
- SPEC-008 requirements checklist: 9/9; gate checklist: 10/10; human Gate A
  approval: APPROVED.
- `.specify/scripts/powershell/Test-AllSpecs.ps1 -Phase Implementation`: PASS
  for all 18 specifications, with every requirements score at 100.
- `dotnet test StudentRegistration.slnx --no-restore --nologo`: PASS, including
  Specification 103/103, Contract 95/95, Authorization 10/10, Architecture
  23/23, Client Unit 116/116, E2E 17/17, Security 53 passed with one
  environment-dependent skip, and Integration 120 passed with nine
  environment-dependent skips.
- Focused nullable-shell evidence: SPEC-003 model 4/4 and AppShell 4/4 PASS.
- `git diff --check`: PASS.

Dependency-ordered SPEC-008 implementation may start at T008. Every later
task still follows its recorded failing test and evidence gate. Production
data integration, production deployment, Gates B-D, release sign-off, and
official AASTMT go-live remain separately authorized and fail closed.
