# SPEC-011: Eligibility and Subject Discovery

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Backend, QA<br>
**Target:** Sprint 3<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-006, SPEC-008, SPEC-009, SPEC-010, SPEC-018<br>

## Context

Students need to see available subjects based on program, GPA, standing,
earned credits, prerequisites, repeats, holds, term, published groups, and
policy. The system must explain both eligible and unavailable outcomes.

## Functional Requirements

- FR-1: The system MUST evaluate every relevant approved rule on the server.
- FR-2: Default discovery MUST list eligible offerings having at least one
  published selectable group.
- FR-3: Students MUST be able to search by code/title and filter by
  eligibility, credits, day, and availability.
- FR-4: Students MUST be able to inspect unavailable offerings and every
  blocking reason.
- FR-5: Results MUST show course code/title/credits and group capacity, staff,
  location, activity, day and time. Each group summary MUST also carry its
  current state, selectable flag, seats remaining, and SectionGroup rowversion
  so the client can identify stale advisory data.
- FR-6: Each evaluated rule MUST return a stable code, passed/failed and
  blocking flags, plain-language message, required/current values when safe,
  approved PolicySet ID/version, source/effective metadata, and support/manual-
  review path when one is approved. Missing policy/profile/provenance produces
  a blocking `DECISION_DATA_UNAVAILABLE` reason, never accidental eligibility.
- FR-7: Client filtering MUST NOT substitute for server eligibility.
- FR-8: Stable sorting and bounded pagination MUST be supported. Page defaults
  to 1, pageSize defaults to 20, maximum pageSize is 100, and invalid or
  oversized values return 400 PAGE_SIZE_INVALID. The default stable order is
  normalized course code then immutable offering ID; any selected sort adds
  offering ID as the final tie-break. The response MUST use the canonical
  SPEC-006 `Page<OfferingEligibilityDto>` and echo the applied canonical sort.
  Search text is at most 100 characters.

## Non-Functional Requirements

- NFR-1: Discovery SHOULD respond within 300 ms p95 at 300 read requests/s.
- NFR-2: Search input MUST be parameterized and limited in length.
- NFR-3: Eligibility MUST be deterministic for a fixed input/version.
- NFR-4: Eligibility/status MUST not rely on color alone.

## Acceptance Criteria

### AC-1: Eligible offering (FR-1, FR-2, FR-5)
Given a student meets prerequisites/load rules and one group is open<br>
When discovery loads<br>
Then the offering is listed with credits and all group staff/location/time
details.

### AC-2: Explain unavailable (FR-4, FR-6)
Given a course requires 96 earned credits and the student has 95<br>
When unavailable offerings are viewed<br>
Then the course is shown with the earned-credit reason, required/current
values, policy version, and source explanation.

### AC-3: Full groups (FR-2)
Given every published group for an otherwise eligible offering is full<br>
When default available discovery loads<br>
Then the offering is not selectable<br>
And its unavailable view states that no group currently has a seat.

### AC-4: Bounded server-authoritative search (FR-3, FR-7, FR-8)
Given a student requests title/day filters with an oversized page<br>
When discovery is evaluated<br>
Then the server rejects the request with 400 PAGE_SIZE_INVALID and no result
data<br>
And a valid bounded request applies eligibility before returning results
stably sorted by the documented tie-break<br>
And client manipulation cannot make an ineligible offering selectable.

### AC-5: Discovery quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given the approved 300-read-per-second fixture, fixed input/version, malicious
search strings, and color-vision/accessibility checks<br>
When discovery quality tests execute<br>
Then response time is at most 300 ms p95<br>
And search is length-bounded and parameterized<br>
And eligibility is deterministic<br>
And every status has text/icon meaning independent of color.

## Edge Cases

- EC-1: Policy/profile data unavailable -> safe unavailable result and support
  reference, never accidental eligibility.
- EC-2: Group becomes full after results load -> details/submit revalidate.
- EC-3: Search contains SQL metacharacters -> treated as literal parameterized
  text.
- EC-4: No eligible offerings -> show the evaluated policy version and reason
  codes, reset-filter action, current window state, and the configured
  Registrar/advisor support path.

## API Contracts

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
  eligible: boolean;
  reasons: EligibilityReasonDto[];
  groups: GroupSummaryDto[];
  evaluatedAtUtc: string;
  academicContextVersion: string;
}
```

The list response is the canonical SPEC-006 `Page<OfferingEligibilityDto>`;
its `sort` field echoes the applied canonical sort. SPEC-011 does not redefine
the shared pagination wrapper.

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, sort, page, and pageSize; and GET
/api/student/offerings/{offeringId}/eligibility for one complete, explained
detail. Both resolve the authenticated student server-side and reject another
student's identifier or an offering outside the authorized registration
context.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| OfferingEligibility.Course | projection | code, title, credits |
| OfferingEligibility.Reasons | array | stable code/message per evaluated rule |
| OfferingEligibility.Groups | array | published selectable detail only |
| OfferingEligibility.Policy references | projection | approved SPEC-009 PolicySet IDs/versions and SPEC-002 provenance; not owned here |

## Out of Scope

- OS-1: Recommendations before a student selects courses.
- OS-2: Search across other colleges/terms unless approved.
- OS-3: Client-authoritative eligibility.
- OS-4: Advisor approval workflow.
