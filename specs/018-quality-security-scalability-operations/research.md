# Research: Quality, Security, Scalability, and Operations

## Decisions

### Modular operations boundary

**Decision**: Keep feature logic in business-module projects and place health,
telemetry, shared-key, and security composition in
`StudentRegistration.Api/Operations`. Evidence schemas are documentation/test
artifacts, not domain entities.

### Shared Data Protection keys

**Decision**: Use the already-required SQL Server as a shared ASP.NET Core Data
Protection key repository for every API replica. Protect keys at rest with a
certificate/key retrieved through the approved secret-store abstraction.
Production starts only when both repository and protection material are
available.

**Rationale**: This supports cross-replica sessions and protected optimizer
tokens without sticky sessions or a new distributed service. The cloud/vendor
secret-store implementation remains a deployment decision.

### Exact load model

**Decision**:

- target: 10 minutes, 75 submissions/s + 300 reads/s;
- 2x: 10 minutes, 150 submissions/s + 600 reads/s;
- burst: 60 seconds, 200 submissions/s + 300 reads/s;
- 5x: 60 seconds, 375 submissions/s + 1,500 reads/s;
- reads: 50% discovery, 25% eligibility, 15% plan/timetable, 10% records;
- submissions: 70% valid unique, 20% expected rejection, 10% idempotent retry;
- soak: 120 minutes at target across at least two replicas.

Correctness has zero tolerance in every profile. Target latency/error SLOs apply
at target; higher loads must remain bounded, recover, and never violate
invariants.

### Threat model

**Decision**: Maintain a versioned STRIDE threat model covering browser/API/SQL
trust boundaries, identity/session, role/data scope, protected tokens,
registration races, Admin/audit/export, telemetry, secrets, and deployment.
Every threat has mitigation, validation evidence, residual risk, owner, and
review date.

### Accessibility evidence

**Decision**: Automation is necessary but insufficient. Critical routes require
manual keyboard and NVDA/Windows journeys with tester/tool version, route,
scenario, result, defects, and UX/QA sign-off.

### Release blocking

**Decision**: CI blocks for required code gates. Release additionally blocks
when signed threat, manual accessibility, load, recovery, or security evidence
is absent/stale, or any invariant/critical defect fails.

## Open Research

AASTMT may approve revised enrollment/load and secret-store/hosting values.
Until approval, the planning baselines above remain explicit and changes require
a spec revision; no institutional value is guessed.
