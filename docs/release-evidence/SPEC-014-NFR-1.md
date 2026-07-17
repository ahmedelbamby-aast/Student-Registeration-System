# SPEC-014 NFR-1 Evidence

**Result:** PASS

Recorded 2026-07-17 by the exact SQL Server load run in `SPEC-014-load-results.json`.

- Target profile: 45,000/45,000 completed; overbooking 0, duplicate active offering enrollments 0, partial commits 0, combined policy/timetable violations 0, counter mismatches 0.
- Spike profile: 12,000/12,000 completed with the same five invariant counts at 0.
- Two-replica 100-way/30-seat collision: 30 accepted, 70 `GROUP_FULL`, 30 active enrollments, final `EnrolledCount` 30, total invariant violations 0.

The policy query enforces the 18-credit ceiling and the timetable query compares persisted meeting-slot ranges for every active enrollment pair.

