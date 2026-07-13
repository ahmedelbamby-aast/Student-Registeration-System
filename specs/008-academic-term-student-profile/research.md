# Research: Academic Term and Student Profile

## Decisions

### Modular boundary
**Decision**: Own this capability in the Academics module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  transcriptSummary: { attemptedCredits: number; earnedCredits: number; attemptCount: number };
  transcriptAttempts: Array<{ courseCode: string; termCode: string; credits: number; grade?: string; status: string; provenance: string }>;
  activeHolds: Array<{ code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; source: string }>;
  dataVersion: string;
  dataAsOfUtc: string;
  provenance: Array<{ source: string; reference: string; importedAtUtc: string }>;
}
type PublicAcademicContextDto = PublicContextDto; // canonical SPEC-006 public shape
interface AcademicAppContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTerm?: TermSummaryDto;
  registrationTerm?: TermSummaryDto;
  registrationWindow?: { id: string; state: "upcoming" | "open" | "closed"; opensAtUtc: string; closesAtUtc: string; rowVersion: string };
  serviceState: "available" | "maintenance" | "unavailable";
  supportReferencePath: string;
}
```

Endpoints: GET /api/public/context, GET /api/context, and GET
/api/students/me/academic-context plus SPEC-008-owned term/window and student
Admin endpoints. SPEC-017 consumes audit/report contracts instead of owning
Academics mutations.

### Academic AppContext composition
**Decision**: SPEC-008 owns `/api/context` composition and the academic
portion; SPEC-007 owns session fields and SPEC-006 owns shared DTO/error
conventions.
**Rationale**: One endpoint gives every replica and page a coherent server
instant, role, term, window, and version without duplicate ownership.
**Alternatives rejected**: Browser clock/term inference and two independently
loaded contexts with no shared version.

### Student-term serialization
**Decision**: Own `StudentTermAcademicState` upstream in Academics. Hold/profile
mutations and registration submissions lock it before reading decision inputs.
**Rationale**: Registration already depends on Academics, avoiding a forward
dependency while preventing hold-versus-submit write skew.
**Alternatives rejected**: A downstream-owned guard consumed by SPEC-008 and
application-instance locks.

### Complete synthetic academic profiles
**Decision**: The Academics seed contributor populates only the existing
profile contract: University ID link, ProgramCode, cohort, GPA, earned and
attempted credits, standing, transcript attempts, active blocking/non-blocking
holds, StudentTermAcademicState, synthetic provenance, version, and as-of time.
Logical values derive from the seed-profile version and stable fixture ordinal.
**Rationale**: Development and tests exercise realistic normal and boundary
states without importing real student data or quietly expanding the personal
data model.
**Alternatives rejected**: Partial student rows, random unrepeatable academic
values, copied production records, and invented email/phone/address/birth-date
fields.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
