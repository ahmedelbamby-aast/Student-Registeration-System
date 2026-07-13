# API Contract: Eligibility and Subject Discovery

## Feature Contract

```typescript
interface EligibilityReasonDto {
  code: string;
  passed: boolean;
  blocking: boolean;
  message: string;
  requiredValue?: string;
  currentValue?: string;
  policySetId: string;
  policyVersion: string;
  sourceReference: string;
  effectiveFromUtc?: string;
  supportReferencePath?: string;
}
interface GroupSummaryDto {
  groupId: string;
  groupCode: string;
  state: "published" | "full" | "closed" | "cancelled";
  selectable: boolean;
  capacity: number;
  enrolledCount: number;
  seatsRemaining: number;
  staff: Array<{ role: "Lecturer" | "TeachingAssistant"; name: string }>;
  meetings: Array<{ activity: "Lecture" | "Tutorial" | "Laboratory"; dayOfWeek: number; startLocal: string; endLocal: string; roomCode: string; location: string }>;
  rowVersion: string;
}
interface OfferingEligibilityDto {
  offeringId: string;
  courseCode: string;
  title: string;
  credits: number;
  currentPlanCredits: number;
  projectedPlanCredits: number;
  defaultTargetCredits: 18;
  maximumAllowedCredits: 12 | 18;
  eligible: boolean;
  reasons: EligibilityReasonDto[];
  groups: GroupSummaryDto[];
  evaluatedAtUtc: string;
  academicContextVersion: string;
}
```

The list endpoint returns the canonical SPEC-006
`Page<OfferingEligibilityDto>` unchanged, including `sort` echoing the applied
canonical sort; SPEC-011 does not define a feature-specific page wrapper.

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, sort, page, and pageSize; and GET
/api/student/offerings/{offeringId}/eligibility for one fully explained
offering/group detail.

The server derives the student from authentication, validates the requested
term/offering against the active authorized context, evaluates all rules
before filtering/paging, and never accepts a client eligibility flag as
authority. `q` is normalized, parameterized, and at most 100 characters.
`page` defaults to 1, `pageSize` to 20, maximum is 100, and invalid values
return `400 PAGE_SIZE_INVALID`. Default ordering is normalized course code then
offering ID; every alternate sort appends offering ID. Group capacity/version
is advisory and details/submission revalidate it.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
