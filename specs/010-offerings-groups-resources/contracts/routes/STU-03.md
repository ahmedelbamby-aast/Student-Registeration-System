# STU-03 Subject Details Contribution

**Route:** `/student/subjects/{offeringId}`  
**Canonical page owner:** SPEC-011  
**Contributor:** SPEC-010  
**Contract version:** `spec010-stu03/1.0`

SPEC-010 contributes the published offering and group scheduling projection
consumed by the future `SubjectDetailsPage.razor`. It does not own or edit that
page and does not make SPEC-011 eligibility decisions.

## Approved owner API

The page consumes:

- `GET /api/offerings/{offeringId}` for the published offering and its groups.
- `GET /api/groups/{groupId}` when the page refreshes one group immediately
  before a selection action.

Both endpoints require the server-issued `Catalogue.ReadAvailable` permission
for a Student. The server is authoritative for lifecycle, capacity,
`selectable`, and `nonSelectableReasons`.

## Required scheduling data

For each group the page receives and presents:

- group ID, code, capacity, enrolled count, registration pause, lifecycle
  state, selectability, stable non-selectable reasons, and rowversion;
- every meeting ID and canonical activity value (`Lecture`, `Tutorial`, or
  `Laboratory`);
- unambiguous day, local start/end time, room code, and location interpreted
  in the configured academic-term timezone; and
- every meeting-specific staff assignment with activity, role, and display
  name.

The page may display the canonical API value `Tutorial` as the user-facing
label `Section`. It must retain `Tutorial` in client state and must not rewrite
the serialized API value.

## Selection and concurrent-change behavior

- `GROUP_FULL`, `GROUP_UNPUBLISHED`, `GROUP_CLOSED`, `GROUP_CANCELLED`, and
  `REGISTRATION_PAUSED` are rendered as text and not by color alone.
- A group with any current Scheduling-owned non-selectable reason has no
  enabled selection action.
- The rowversion is advisory input for SPEC-012/SPEC-014. A later
  `GROUP_CHANGED` response requires a refresh and review; the page never
  claims a queued or successful selection.
- Student GET responses never use `GROUP_CHANGED`; that code belongs to a
  later write/concurrency response.

## Scope boundary

SPEC-011 adds course credits, projected load, policy provenance, prerequisites,
holds, and other eligibility explanations. SPEC-010 contributes only current
offering/group/resource facts and does not turn an ineligible offering into an
eligible one.

