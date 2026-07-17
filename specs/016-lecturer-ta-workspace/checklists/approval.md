# Gate A Demo Implementation Approval: Lecturer and Teaching Assistant Workspace

**Feature status**: APPROVED<br>
**Human approval**: APPROVED<br>
**Approved by**: Ahmed ELbamby<br>
**Approved on**: 2026-07-13

Ahmed ELbamby authorizes non-production demo implementation of SPEC-016 after
its remaining dependency/readiness tasks pass, including staff-owned
availability and the absence of an Admin correction command. Application
source, implementation tests, and synthetic demo fixtures may proceed in the
documented test-first order.

This approval does not authorize production deployment, official AASTMT
go-live, Gate B-D, or release sign-off.

## Constitution compliance review

**Reviewed on:** 2026-07-17
**Constitution version:** 1.1.0
**Result:** PASS

- SPEC-016 retains stable requirement, acceptance, edge, route, entity, and
  task identifiers, with task completion tied to named evidence.
- The modular monolith remains intact: StaffAdministration owns bounded staff
  workspace projections and endpoints, Scheduling remains the sole owner of
  availability, group assignment, and schedule-impact state, and the Client
  owns only STF-01 through STF-04.
- Every staff object read is server-authorized from the authenticated identity
  and current GroupStaffAssignment scope. Roster output is limited to
  UniversityId, DisplayName, and EnrollmentState.
- Availability changes use the Scheduling aggregate boundary, server time,
  expected rowversion, complete-range replacement, and one local transaction
  with any required durable ScheduleImpactAlert.
- Admin availability mutation/correction/override, automatic rescheduling,
  unrelated rosters, policy/capacity/term administration, grades, attendance,
  and messaging remain excluded.
- No microservice, queue, duplicate persistence model, or new migration owner
  is introduced.

This review is planning analysis. It becomes actionable only with the approved
Gate A record and the passing dependency/readiness evidence for T002-T008.

## 2026-07-17 approval currency

Ahmed Elbamby's current instruction to implement and finish only SPEC-016,
preserve the architecture, keep the solution simple, require evidence before
checking tasks, and treat required demo approvals as granted revalidates the
existing Gate A approval against the dependency baseline recorded on
2026-07-17. It changes no production authority, actor, persistence owner,
route owner, or out-of-scope boundary.
