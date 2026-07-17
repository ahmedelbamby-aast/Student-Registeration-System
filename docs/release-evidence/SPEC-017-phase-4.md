# SPEC-017 Phase 4 Evidence

Date: 2026-07-17
Branch: `codex/017-admin-operations-audit-reporting`
Tasks: T047-T072 (72/101 total checked)

## Delivered boundaries

- read-only merged SPEC-004/SPEC-007 audit projection with scoped paging and
  bounded allow-list redaction;
- timestamped live/stale/degraded registration-window metrics query through a
  narrow Contracts port;
- durable scoped audit export request/status/download service, conditional SQL
  lease store, and configured shared-file demo artifact adapter;
- owner-command delegation, no-bypass/no-break-glass, expected-version,
  preview, idempotency, and final-Admin conformance records.

No generic Admin command/confirmation service, broker, route handler, worker
host, Blazor page, or composition-root registration was added in this phase.

## Red-before evidence

| Task lane | Expected-red result before delivery |
|---|---|
| T049 audit merge | 1 passed, 1 failed; failure named only the three absent T070 delivery paths |
| T051 metrics | test failed because the three T052 delivery paths were absent |
| T055/T057 export | 3/3 failed because the service, two ports, and SQL store were absent |
| T065 command metadata | compile-safe focused test reported the missing retryable export idempotency implementation before that service arrived |

Canonical upstream conformance/absence tasks were permitted to pass
immediately by their task text and were checked only after their focused tests
were green.

## Final automated evidence

| Suite | Result |
|---|---|
| `AdminCommandInvariantTests` + `AuditExportScopeTests` | 8/8 passed |
| `AdminMetricsTests` + `AdminConfirmationConcurrencyTests` + `AuditAggregationConformanceTests` + `AuditExportSqlConcurrencyTests` | 14/14 passed, including real SQL and two replicas |
| Export service/worker application checks delivered by T056 | 2/2 passed; the separate endpoint check remains the intentional T091 red |
| SPEC-017 EC-2 and EC-6 | 2/2 passed |
| `AuditSourceMergeContractTests` | 1/1 passed |
| Canonical SPEC-004 `AuditAtomicityTests` | 3/3 passed |
| Staff availability/module-boundary focus | 8/9 passed; the sole failure is the pre-existing SPEC-016 `Spec016Endpoints.cs` reference to `Academics.Application`/`Scheduling.Domain`, outside SPEC-017 scope; no new SPEC-017 file caused it |
| `dotnet build StudentRegistration.slnx --no-restore` | succeeded, 0 warnings, 0 errors |
| `Test-AllSpecs.ps1 -Phase Implementation` scoped row | SPEC-017: 101 actionable tasks, score 100, PASS, APPROVED; zero SPEC-017 findings |

The repository-wide validator remains FAIL for already disclosed findings in
other specifications. This phase claims only the scoped SPEC-017 PASS. The
generated `docs/SPECKIT_AUDIT.md` was restored after inspection because it is a
repository-wide generated artifact, not Phase 4 evidence.

## Task evidence links

- FR-1: `SPEC-017-FR-1-delegation.md`
- FR-4: `SPEC-017-FR-4-excluded-actions.md`
- FR-7/FR-12: `SPEC-017-audit-append-only.md` and
  `contracts/audit-source-merge.md`
- FR-8: `SPEC-017-FR-8-owner-preview.md`
- FR-9: `SPEC-017-FR-9-no-break-glass.md`
- FR-10/FR-11: `SPEC-017-FR-10-command-metadata.md` and
  `docs/architecture/admin-confirmation-conformance.md`
- FR-13 and facade exclusion:
  `docs/architecture/admin-command-delegation-conformance.md`

Phase 5 route, handler, worker-host, client, and composition evidence remains
pending. No Phase 5 task is checked by this record.
