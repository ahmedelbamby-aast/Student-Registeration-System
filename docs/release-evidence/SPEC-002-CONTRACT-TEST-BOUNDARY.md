# SPEC-002 Contract-Test Ownership Boundary

**Applies to:** `DEMO-POC-2026.1`  
**Reference oracle:** `tests/StudentRegistration.TestSupport/Spec002/Spec002PolicyTestHarness.cs`  
**Classification:** Executable governance reference; not runtime or release evidence

## Purpose

The SPEC-002 TestSupport harness is an executable reference oracle for the
approved rulebook contract, provenance shape, and PB-01 through PB-19 boundary
fixtures. It gives downstream owners one deterministic set of expected
decisions, stable reason codes, and governance outcomes to replay.

Passing the reference-oracle tests proves only that the executable fixture and
its contract-focused tests agree with the governed SPEC-002 artifacts. It does
not prove behavior of a server, API, database, Entity Framework model,
publication store, concurrent registration flow, or durable decision history.
The harness is not production policy code and is not an AASTMT production
authority.

## Runtime ownership and required evidence

| Concern | Owning specification | Evidence required from the owner |
|---|---|---|
| Rulebook publication, deterministic policy selection, typed-rule validation, and runtime evaluation | SPEC-009 | Replay the applicable AC, EC, and PB fixtures against the server-authoritative implementation, including full decision and provenance comparisons. |
| Student offering-eligibility projection | SPEC-011 | Replay applicable eligibility fixtures through the owned API projection and verify the complete `PolicyDecisionDto`, authorization scope, authoritative term context, and fail-closed responses. |
| Atomic capacity allocation and first-successful-commit behavior | SPEC-014 | Replay the applicable capacity boundaries against the transactional SQL Server implementation and prove that concurrent attempts cannot overbook, partially commit, or bypass final policy revalidation. |
| Durable registration decision snapshots and historical explainability | SPEC-015 | Persist and retrieve the exact version, bounded input summary, ordered results, explanations, and source metadata; replay AC-3 and EC-3 without rewriting history. |
| Production-like performance, load, scalability, and release gates | SPEC-018 | Measure the owning server/runtime path under the approved production-like fixture and publish reproducible p95/load evidence. The SPEC-002 harness benchmark cannot satisfy this gate. |

## Replay rule

Each owning implementation MUST replay the SPEC-002 fixtures relevant to its
scope before its release gate can pass. A replay must:

1. identify the immutable SPEC-002 profile and fixture version;
2. use the owning server/runtime implementation rather than the TestSupport
   oracle as the system under test;
3. compare all contractually required outputs, not only eligibility and the
   primary reason code;
4. retain run provenance, including the implementation revision, build
   configuration, environment, and measured results; and
5. fail the downstream gate when the implementation differs from an approved
   fixture until SPEC-002 is amended and approved or the implementation is
   corrected.

Downstream tests may use the reference oracle to obtain expected values, but
must not call it as the implementation under test. Copying its test-only logic
into a server component does not constitute replay evidence.

## Evidence classification

- **Contract-oracle evidence:** May report that the governed executable
  reference is internally consistent.
- **Runtime evidence:** Remains pending until the applicable SPEC-009,
  SPEC-011, SPEC-014, or SPEC-015 implementation replay passes.
- **Release and performance evidence:** Remains pending until SPEC-018 records
  the required production-like measurements and approvals.

This boundary applies to all SPEC-002 acceptance, edge, determinism,
provenance, and performance-harness records.
