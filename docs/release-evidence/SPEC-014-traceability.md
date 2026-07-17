# SPEC-014 Complete Traceability Evidence

**Recorded:** 2026-07-17  
**Scope:** Bounded non-production Registration Capacity and Concurrency demo

Release is rejected if a cited suite fails, a required artifact is absent, the
load-result source fingerprint is stale, a measurable threshold fails, any
matrix row loses its executable evidence, or any SPEC-014 task through T122
remains unchecked. T123 is not claimed by this matrix; release approval remains
separate. This evidence is not production authorization.

## Normative sources

- `specs/014-registration-capacity-concurrency/requirements.md`
- `specs/014-registration-capacity-concurrency/concurrency-matrix.md`
- `specs/014-registration-capacity-concurrency/contracts/api.md`
- `specs/014-registration-capacity-concurrency/contracts/routes/STU-06.md`
- `specs/014-registration-capacity-concurrency/contracts/routes/ADM-08.md`

## Functional requirements

| Requirement | Acceptance / race link | Delivery | Executable evidence |
|---|---|---|---|
| FR-1 authenticated server identity only | AC-12 | `src/StudentRegistration.Registration/Application/RegistrationCommandFactory.cs`; `src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs` | `tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint01BehaviorTests.cs`; `tests/StudentRegistration.SecurityTests/Specs/Spec014/RegistrationEndpointSecurityBoundaryTests.cs` |
| FR-2 route term plus bounded request body | AC-3 | `src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs` | `tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint01ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint01BehaviorTests.cs` |
| FR-3 transactional final revalidation | AC-5, AC-7 | `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationEndpointStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-5Tests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-7Tests.cs` |
| FR-4 conditional atomic SQL seat allocation | AC-1, Race R01 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-1Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR01Tests.cs` |
| FR-5 stable database boundary and sorted group order | AC-7, Race R02/R06/R09/R10/R11 | `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR02Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR06Tests.cs` |
| FR-6 savepoint rollback and stable all-or-nothing rejection | AC-2, EC-2, Race R05 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-2Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-2Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR05Tests.cs` |
| FR-7 scoped replay, mismatch, and bounded non-durable 202 | AC-3, AC-8, EC-7/EC-8, Race R03/R04 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-3Tests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-8Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-7Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-8Tests.cs` |
| FR-8 unique/FK/check constraints as final guards | AC-4 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-4Tests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-3EvidenceTests.cs` |
| FR-9 no drop, correction, withdrawal, or decrement workflow | AC-11, OS-5 | bounded absence in API and route surface | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-11Tests.cs`; `docs/release-evidence/SPEC-014-scope-review.md` |
| FR-10 stable 409 conflicts without mutation | AC-5 | `src/StudentRegistration.Registration/Application/RegistrationConflictMapper.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-5Tests.cs`; `tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint01ContractTests.cs` |
| FR-11 pause, privacy-safe alert, authorized idempotent repair | AC-6, EC-5, Race R15 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-6Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-5Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR15Tests.cs` |
| FR-12 shared SPEC-008 student/term SQL boundary | AC-7, EC-6, Race R02 | `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-7Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-6Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR02Tests.cs` |
| FR-13 sole payload-bound idempotency claim/final result | AC-8, Race R03/R04/R13 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-8Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR03Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR04Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR13Tests.cs` |
| FR-14 one short local SQL transaction with complete snapshots/audit | AC-5, AC-9 | `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationEndpointStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-9Tests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-4EvidenceTests.cs` |
| FR-15 server ReceivedAtUtc and emergency closure recheck | AC-10, Race R07/R08 | `src/StudentRegistration.Registration/Application/RegistrationCommandFactory.cs`; `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-10Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR07Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR08Tests.cs` |
| FR-16 final-result replay and pre-commit complete retry | AC-3, AC-13, EC-3/EC-9, Race R14 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-13Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-3Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-9Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR14Tests.cs` |
| FR-17 shared upstream serialization boundaries | AC-9, Race R06/R09/R10/R11 | `src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR09Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR10Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR11Tests.cs` |
| FR-18 in-transaction claim before savepoint; no orphan Processing | AC-13, EC-10, Race R13 | `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-13Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-10Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR13Tests.cs` |

## Measurable non-functional requirements

The six tests read `docs/release-evidence/SPEC-014-load-results.json`, reject a
source-fingerprint mismatch, and assert the corresponding measured values.

| Requirement | Automated test | Recorded evidence | Release threshold |
|---|---|---|---|
| NFR-1 zero overbooking, duplicate active offering enrollment, partial commit, or combined policy/timetable violation | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-1EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-1.md` | zero target/spike violations |
| NFR-2 target submission latency | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-2EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-2.md` | 75/s for 600 s; 45,000 completed; p95 <= 2,000 ms |
| NFR-3 two-replica spike and exact collision | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-3EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-3.md` | 200/s for 60 s; 12,000 completed; two replicas; 100 concurrent submissions produce exactly 30 active enrollments at capacity 30 |
| NFR-4 short, cancellation-aware, local transactions | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-4EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-4.md` | zero remote calls in a transaction; cancellation before commit verified |
| NFR-5 conflict classification and reliability | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-5EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-5.md` | expected conflicts excluded; unexpected failures < 0.1% |
| NFR-6 observable privacy-safe concurrency metrics | `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-6EvidenceTests.cs` | `docs/release-evidence/SPEC-014-NFR-6.md` | deadlock, lock-wait p95, replay, conflict, and reconciliation metrics present; zero unsafe tags/privacy violations |

## Success criteria, acceptance, and edge evidence

| ID | Direct executable evidence |
|---|---|
| SC-1 concurrent submissions never overbook or duplicate active offering enrollment | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-1Tests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-1EvidenceTests.cs` |
| SC-2 a multi-group submission is wholly accepted or leaves no enrollment mutation | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-2Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR05Tests.cs` |
| SC-3 replay never allocates another seat | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-3Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR14Tests.cs` |
| AC-1 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-1Tests.cs` |
| AC-2 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-2Tests.cs` |
| AC-3 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-3Tests.cs` |
| AC-4 exact 100/30 collision | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-4Tests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-3EvidenceTests.cs` |
| AC-5 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-5Tests.cs` |
| AC-6 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-6Tests.cs` |
| AC-7 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-7Tests.cs` |
| AC-8 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-8Tests.cs` |
| AC-9 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-9Tests.cs` |
| AC-10 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-10Tests.cs` |
| AC-11 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-11Tests.cs` |
| AC-12 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-12Tests.cs` |
| AC-13 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-13Tests.cs` |
| EC-1 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-1Tests.cs` |
| EC-2 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-2Tests.cs` |
| EC-3 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-3Tests.cs` |
| EC-4 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-4Tests.cs` |
| EC-5 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-5Tests.cs` |
| EC-6 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-6Tests.cs` |
| EC-7 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-7Tests.cs` |
| EC-8 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-8Tests.cs` |
| EC-9 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-9Tests.cs` |
| EC-10 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-10Tests.cs` |

## Concurrency matrix

| Race | Required result | Real-SQL executable evidence |
|---|---|---|
| Race R01 two students, final seat | one winner, GROUP_FULL loser, no overbooking | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR01Tests.cs` |
| Race R02 one student, different plans | shared student/term serialization and valid combined state | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR02Tests.cs` |
| Race R03 same key and payload | one execution, replay or bounded non-durable 202 | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR03Tests.cs` |
| Race R04 same scoped key, different payload | IDEMPOTENCY_KEY_REUSED; different-term key independent | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR04Tests.cs` |
| Race R05 multi-group allocation | savepoint rollback and replayable whole rejection | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR05Tests.cs` |
| Race R06 hold/profile edit vs submit | final serialized profile governs | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR06Tests.cs` |
| Race R07 scheduled close vs request | server ingress instant governs | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR07Tests.cs` |
| Race R08 emergency close vs submit | WINDOW_CHANGED and no late commit | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR08Tests.cs` |
| Race R09 policy/catalogue publish vs submit | one current governing version | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR09Tests.cs` |
| Race R10 group mutation vs submit | no enrollment in invalid group state | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR10Tests.cs` |
| Race R11 capacity reduction vs submit | only invariant-preserving serial outcomes | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR11Tests.cs` |
| Race R12 deadlock/transient failure | complete idempotent transaction retry only | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR12Tests.cs` |
| Race R13 process death after claim before commit | claim rolls back and can be reclaimed | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR13Tests.cs` |
| Race R14 process/network loss after commit | stored final replay without duplicate state | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR14Tests.cs` |
| Race R15 reconciliation mismatch | one authorized repair, duplicate replay, audited safe resume | `tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR15Tests.cs` |

## API, route, security, and accessibility boundaries

| Boundary | Delivery | Evidence |
|---|---|---|
| POST /api/student/terms/{termId}/registrations | `src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs`; `src/StudentRegistration.Registration/Application/RegistrationEndpointService.cs` | `tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint01ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint01BehaviorTests.cs` |
| GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId} | `src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs`; `src/StudentRegistration.Registration/Application/RegistrationEndpointService.cs` | `tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint02ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint02BehaviorTests.cs` |
| Runtime security: authenticated Student plus exact Registration.SubmitOwn; POST antiforgery before handler; GET authorization before private lookup | endpoint policy and middleware | `tests/StudentRegistration.SecurityTests/Specs/Spec014/RegistrationEndpointSecurityBoundaryTests.cs` |
| STU-05 canonical review and submit route | canonical SPEC-003 page with bounded SPEC-014 behavior | `tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationReviewPageFeatureTests.cs` |
| STU-05 WCAG 2.2 AA automation | keyboard/focus, target size, responsive overflow, and zero serious/critical axe findings | `tests/StudentRegistration.AccessibilityTests/Routes/RegistrationReviewPageAccessibilityTests.cs`; `docs/release-evidence/SPEC-014-accessibility.md` |
| STU-06 result/recovery contribution | contract remains owned by SPEC-015 | `tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationResultPageContributorTests.cs` |
| ADM-08 monitoring contribution | observation only; no public repair/override | `tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationAdministrationPageContributorTests.cs` |

## Scope boundary

`docs/release-evidence/SPEC-014-scope-review.md` records the passing source,
contract, migration, route, and test scans for OS-1 waitlist/reservation/queue,
OS-2 distributed/application locks, OS-3 partial acceptance, OS-4 capacity
override, and OS-5 drop/withdrawal/correction/decrement. None is introduced by
SPEC-014.

This trace matrix records executable delivery evidence for a non-production
demo only. Gate B-D, official AASTMT authorization, production deployment, and
T123 approval remain separate decisions.
