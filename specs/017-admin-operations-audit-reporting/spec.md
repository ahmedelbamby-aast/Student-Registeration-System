# Feature Specification: Admin Operations, Audit, and Reporting

**Feature Branch**: 017-admin-operations-audit-reporting
**Created**: 2026-07-12
**Status**: Approved for demo implementation by Ahmed ELbamby on 2026-07-13; global line-approval and first-term monitoring amendment approved 2026-07-20
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

**Owner-approved amendment (2026-07-20):** Admin gains the narrow
`RegistrationApproval.DecideAll` capability over pending SPEC-014 lines and
read/retry-failed monitoring for first-term automatic batches. This is not an
enrollment correction, drop, withdrawal, seat-decrement, capacity override, or
rule waiver. SPEC-014 remains the only decision/hold/finalization writer.

## Context

Admins need safe master-data operations, registration-record inspection, peak
monitoring, audit evidence, and operational exports. Broad Admin access still
uses least privilege, reasons, optimistic concurrency, immutable audit, and
durable cross-replica work claims. Enrollment correction, drop, and withdrawal
are not part of this MVP. Staff own availability edits; Admin availability use
is limited to bounded viewing and read-only import into offering planning.

## User Scenarios and Testing

### User Story 1 - Reasoned sensitive mutation (FR-2, FR-4) (P1)

As a Authorized administrator, I need the Reasoned sensitive mutation (FR-2, FR-4) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given Admin has the owning feature permission and provides a valid reason<br>
When a safe feature-spec master-data mutation is committed<br>
Then the invariant remains valid<br>
And an append-only event records actor, reason, time and before/after summary.
### User Story 2 - Capacity bypass rejected (FR-4, FR-9) (P1)

As a Authorized administrator, I need the Capacity bypass rejected (FR-4, FR-9) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a group has active enrollments at its current capacity<br>
When Admin attempts a SPEC-010 capacity reduction below EnrolledCount<br>
Then the owning feature command rejects the change<br>
And no capacity/enrollment state changes.
### User Story 3 - Audit export scope (FR-5, FR-7) (P2)

As a Authorized administrator, I need the Audit export scope (FR-5, FR-7) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given Admin lacks permission for restricted security events<br>
When an audit export is requested<br>
Then restricted rows/fields are omitted or request denied<br>
And the export action itself is audited.
### User Story 4 - Monitor degradation (FR-3) (P2)

As a Authorized administrator, I need the Monitor degradation (FR-3) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given server failures or capacity conflicts spike above configured threshold<br>
When the admin dashboard refreshes<br>
Then a timestamped alert identifies metric, threshold and investigation link.
### User Story 5 - Governed bounded master-data command (FR-1, FR-6, FR-8) (P3)

As a Authorized administrator, I need the Governed bounded master-data command (FR-1, FR-6, FR-8) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given authorized Admin filters a large master-data list and previews a change<br>
When the bounded request and confirmed mutation execute<br>
Then only a paged parameterized result is returned<br>
And staff availability can be viewed/imported as read-only planning input<br>
And no Admin availability correction/override command, permission, editable
control, notification workflow, or correction-audit flow exists.
### User Story 6 - Stale preview confirmation (FR-8, FR-10, FR-11) (P3)

As a Authorized administrator, I need the Stale preview confirmation (FR-8, FR-10, FR-11) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given an Admin previews an offering publication and its dependency version
changes<br>
When the Admin confirms the old token twice with the same idempotency key<br>
Then both responses report 409 STALE_PREVIEW<br>
And no publication or duplicate audit event commits.
### User Story 7 - Audit failure rolls back mutation (FR-2, FR-12) (P3)

As a Authorized administrator, I need the Audit failure rolls back mutation (FR-2, FR-12) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given a sensitive change passes validation<br>
When audit-event persistence is fault-injected to fail<br>
Then the business mutation rolls back<br>
And the API returns a generic correlated failure without reporting success.
### User Story 8 - Final Admin safeguard (FR-13) (P3)

As a Authorized administrator, I need the Final Admin safeguard (FR-13) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given exactly two active Admin role assignments remain<br>
When two replicas concurrently revoke different assignments<br>
Then both transactions serialize through the shared AdminSecurityGuard<br>
And at most one revocation commits, the loser returns 409
FINAL_ADMIN_REQUIRED, and at least one active Admin remains.
### User Story 9 - Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Authorized administrator, I need the Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Admin Operations, Audit, and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-9 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-9)**

Given target operational metrics, a production-size audit dataset, large export,
and positive/negative/concurrent admin command matrix<br>
When admin quality tests execute<br>
Then metrics are no more than 60 seconds stale and show observation time<br>
And audit first page returns within 1 second p95<br>
And two replicas competing for one export publish exactly one artifact through
a durable lease, while authorized status/download and secure expiry are
enforced<br>
And every admin action passes authorization, audit, concurrency, and
anti-forgery checks.

### User Story 10 - Global pending-line decision (FR-14, FR-15) (P1)

As an authorized Admin, I need to decide any pending registration line so that
student plans can complete without bypassing academic or capacity rules.

**Acceptance Scenario (AC-10)**

Given multiple pending student plans and Admin has DecideAll<br>
When Admin searches and decides one line<br>
Then the bounded result shows approved context and truthful
capacity/enrolled/held/available counts<br>
And the versioned, idempotent, audited decision delegates to SPEC-014 without
rule/capacity bypass or holder-list disclosure.

### User Story 11 - First-term automatic-batch monitoring (FR-16, FR-17) (P2)

As an authorized Admin, I need to monitor first-term automatic enrollment and
retry failed items safely so that capacity or timetable gaps are actionable.

**Acceptance Scenario (AC-11)**

Given a batch contains accepted and failed student items<br>
When Admin reviews and retries failed items<br>
Then progress/failure codes are bounded, timestamped, and actionable<br>
And retry is idempotent, cannot change roadmap subjects, and never creates a
partial student schedule.

## Edge Cases

- EC-1: Metrics backend unavailable -> show stale timestamp/degraded state, not
  fabricated zero.
- EC-2: Export fails/expires -> safe status and authorized retry.
- EC-3: Concurrent admin edit -> 409 current version, no lost update.
- EC-4: Bulk import partially invalid -> preview errors; publish all-or-nothing.
- EC-5: Admin disables own final Admin role -> require safeguard/second actor
  according to security approval.
- EC-6: The same idempotency key is reused with a different admin command
  payload -> return 409 IDEMPOTENCY_KEY_REUSED and execute neither new payload.

## Requirements

### Functional Requirements

- FR-1: Authorized Admin pages MUST call feature-owner endpoints/commands for
  terms/windows, users/roles, student records/holds, catalogue/policies,
  resources, offerings/groups, and imports; no generic AdminCommandService is
  permitted. Availability access is limited to SPEC-010's bounded read-only
  view; Admin MAY import staff-declared ranges into offering planning as
  read-only inputs.
- FR-2: Admin audit search MUST merge scoped SPEC-004 AuditEvent and SPEC-007
  SecurityEvent records with actor, reason, timestamp, redacted before/after,
  correlation, action, and source stream.
- FR-3: The system MUST provide registration-window metrics for traffic,
  success, expected rejections, server failures, fill rates, lock waits, and
  data-quality alerts.
- FR-4: Admin orchestration MUST NOT expose enrollment correction, drop,
  withdrawal, or seat-decrement commands in MVP. Delegated master-data
  commands MUST preserve capacity and timetable invariants and cannot bypass
  the owning feature module. It also MUST NOT expose an Admin availability
  mutation/correction/override command, permission, editable control,
  notification workflow, or correction-audit flow. The FR-14 line decision is
  an approved pending-request decision through SPEC-014 and is not a mutation
  of an accepted Enrollment or capacity configuration.
- FR-5: Exports MUST enforce the same row/data scope and PII minimization as
  UI and use an explicit request/status/download lifecycle. ExportJob MUST be
  durable, owner/scope/request-bound, expiring, and claimed by workers through
  a conditional SQL lease; only the current lease owner may publish one
  artifact, and request/download actions MUST be audited.
- FR-6: Admin list/search endpoints MUST be paged, filtered, and safely
  parameterized.
- FR-7: Consumed AuditEvent and SecurityEvent records MUST be append-only to normal users.
- FR-8: Import, publication, and other approved sensitive feature-spec
  mutations MUST use preview and explicit confirmation; this does not
  authorize enrollment correction. Availability import copies staff-declared
  ranges into offering-planning input and MUST NOT mutate
  StaffTermAvailability or create an Admin correction workflow.
- FR-9: Break-glass behavior MUST NOT exist without a separate approved spec.
- FR-10: Update/delete commands MUST require expected rowversion; retryable
  creates, imports, exports, and confirmed feature-spec mutations MUST require
  an idempotency key.
- FR-11: Preview tokens MUST bind actor, permission scope, canonical payload,
  dependency versions, and expiry; confirmation MUST reject any changed input,
  scope, permission, dependency, or expired token without introducing a
  generic AdminConfirmationService.
- FR-12: Conformance tests MUST prove features use SPEC-004's transaction-aware
  writer so mutation/audit commit or roll back together; SPEC-017 MUST NOT own
  a second writer.
- FR-13: Role changes MUST delegate to SPEC-007, which owns AdminSecurityGuard,
  RoleAssignment mutation, audit, and FINAL_ADMIN_REQUIRED; SPEC-017 MUST NOT
  implement a competing role writer.
- FR-14: Admin with `RegistrationApproval.DecideAll` MUST list/read and
  approve/reject any pending SPEC-014 line through the owner endpoint using
  expected submission/line versions, reason, ClientRequestId, and antiforgery.
  The decision cannot bypass eligibility/load/prerequisite/hold/conflict/
  capacity rules or directly mutate Enrollment/SectionGroup counters.
- FR-15: Admin approval/registration monitoring MUST show requested credits,
  normal/probation/overload explanation, line states/decisions, window close,
  and capacity/enrolled/held/available counts without holder lists or
  unrelated student data. Every read/decision is permission-scoped and
  audited.
- FR-16: Admin MUST view bounded durable FirstTermAutoEnrollmentBatch progress
  and safe per-student failure codes. Admin MAY idempotently retry failed items
  with expected batch version and reason but cannot select different roadmap
  subjects, waive a failure, or create a partial student schedule.
- FR-17: ADM-01/ADM-08 and linked approval/batch views MUST compose unified
  SPEC-003 roadmap, capacity, approval status/timeline, decision, and standard
  route-state components shared with Student and staff.

### Non-Functional Requirements

- NFR-1: Operational metrics SHOULD be no more than 60 seconds stale and show
  observation timestamp.
- NFR-2: Audit search SHOULD return first page within 1 second p95 at approved
  retention volume.
- NFR-3: Export generation MUST be asynchronous and bounded, use a 60-second
  renewable SQL lease with at most three attempts, publish at most one
  artifact, and expire the artifact after the configured approved retention
  interval.
- NFR-4: Admin actions MUST have authorization, audit, concurrency, and
  validation tests.

### Key Entities

- **AuditEvent**: Consumed append-only entity owned by SPEC-004.
- **SecurityEvent**: Consumed Identity event owned by SPEC-007.
- **ImportBatch**: Consumed aggregate owned by SPEC-009.
- **AdminSecurityGuard**: Consumed Identity serialization row owned by SPEC-007.
- **ExportJob**: SPEC-017-owned durable request/lease/artifact lifecycle.
- **OperationalMetric**: Consumed contract owned by SPEC-018.

## Success Criteria

- **SC-1**: Every sensitive administrative mutation records actor, reason, time, and before/after context.
- **SC-2**: Normal administrative actions cannot bypass capacity or timetable invariants.
- **SC-3**: Operational information is timestamped, scoped, and distinguishable from stale or unavailable data.
- **SC-4**: Admin can decide every pending line and monitor first-term
  automatic batches without acquiring an enrollment/capacity bypass or
  exposing holder PII.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-014](../014-registration-capacity-concurrency/spec.md)
- [SPEC-015](../015-student-registration-records/spec.md)
- [SPEC-016](../016-lecturer-ta-workspace/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| ADM-01 | /admin | AdminDashboardPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-017 |
| ADM-02 | /admin/terms | TermAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-008 |
| ADM-03 | /admin/users | UserAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-007 |
| ADM-04 | /admin/students | StudentAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-008 |
| ADM-05 | /admin/catalogue | CatalogueAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-009 |
| ADM-06 | /admin/offerings | OfferingAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-010 |
| ADM-07 | /admin/resources | ResourceAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-010 |
| ADM-08 | /admin/registrations | RegistrationAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-017 |
| ADM-09 | /admin/audit | AuditAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-017 |

## Out of Scope

- OS-1: Unrestricted super-admin and unaudited direct database edits.
- OS-2: Break-glass capacity/conflict override; enrollment correction, drop,
  withdrawal, or seat-decrement; and any Admin availability
  mutation/correction/override, permission, editable control, notification, or
  correction-audit workflow.
- OS-3: Business-intelligence warehouse.
- OS-4: Long-term report replica until primary impact is measured.
