# SPEC-011: Eligibility and Subject Discovery

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Backend, QA<br>
**Target:** Sprint 3<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-006, SPEC-008, SPEC-009, SPEC-010, SPEC-018<br>

**Owner-approved progression amendment (2026-07-20):** Ahmed ELbamby
explicitly approved roadmap-aware term progression, matching-cohort term-1
automatic enrollment, self-registration from term 2 onward, and the bounded
19-21-credit CGPA/per-subject-approval state.

## Context

Students need to see available subjects based on program, GPA, standing,
earned credits, prerequisites, repeats, holds, term, published groups, and
policy. The system must explain both eligible and unavailable outcomes.
The demo evaluates the approved `DEMO-POC-2026.1` rules against the exact
curated catalogue in `docs/DEMO_CURRICULUM.md`.

## Functional Requirements

- FR-1: The system MUST evaluate every relevant approved rule on the server.
  For `DEMO-POC-2026.1`, this includes configured window, standing, blocking
  hold, prerequisite, course GPA/earned-credit, current-plan load, published
  capacity, repeat eligibility, and exact meeting-conflict checks. A normal
  plan targets and caps at 18 credits; GPA below 2.0 caps at 12 credits. A
  passed current transcript leaf for the requested course fails closed with
  `REPEAT_POLICY_UNAVAILABLE`; no generic advisor, repeat, prerequisite-waiver,
  or arbitrary exception workflow is evaluated. The bounded 19-21 per-subject
  approval state is governed by FR-10. Plan input crosses a Registration-owned `ICurrentPlanReader`;
  until SPEC-012 contributes its reader, the live adapter returns a versioned
  empty plan while direct service fixtures prove 15-credit/conflict inputs.
- FR-2: Default discovery MUST list eligible offerings having at least one
  published selectable group.
- FR-3: Students MUST be able to search by code/title and filter by
  eligibility, credits, day, and availability.
- FR-4: Students MUST be able to inspect unavailable offerings and every
  blocking reason.
- FR-5: Results MUST show course code/title/credits and group capacity, staff,
  location, activity, day and time. Staff MUST remain nested under the meeting
  they teach. Each group summary MUST also carry lifecycle-only state,
  selectable flag, seats remaining, stable non-selectable reasons, and
  SectionGroup rowversion; "full" is derived capacity status, not lifecycle.
  Each offering MUST also show current-plan credits, projected credits if
  selected, default target 18, the applicable maximum 18 or 12, and academic,
  catalogue, PolicySet, offering, group, and current-plan dependency versions.
- FR-6: Each evaluated rule MUST return a stable code, passed/failed and
  blocking flags, plain-language message, required/current values when safe,
  approved PolicySet ID/version, source reference/access date, approval
  reference, effective start/end, `overridePossible=false` for a blocker, and
  support/manual-review path when one is approved. The offering carries a
  bounded privacy-safe input summary. Missing policy/profile/provenance
  produces a blocking `DECISION_DATA_UNAVAILABLE` aggregate reason plus the
  applicable governed unavailable code, never accidental eligibility.
- FR-7: Client filtering MUST NOT substitute for server eligibility.
- FR-8: Stable sorting and bounded pagination MUST be supported. Page defaults
  to 1, pageSize defaults to 20, maximum pageSize is 100, and invalid or
  oversized values return 400 PAGE_SIZE_INVALID. The exact allow-listed
  filters/sorts and validation semantics are normative in contracts/api.md;
  every sort adds offering ID as the final tie-break. Search is NFKC-normalized,
  trimmed, literal, parameterized, and at most 100 characters. The response
  MUST use canonical SPEC-006 `Page<OfferingEligibilityDto>` and echo the
  applied canonical sort.
- FR-9: Eligibility MUST consume `CurriculumCourse` as the programme/cohort
  roadmap. Recommended-term-1 roots are automatic-registration subjects for a
  matching term-1 cohort and MUST expose no self-registration action. Student
  self-registration begins at recommended term 2.
- FR-10: From term 2 onward, every roadmap prerequisite and existing hard rule
  MUST pass before a subject can be requested. Every valid self-selected
  subject MUST hold a seat pending per-subject approval, including a plan up to
  the normal 18-credit maximum. A 19-21-credit plan additionally requires CGPA
  at least 3.0 before it can enter that same approval-held state; CGPA below
  3.0 or a plan above 21 credits MUST be rejected.
- FR-11: Group projections MUST include Capacity, EnrolledCount, HeldCount, and
  AvailableCount from one SPEC-010 version. Pending approval MUST use a
  distinct accessible status, MUST NOT expose holder PII, and MUST NOT be
  represented as enrolled until SPEC-014 converts the hold.

## Non-Functional Requirements

- NFR-1: Discovery SHOULD respond within 300 ms p95 at 300 read requests/s.
- NFR-2: Search input MUST be parameterized and limited in length.
- NFR-3: Eligibility MUST be deterministic for a fixed input/version.
- NFR-4: Eligibility/status MUST not rely on color alone.

## Acceptance Criteria

### AC-1: Eligible offering (FR-1, FR-2, FR-5)
Given a normal-standing student has 15 selected credits, meets the next
three-credit course prerequisites, and one complete group is open<br>
When discovery loads<br>
Then the offering is listed with a projected 18 of 18 credits<br>
And all Lecture/Tutorial/Laboratory staff, location, and time details are shown.

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

### AC-6: Roadmap term progression (FR-9, FR-10)
Given matching-cohort term-1 and term-2 students<br>
When discovery evaluates the same published roadmap<br>
Then the term-1 roots are labelled automatically registered with no self
action<br>
And the term-2 student can request only later subjects whose prerequisites and
hard rules pass.

### AC-7: Bounded overload and held capacity (FR-10, FR-11)
Given CGPA 2.99 and 3.00 students with plans at 18 credits<br>
When they evaluate an additional three-credit subject<br>
Then normal-load self-registration is approval-held<br>
And only the CGPA 3.00 result can become approval-held at 21 credits<br>
And 22 credits is rejected<br>
And total/enrolled/held/available capacity remains visible without holder PII.

## Edge Cases

- EC-1: Policy/profile data unavailable -> safe unavailable result and support
  reference, never accidental eligibility.
- EC-2: Group becomes full after results load -> the detail GET refreshes the
  unavailable decision/version; final submission revalidation remains
  SPEC-014-owned.
- EC-3: Search contains SQL metacharacters -> treated as literal parameterized
  text.
- EC-4: No eligible offerings -> show the evaluated policy version and reason
  codes, reset-filter action, current window state, and the configured
  Registrar support path.
- EC-5: Term-1 self-registration deep link -> no duplicate plan or enrollment.
- EC-6: CGPA/credit change during approval-required view -> server refresh
  replaces the stale decision.
- EC-7: Held capacity changes -> refresh counts and group version.

## API Contracts

The complete normative DTO declarations, canonical
`Page<OfferingEligibilityDto>` use, exact query allow-list, authority rules,
and endpoint outcome matrices are defined once in
[contracts/api.md](contracts/api.md). SPEC-011 does not redefine the shared
pagination wrapper.

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, sort, page, and pageSize; and GET
/api/student/offerings/{offeringId}/eligibility for one complete, explained
 detail. Both resolve the authenticated student server-side and reject another
 student's identifier or an offering outside the authorized registration
 context. The complete authorization, query, success, and error matrices are
 normative in contracts/api.md.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| OfferingEligibility.Course | projection | code, title, credits |
| OfferingEligibility.Reasons | array | stable code/message per evaluated rule |
| OfferingEligibility.Groups | array | bounded selectable and unavailable lifecycle/capacity detail with nested meeting staff |
| OfferingEligibility.Policy references | projection | approved SPEC-009 PolicySet IDs/versions and SPEC-002 provenance; not owned here |
| OfferingEligibility.Dependency versions | projection | academic, catalogue, policy, offering, group, and current-plan versions |
| Eligibility read seams | provider ports | narrow Academics/Scheduling readers plus Registration-owned current-plan reader; no writable eligibility/plan table |

## Out of Scope

- OS-1: Recommendations before a student selects courses.
- OS-2: Search across other colleges/terms unless approved.
- OS-3: Client-authoritative eligibility.
- OS-4: Generic advisor, prerequisite-waiver, and arbitrary exception
  workflows. The bounded 19-21-credit CGPA-at-least-3.0 per-subject approval
  flow is in scope through SPEC-014/SPEC-016/SPEC-017.
