# SPEC-016 NFR-1 Evidence

**Requirement:** Staff dashboard/assignment reads SHOULD respond within 300 ms p95.

**Automated fixture:** `NFR-1EvidenceTests.Staff_assignment_reads_meet_the_300ms_p95_target`
uses the real `StaffWorkspaceQueries` projection with a bounded in-memory reader,
20 warmups, and 200 measured assignment reads. The test prints the measured
p95 and fails above 300 ms.

**Run command:**

```text
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter FullyQualifiedName~Spec016.NFR_1EvidenceTests
```

The fixture is a bounded application-read diagnostic, not a production SQL
capacity or SLA claim. SQL load, replica, and Gate-D evidence remain separate.

**Measured result (2026-07-17):** 200 samples, p95 `0.0154 ms`, threshold
`300 ms`; PASS.
