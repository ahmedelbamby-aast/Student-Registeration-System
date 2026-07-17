# Research: Lecturer and Teaching Assistant Workspace

## Decisions

### Availability ownership

**Decision**: SPEC-010/Scheduling canonically owns StaffTermAvailability and
StaffAvailability. SPEC-016 invokes a narrow Scheduling application port for
complete-range replacement and never maps a duplicate StaffAdministration
aggregate.

**Rationale**: Publication already validates staff/resource availability.
Keeping one aggregate owner removes the former downstream dependency cycle and
gives availability edits and group publication one SQL serialization boundary.

Admin consumes SPEC-010's bounded read-only view and may select an aggregate ID
and rowversion as an immutable offering-planning dependency. The POC
deliberately has no range copy and no
Admin availability mutation/correction/override route, permission, editable
control, notification workflow, or correction-audit flow.

### Durable schedule impact

**Decision**: If an accepted availability update conflicts with a published
assignment, the SPEC-010 Scheduling transaction creates an idempotent Open
ScheduleImpactAlert with captured group/availability versions. SPEC-016
consumes its state and SPEC-017 exposes Admin discovery/revalidation. No
automatic move occurs.

**Rationale**: A transient toast can be lost and cannot prove revalidation.
One Scheduling-owned durable versioned state survives replicas/restarts,
supports audit, and avoids duplicate ownership.

The alert is schedule-impact state, not a notification or authorization for an
Admin availability correction workflow.

### Minimal roster

**Decision**: Return only UniversityId, DisplayName, and EnrollmentState in a
default-20/max-100 page, ordered by DisplayName then UniversityId. Deny before
query when assignment scope is absent and audit access metadata without row
content.

**Rationale**: These are sufficient to identify an active roster member while
excluding GPA, holds, contacts, grades, and transcript.

### Shared UI

**Decision**: Lecturer and TA use the same components; the server-provided
assignment role and scope drive content. Role context remains visible and all
calendar views have keyboard/list equivalents.

## Open Research

Institutional availability deadlines remain effective-dated configuration with
approved provenance. Unknown values fail closed; this spec does not invent
them.
