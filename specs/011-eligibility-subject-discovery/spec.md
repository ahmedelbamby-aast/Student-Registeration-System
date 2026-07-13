# Feature Specification: Eligibility and Subject Discovery

**Feature Branch**: 011-eligibility-subject-discovery
**Created**: 2026-07-12
**Status**: APPROVED
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

Students need to see available subjects based on program, GPA, standing,
earned credits, prerequisites, repeats, holds, term, published groups, and
policy. The system must explain both eligible and unavailable outcomes.
The demo evaluates the approved `DEMO-POC-2026.1` rules against the exact
curated catalogue in `docs/DEMO_CURRICULUM.md`.

## User Scenarios and Testing

### User Story 1 - Eligible offering (FR-1, FR-2, FR-5) (P1)

As a Student, I need the Eligible offering (FR-1, FR-2, FR-5) behavior so that Eligibility and Subject Discovery produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a normal-standing student has 15 selected credits, meets the next
three-credit course prerequisites, and one complete group is open<br>
When discovery loads<br>
Then the offering is listed with a projected 18 of 18 credits<br>
And all Lecture/Tutorial/Laboratory staff, location, and time details are shown.
### User Story 2 - Explain unavailable (FR-4, FR-6) (P1)

As a Student, I need the Explain unavailable (FR-4, FR-6) behavior so that Eligibility and Subject Discovery produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a course requires 96 earned credits and the student has 95<br>
When unavailable offerings are viewed<br>
Then the course is shown with the earned-credit reason, required/current
values, policy version, and source explanation.
### User Story 3 - Full groups (FR-2) (P2)

As a Student, I need the Full groups (FR-2) behavior so that Eligibility and Subject Discovery produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given every published group for an otherwise eligible offering is full<br>
When default available discovery loads<br>
Then the offering is not selectable<br>
And its unavailable view states that no group currently has a seat.
### User Story 4 - Bounded server-authoritative search (FR-3, FR-7, FR-8) (P2)

As a Student, I need the Bounded server-authoritative search (FR-3, FR-7, FR-8) behavior so that Eligibility and Subject Discovery produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a student requests title/day filters with an oversized page<br>
When discovery is evaluated<br>
Then the server applies eligibility before returning bounded, stably sorted
results<br>
And client manipulation cannot make an ineligible offering selectable.
### User Story 5 - Discovery quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Student, I need the Discovery quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Eligibility and Subject Discovery produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

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
  Registrar support path.

## Requirements

### Functional Requirements

- FR-1: The system MUST evaluate every relevant approved rule on the server.
  For `DEMO-POC-2026.1`, this includes configured window, standing, blocking
  hold, prerequisite, course GPA/earned-credit, current-plan load, published
  capacity, and exact meeting-conflict checks. A normal plan targets and caps
  at 18 credits; GPA below 2.0 caps at 12 credits. Advisor/exception workflows
  are not evaluated because they are outside the demo.
- FR-2: Default discovery MUST list eligible offerings having at least one
  published selectable group.
- FR-3: Students MUST be able to search by code/title and filter by
  eligibility, credits, day, and availability.
- FR-4: Students MUST be able to inspect unavailable offerings and every
  blocking reason.
- FR-5: Results MUST show course code/title/credits and group capacity, staff,
  location, activity, day/time, state, seats remaining, and advisory version.
  Each offering MUST also show current-plan credits, projected credits if
  selected, default target 18, and the applicable maximum 18 or 12.
- FR-6: Each decision MUST include complete stable per-rule explanations,
  approved PolicySet/source metadata, safe required/current values, and a
  fail-closed reason/support path when decision data is unavailable.
- FR-7: Client filtering MUST NOT substitute for server eligibility.
- FR-8: Sorting/pagination MUST use page >= 1, default 20, maximum 100,
  400 PAGE_SIZE_INVALID for invalid values, and immutable offering ID as the
  final tie-break; the response consumes canonical SPEC-006
  `Page<OfferingEligibilityDto>` and echoes its applied sort; search text is
  limited to 100 characters.

### Non-Functional Requirements

- NFR-1: Discovery SHOULD respond within 300 ms p95 at 300 read requests/s.
- NFR-2: Search input MUST be parameterized and limited in length.
- NFR-3: Eligibility MUST be deterministic for a fixed input/version.
- NFR-4: Eligibility/status MUST not rely on color alone.

### Key Projections and References

- **OfferingEligibility**, **EligibilityReason**, and **GroupSummary** are
  SPEC-011-owned immutable response/domain projections.
- Catalogue `CourseOffering`/`SectionGroup` data is consumed from SPEC-010,
  student/context data from SPEC-008, and approved `PolicySet` versions and
  provenance from SPEC-009/SPEC-002. SPEC-011 does not own or redefine a
  separate PolicyVersion entity.

## Success Criteria

- **SC-1**: Every available or unavailable subject has an understandable reason.
- **SC-2**: Students can find relevant offerings within the approved response-time target.
- **SC-3**: No client-side action can turn an ineligible offering into an eligible one.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STU-02 | /student/subjects | SubjectDiscoveryPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-011 |
| STU-03 | /student/subjects/{offeringId} | SubjectDetailsPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-011 |

## Out of Scope

- OS-1: Recommendations before a student selects courses.
- OS-2: Search across other colleges/terms unless approved.
- OS-3: Client-authoritative eligibility.
- OS-4: Advisor approval workflow.
