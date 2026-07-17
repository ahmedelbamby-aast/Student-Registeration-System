# SPEC-014 NFR-6 Evidence

**Result:** PASS

The measured run published every required privacy-safe concurrency metric:

- `sql.deadlocks`: 0.
- `sql.lock_wait.duration.ms` p95: 837.0807 ms.
- `registration.idempotent_replays`: 4,563.
- `registration.capacity_conflicts`: 11,470.
- `registration.counter_mismatches`: 1 controlled mismatch, followed by verified repair.

Unsafe metric tags: 0. Sensitive evidence fields/privacy violations: 0. Authenticated boundary probes also recorded one rejected identity-substitution attempt, one anonymous 401, and one antiforgery rejection without logging credentials or academic records.

Machine-readable source: `SPEC-014-load-results.json`.

