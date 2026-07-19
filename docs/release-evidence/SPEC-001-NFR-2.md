# SPEC-001 NFR-2 Scalability Evidence

**Requirement:** `SPEC-001/NFR-2`
**Recorded:** 2026-07-19
**Release result:** PASS
**Production authority: not granted**

## Approved profile and measured result

The exact approved SPEC-018 profile ran against a migrated SQL Server 2022
fixture with 25,000 accounts, 5,000 sessions, and two stateless replicas of the
same API:

| Profile | Required load | Measured completion | Unexpected failures | Result |
|---|---:|---:|---:|---|
| Target submissions | 75 submissions/s for 600 seconds | 45,000 | 0 | PASS |
| Target reads | 300 reads/s for 600 seconds | 180,000 | 0 | PASS |
| Spike | 200 submissions/s for 60 seconds | 12,000 | 0 | PASS |
| Collision | 100 contenders for 30 seats | exactly 30 accepted | 0 unexpected | PASS |

The target read profile removed the first replica at the five-minute point and
continued on shared SQL state. Across target, spike, and collision profiles,
the measured counters report zero overbooking, zero duplicate active offering
enrollments, zero partial schedule commits, and zero domain-invariant
violations. Privacy violations are zero.

## Domain and architecture boundary

Horizontal scale did not change domain behavior: both replicas execute the
same `StudentRegistration.Api` composition and the same registration/domain
modules, with SQL concurrency and idempotency remaining authoritative.
`SPEC-001-NFR-4.md` independently verifies one deployable API, an explicit
acyclic module graph, and two replicas of the same API rather than distributed
business services.

The machine-readable authority is
`docs/release-evidence/SPEC-018-load-results.json`. This non-production POC
measurement is not production sizing, hosting approval, or an AASTMT SLA.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec001.NFR_2EvidenceTests"
```
