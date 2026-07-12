# Data Model: Eligibility and Subject Discovery

## Owned Entities

- **OfferingEligibility**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **EligibilityReason**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupSummary**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyVersion**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| OfferingEligibility.Course | projection | code, title, credits |
| OfferingEligibility.Reasons | array | stable code/message per evaluated rule |
| OfferingEligibility.Groups | array | published selectable detail only |
| OfferingEligibility.PolicyVersion | string | required |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
