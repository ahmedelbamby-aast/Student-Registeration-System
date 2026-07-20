# Gate A Demo Implementation Approval: Registration Capacity and Concurrency

**Feature status**: APPROVED<br>
**Human approval**: APPROVED<br>
**Approved by**: Ahmed ELbamby<br>
**Approved on**: 2026-07-13; reaffirmed 2026-07-17

Ahmed ELbamby authorizes non-production demo implementation of SPEC-014 after
its remaining dependency/readiness tasks pass. Application source,
implementation tests, Code First persistence, and synthetic concurrency
fixtures may proceed in the documented test-first order.

This approval does not authorize production deployment, official AASTMT
go-live, Gate B-D, or release sign-off.

## Constitution compliance review

**Reviewed on**: 2026-07-17<br>
**Constitution version**: 1.1.0<br>
**Result**: PASS

- SPEC-014 remains specification-first, traceable, and explicitly approved for
  non-production demo implementation by Ahmed Elbamby.
- The design preserves the modular-monolith boundary: Registration owns its
  aggregate and consumes IdentityAccess, Academics, and Scheduling contracts.
- SQL Server remains the sole cross-replica linearization authority; the
  design adds no distributed lock, queue, reservation, or partial-acceptance
  workflow.
- Authentication, policy, schedule, capacity, and term validation remain
  server-authoritative, with one atomic local SQL transaction and final
  database constraints.
- Security, privacy-safe errors and telemetry, accessibility, concurrency,
  operability, and measurable verification remain explicit task obligations.
- No constitution exception is requested or required for the approved demo
  scope.

## 2026-07-17 baseline amendment review

The dependency audit returned the package to In Review because the original
planning baseline duplicated SPEC-008's student-term serialization row and
left the S6 migration under a projection consumer. The reconciled baseline:

- consumes the one SPEC-008 `StudentTermAcademicState` transaction boundary;
- assigns the S6 registration migration to SPEC-014-owned persisted state;
- pins `Registration.SubmitOwn`, antiforgery, policy-scope range locks,
  meeting-bound receipt data, and SPEC-018 metrics/load ownership; and
- introduces no new actor, workflow, endpoint, production authority, or
  distributed component.

Ahmed Elbamby's 2026-07-17 instruction to implement and finish SPEC-014,
resolve encountered issues using best practices, preserve the architecture,
and keep the solution simple reaffirms Gate A approval for this reconciled
non-production demo baseline. Gate B-D and production approval remain outside
this authorization.

## 2026-07-20 amendment approval

**Amendment:** `registration-roadmap-line-approval/1.0`<br>
**Human approval:** APPROVED for non-production demo implementation<br>
**Approved by:** Ahmed Elbamby<br>
**Approved on:** 2026-07-20

The owner explicitly requested roadmap subjects with prerequisites and exactly
three credits, automatic first-program-term registration, self-service from
term two, per-subject Admin/Lecturer/Teaching Assistant approval, held seats
until decision, capacity visibility for every supported role, the 18/21 credit
and CGPA 3.00 overload boundary, and a unified frontend pattern. This approval
authorizes T125-T144 implementation and evidence work. T145 remains the final
amendment release gate and production authority remains excluded.
