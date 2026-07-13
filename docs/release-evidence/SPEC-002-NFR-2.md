# SPEC-002 NFR-2 Boundary Regression Evidence

**Requirement:** All boundary examples supplied by the Registrar MUST have automated regression tests.  
**Measured:** 2026-07-13  
**Configuration:** Release, .NET 10.0.9, x64  
**Automated test:** `tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-2EvidenceTests.cs`

## Method

The single xUnit method
`Every_approved_PB_01_through_PB_19_input_executes_with_its_governed_outcome`
parsed the exact, unique ID, **Relevant input**, expected result, and stable
reason from every approved Markdown row. For each of the 19 rows, the test
first matched the complete Relevant input text and then matched its typed
executable representation: GPA, planned credits, window state, standing,
hold states, prerequisite state, earned-credit values, capacity values,
repeat state, candidate count, and complete meeting sequence. It then
executed the evaluator once and compared eligibility and reason to the
governed row.

PB-11 additionally proves that its two exact intervals overlap. PB-12 proves
that one interval ends at 10:00 exactly when the differently located next
interval begins and that this adjacency is not an overlap. PB-18 proves zero
candidate rulebooks, zero matches, and an Admin alert. PB-19 proves two
distinct, approved, published candidates with equal scope, priority, and
effective period, then separately submits those candidates to publication
validation and requires `POLICY_SCOPE_AMBIGUOUS` rejection.

## Executed fixtures

| Fixture | Typed Relevant input verified | Expected eligibility | Expected stable reason | Result |
|---|---|---:|---|---|
| PB-01 | Active; GPA 2.50; 8 credits | False | `LOAD_BELOW_MINIMUM` | Pass |
| PB-02 | Active; GPA 2.50; 9 credits | True | `LOAD_ALLOWED` | Pass |
| PB-03 | Active; GPA 2.50; 18 credits | True | `LOAD_ALLOWED` | Pass |
| PB-04 | Active; GPA 2.50; 19 credits | False | `LOAD_ABOVE_NORMAL_MAXIMUM` | Pass |
| PB-05 | Active; GPA 1.99; 12 credits | True | `PROBATION_LOAD_ALLOWED` | Pass |
| PB-06 | Active; GPA 1.99; 13 credits | False | `PROBATION_LOAD_EXCEEDED` | Pass |
| PB-07 | Active; GPA exactly 2.00; 18 credits | True | `LOAD_ALLOWED` | Pass |
| PB-08 | Exact DS421/DS413 text; `PrerequisitesMet=false` | False | `PREREQUISITE_NOT_COMPLETED` | Pass |
| PB-09 | GPA 2.40; earned 95; required 96; exact DS413 text | False | `MINIMUM_EARNED_CREDITS_NOT_MET` | Pass |
| PB-10 | Capacity 30; committed 30 | False | `GROUP_FULL` | Pass |
| PB-11 | Sunday 09:00-10:00 A and 09:30-10:30 B; exact overlap | False | `MEETING_CONFLICT` | Pass |
| PB-12 | Sunday 09:00-10:00 A and 10:00-11:00 B; different locations; adjacent only | True | `NO_MEETING_CONFLICT` | Pass |
| PB-13 | `WithinRegistrationWindow=false` | False | `REGISTRATION_WINDOW_CLOSED` | Pass |
| PB-14 | Current hold; `HasBlockingHold=true` | False | `REGISTRATION_HOLD` | Pass |
| PB-15 | Current hold; `HasBlockingHold=false` | True | `NO_BLOCKING_HOLD` | Pass |
| PB-16 | `StandingCode=Unknown` | False | `ACADEMIC_STANDING_UNAVAILABLE` | Pass |
| PB-17 | `RequiresRepeatInterpretation=true` | False | `REPEAT_POLICY_UNAVAILABLE` | Pass |
| PB-18 | Candidate rulebooks 0; matches 0; Admin alert true | False | `POLICY_UNAVAILABLE` | Pass |
| PB-19 | Two distinct approved/published candidates; equal scope/priority/period; publication rejected | False | `POLICY_SCOPE_AMBIGUOUS` | Pass |

## Measured result

| Measure | Value |
|---|---:|
| xUnit test methods | 1 |
| Required fixture IDs | 19 |
| Governed Relevant input rows parsed | 19 |
| Complete Relevant input strings matched | 19 |
| Typed executable fixture inputs matched | 19 |
| Executed fixture inputs | 19 |
| PB-18 zero-candidate/Admin-alert scenarios checked | 1 |
| PB-19 ambiguous publication validations executed | 1 |
| Missing or duplicate IDs | 0 |
| Relevant-input mismatches | 0 |
| Eligibility/reason mismatches | 0 |

**Result: PASS.** Every approved PB-01 through PB-19 boundary fixture executed
from its exact governed Relevant input and returned its approved eligibility
and stable reason. The same test also rejected the PB-19 candidate set at
publication time.

This evidence verifies the governed SPEC-002 test contract. SPEC-009 must run
the same immutable fixtures against its runtime evaluator before release.
