# Persistence and Query Boundary

```yaml
schemaVersion: 1.0
ownerSpec: SPEC-004
approvalVersion: Gate-A-2026-07-13
approvedBy: Ahmed ELbamby
```

Infrastructure.SqlServer owns the single StudentRegistrationDbContext at
`src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs`.
Business modules own models and mapping contributions, but they do not create a
second context, reference the SQL provider, or reach directly into another
module's table.

## Read boundary

- A read implementation starts from the single context, uses `AsNoTracking`,
  applies bounded filters and deterministic ordering, and uses `Select` to
  project only the declared DTO fields.
- API and module ports return DTO/value contracts. They never return an EF
  entity, change-tracking proxy, or persistence navigation graph.
- Read caches are advisory optimizations only. A stale cache can affect a
  displayed hint, but final registration must revalidate policy, plan,
  timetable, capacity, and version state against authoritative SQL inside the
  command boundary.

## Command boundary

- A command opens one short atomic transaction through the
  StudentRegistrationDbContext only when the use case needs it.
- The application use case coordinates narrow module ports; EF Core and SQL
  details remain in Infrastructure.SqlServer.
- Every business mutation, capacity update, idempotency result, and required
  audit append succeeds in that transaction or rolls back together.
- The caller owns commit. A participating adapter does not start or commit an
  independent transaction.
- EF Core is used directly for explicit LINQ, change tracking, constraints,
  and transactions. No generic repository or unit-of-work wrapper is added.

## Cross-module and cache edge cases

When a cross-module transaction emerges, keep it inside the single
StudentRegistrationDbContext when the data belongs to this modular monolith.
If that boundary no longer fits, stop for architecture review; do not introduce
a second database, distributed transaction protocol, or remote call inside the
transaction without a separately approved ADR/spec.

When any stale cache conflicts with current persisted state, the cache remains
advisory. The final registration path must revalidate against authoritative SQL
and reject or retry with a stable reason; cached eligibility or capacity is
never commit authority.

## Change rule

A new DbContext, provider, direct table dependency, or transaction boundary
requires an approved ADR plus updated architecture and real-SQL integration
tests. Projection and transaction behavior belongs in the owning feature test,
while this record defines the shared invariant.
