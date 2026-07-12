# API Contract: Identity and Account Lifecycle

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
