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
interface StaffLoginRequest { userName: string; password: string; mfaCode?: string; }
interface ActivateStudentRequest {
  universityId: string;
  activationCode: string;
  password: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  expiresAtUtc: string;
}
```

Endpoints: POST /api/auth/student/login, POST /api/auth/student/activate,
POST /api/auth/staff/login, POST /api/auth/logout, POST /api/auth/recovery,
GET /api/auth/session.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
