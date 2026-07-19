# SPEC-005 Critical Query and Row-Count Inventory

**Inventory version:** `spec005-critical-queries/1.0`  
**Approved:** 2026-07-19 by Ahmed ELbamby in the Data Lead and Operations review perspectives  
**Scope:** non-production POC production-like fixture; this is not an AASTMT production sizing approval

The release gate requires an actual SQL Server execution plan for every listed
query that touches a table forecast above 10,000 rows. The fixture uses the
approved 25,000-account population and deterministic synthetic-only data.
Measured plans are recorded in
`docs/release-evidence/SPEC-005-NFR-1-plans.json`; plan hashes bind the reviewed
operator/index summary to the executed actual ShowPlan XML.

| ID | Critical flow | Canonical query boundary | Forecast rows | Actual plan required | Bound / target |
|---|---|---|---:|---|---|
| CQ-01 | Student discovery/detail identity scope | `RegistrationDiscoveryQueryAdapter` owner resolution | `ApplicationUsers` 25,000; `Students` 25,000 | Yes | Unique normalized-user/application-user seeks; p95 <= 300 ms |
| CQ-02 | Offering discovery/detail | `RegistrationDiscoveryQueryAdapter` offering/group projection | `CourseOfferings` 2,000; `SectionGroups` 8,000 | No (both below threshold); measured diagnostically | One term/offering page, maximum 100 rows; p95 <= 300 ms |
| CQ-03 | Plan read/write/validation | `RegistrationPlanSqlServerAdapter` owner/term plan lookup | `RegistrationPlans` 8,000 | No (below threshold); measured diagnostically | One unique student/term plan; p95 <= 300 ms |
| CQ-04 | Submission claim/replay | `RegistrationSubmissionStore` scoped replay | `RegistrationSubmissions` 25,000 | Yes | Unique student/term/request seek; p95 <= 300 ms |
| CQ-05 | Authorized roster | `RegistrationRecordsSqlServerAdapter` group roster page | `Enrollments` 75,000 | Yes | One offering/group, first 100 ordered rows; p95 <= 300 ms |
| CQ-06 | Audit review | `AdminAuditQueryStore` time-ordered page | `AuditEvents` 100,000 | Yes | UTC lower bound plus first 100 rows; p95 <= 300 ms |
| CQ-07 | Operational enrollment metric | `EnrollmentCounterReconciler` active group count | `Enrollments` 75,000 | Yes | One offering/group aggregate; p95 <= 300 ms |

## Bounded-scan decision

`CQ-05` currently requires a bounded scan because the canonical enrollment
indexes do not cover the roster filter plus student ordering. The fixture is
limited to 75,000 synthetic enrollment rows, CQ-05 returns at most 100 rows,
and it must still meet the 300 ms POC p95 target. `CQ-07` uses the existing
offering/group index and requires no exception. Ahmed ELbamby,
acting in the Data Lead perspective, approves exception `SPEC005-SCAN-20260719`
only for this non-production POC through **2026-08-02**. The exception expires
automatically, does not authorize production, and requires a reviewed
`(OfferingId, GroupId, State, StudentId)` enrollment index before any
production-readiness claim.

Every other required plan must contain no table scan or unbounded index scan.
Any missing plan, row-count mismatch, p95 breach, unknown scan, missing index
name, or expired/unapproved exception fails closed.
