# SPEC-014 NFR-5 Evidence

**Result:** PASS

At the required 75/second target load:

- Accepted: 31,500.
- Expected `GROUP_FULL` conflicts: 9,000.
- Idempotent final replays: 4,500.
- Bounded `202 InProgress`: 0.
- Unexpected failures: 0 of 45,000, or 0% (required: below 0.1%).

All 45,000 requests account exactly once. Expected business conflicts and bounded retry guidance are recorded separately and are not server failures.

