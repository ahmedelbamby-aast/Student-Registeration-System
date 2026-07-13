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
  score: number;
  scoreExplanation: Array<{
    factor: "preference-violations" | "idle-minutes" | "teaching-days" | "stable-group-tuple";
    value: number | string;
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
  planRowVersion: string;
  catalogueVersion: string;
  groupVersionSetHash: string;
  policyVersion: string;
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

`MeetingIntervalDto.dayOfWeek` is 1..7 and its `HH:mm` interval is interpreted
in the academic-term timezone with end greater than start. Preferences default
to an empty avoided-day set and no time bounds. The immutable MVP
`OptimizerConfiguration` compares the four score factors in the declared
order with no numeric weights; Product Owner and Technical Lead approval is
required for every new configuration version.

## Endpoints

### POST /api/student/terms/{termId}/registration-plan/recommendations

The server derives the authenticated student and the one plan for route
`termId`; no student/plan identifier is accepted. Success is 200. A stale plan or
captured dependency returns 409 `PLAN_CHANGED` or `STALE_INPUT`. The
response contains only complete verified options and/or detailed diagnostics;
a time-budget response never labels an incomplete option valid.

### PUT /api/student/terms/{termId}/registration-plan/recommended-option

The server again derives student and term plan. The token is protected with
purpose `Registration.ScheduleOption.v1`, is
valid for 10 minutes, and binds the authenticated student, plan, complete group
payload, captured versions, correlation ID, issue time, and expiry. Any API
replica validates it using the shared SPEC-018 key repository; option lookup,
sticky sessions, and a durable option table are prohibited.

- 200: atomic plan update with the new rowversion.
- 400 `INVALID_OPTION_TOKEN`: malformed, wrong-purpose, or tampered token.
- 404: plan/token owner mismatch, without disclosing another student's data.
- 409 `OPTION_EXPIRED`, `PLAN_CHANGED`, or `STALE_INPUT`: safe retry or
  recomputation is required; no plan mutation occurs.

## Diagnostic Semantics

“Inclusion-minimal” means removing any member makes the reported hard conflict
cease to hold. Diagnostics list stable reason codes, all involved intervals,
and a change-group/remove-course action for every member. They are ordered by
member count and then stable course/group identifiers.

## Shared Rules

All operations use server authentication/authorization, ISO-8601 server time,
stable privacy-safe errors, bounded payloads, and correlation IDs.
