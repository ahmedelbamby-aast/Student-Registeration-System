# API Contract: Schedule Builder and Conflicts

**Draft amendment:** The credit-load response projection is pending Ahmed
ELbamby's review and is not yet an immutable downstream contract pin.

## Feature Contract

```typescript
interface ScheduleConflictDto {
  code: "MEETING_OVERLAP" | "TRAVEL_BUFFER";
  first: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  second: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
  actions: Array<{ action: "change-group" | "remove-group"; targetGroupId: string; label: string; route: string }>;
}
interface ValidationSnapshotDto {
  evaluatedAtUtc: string;
  academicContextVersion: string;
  policyVersion: string;
  catalogueVersion: string;
  offeringVersions: Record<string, string>;
  groupVersions: Record<string, string>;
}
interface LoadPolicyReasonDto {
  code: string;
  blocking: boolean;
  message: string;
  requiredValue?: string;
  currentValue?: string;
  policySetId: string;
  policyVersion: string;
  sourceReference: string;
}
interface RegistrationPlanDto {
  id: string;
  termId: string;
  rowVersion: string;
  selectedGroups: GroupDto[];
  totalCredits: number;
  defaultTargetCredits: 18;
  maximumAllowedCredits: 12 | 18;
  loadReasons: LoadPolicyReasonDto[];
  conflicts: ScheduleConflictDto[];
  validation: ValidationSnapshotDto;
  reviewBlocked: boolean;
}
interface RegistrationPlanMutationRequest { expectedPlanRowVersion: string; selectedGroupIds: string[]; }
```

Endpoints: GET and PUT /api/student/terms/{termId}/registration-plan, and POST
/api/student/terms/{termId}/registration-plan/validate.

The server derives the student from authentication and validates term access.
PUT replaces the complete selection atomically and requires
`expectedPlanRowVersion`; stale updates return `409 STALE_VERSION` plus the
current authorized plan. Validation is non-mutating. Selection validation
returns `DUPLICATE_OFFERING_SELECTION`, `GROUP_CHANGED`, `GROUP_FULL`,
`GROUP_UNAVAILABLE`, or complete conflict data and actions. Conflict detection
uses half-open intervals. `TRAVEL_BUFFER` is reserved for a future approved
policy and is not emitted by the demo.
Neither read, PUT, nor validate reserves a seat.
Every response composes `defaultTargetCredits`, `maximumAllowedCredits`, and
`loadReasons` from the current SPEC-011 eligibility/policy evaluation. The
browser never calculates an effective maximum from GPA, and a 12-credit
probation limit is returned with a safe policy/source explanation.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
