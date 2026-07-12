# SPEC-003: UX Storyboard and Accessibility

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** UX Lead<br>
**Reviewers:** Product Owner, QA, Student/Admin/Lecturer/TA representatives<br>
**Target:** Sprint 0<br>

## Context

Registration occurs under time pressure and must communicate eligibility,
capacity, stale data, and conflicts without ambiguity. A calendar-only or
color-only interface would exclude users. The 27 route templates and states
are defined in docs/STORYBOARD.md.

## Functional Requirements

- FR-1: The product MUST implement exactly the 27 MVP route templates in the
  approved storyboard using reusable components.
- FR-2: Every data route MUST implement loading, empty, success, error,
  unauthorized, and stale/concurrent states where applicable.
- FR-3: A hard conflict MUST show icon, Conflict text, involved subjects and
  times, and manual resolution actions.
- FR-4: Submission MUST remain disabled while a hard conflict exists, with the
  reason visible.
- FR-5: Every timetable calendar MUST have an equivalent chronological
  list/table.
- FR-6: Student and shared staff login MUST remain visually and semantically
  distinct.
- FR-7: The authenticated shell MUST display server date/time, term/window,
  user, authorized role context, and session status.

## Non-Functional Requirements

- NFR-1: Critical flows MUST conform to WCAG 2.2 AA.
- NFR-2: All actions MUST be keyboard operable with visible focus.
- NFR-3: Normal text contrast MUST be at least 4.5:1.
- NFR-4: Core pages MUST reflow at 200% zoom without two-dimensional scrolling
  except inherently tabular content.
- NFR-5: Go-live registration usability completion MUST be at least 90% in the
  approved representative UAT sample.

## Acceptance Criteria

### AC-1: Accessible conflict (FR-3, FR-4, NFR-1)
Given two selected groups overlap<br>
When the schedule builder renders<br>
Then a red X and the word Conflict identify the issue<br>
And a screen reader receives subject/day/time details<br>
And submit remains disabled with a visible explanation.

### AC-2: Timetable alternative (FR-5, NFR-1)
Given a student opens a visual weekly calendar<br>
When the student selects list view<br>
Then the same groups, staff, rooms, days and times are presented in
chronological semantic markup.

### AC-3: Route states (FR-2)
Given any route in the approved storyboard<br>
When its UX acceptance review occurs<br>
Then every applicable loading/empty/success/error/denied/stale state has a
design and test identifier.

### AC-4: Distinct entry and authoritative shell (FR-6, FR-7)
Given a user navigates from public entry to an authenticated route<br>
When Student or Staff entry is selected and authentication succeeds<br>
Then the selected entry experience remains distinct<br>
And the shell shows server time, term/window, user, authorized role and
session state.

## Edge Cases

- EC-1: Status changes while keyboard focus is in a group card -> announce
  politely without stealing focus.
- EC-2: Session expires with unsaved plan -> preserve safe draft, authenticate,
  then revalidate before display.
- EC-3: 200% zoom/mobile -> actions remain reachable and labels do not truncate
  critical reasons.
- EC-4: Reduced motion enabled -> disable nonessential transitions.

## API Contracts

UI consumes contracts in SPEC-006 through SPEC-017, including GET
/api/context. UI state mapping from each stable error/reason code is mandatory
in those specs.

## Data Models

| View model | Required data |
|---|---|
| AppContext | server time, timezone, teaching term, registration term/window, user/role |
| UiStatus | code, heading, message, severity, next actions, reference ID |
| ConflictView | subjects/groups, overlap slots, alternatives, resolution links |

## Out of Scope

- OS-1: Final visual brand identity beyond accessible design tokens.
- OS-2: Arabic/RTL delivery; structure MUST remain localization-ready.
- OS-3: Native mobile application.
- OS-4: Drag-and-drop as the only editing mechanism.
