# SPEC-017 Phase 3 Evidence

**Recorded:** 2026-07-17
**Phase 2 baseline:** `ff9b96804fa68e2cbabf0f684188368c4bcf9d61`
**Scope:** T032-T046
**Result:** PASS - eight conformance scenarios green and seven compile-safe
expected-red scenarios deferred to their named Phase 4/5 deliveries

## Acceptance criteria

| Task | Criterion | Evidence state |
|---|---|---|
| T032 | AC-1 reasoned mutation | SPEC-010 owner publication/invariant/audit behavior executes; expected red only for T070 merged projection |
| T033 | AC-2 capacity floor | SPEC-010 capacity reduction below enrollment rejected with zero state/version commits; green |
| T034 | AC-3 audit export scope | Contract scope/redaction/request-audit assertions pass; expected red only for T056 export service |
| T035 | AC-4 monitor degradation | Timestamp/threshold/degraded/no-zero contract assertions pass; expected red only for T052 metrics query |
| T036 | AC-5 governed bounded command | Page cap, owner preview/confirmation, read-only availability planning input, zero availability writes, and absent correction/override surface; green |
| T037 | AC-6 stale preview | Same-key replay returns STALE_PREVIEW twice with zero commits/audits; green |
| T038 | AC-7 audit rollback | Existing real-SQL fault assertions and safe correlated endpoint mapping verified; green |
| T039 | AC-8 final Admin | Existing two-replica SQL race, stable conflict, and SPEC-017 delegation verified; green |
| T040 | AC-9 quality gate | Compiles and fails only for exact T052/T056/T070/T091 and T092-T095 deliverables |

Green acceptance command:

```text
dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --no-build --filter "FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_2Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_5Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_6Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_7Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_8Tests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 5/5 passed.

Expected-red acceptance command:

```text
dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --no-build --filter "FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_1Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_3Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_4Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec017.AC_9Tests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 4/4 failed only at explicit missing T052/T056/T070/T091/T092-T095
assertions after their owner/contract prerequisites passed.

## Edge cases

| Task | Edge | Evidence state |
|---|---|---|
| T041 | EC-1 metrics unavailable | Expected red only for T052; test prohibits fabricated zero |
| T042 | EC-2 failed/expired export | Expected red only for T056; contract requires authorized new-key retry and forbids reset/requeue |
| T043 | EC-3 concurrent Admin edit | Concurrent SPEC-009 owner confirmations yield one commit/audit and one stale current-version response; green |
| T044 | EC-4 partially invalid import | Two bounded preview errors; Invalid cannot publish; green |
| T045 | EC-5 final Admin self-disable | Real-SQL SPEC-007 guard race preserves one Admin; green |
| T046 | EC-6 idempotency key reuse | Expected red only for T056 payload-bound idempotency service |

Green edge command:

```text
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_3Tests|FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_4Tests|FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_5Tests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 3/3 passed, including the real-SQL final-Admin test.

Expected-red edge command:

```text
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_1Tests|FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_2Tests|FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases.EC_6Tests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 3/3 failed only at the explicit absent T052/T056 source assertions.
