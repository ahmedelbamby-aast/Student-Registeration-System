# SPEC-017 FR-4 Excluded-Action Evidence

Date: 2026-07-17
Evidence test: `AdminCommandInvariantTests` (5/5 green)

The source and contracts contain no SPEC-017 command, permission, editable
control, notification workflow, or correction-audit path for:

- enrollment correction, drop, withdrawal, or seat decrement;
- capacity or timetable-conflict bypass;
- Admin mutation, correction, or override of staff availability; or
- a break-glass or final-Admin override.

`GET /api/admin/staff-availability` remains the Scheduling owner’s bounded,
read-only import surface. Capacity and conflict values may be displayed and
validated, but cannot be bypassed by SPEC-017.

Phase 5 contributor and route tests remain pending. T100 must reject release
until those tests prove that no prohibited control or endpoint was introduced.
