# SPEC-004 NFR-1 Architecture-Rejection Evidence

**Requirement:** Architecture tests MUST fail on forbidden module references/cycles.
**Measured:** 2026-07-13
**Configuration:** Release, .NET SDK 10.0.301, .NET runtime 10.0.9, x64
**Automated evidence test:** `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-1EvidenceTests.cs`
**Executed architecture fixture:** `StudentRegistration.ArchitectureTests.ModuleDependencyTests.Forbidden_reference_and_cycle_fixtures_are_rejected`

## Method

The quality gate executes the existing focused architecture fault-injection
fixture rather than maintaining a second dependency-graph implementation. The
fixture introduces two independently invalid conditions in memory:

1. `StudentRegistration.Registration` directly references
   `StudentRegistration.Academics` although its injected allowlist permits only
   `StudentRegistration.Contracts`.
2. `StudentRegistration.Academics` and `StudentRegistration.Registration`
   reference each other, producing a two-node cycle.

The first condition must be returned by the forbidden-reference detector, and
the second must be returned by the cycle detector. The fixture fails if either
invalid condition is accepted. The normal repository conformance tests apply
the same governed constraints to every source project: reference differences
fail the explicit-allowlist assertion, while a cycle fails the acyclic-graph
assertion. Consequently, either violation makes the architecture test project
and its CI gate fail.

The focused command executed for this record was:

```powershell
dotnet test tests/StudentRegistration.ArchitectureTests/StudentRegistration.ArchitectureTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName=StudentRegistration.ArchitectureTests.ModuleDependencyTests.Forbidden_reference_and_cycle_fixtures_are_rejected" --logger "console;verbosity=normal"
```

## Measured result

| Measure | Value |
|---|---:|
| Focused architecture fixtures executed | 1 |
| Forbidden-reference violations injected | 1 |
| Cyclic dependency graphs injected | 1 |
| Invalid dependency conditions rejected | 2 |
| Failed architecture fixtures | 0 |

The focused run reported one passed test, zero failed tests, and a successful
process exit. Passing means both deliberately invalid graphs were recognized;
it does not mean the invalid graphs were admitted into the repository.

**Result: PASS.** The executable architecture gate recognizes both a forbidden
project reference and a dependency cycle, and the real repository graph remains
subject to the corresponding fail-the-build assertions.

## Scope

This evidence covers SPEC-004 NFR-1 project-reference and cycle enforcement.
It does not approve a new dependency, replace the canonical module-boundary
record, or provide production deployment approval.
