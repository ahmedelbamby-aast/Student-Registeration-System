# Planning Quickstart: Offerings, Groups, and Resources

This is a pre-implementation verification guide. It does not run or create application code.

1. Confirm APPROVED Gate A demo status, the accepted DEC-11 simple demo
   staffing rule, and DEC-12 staff-only edit/read-only Admin boundary.
2. Baseline UX, persistence, shared contract, catalogue, and quality specs.
3. Verify every selectable group has a Lecturer-staffed Lecture plus a
   TA-staffed Tutorial or Laboratory, every present Tutorial/Laboratory has a
   TA, all activity staff/room/time details are returned, and Tutorial-to-
   Section is display-only; then verify staff-only availability mutation,
   bounded Admin view/read-only planning import, absence of an Admin
   availability mutation route, resource versions, stable lock order, alert
   state, and page contributions.
4. Walk every AC/EC/SC and real-SQL race against tasks.md.
5. Run readiness and cross-spec consistency analysis.
6. Verify Ahmed ELbamby's 2026-07-13 Gate A approval record before
   implementation starts.
7. Execute the test-first phases in dependency order while preserving separate
   Gate B-D, release, production, and official AASTMT go-live approvals.
