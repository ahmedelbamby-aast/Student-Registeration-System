# Clarification Record: Offerings, Groups, and Resources

**Reviewed**: 2026-07-13
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for Gate A demo implementation

## Session 2026-07-13

- Q: What minimum teaching activities and staff make an offering available for
  the demo?
- A: Ahmed ELbamby approved a simple POC rule: every Published/Open offering
  exposes only complete selectable groups. Each group has at least one Lecture
  with a Lecturer and at least one Tutorial/Section or Laboratory (or both);
  every present Tutorial and Laboratory has at least one TA. Students see the
  activity type, staff, room/location, day, and time. `Tutorial` is canonical
  in the model/API and may be displayed as `Section`.
- Integration: FR-2/FR-3, AC-1, the existing MeetingSlot and
  GroupStaffAssignment model, API contract, and task wording were tightened;
  no entity, endpoint, requirement ID, or task ID was added.
- Q: Who may edit staff availability in the POC? → A: Staff edit only their
  own declaration through the staff workspace. Admin may view the declaration
  and import it into offering planning as read-only input, but cannot create,
  replace, edit, or override it. The Admin availability mutation contract is
  excluded; staff-owned
  changes still create durable schedule-impact alerts when required.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Remaining production-only product/institutional decisions are registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
rule. They are not invented requirements and do not extend this demo approval
to production, official AASTMT go-live, or later release gates.
