# DEMO-POC-2026.1 Rule Coverage

**Contract:** `policy-rule-coverage/1.0`
**Rule schema:** `schemas/policy-rule-definition.schema.json`
**Runtime evaluator owner:** SPEC-009

## Required typed categories

| Type key | Demo configuration | Pass/block outcome |
|---|---|---|
| `RegistrationWindow` | A configured term `OpensUtc` and `ClosesUtc` must contain server time | Outside/missing window blocks |
| `AcademicStanding` | `Active` is allowed; any absent/unknown code fails closed | Unknown standing blocks |
| `BlockingHold` | Any current hold whose `blocksRegistration=true` | Blocking hold blocks; taxonomy is not guessed |
| `Prerequisite` | Completed-course, MinimumGpa, and MinimumEarnedCredits conditions | Any unmet condition blocks |
| `CreditLoad` | regular minimum 9, displayed target 18, hard maximum 18 | Below 9 or above 18 blocks |
| `ProbationLoad` | GPA below 2.0 has hard maximum 12 | Above 12 blocks; GPA exactly 2.0 uses normal load |
| `RepeatEligibility` | `FailClosedPendingApproval` | Any repeat interpretation blocks with `REPEAT_POLICY_UNAVAILABLE` |
| `Capacity` | `FirstSuccessfulCommit`, waitlist false, override false | No remaining seat blocks; SQL commit determines winner |
| `MeetingConflict` | Unresolved exact overlap blocks; travel buffer 0; override false | Exact overlap blocks; adjacent meetings do not |

Every blocking result has `overridePossible=false`. Rule order cannot make an
invalid plan valid; the runtime evaluator returns all applicable reasons in a
stable type-key/reason-code order.

## Curriculum coverage

The published demo catalogue contains exactly 19 rows from
`docs/DEMO_CURRICULUM.md`.

- Code, title, sequence, and displayed prerequisite/condition fields are
  `OfficialAASTMT` and reference `SRC-DATA-SCIENCE`.
- Every `Credits=3` field is `SyntheticDemo` and references
  `DEMO-CREDITS-3`; the public page does not supply that value.
- DS413 exercises MinimumGpa 2.0 and MinimumEarnedCredits 96.
- DS421 exercises the completed-course prerequisite DS413.
- Every code prerequisite resolves inside the curated graph, which is acyclic.
- Additional wholly synthetic rows are forbidden unless their code begins
  `DEMO-`, their provenance is `SyntheticDemo`, and their UI/admin detail says
  they are not AASTMT-published.

## Deliberately disabled behavior

The profile contains no enabled waitlist, reservation, capacity/conflict
override, automatic exception, passed/failed-course repeat workflow, advisor
workflow, add/drop, withdrawal, correction, or travel-time rule. Disabled or
unapproved behavior never falls through to an implicit allowance.
