# SPEC-017 Audit Append-Only Evidence

**Recorded:** 2026-07-17
**Scope:** SPEC-017 merged audit query and consumed-stream write boundary
**Result:** PASS for T060

## Consumed ownership

- SPEC-004 remains the sole owner of `AuditEvent`, `IAuditEventWriter`,
  `AuditTransactionWriter`, mapping, and transactional append behavior.
- SPEC-007 remains the sole owner of `SecurityEvent`, its mapping, and Identity
  security-fact writes.
- SPEC-017 contributes only `IAdminAuditReader`, `AuditEventQueries`, and
  `SqlAdminAuditReader`. It defines no second event entity, mapping, writer,
  update/delete command, or transaction boundary.

## Automated evidence

Command:

```powershell
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Audit.AuditAggregationConformanceTests"
```

Result: **3 passed, 0 failed, 0 skipped**.

The focused suite proves:

- the public port exposes one `ReadAsync` method only;
- both canonical sources use `AsNoTracking`;
- explicit row scope is applied to both streams before counts and page
  boundaries;
- Identity security visibility is separately scoped;
- results use `OccurredAtUtc DESC, Id DESC` and stable page boundaries;
- before/after summaries expose only bounded allow-listed scalar fields under
  redaction version `spec017-v1`;
- `SecurityEvent.MetadataJson`, passwords, tokens, nested/unbounded values,
  and restricted rows are absent;
- the SQL adapter contains no add/update/remove/save/raw-SQL path and no audit
  writer dependency; and
- StaffAdministration contains no `IAuditEventWriter` or competing
  `AuditTransactionWriter`.

Real SQL created both canonical event types, included an out-of-scope newer
row, and verified that the restricted row did not alter total count or paging.
It also verified source filtering and the omission of restricted fields.

## Atomic rollback evidence

Command:

```powershell
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-build --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Audit.AuditAtomicityTests"
```

Result: **3 passed, 0 failed, 0 skipped**.

The upstream SPEC-004 real-SQL suite proves business state and its audit event
commit in the caller's transaction, injected audit persistence failure rolls
back both states, and an explicit caller rollback removes both saved states.
SPEC-017 consumes that writer contract unchanged and does not represent a
read-model test as mutation evidence.

## Authorization boundary

Normal users reach audit data only through the later authorized Admin endpoint
and this read-only port. Missing Identity security scope returns no security
rows and does not reveal their existence. This evidence does not claim that
the later endpoint authorization/rate-limit work, audit export, NFR latency,
or release approval is complete.
