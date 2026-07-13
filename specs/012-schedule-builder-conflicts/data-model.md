# Data Model: Schedule Builder and Conflicts

## Canonical Ownership and Consumption

- **RegistrationPlan**, **RegistrationPlanItem**, **ScheduleConflict**, and **ValidationSnapshot** are canonical entities owned by SPEC-012.
- SPEC-013 consumes the versioned plan and produces advisory recommendations; SPEC-014 consumes it at submission and MUST revalidate server-side.
- No consumer may redefine the plan, conflict, or validation snapshot lifecycle.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationPlan | aggregate | unique active student + term; rowversion; total credits; review-blocked state |
| RegistrationPlanItem | entity | unique plan + offering; selected group ID; captured group/offering versions |
| ScheduleConflict | immutable value | both group/course labels and intervals, day, exact overlap, stable code/message, change/remove actions |
| ValidationSnapshot | immutable JSON/value | evaluated UTC time; academic context, policy, catalogue, offering and group versions; advisory only |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- PUT locks the plan root, requires its expected rowversion, validates the
  complete replacement set, and advances one version; item rows are not
  independently versioned or patched.
- Strict overlap uses half-open intervals. Duplicate meeting rows are
  de-duplicated by stable slot identity before pair evaluation.
- Only the authenticated owner and requested authorized term can read/update a
  plan. Validation is side-effect-free and never reserves capacity.
