# SPEC-010 NFR-1 Offering Read Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T18:00:00Z  
**Boundary:** Non-production Spec 010 component evidence

## Executable gate

The focused quality test issues 300 concurrent reads through
`OfferingService.GetStudentDetailAsync`. Each read projects a published
offering, one group, and its Lecture and Tutorial details from a passive
read-only port. The executable assertions calculate every request latency,
the measured p95, and the achieved component throughput from `Stopwatch`
values produced during the run.

The gate requires:

- 300 concurrent reads;
- no missing or incomplete result;
- measured p95 at or below the 300 ms p95 budget; and
- measured component throughput of at least 300 reads per second.

This is direct, bounded component-level evidence for the Scheduling read
projection. It does not include SQL Server, HTTP, authentication, external
network, TLS, or two-replica costs. The SPEC-018 continuous ten-minute mixed
load and spike evidence is not replaced or claimed by this record.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec010.NFR_1EvidenceTests" -p:TreatWarningsAsErrors=true
```

- 2 passed
- 0 failed
- Request sample: 300
- Maximum p95: 300 ms

Quality test normalized-LF SHA-256: `1A9EF149A354B1661B7F40A3F18A64D0BEB1E100699C3F6D71752EDDCE940F08`

**Result: PASS.**
