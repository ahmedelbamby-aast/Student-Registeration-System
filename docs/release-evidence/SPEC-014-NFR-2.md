# SPEC-014 NFR-2 Evidence

**Result:** PASS

The required target profile ran against SQL Server 2022 with 25,000 synthetic accounts, 5,000 logical authenticated sessions, and two application replicas.

- Configured rate and duration: 75 submissions/second for 600 seconds.
- Scheduled/completed: 45,000/45,000.
- Submission p95: 19.3534 ms (required maximum: 2,000 ms).
- Transaction p95: 17.9534 ms; maximum transaction: 397.3854 ms.
- Unexpected failures: 0 (0%).

Machine-readable source: `SPEC-014-load-results.json`, fingerprint `5694E04567C69685FA189FC29ED834889D7B1116099BF3A1AADA61C994F8A5E6`.

