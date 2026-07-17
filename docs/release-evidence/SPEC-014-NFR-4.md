# SPEC-014 NFR-4 Evidence

**Result:** PASS

Transaction timing starts immediately after `BeginTransactionAsync` succeeds and stops after commit or rollback; connection-pool and scheduler wait are excluded from the database-transaction duration.

- Target transaction p95/max: 17.9534/397.3854 ms.
- Spike transaction p95/max: 943.2604/1,023.8032 ms.
- Cancellation before commit: verified with no durable submission.
- Fault-injected HTTP activities: 1 observed; remote activities inside a transaction: 0.
- Remote dependency types on the transaction boundary: 0; measured remote calls inside transactions: 0.
- Complete claim/allocation/enrollment/audit/commit units execute inside EF's configured retrying execution strategy; fragment retry is not used.

