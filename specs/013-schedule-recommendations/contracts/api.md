# API Contract: Schedule Recommendations

## Feature Contract

```typescript
interface MeetingIntervalDto {
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
}
interface SchedulePreferencesDto {
  avoidedWeekdays: number[];
  earliestPreferredStartLocal?: string;
  latestPreferredEndLocal?: string;
}
interface ScheduleOptionDto {
  optionToken: string;
  rank: number;
  groups: GroupDto[];
  scoreExplanation: Array<{
    factor: "preference-violations" | "idle-minutes" | "teaching-days" | "stable-group-tuple";
    value: string;
    message: string;
  }>;
}
interface OptimizationDiagnosticDto {
  diagnosticId: string;
  reasonCode: string;
  members: Array<{
    courseId: string;
    groupId?: string;
    interval?: MeetingIntervalDto;
    action: "change-group" | "remove-course";
  }>;
  conflictingInterval?: MeetingIntervalDto;
  minimality: "inclusion-minimal";
}
interface OptimizationResultDto {
  requestCorrelationId: string;
  planId: string;
  planRowVersion: string;
  academicContextVersion: string;
  catalogueVersion: string;
  policySetId: string;
  policyVersion: string;
  offeringVersions: Record<string, string>;
  groupVersions: Record<string, string>;
  optimizerConfigurationVersion: string;
  status: "complete" | "no-solution" | "time-budget";
  options: ScheduleOptionDto[];
  diagnostics: OptimizationDiagnosticDto[];
  evaluatedAtUtc: string;
}
interface RecommendScheduleRequest {
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
  preferences: SchedulePreferencesDto;
}
interface ApplyScheduleOptionRequest {
  optionToken: string;
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
}
```

`MeetingIntervalDto.dayOfWeek` and `SchedulePreferencesDto.avoidedWeekdays`
use the existing .NET `DayOfWeek` serialization `0..6`. Each `HH:mm` interval
is interpreted in the academic-term timezone with end greater than start.
Preferences default to an empty avoided-day set and no time bounds. The immutable MVP
`OptimizerConfiguration` compares the four score factors in the declared
order with no numeric weights; Product Owner and Technical Lead approval is
required for every new configuration version.

The response and protected token bind one coherent dependency snapshot:
authenticated student, route term, plan ID and rowversion, academic-context
version, catalogue version, PolicySet ID/version, exact offering/group version
maps, complete group selection, optimizer-configuration version, correlation
ID, issue time, and expiry. The explicit bounded maps avoid an undefined
aggregate hash and use stable identifier ordering when serialized.

Score-component values are strings so the contract remains closed and
deterministic: numeric factors use invariant integer text and the stable group
tuple uses its canonical identifier text. Ranking uses typed server values,
never client parsing or numeric weights.

## Endpoints

### POST /api/student/terms/{termId}/registration-plan/recommendations

The server derives the authenticated student and the one plan for route
`termId`; no student/plan identifier is accepted. A successful request returns
`200 OptimizationResultDto`. The response contains only complete verified
options and/or detailed diagnostics; a time-budget response never labels an
incomplete option valid.

| Outcome | Status and body |
|---|---|
| Complete, no-solution, or time-budget result | `200 OptimizationResultDto` |
| Malformed request, invalid correlation ID, weekday, or preference bound | `400 VALIDATION_ERROR` as `ApiError` |
| Unauthenticated | `401 ApiError` |
| Authenticated without Student permission | `403 ApiError` |
| Authorized term/plan context absent | `404 REGISTRATION_CONTEXT_NOT_FOUND` as `ApiError` |
| Plan changed before coherent capture | `409 PLAN_CHANGED` as `ApiError` |
| Catalogue, PolicySet, offering, or group input changed during capture | `409 STALE_INPUT` as `ApiError` |
| Bounded request rate exceeded | `429 RATE_LIMITED` as `ApiError` |
| Required dependency unavailable | `503 RECOMMENDATIONS_UNAVAILABLE` as `ApiError` |
| Unexpected failure | `500 INTERNAL_ERROR` as `ApiError` |

### PUT /api/student/terms/{termId}/registration-plan/recommended-option

The server again derives student and term plan. The token is protected with
purpose `Registration.ScheduleOption.v1`, is
valid for 10 minutes, and binds the authenticated student, plan, complete group
payload, captured versions, correlation ID, issue time, and expiry. Any API
replica validates it using the shared SPEC-018 key repository; option lookup,
sticky sessions, and a durable option table are prohibited.

The option groups replace the complete existing selected group set through the
canonical SPEC-012 plan owner. The selected course set and fixed 18/18 plan
credit contract are preserved. A stale plan-store outcome is translated to
`PLAN_CHANGED`.

| Outcome | Status and body |
|---|---|
| Atomic complete plan replacement | `200 RegistrationPlanDto` |
| Malformed request, wrong-purpose token, or tampered token | `400 INVALID_OPTION_TOKEN` as `ApiError` |
| Unauthenticated | `401 ApiError` |
| Authenticated without Student permission | `403 ApiError` |
| Owner/term/plan mismatch | `404 REGISTRATION_CONTEXT_NOT_FOUND` as `ApiError`, without an ownership oracle |
| Token expired | `409 OPTION_EXPIRED` as `ApiError` |
| Current plan rowversion differs | `409 PLAN_CHANGED` as `ApiError` |
| Captured academic/catalogue/policy/offering/group dependency differs | `409 STALE_INPUT` as `ApiError` |
| Bounded request rate exceeded | `429 RATE_LIMITED` as `ApiError` |
| Required dependency unavailable | `503 RECOMMENDATIONS_UNAVAILABLE` as `ApiError` |
| Unexpected failure | `500 INTERNAL_ERROR` as `ApiError` |

## Diagnostic Semantics

“Inclusion-minimal” means removing any member makes the reported hard conflict
cease to hold. Diagnostics list stable reason codes, all involved intervals,
and a change-group/remove-course action for every member. They are ordered by
member count and then stable course/group identifiers.

`remove-course` means removing that course's selected group through the
canonical complete-plan replacement. STU-04 may present the action label as
“Remove group” while preserving the diagnostic action meaning.

## Shared Rules

All operations use server authentication/authorization, ISO-8601 server time,
stable privacy-safe errors, bounded payloads, and correlation IDs. Option
protection uses the SPEC-018 shared POC SQL-backed Data Protection key
repository and external local certificate. Production key custody remains
undecided and is not claimed by this contract.
