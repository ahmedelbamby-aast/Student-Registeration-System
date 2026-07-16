# SPEC-012 NFR-4 Two-Editor No-Lost-Update Evidence

**Requirement:** SPEC-012/NFR-4  
**Task:** SPEC-012/T051  
**Recorded:** 2026-07-16  
**Focused verification:** PASS - 3 quality evidence tests and 1 real-SQL integration test passed on 2026-07-16

## Evidence boundary

The real-SQL integration proof opens two independent EF DbContext instances
against the same SQL Server database. Both editors submit the same captured
rowversion for a complete plan replacement. The production adapter performs
the replacement in a serializable transaction and explicitly tracks the
parent-controlled aggregate.

The first editor is the one winner. Its complete replacement commits and SQL
Server advances the plan rowversion. The second editor is rejected as stale,
and its result contains the stale current plan: the winner's new rowversion
and winning selection. The assertions prove no lost update because the stale
writer cannot replace the winning item.

The endpoint maps that stale result and current plan to HTTP 409 with the
stable `STALE_VERSION` code and current rowversion. The SQL proof also checks
canonical application-user-to-student owner resolution and an empty read for
an unrelated student scope.

## Verification commands

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec012.NFR_4EvidenceTests"

dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~RegistrationPlanModelConfigurationTests.Real_sql_round_trips_plan_values_and_enforces_unique_scope_and_offering"
```

## Explicit non-claims

- The proof does not allocate, reserve, or mutate seats.
- The proof covers registration-plan replacement only, not final submission.
- Mixed-traffic load, browser visual approval, and release authorization remain separate gates.

**Result: PASS.**
