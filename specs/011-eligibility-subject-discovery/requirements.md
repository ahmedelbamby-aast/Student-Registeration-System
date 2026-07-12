# SPEC-011: Eligibility and Subject Discovery

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Backend, QA<br>
**Target:** Sprint 3<br>
**Dependencies:** SPEC-002, SPEC-008, SPEC-009, SPEC-010, SPEC-018<br>

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
  location, day and time.
- FR-6: Each decision MUST include policy version and stable reasons.
- FR-7: Client filtering MUST NOT substitute for server eligibility.
- FR-8: Stable sorting and bounded pagination MUST be supported.

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
Then the server applies eligibility before returning bounded, stably sorted
results<br>
And client manipulation cannot make an ineligible offering selectable.

## Edge Cases

- EC-1: Policy/profile data unavailable -> safe unavailable result and support
  reference, never accidental eligibility.
- EC-2: Group becomes full after results load -> details/submit revalidate.
- EC-3: Search contains SQL metacharacters -> treated as literal parameterized
  text.
- EC-4: No eligible offerings -> meaningful next action/advisor guidance.

## API Contracts

```typescript
interface OfferingEligibilityDto {
  offeringId: string;
  courseCode: string;
  title: string;
  credits: number;
  eligible: boolean;
  reasons: Array<{ code: string; passed: boolean; message: string }>;
  policyVersion: string;
  groups: GroupDto[];
}
```

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, page, and pageSize.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| OfferingEligibility.Course | projection | code, title, credits |
| OfferingEligibility.Reasons | array | stable code/message per evaluated rule |
| OfferingEligibility.Groups | array | published selectable detail only |
| OfferingEligibility.PolicyVersion | string | required |

## Out of Scope

- OS-1: Recommendations before a student selects courses.
- OS-2: Search across other colleges/terms unless approved.
- OS-3: Client-authoritative eligibility.
- OS-4: Advisor approval workflow.
