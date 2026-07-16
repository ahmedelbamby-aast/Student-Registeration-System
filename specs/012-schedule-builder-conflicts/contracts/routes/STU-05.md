# STU-05 Registration Review Contribution

**Route:** `/student/review`  
**Canonical page owner:** SPEC-014  
**Contributor:** SPEC-012  
**Contract version:** `spec012-stu05/1.0`

SPEC-012 contributes the current server-authoritative plan validation state to
the future `RegistrationReviewPage.razor`. It does not own or edit that page,
submit a registration, reserve a seat, or permit a conflict override.

## Approved owner APIs

The review page consumes these SPEC-012 endpoints:

- `GET /api/student/terms/{termId}/registration-plan` for the authenticated
  Student's current term-scoped plan and rowversion.
- `POST /api/student/terms/{termId}/registration-plan/validate` for a current,
  non-mutating validation immediately before SPEC-014 submission behavior.

The server derives Student identity and term access. Another Student's plan,
existence, rowversion, group identifiers, and validation details are not
disclosed. Validation never reserves a seat.

## Conflict and review-block contribution

For every `MEETING_OVERLAP`, the contribution preserves:

- both group IDs/codes, course codes, subject titles, and local intervals;
- day plus the exact overlap start and end;
- stable code and privacy-safe message; and
- server-authored `change-group` and `remove-group` actions for either affected
  selection, each with a target group ID, accessible label, and
  application-relative route.

The review renders a red X plus the visible word **Conflict**, names both
subjects/groups and the exact interval, lists every blocker in text, and keeps
Review/submit disabled. Its resolution link returns the Student to
`STU-04 /student/schedule`; there is no override or silent replacement.

## Dependency-version contribution

The validation snapshot carries its evaluated UTC time, academic context,
policy, catalogue, selected offering, and selected group versions.
A changed, full, closed, cancelled, or unpublished selection blocks review
with a safe change/remove path. `STALE_VERSION`, `GROUP_CHANGED`, `GROUP_FULL`,
and `GROUP_UNAVAILABLE` require an authoritative refresh and revalidation; the
client does not convert cached state into success.

## Presentation and ownership boundary

- Calendar and chronological list receive the same canonical meeting collection
  and expose equivalent subject, group, activity, staff,
  room/location, day, local time, timezone, and conflict text.
- The blocker summary precedes the disabled command in DOM/focus order and is
  connected through `aria-describedby`; background refresh does not steal
  focus and submitted validation focuses the summary.
- SPEC-014 alone owns submission, confirmation, idempotency, atomic commit,
  result recovery, and the canonical Razor page.
- The approved `spec012-credit-load/1.0` fields remain server-authored plan
  data: fixed 18/18 values plus sourced load reasons. They do not change
  SPEC-014 page/submission ownership or create a browser GPA/overload path.
