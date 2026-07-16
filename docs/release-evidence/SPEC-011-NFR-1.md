# SPEC-011 NFR-1 Discovery Performance Evidence

## Requirement and method

This is the canonical executable evidence for SPEC-011 NFR-1 and T045. The
focused quality test performs 300 discovery reads through
`OfferingSearchQuery.SearchAsync` using the fixed SPEC-011 scenario after one
warm-up read. It measures full-batch throughput and each read latency.

The feature gate is:

- 300 discovery reads;
- p95 <= 300 ms; and
- >= 300 reads/s.

## Boundary

This is component-level evidence for the bounded discovery query and
server-authoritative eligibility path. It does not simulate database,
network, browser, multi-replica, or mixed write traffic. The SPEC-018
ten-minute mixed-load release run is not replaced by this focused feature
test.

## Execution

Command:

`dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --filter FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec011.NFR_1EvidenceTests`

Recorded focused run on 2026-07-16:

- elapsed: 50.741 ms;
- p95: 0.284 ms; and
- throughput: 5,912.4 reads/s.

The test emits current machine-local measurements in xUnit output as
`SPEC011_NFR001` and enforces the thresholds on every run.

**Result: PASS.**
