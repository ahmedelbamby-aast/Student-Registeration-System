# Research: Schedule Recommendations

## Decisions

### Modular boundary

**Decision**: Own optimizer values, orchestration, and endpoints in
`StudentRegistration.Registration`; consume Scheduling and Academics through
their public ports.

The accepted dependency baseline is SPEC-010 for published group/resource
availability, SPEC-011 for server-authoritative eligibility, and SPEC-012 for
the versioned plan/conflict snapshot; the optimizer does not reach into their
internal types.

**Rationale**: It preserves the project-per-module modular monolith and avoids
a service or generic-layer project.

### Replica-safe option application

**Decision**: Protect the complete option payload with ASP.NET Core Data
Protection purpose `Registration.ScheduleOption.v1`. Bind authenticated
student, plan, plan rowversion, group selection, catalogue/group/policy/
configuration versions, request correlation ID, issued time, and a 10-minute
expiry.

**Rationale**: A bare transient option ID cannot be resolved after load
balancing or process restart. A short-lived protected token is stateless,
tamper evident, confidential, and works across replicas using SPEC-018's shared
key repository.

**Rejected**: sticky sessions, process-memory option caches, a new option
table, and trusting client-supplied group payloads.

### Diagnostic minimality

**Decision**: “Minimal” means inclusion-minimal, not globally
minimum-cardinality. Removing any member makes that reported hard conflict
cease to hold. Order diagnostics by member count and stable IDs.

**Rationale**: This is deterministic, useful, and bounded without an
unnecessary global minimization pass.

### Deterministic bounded search

**Decision**: Sort constrained courses first, prune a partial assignment as
soon as a hard constraint fails, cap outputs at three, honor cancellation, and
use stable identifiers for ties.

**Rationale**: It meets the workload with a simple explainable algorithm.
OR-Tools stays out of scope until benchmarks justify it.

### Authority and consistency

**Decision**: Capture one coherent input-version set. Applying a result rechecks
token ownership/expiry and every current version, then atomically updates the
SPEC-012 plan. SPEC-014 performs final registration revalidation; a
recommendation never reserves capacity.

## Open Research

No unresolved implementation clarification remains. Institutional policy
values stay configurable and fail closed until the owning authority approves
them.
