# SPEC-017 Phase Ledger

**Branch:** `codex/017-admin-operations-audit-reporting`

This ledger records only commits whose phase tasks have named evidence and
passed their scoped verification. A matching local and remote SHA proves the
required phase push; it does not replace tests or task-specific evidence.

| Phase | Tasks | Evidence result | Evidence commit | Remote verification |
|---|---|---|---|---|
| 1 - Planning baseline and Gate A verification | T001-T012 | SPEC-017 score 100, automated gates PASS, human approval APPROVED; no scoped failures | `98d25e9afe207d42dfc46ddb70c45752f14045de` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 2 - Models and API contracts | T013-T031 | 31/101 tasks checked; solution build clean; 16 focused integration and 15 endpoint contract tests green; four Docker/SQL proofs green; deferred T014/T025/T028/T031 red only for named future delivery; SPEC-017 validator PASS | `7f29e7e836dd3f5970395531830a5e74a53eb15e` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 3 - Acceptance and edge evidence | T032-T046 | 46/101 tasks checked; 5 acceptance and 3 edge conformance tests green; 4 acceptance and 3 edge tests compile-safe red only for named future deliveries; solution build and SPEC-017 validator PASS | `8da68af94876770c3b5a4606f5cbcef57fad62d8` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 4 - Requirement tests and bounded delivery | T047-T072 | 72/101 tasks checked; clean red-before evidence for audit, metrics, export, and command metadata; 8 authorization, 14 integration (including real SQL/two replicas), 16 SPEC-017 contract, 2 export application, 2 edge, and 3 upstream atomicity checks green; solution build clean; SPEC-017 score 100/PASS with zero scoped findings | `bc20c347d1cbce4f95170d4ad8e0769e5d474f8a` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 5 - Frontend routes and API integration | T073-T091 | 91/101 tasks checked; UI red-before gates clean; 16 API contract, 7 application, 8 authorization, 16 integration, 8 focused acceptance, 21 E2E, 3 client contract, 5 component, 9 accessibility, 8 visual, 1 architecture, and 1 migration check green; solution build clean; SPEC-017 score 100/PASS with zero scoped findings; AC-9 remains downstream-only red for T092-T095 | `8d997d0343ed58390462f966822b1cf7c2d5b0d0` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 6 - Measurable non-functional evidence | T092-T095 | 95/101 tasks checked; 9/9 NFR quality checks green; 60-second freshness boundary, real-SQL 100,000-row audit p95, two-replica maximum-size export publication/expiry, and complete Admin control matrix recorded; Release build clean; SPEC-017 score 100/PASS with zero scoped findings | `5550bf19427fbc4d390f665ea751384f015c8234` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |
| 7 - Scope and release evidence | T096-T101 | 101/101 tasks checked; four out-of-scope exclusions inspected; 49/49 trace rows PASS; 6/6 trace/approval schema checks and AC-1 through AC-9 green; Release build clean; SPEC-017 score 100/PASS with zero scoped findings; seven demo review perspectives approved with explicit nonclaims | `2a9d1003b84a7e05a25f751ff387d03572c34db3` | `origin/codex/017-admin-operations-audit-reporting` resolved to the same SHA on 2026-07-17 |

The repository-wide specification audit still reports disclosed failures
outside SPEC-017. Phase 1 claims a scoped SPEC-017 PASS only.
