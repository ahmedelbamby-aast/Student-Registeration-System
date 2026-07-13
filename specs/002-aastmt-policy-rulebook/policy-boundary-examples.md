# Approved Policy Boundary Examples

**Fixture schema:** `policy-boundary-examples/1.0`
**Profile:** `DEMO-POC-2026.1`
**Approved by:** Ahmed ELbamby for the non-production demo on 2026-07-13

Each row is deterministic: the same input and immutable policy version must
produce the same pass/block result and reason code. Values not present in the
input are assumed valid only when the row explicitly says “all other gates
pass.” Runtime execution belongs to SPEC-009.

## Registrar-approved examples

| ID | Boundary | Relevant input | Expected result | Stable reason |
|---|---|---|---|---|
| PB-01 | Below 9-credit minimum | Active, GPA 2.50, regular plan 8 credits; all other gates pass | Block | `LOAD_BELOW_MINIMUM` |
| PB-02 | 9-credit minimum | Active, GPA 2.50, regular plan 9 credits; all other gates pass | Pass | `LOAD_ALLOWED` |
| PB-03 | 18-credit normal maximum and target | Active, GPA 2.50, regular plan 18 credits; all other gates pass | Pass | `LOAD_ALLOWED` |
| PB-04 | Above normal maximum | Active, GPA 2.50, regular plan 19 credits | Block | `LOAD_ABOVE_NORMAL_MAXIMUM` |
| PB-05 | 12-credit probation maximum | Active, GPA 1.99, regular plan 12 credits; all other gates pass | Pass | `PROBATION_LOAD_ALLOWED` |
| PB-06 | Above probation maximum | Active, GPA 1.99, regular plan 13 credits | Block | `PROBATION_LOAD_EXCEEDED` |
| PB-07 | GPA threshold equality | Active, GPA 2.00, regular plan 18 credits; all other gates pass | Pass | `LOAD_ALLOWED` |
| PB-08 | Missing prerequisite | DS421 selected without completed DS413 | Block | `PREREQUISITE_NOT_COMPLETED` |
| PB-09 | DS413 earned-credit condition | GPA 2.40, 95 earned credits, DS413 selected | Block | `MINIMUM_EARNED_CREDITS_NOT_MET` |
| PB-10 | No remaining capacity | Published group capacity 30 with 30 committed enrollments | Block | `GROUP_FULL` |
| PB-11 | Exact meeting overlap | Two selected meetings share at least one instant | Block | `MEETING_CONFLICT` |
| PB-12 | Zero travel buffer | First meeting ends 10:00 and next begins 10:00 at another location; no exact overlap | Pass | `NO_MEETING_CONFLICT` |
| PB-13 | Closed registration window | Server time is outside configured OpensUtc/ClosesUtc | Block | `REGISTRATION_WINDOW_CLOSED` |
| PB-14 | Active blocking hold | One current hold has `blocksRegistration=true` | Block | `REGISTRATION_HOLD` |
| PB-15 | Non-blocking hold | One current hold has `blocksRegistration=false`; all other gates pass | Pass | `NO_BLOCKING_HOLD` |
| PB-16 | Unknown standing | Standing code is absent or not in the approved policy value set | Block | `ACADEMIC_STANDING_UNAVAILABLE` |
| PB-17 | Repeat policy unavailable | Selected course has any earlier attempt requiring repeat interpretation | Block | `REPEAT_POLICY_UNAVAILABLE` |
| PB-18 | Missing approved policy | No approved effective rulebook matches student and term | Block and alert Admin | `POLICY_UNAVAILABLE` |
| PB-19 | Equal applicable priority | Two approved rulebooks match the same context at the same highest priority | Reject publication/evaluation | `POLICY_SCOPE_AMBIGUOUS` |

## Required demonstrations

- 9-credit minimum: PB-01/PB-02.
- 12-credit probation maximum: PB-05/PB-06.
- 18-credit normal maximum: PB-03/PB-04.
- Missing prerequisite: PB-08.
- No remaining capacity: PB-10.
- Exact meeting overlap: PB-11.
- Zero travel buffer: PB-12; adjacency is not an overlap.
- Repeat policy unavailable: PB-17 fails closed instead of inventing an
  institutional repeat workflow.

`overridePossible` is false for every blocking result in this profile.
