# SPEC-017 Phase 5 Evidence

Date: 2026-07-17
Branch: `codex/017-admin-operations-audit-reporting`
Tasks: T073-T091 (91/101 total checked)

## Delivered routes and integration

- ADM-01 live Admin dashboard with server timestamps, explicit
  live/stale/degraded states, missing-series text, accessible metric
  equivalents, warnings, pause/resume, and manual/30-second refresh;
- ADM-02 through ADM-07 contributor contracts and executable checks while the
  canonical owner pages remained unchanged;
- ADM-08 read-only registration-monitor route with no repair/correction
  surface and ADM-09 scoped audit/export route shell;
- five authorized, rate-limited SPEC-017 endpoints, canonical shared DTOs,
  the registered Blazor client, and existing composition-root registration;
- bounded durable export worker using a conditional renewable SQL lease,
  persisted canonical filter snapshot, at most three attempts, and one
  shared-file demo artifact; and
- four Admin permission policies plus safe `429 RATE_LIMITED` JSON handling.

The small `Spec017ExportFilter` migration adds only the bounded canonical
filter snapshot required for a recovered worker to reproduce a durable job.
No in-memory queue or message broker was introduced.

## Red-before evidence

| Lane | Expected-red result |
|---|---|
| ADM-01 T073 | all five named test families compiled and failed only for the absent page/style delivery |
| ADM-08/ADM-09 T087/T089 | all ten named checks compiled and failed only for the absent two pages/styles |
| T091 handlers | 3/3 focused application checks failed only because `Spec017Endpoints.cs` was absent |

ADM-02 through ADM-07 are contributor-only tasks and were verified against
their existing owner pages without manufacturing a red owner implementation.

## Final automated evidence

| Suite | Result |
|---|---|
| SPEC-017 API contract tests | 16/16 passed |
| SPEC-017 application behavior tests | 7/7 passed |
| SPEC-017 authorization/policy focus | 8/8 passed |
| Metrics/audit/export/concurrency/edge integration focus | 16/16 passed, including real SQL, two replicas, persisted-filter round trip, and next-job claim |
| SPEC-017 acceptance excluding downstream-only AC-9 | 8/8 passed |
| SPEC-017 E2E and contributor journeys | 21/21 passed |
| ADM-01/08/09 client contract | 3/3 passed |
| ADM-01/08/09 component | 5/5 passed |
| ADM-01/08/09 accessibility/axe | 9/9 passed |
| ADM-01/08/09 responsive visual | 8/8 passed |
| Scoped SPEC-017 architecture boundary | 1/1 passed |
| Migration bundle | 1/1 passed |
| `dotnet build StudentRegistration.slnx --no-restore` | succeeded, 0 warnings, 0 errors |
| `Test-AllSpecs.ps1 -Phase Implementation` scoped row | SPEC-017: 22 artifacts, 101 tasks, score 100, PASS, APPROVED; zero SPEC-017 findings |

AC-9 remains the deliberate downstream red for the four measurable NFR
documents/tests in T092-T095. This does not claim those tasks are complete;
T100 rejects release until AC-9 and every NFR row are green. The
repository-wide validator still reports disclosed failures in other specs,
while SPEC-017 itself is clean.
