# Data Model: Eligibility and Subject Discovery

## Ownership and References

SPEC-011 owns the immutable `OfferingEligibility`, `EligibilityReason`, and
`GroupSummary` evaluation projections. It consumes SPEC-008 academic context,
SPEC-009 `PolicySet` IDs/versions, and SPEC-010 offering/group versions; those
aggregates are not redefined or persisted as SPEC-011-owned entities.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| OfferingEligibility.Course | projection | code, title, credits |
| OfferingEligibility.Load | projection | current-plan credits, projected credits, default target 18, applicable maximum 18 or probation 12 |
| OfferingEligibility.Reasons | array | code, pass/block flags, safe required/current values, message, PolicySet/version/source/effective/support metadata per evaluated rule |
| OfferingEligibility.Groups | array | published and unavailable group summaries with state, selectable flag, capacity/count/seats, staff, activity/room/time, SectionGroup rowversion |
| OfferingEligibility.Context | value | evaluated time plus StudentTermAcademicState/policy/catalogue dependency versions |

## Projection Rules

- Evaluation is stateless and deterministic for the same complete versioned
  input; no eligibility projection is a source of truth for submission.
- Any missing required input produces `DECISION_DATA_UNAVAILABLE` and an
  ineligible result.
- Group summaries preserve SPEC-010 IDs/versions; details and final submission
  re-resolve them.
- Pages are evaluated and filtered server-side, default to 20, reject more
  than 100, and use offering ID as the stable final sort key.
