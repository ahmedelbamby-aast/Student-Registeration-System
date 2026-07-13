# Research: Identity and Account Lifecycle

## Decisions

### Modular boundary
**Decision**: Own this capability in the IdentityAccess module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface StudentLoginRequest { universityId: string; password: string; }
interface StaffLoginRequest { userName: string; password: string; }
interface ActivateStudentRequest {
  universityId: string;
  activationCode: string;
  password: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant";
  expiresAtUtc: string;
}
```

Endpoints are split by lifecycle action so recovery request and completion,
MFA verification, password change, revoke-all, and role-context selection have
separate authorization, antiforgery, rate-limit, and replay semantics.

### Replica-safe session state
**Decision**: Use shared SQL-backed Identity state and security-stamp
validation, plus the SPEC-018 shared Data Protection key ring. Rotate the
stamp for recovery/password/revoke-all and store challenge/rate-limit state in
the shared database.
**Rationale**: Either replica can validate a session or reject an invalidated
one without sticky routing.
**Alternatives rejected**: In-memory counters, per-node key rings, and sticky
sessions as correctness mechanisms.

### Institutional providers
**Decision**: Keep DEC-01, DEC-02, and DEC-13 explicit fail-closed production
gates. Provider adapters expose narrow verification ports; no local fallback
credential is treated as institutional proof.
**Rationale**: Provider identity and MFA are security/institutional decisions,
not implementation defaults.
**Alternatives rejected**: Invented activation secrets or self-managed staff
MFA promoted without AASTMT approval.

### Governed Admin user lifecycle
**Decision**: Identity owns pre-provisioned user import/list/status/role
commands, with expected versions, reason, audit facts, idempotent import
publication, and an Identity-owned singleton AdminSecurityGuard that
database-serializes the final-enabled-Admin invariant.
**Rationale**: ApplicationUser and RoleAssignment have one writer and two
replicas cannot remove the last Admin through write skew.
**Alternatives rejected**: Duplicate SPEC-017 writers and check-then-update
role counts.

### Security-event direction
**Decision**: SPEC-007 owns append-only `SecurityEvent`; downstream SPEC-017
consumes its read contract.
**Rationale**: Dependency direction remains acyclic while identity/abuse facts
retain a canonical owner.
**Alternatives rejected**: SPEC-017 becoming a second identity writer or
Identity depending on a downstream audit implementation. The shared write
port is owned upstream by SPEC-004.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
