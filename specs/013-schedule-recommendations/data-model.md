# Data Model: Schedule Recommendations

## Owned Transient Values

- **SchedulePreferences**: Registration-module value object with avoided
  weekdays and optional earliest/latest local times; empty preferences are the
  default.
- **ScheduleOption**: Complete transient result; no option row is persisted.
- **ScoreComponent**: Transient deterministic scoring value.
- **OptimizerConfiguration**: Immutable approved factor-order/version value.
- **OptimizationDiagnostic**: Transient inclusion-minimal blocking diagnostic.

## Detailed Model

| Model | Type | Constraints |
|---|---|---|
| SchedulePreferences | value object | unique .NET `DayOfWeek` set 0..6; optional earliest/latest local times inside the term grid; earliest <= latest; empty defaults |
| ScheduleOption | transient result | exactly one group per selected course; protected by signed/encrypted expiring token |
| ScoreComponent | value object | stable factor code, invariant string value, localized-safe explanation |
| OptimizerConfiguration | immutable value | version; factor order PreferenceViolations, IdleMinutes, TeachingDays, StableGroupTuple; Product Owner and Technical Lead approval reference |
| OptimizationDiagnostic | transient value | reason code, member course/group IDs, intervals, actions, inclusion-minimal marker |
| ProtectedOptionPayload | protected value | student, term, plan ID/rowversion, group IDs, academic/catalogue/PolicySet/offering/group/config versions, correlation, issued/expiry |

## Integrity Rules

- The option token uses Data Protection purpose
  `Registration.ScheduleOption.v1`, expires 10 minutes after issue, and binds
  the complete protected payload.
- Token validation uses the shared SPEC-018 POC SQL-backed key repository and
  external local certificate, works on every API replica, and fails closed for
  tamper, expiry, ownership, or purpose mismatch. Production key custody is
  outside this demo contract.
- Applying a token re-reads current dependency versions and updates the
  canonical SPEC-012 plan in one optimistic-concurrency mutation.
- Recommendations replace groups only for the existing selected course set and
  preserve SPEC-012's fixed 18/18 plan credit contract.
- Feasible schedules compare factors lexicographically; factors are never
  silently reweighted. Adding or reordering a factor requires a new version,
  Product Owner/Technical Lead approval, contract fixtures, and an ADR/spec
  update.
- These values are transient; they have no EF table, foreign-key, deletion, or
  retention lifecycle and never represent a seat reservation.
