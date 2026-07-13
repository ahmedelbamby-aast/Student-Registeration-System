# API Contract: Academic Term and Student Profile

## Feature Contract

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
interface TermMutationRequest {
  expectedTermRowVersion?: string;
  expectedWindowRowVersions: Record<string, string>;
  reason: string;
  source: string;
  term: {
    code: string;
    displayName: string;
    timeZoneId: string;
    teachingStartsOn: string;
    teachingEndsOn: string;
  };
  windows: Array<{
    id?: string;
    scopeType: "all-students" | "program" | "cohort";
    scopeValue?: string;
    opensAtUtc: string;
    closesAtUtc: string;
  }>;
}
type AcademicProfileCorrectionOperation =
  | { kind: "set-gpa"; currentGpa: number; sourceReference: string }
  | { kind: "set-earned-credits"; earnedCredits: number; sourceReference: string }
  | { kind: "set-standing"; standingCode: string; sourceReference: string }
  | { kind: "upsert-transcript-attempt"; attemptId?: string; courseCode: string; termCode: string; credits: number; grade?: string; status: "in-progress" | "passed" | "failed" | "withdrawn"; sourceReference: string }
  | { kind: "upsert-hold"; holdId?: string; code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; sourceReference: string }
  | { kind: "remove-hold"; holdId: string; sourceReference: string };
interface AcademicProfileCorrectionRequest {
  expectedStudentRowVersion: string;
  expectedStudentTermStateRowVersion: string;
  reason: string;
  source: string;
  operations: AcademicProfileCorrectionOperation[];
}
```

`TermSummaryDto` is consumed unchanged from the canonical SPEC-006 shared
contract; SPEC-008 supplies its AcademicTerm data but does not redefine its
fields or lifecycle-state vocabulary.
`PublicAcademicContextDto` is an alias, not a second DTO definition. Correction
commands accept only the discriminated operation allow-list above; arbitrary
field names, navigation properties, password/role fields, and untyped values
are rejected before mutation.

Endpoints: GET /api/public/context, GET /api/context, GET
/api/students/me/academic-context, GET /api/admin/terms, POST
/api/admin/terms, PUT /api/admin/terms/{termId}, POST
/api/admin/terms/{termId}/registration-windows/{windowId}/publish, GET
/api/admin/students, GET /api/admin/students/{studentId}/academic-context, and
PATCH /api/admin/students/{studentId}/academic-profile.

`GET /api/context` returns the shared `AppContextDto`: SPEC-007 supplies its
identity/session portion and SPEC-008 supplies `AcademicAppContextDto`. The
public endpoint exposes only its public subset. Lists use the SPEC-006 default
page size 20 and maximum 100. Mutations use body `expectedRowVersion` fields;
stale state returns `409 STALE_VERSION` with current versions only when the
caller remains authorized. Window overlap returns `409 WINDOW_OVERLAP` with
conflicting window IDs and intervals.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
