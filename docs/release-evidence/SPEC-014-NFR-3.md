# SPEC-014 NFR-3 Evidence

**Result:** PASS

The exact spike scheduled 200 submissions/second for 60 seconds across two logical application replicas sharing one SQL Server database.

- Scheduled/completed: 12,000/12,000.
- Replica distribution: `replica-a` 6,000; `replica-b` 6,000.
- Accepted: 8,400; expected conflicts: 2,400; final replays: 63; bounded `202 InProgress`: 1,137; unexpected failures: 0.
- Overbooking, duplicates, partial commits, combined policy/timetable violations, and counter mismatches: all 0.
- Independent hot-group proof: 100 simultaneous requests for 30 seats produced exactly 30 accepted and 70 conflicts across two replicas.

`202 InProgress` is the specified non-durable bounded retry response and is accounted separately from server failures.

