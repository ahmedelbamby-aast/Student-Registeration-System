# SPEC-010 Dependency Baseline

This file freezes only the upstream contracts needed by SPEC-010. The linked
dependency artifacts remain authoritative; this baseline does not copy their
ownership or introduce another abstraction.

## T001 - SPEC-003 frontend routes, states, and accessibility

- `STU-03` is `/student/subjects/{offeringId}`. SPEC-010 contributes offering
  and group data while SPEC-011 owns the Razor page. The route reads offering
  and group details, shows capacity plus every activity's Lecturer/TA,
  room/location, day, and start/end time, and exposes no selection action for
  a full, unpublished, stale, or incomplete group. It passes only a selected
  group intent to STU-04; it does not write the plan.
  Evidence: [page matrix](../003-ux-storyboard-accessibility/page-matrix.md),
  [STU-03 design record](../003-ux-storyboard-accessibility/design/pages/STU-03.md).
- `ADM-06` is `/admin/offerings` and is implemented by SPEC-010. Its bounded
  list/editor flow covers offering creation, group/capacity edits, staffing,
  rooms, meetings, validation, and publication. Blocking resource, overlap,
  capacity, and stale-version findings stay linked to their controls; publish
  cannot present success while a blocker remains.
  Evidence: [ADM-06 design record](../003-ux-storyboard-accessibility/design/pages/ADM-06.md).
- `ADM-07` is `/admin/resources` and is implemented by SPEC-010. It manages
  rooms and schedule-impact alerts while Staff availability is a read-only
  imported planning input. It exposes revalidate/resolve alert actions but no
  Admin availability edit or override, and supplies a chronological/list
  alternative to any schedule grid.
  Evidence: [ADM-07 design record](../003-ux-storyboard-accessibility/design/pages/ADM-07.md).
- All three routes implement the required loading, empty, success,
  validation-error, service-error, unauthorized, session-expired, stale, and
  offline cases with deterministic fixtures, stable reason codes, explicit
  focus/live-region behavior, and no false success. Verification covers
  component, API-contract, Playwright primary/failure, accessibility, and
  visual test families. The UI must meet WCAG 2.2 AA, remain keyboard
  operable, restore focus, use visible focus, reflow from 320 through 1920 CSS
  pixels and at 400% zoom, and provide at least 44-by-44 CSS-pixel targets
  except documented WCAG exceptions.
  Evidence: [SPEC-003 requirements](../003-ux-storyboard-accessibility/requirements.md),
  [SPEC-003 plan](../003-ux-storyboard-accessibility/plan.md).

## T002 - SPEC-005 scheduling persistence and transaction rules

- SPEC-010 owns the writable Scheduling mapping for `CourseOffering`,
  `SectionGroup`, `MeetingSlot`, `Room`, `GroupStaffAssignment`,
  `StaffTermAvailability`, `StaffAvailability`, and `ScheduleImpactAlert`;
  the shared SQL infrastructure composes that mapping and migrations.
  Evidence: [SPEC-005 data model](../005-erd-data-lifecycle/data-model.md),
  [ERD](../../docs/diagrams/ERD.md).
- Required keys are unique `CourseOffering(TermId, CourseId)`,
  `SectionGroup(OfferingId, GroupCode)`, `Room.Code`, and
  `StaffTermAvailability(StaffId, TermId)`, plus the composite staff-assignment
  key. `SectionGroup(Id, OfferingId)` is the alternate relationship key that
  prevents allocating a group under another offering.
  Evidence: [unique-invariant catalogue](../005-erd-data-lifecycle/contracts/unique-invariants.md),
  [CourseOffering reference](../005-erd-data-lifecycle/contracts/entities/CourseOffering.md),
  [SectionGroup reference](../005-erd-data-lifecycle/contracts/entities/SectionGroup.md).
- Database checks enforce `Capacity >= 0`,
  `0 <= EnrolledCount <= Capacity`, `MeetingSlot.EndLocal > StartLocal`, and
  `StaffAvailability.EndLocal > StartLocal`. `RegistrationPaused = false`,
  resource overlaps, and staff/room availability are transactional predicates,
  not substitutes for row-local checks.
  Evidence: [check-constraint catalogue](../005-erd-data-lifecycle/contracts/check-constraints.md).
- SQL rowversion applies to `CourseOffering`, `SectionGroup`, `Room`,
  `StaffTermAvailability`, and `ScheduleImpactAlert`. Every group-state,
  meeting, room-assignment, or staff-assignment mutation locks and advances
  the owning `SectionGroup.Version`; availability range replacement advances
  the parent `StaffTermAvailability.Version`. Child ranges do not create an
  independent concurrency boundary. Authorized stale writes return
  `409 STALE_VERSION` without lost updates.
  Evidence: [concurrency-token catalogue](../005-erd-data-lifecycle/contracts/concurrency-tokens.md).
- `ScheduleImpactAlert` durably links the affected group and staff-term
  availability, reason, validation result, Open/Revalidated/Resolved state,
  timestamps, and rowversion. Scheduling business mutations and the shared
  append-only `AuditEvent` commit together or roll back together; alert and
  audit summaries remain privacy-safe.
  Evidence: [ERD scheduling fields](../../docs/diagrams/ERD.md),
  [AuditEvent reference](../005-erd-data-lifecycle/contracts/entities/AuditEvent.md).

## T003 - SPEC-006 API and command conventions

- Endpoint responses are feature-owned DTO projections only; EF entities,
  navigation graphs, provider types, secrets, and persistence-only fields do
  not cross the boundary. `ApiError` contains stable `code`, safe `message`,
  `correlationId`, bounded optional `fieldErrors`, and an authorized
  `currentVersion` only where declared. Authorization precedes existence and
  version disclosure.
  Evidence: [SPEC-006 requirements](../006-domain-class-api-contracts/requirements.md),
  [DTO isolation contract](../006-domain-class-api-contracts/contracts/dto-isolation.md).
- List endpoints use `Page<T>`, default page/pageSize `1/20`, maximum page size
  `100`, `400 PAGE_SIZE_INVALID` for invalid values without silent capping,
  and a declared stable sort ending in a unique-ID tie-breaker.
  Evidence: [SPEC-006 API contract](../006-domain-class-api-contracts/contracts/api.md).
- Versioned update/delete requests carry required request-body
  `expectedRowVersion`; authorized stale commands return
  `409 STALE_VERSION`. `If-Match`/412 is outside the MVP.
  Evidence: [SPEC-006 API contract](../006-domain-class-api-contracts/contracts/api.md).
- SPEC-006 defines no generic preview envelope. SPEC-010 therefore owns its
  preview DTO/token, while applying the shared rules: the preview is a
  transport value, is safe to serialize, binds the feature-declared actor,
  canonical content, dependency versions, and expiry, and never replaces
  current authorization or in-transaction version validation.
  Evidence: [SPEC-006 ownership boundary](../006-domain-class-api-contracts/requirements.md),
  [SPEC-010 API contract](contracts/api.md).
- Retryable creates/publishes declare the authenticated owner and scope,
  hash the server-canonical payload, claim atomically, replay the same
  key/payload result, reject a different payload with
  `409 IDEMPOTENCY_KEY_REUSED`, and distinguish cancellation before commit
  from response loss after commit. No generic command/result abstraction is
  introduced.
  Evidence: [SPEC-006 API contract](../006-domain-class-api-contracts/contracts/api.md).

## T004 - SPEC-009 course, catalogue, and policy versions

- SPEC-009 is the sole owner of `Course`, `CatalogueDraft`,
  immutable `CatalogueVersion`, `PolicySet`, and `PolicyRule`. SPEC-010
  consumes published identifiers/versions and must not redefine or edit these
  aggregates.
  Evidence: [SPEC-009 data model](../009-catalog-prerequisites-policy-admin/data-model.md).
- Course/program codes are normalized and unique within a catalogue version.
  Published catalogue graphs are immutable and superseded as a unit; course
  DTO rowversions protect draft/admin edits, while an offering binds to the
  canonical published course identity rather than copying catalogue fields.
  Evidence: [SPEC-009 requirements](../009-catalog-prerequisites-policy-admin/requirements.md),
  [SPEC-009 API contract](../009-catalog-prerequisites-policy-admin/contracts/api.md).
- Policy sets are typed, effective-dated, versioned, source-classified, and
  immutable after publication. Scheduling validation consumes the applicable
  published policy version, including published-group capacity and timetable
  conflict rules, and retains the catalogue/policy dependency versions in its
  validation preview so a later change invalidates publication.
  Evidence: [SPEC-009 specification](../009-catalog-prerequisites-policy-admin/spec.md),
  [SPEC-009 API contract](../009-catalog-prerequisites-policy-admin/contracts/api.md).

## T005 - SPEC-018 quality, security, replica, and operations gates

- Offering/group reads inherit the `<= 300 ms` p95 catalogue/read budget under
  the blocking 10-minute target of 75 registration submissions/s plus
  300 reads/s. The required spike is 200 submissions/s for 60 seconds. Target
  and spike run across at least two stateless API replicas; optional 2x, 5x,
  and soak profiles are diagnostic only.
  Evidence: [SPEC-018 requirements](../018-quality-security-scalability-operations/requirements.md),
  [SPEC-018 plan](../018-quality-security-scalability-operations/plan.md).
- Correctness gates require zero overbooking, zero duplicate active offering
  enrollment, and zero partial atomic submission at target, spike, and
  replica-failover profiles. SPEC-010 capacity and publication concurrency
  tests are blocking evidence, not replaceable by coverage percentages.
  Evidence: [SPEC-018 requirements](../018-quality-security-scalability-operations/requirements.md).
- Protected routes require negative role/ownership tests. Secrets come only
  from User Secrets/environment variables; generated credentials, full
  student profiles, SQL details, and sensitive payloads must not enter source,
  SQL, logs, traces, or evidence. Release is blocked by unresolved
  critical/high security findings or a stale/unreviewed STRIDE model.
  Evidence: [SPEC-018 specification](../018-quality-security-scalability-operations/spec.md).
- Operations evidence includes privacy-safe health, structured logs, metrics,
  traces, and correlation IDs, monitoring latency/throughput/error rate,
  business rejection codes, SQL latency, lock waits, deadlocks, capacity
  conflicts, and reconciliation. Real-SQL gates use isolated per-run SQL
  Server 2022 Developer compatibility-160 databases migrated before synthetic
  seed and disposed afterward. Backup/restore and rollback rehearsal must meet
  RPO `<= 5 minutes` and RTO `<= 1 hour`.
  Evidence: [SPEC-018 requirements](../018-quality-security-scalability-operations/requirements.md),
  [SPEC-018 plan](../018-quality-security-scalability-operations/plan.md).
