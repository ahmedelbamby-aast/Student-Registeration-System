# Canonical Demo Policy Rulebook

**Schema:** `policy-rulebook/1.0`
**Rulebook version:** `DEMO-POC-2026.1`
**Approval state:** Published
**Approved by:** Ahmed ELbamby
**Approved at:** 2026-07-13
**Effective period:** `EffectiveFromUtc=2026-07-13T00:00:00Z`, `EffectiveToUtc=null`
**Priority:** 100 inside the synthetic demo scope
**Runtime PolicySet and PolicyRule owner: SPEC-009**
**Historical decision snapshot owner: SPEC-015**

This governed artifact is approved only for the Development and Testing
proof-of-concept populations. It is not official AASTMT production policy and
owns no API handler, runtime entity, database mapping, or executable rule code.

## Academic scope

| Dimension | Required match |
|---|---|
| Environment | Development or isolated Testing |
| Population | Wholly synthetic demo identities |
| College | College of Artificial Intelligence demo configuration |
| Program | Data Science 19-course snapshot |
| Term | Explicitly configured demo AcademicTerm and registration window |
| Standing | `Active`; GPA below 2.0 additionally invokes probation load |

Any absent/unknown standing, environment, term, window, provenance, or policy
match fails closed. This rulebook is never a global fallback.

## Typed rule definitions

Rules conform to `schemas/policy-rule-definition.schema.json` and are evaluated
in `schemas/policy-rule-types.json` order.

### RegistrationWindow

```json
{"typeKey":"RegistrationWindow","reasonCode":"REGISTRATION_WINDOW_CLOSED","configuration":{"configuredTermWindowRequired":true},"sourceRef":"DEMO-APPROVAL-2026.1","provenanceKind":"AhmedApprovedDemo","runtimeOwner":"SPEC-009"}
```

Server time must be within the configured term OpensUtc/ClosesUtc.

### AcademicStanding

```json
{"typeKey":"AcademicStanding","reasonCode":"ACADEMIC_STANDING_UNAVAILABLE","configuration":{"allowedStandingCodes":["Active"],"unknownFailsClosed":true},"sourceRef":"DEMO-APPROVAL-2026.1","provenanceKind":"AhmedApprovedDemo","runtimeOwner":"SPEC-009"}
```

### BlockingHold

```json
{"typeKey":"BlockingHold","reasonCode":"REGISTRATION_HOLD","configuration":{"blockingFlagField":"blocksRegistration","activeBlockingValue":true},"sourceRef":"DEMO-APPROVAL-2026.1","provenanceKind":"AhmedApprovedDemo","runtimeOwner":"SPEC-009"}
```

Hold codes remain data; the rule does not guess a financial/academic taxonomy.

### Prerequisite

```json
{"typeKey":"Prerequisite","reasonCode":"PREREQUISITE_NOT_COMPLETED","configuration":{"completedCourseRequired":true,"conditionKinds":["CompletedCourse","MinimumGpa","MinimumEarnedCredits"]},"sourceRef":"SRC-DATA-SCIENCE","provenanceKind":"OfficialAASTMT","runtimeOwner":"SPEC-009"}
```

General prerequisite completion uses `SRC-GENERAL-2016`; course conditions use
the field-level source record for the selected curriculum row.

### CreditLoad

```json
{"typeKey":"CreditLoad","reasonCode":"LOAD_ABOVE_NORMAL_MAXIMUM","configuration":{"minimumCredits":9,"targetCredits":18,"maximumCredits":18},"sourceRef":"SRC-GENERAL-2016","provenanceKind":"OfficialAASTMT","runtimeOwner":"SPEC-009"}
```

Ahmed's demo approval adopts 18 as the displayed default/recommended target and
suppresses the College page's usual 12-credit guidance.

### ProbationLoad

```json
{"typeKey":"ProbationLoad","reasonCode":"PROBATION_LOAD_EXCEEDED","configuration":{"gpaBelow":2.0,"maximumCredits":12},"sourceRef":"SRC-GENERAL-2016","provenanceKind":"OfficialAASTMT","runtimeOwner":"SPEC-009"}
```

No advisor workflow is enabled in this profile.

### RepeatEligibility

```json
{"typeKey":"RepeatEligibility","reasonCode":"REPEAT_POLICY_UNAVAILABLE","configuration":{"mode":"FailClosedPendingApproval"},"sourceRef":"UNRESOLVED-REPEAT-WORKFLOW","provenanceKind":"UnresolvedInstitutional","runtimeOwner":"SPEC-009"}
```

Any earlier-attempt condition requiring repeat interpretation blocks. This is
not an interpretation of the sourced institutional repeat rule. The stable
blocking reason is `REPEAT_POLICY_UNAVAILABLE`.

### Capacity

```json
{"typeKey":"Capacity","reasonCode":"GROUP_FULL","configuration":{"allocationMode":"FirstSuccessfulCommit","waitlistEnabled":false,"overrideEnabled":false},"sourceRef":"DEMO-CAPACITY-FIRST-COMMIT","provenanceKind":"AhmedApprovedDemo","runtimeOwner":"SPEC-009"}
```

The final successful SQL commit allocates a seat; browser availability is only
advisory.

### MeetingConflict

```json
{"typeKey":"MeetingConflict","reasonCode":"MEETING_CONFLICT","configuration":{"unresolvedExactOverlapBlocks":true,"travelBufferMinutes":0,"overrideEnabled":false},"sourceRef":"DEMO-CONFLICT-EXACT","provenanceKind":"AhmedApprovedDemo","runtimeOwner":"SPEC-009"}
```

## Decision output

Each evaluation returns eligibility, policy version, server evaluation time,
privacy-safe input summary, approval/effective metadata, and stable ordered
results containing reason, pass/block, explanation, source URL, source access
date, and `overridePossible=false` for a blocker.

## Immutability and supersession

Published versions are immutable. `DEMO-POC-2026.1` MUST NOT be edited and
MUST NOT be deleted. A correction uses an expected version to create and
approve a new rulebook, then marks this one Superseded only after the successor
is published.

SPEC-015 retains the original policy version, input summary, source URL, source access date,
explanations, and results for every historical registration
decision. Supersession never rewrites those records.

## Explicit exclusions

No waitlist, reservation, capacity/conflict override, automatic exception,
advisor approval, repeat workflow, add/drop, withdrawal, correction, or travel
buffer is enabled. An absent rule never means allowed.
