# Clarification Record: Lecturer and Teaching Assistant Workspace

**Reviewed**: 2026-07-13
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for non-production demo implementation (Gate A)

## Resolved Q5 availability boundary

Ahmed approved staff-owned availability edits. Admin may consume the bounded
read-only view and select a staff availability aggregate ID and rowversion as
an immutable offering-planning dependency, but the POC has no range-copy or
Admin correction/override command,
permission, editable control, notification workflow, or correction-audit flow.
ScheduleImpactAlert remains the durable response to staff changes affecting a
published schedule.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown production or release decisions remain registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
rule. Gate A does not resolve or waive those later Gate B-D obligations and is
not official AASTMT production authorization.

## 2026-07-17 implementation revalidation

Ahmed Elbamby's current instruction explicitly revalidates the approved
non-production demo baseline for SPEC-016. The dependency review found no
superseding behavior or ownership change. The missing narrow production
Scheduling availability port is an implementation gap already bounded by
T012, T056-T057, and T060-T065; it does not transfer aggregate ownership to
StaffAdministration. Any later accepted dependency hash, authorization,
route, persistence, or scope change returns affected work to In Review.
