# API Contract: Eligibility and Subject Discovery

## Response contract

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
  sourceAccessedOn: string;
  approvedBy: string;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  overridePossible: false;
  supportReferencePath?: string;
}
interface GroupNonSelectableReasonDto {
  code: string;
  message: string;
}
interface GroupMeetingStaffDto {
  role: "Lecturer" | "TeachingAssistant";
  name: string;
}
interface GroupMeetingDto {
  meetingId: string;
  activity: "Lecture" | "Tutorial" | "Laboratory";
  dayOfWeek: number; // 0..6 in the configured term timezone
  startLocal: string;
  endLocal: string;
  roomCode: string;
  location: string;
  staff: GroupMeetingStaffDto[];
}
interface GroupSummaryDto {
  groupId: string;
  groupCode: string;
  state: "draft" | "published" | "closed" | "cancelled";
  selectable: boolean;
  capacity: number;
  enrolledCount: number;
  seatsRemaining: number;
  nonSelectableReasons: GroupNonSelectableReasonDto[];
  meetings: GroupMeetingDto[];
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
  inputSummary: Record<string, string>;
  evaluatedAtUtc: string;
  academicContextVersion: string;
  catalogueVersion: string;
  policySetId: string;
  policyVersion: string;
  offeringRowVersion: string;
  currentPlanVersion: string;
}
```

`state` is lifecycle-only. A full group remains `published` and is explained
through `selectable=false`, `seatsRemaining=0`, and a `GROUP_FULL`
non-selectable reason. Staff remain nested under their meeting so a Lecturer
or TeachingAssistant is never associated by array order.

The list response is the canonical SPEC-006
`Page<OfferingEligibilityDto>` unchanged, including `sort` echoing the applied
canonical sort.

## Evaluation and authority

- The server derives the Student from authentication and authorizes Student plus `Catalogue.ReadAvailable` before context/resource lookup.
- Registration evaluates the complete governed SPEC-002 registry against
  immutable snapshots supplied through narrow Academics, Scheduling, and
  current-plan ports.
- Before SPEC-012 contributes a plan reader, the live current-plan adapter
  returns version `initial-empty/1`, zero credits, and no meetings. Tests may
  supply versioned 15-credit and conflict snapshots through the same port.
- A passed current transcript leaf for the requested course fails closed with
  `REPEAT_POLICY_UNAVAILABLE`; no repeat/advisor exception exists.
- Per-rule codes preserve the SPEC-002 registry. When required decision input
  is missing, the offering is ineligible and includes
  `DECISION_DATA_UNAVAILABLE` plus the applicable governed unavailable reason.
- Eligibility is evaluated before result filtering, sorting, and paging.
  Browser values never author eligibility, plan credits, policy version,
  capacity, or selectability.
- Discovery is advisory. Detail refresh and SPEC-014 submission re-resolve
  authoritative versions.

## List query protocol

Only `q`, `eligibility`, `credits`, `day`, `availability`, `sort`, `page`, and
`pageSize` are accepted. Unknown or duplicate scalar keys return
`400 VALIDATION_ERROR`.

| Parameter | Contract |
|---|---|
| `q` | Optional Unicode NFKC-normalized, trimmed, literal parameterized code/title contains search; 1..100 characters when supplied |
| `eligibility` | `eligible`, `unavailable`, or `all`; default `eligible` |
| `credits` | Optional exact positive decimal from 0.5 through 30, maximum two decimal places |
| `day` | Optional integer 0..6 in the configured term timezone |
| `availability` | `available`, `full`, `unavailable`, or `all`; default `all` |
| `sort` | `courseCode,id` (default), `courseCode-desc,id`, `title,id`, `title-desc,id`, `credits,id`, or `credits-desc,id` |
| `page` | Default 1; values below 1 return `PAGE_SIZE_INVALID` |
| `pageSize` | Default 20; 1..100; invalid values return `PAGE_SIZE_INVALID` |

`available` means at least one selectable group. `full` means every otherwise
published candidate group has zero remaining seats. `unavailable` means no
selectable group for a capacity, lifecycle, staffing, or decision reason. Every
sort ends in immutable offering ID.

## Endpoint 01

### `GET /api/student/terms/{termId}/offerings`

Success: `200 Page<OfferingEligibilityDto>`.

Every successful list response, including an empty page, carries these bounded
advisory headers so the canonical `Page<T>` remains unchanged:

| Header | Value |
|---|---|
| `X-Eligibility-Policy-Version` | Applied immutable PolicySet version |
| `X-Eligibility-Reason-Codes` | Comma-separated, stable, ordinally ordered reason codes; no messages or student data |
| `X-Registration-Window-State` | Server-authored current window state |
| `X-Support-Reference-Path` | Configured safe Registrar/support reference path |

Header values are derived from the same versioned evaluation snapshot as the
page. They are never accepted from the client. This metadata lets the empty
state show the evaluated policy, reasons, window state, and support path
without defining a feature-specific page wrapper.

| Outcome | Status and body |
|---|---|
| Invalid query/page | `400 PAGE_SIZE_INVALID` or `400 VALIDATION_ERROR` as `ApiError` |
| Unauthenticated | `401 ApiError` |
| Wrong role/permission | `403 ApiError`, no protected data |
| Authorized term outside current registration context | `404 REGISTRATION_CONTEXT_NOT_FOUND` |
| Dependency/storage unavailable | `503 DISCOVERY_UNAVAILABLE` |
| Unexpected failure | `500 INTERNAL_ERROR` |
| Conflict | Not applicable to this committed-state GET |

The authenticated Student and authorized term scope are server-resolved; no Student identifier is accepted.

## Endpoint 02

### `GET /api/student/offerings/{offeringId}/eligibility`

Success: `200 OfferingEligibilityDto` containing the complete available or
unavailable decision and current advisory versions.

| Outcome | Status and body |
|---|---|
| Unauthenticated | `401 ApiError` |
| Wrong role/permission | `403 ApiError`, no existence/version oracle |
| Authorized offering absent or outside registration context | `404 OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT` |
| Dependency/storage unavailable | `503 DISCOVERY_UNAVAILABLE` |
| Unexpected failure | `500 INTERNAL_ERROR` |
| Validation/conflict | Not applicable to a valid route-identifier GET |

If a group becomes full after the list was read, this endpoint refreshes and
returns `selectable=false`, `GROUP_FULL`, zero seats, and the new rowversion.
It does not reserve or submit a seat; final submission belongs to SPEC-014.

## Shared rules

- All errors use canonical privacy-safe `ApiError`.
- Dates use ISO 8601 and server time; local meeting values use the configured
  academic-term timezone.
- Search predicates are parameterized and metacharacters are literal text.
- Lists are bounded, stable, and server-filtered.
- DTOs expose no EF entity, navigation graph, raw claim, credential, or
  unauthorized identifier/version.
