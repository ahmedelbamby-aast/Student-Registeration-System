# SPEC-016 Dependency Baseline

**Recorded:** 2026-07-17
**Feature:** SPEC-016 Lecturer and Teaching Assistant Workspace
**Repository source baseline:** `7235d1770a92faa6fbb794b41f2cc80b8387a1b7`
**Result:** PASS for T002-T006 after aligning SPEC-016 to the canonical
SPEC-010 availability vocabulary and recording a time-bounded declaration-lint
exception. The bounded runtime delivery gap is described under SPEC-010.

This baseline freezes only the approved contracts consumed by SPEC-016. It
authorizes dependency-ordered non-production demo work after T007/T008 pass.
It does not authorize production deployment, official AASTMT use, Gate B-D,
or release sign-off.

## Accepted artifacts

Hashes are SHA-256 for `requirements.md`, `plan.md`, `data-model.md`, and
`contracts/api.md`, in that order.

| Task | Specification | Accepted status/version | Artifact hashes |
|---|---|---|---|
| T002 | SPEC-003 | Gate A approved; frontend governance 1.1, route manifest 2.1.0, immutable STF-01..STF-04 records 1.0; design baseline commit `8ee8f724af3b60bbbc464532bc68de6f173247e5`; implementation remains partial (199/276 tasks) | `0461b4d864a5898cc02ee124103d3199517754173400df6c280bb8dba19c3491`, `283c7efd72ae5a3e0a55abc7d81d45e471b4a9bc9a58a0cad81daba34d9d281f`, `e0af1871709e4e28de2cf99eee7f31ebf63344d1b71e0864879ec7e1cfb566df`, `03218fb4aadae2e2b9420559316816776aa4530eeec117fcedd9b98798a9c31e` |
| T003 | SPEC-007 | Gate A approved and bounded demo release complete; 133/133 tasks; release commit `99f1db3358ec66c948d8f404b28891784880cfc1` | `da64ac6585e09fcb67105d3b755f095d969f76aeecb693dcf0214de828fb1fbc`, `3ded6edf4763a5c21c1396dedd08d404650e5f4c551cdf329a6d5c2ece610192`, `c0821084dbe77b8354c4e94b3cd615a2463141b7b99f4088682eed17406d0ab4`, `28b2cd5b9e1ecc3152cd6bf5ba2ba48bb85255a94c7ad44fdce72a9c04997e5b` |
| T004 | SPEC-010 | Gate A approved; canonical Scheduling baseline and 110/110 tasks complete; current artifact commit `99c25eee2fc1078605a426d19325351e69916140` | `7f5d133195dab8ac0d17092cd096eeea1300e8ed11f95689231f042243ed0e41`, `423dc929bebc756db37efdd77eee8244b621e9c7f2d9cb38385a85430c28cd0d`, `83862f7065e299cc622648fd9dfe1b7cab0e4532207e96a339835a5bc0e5f739`, `e17932d1088b28ac9738cc874bc269103cd0609e0af35d012a00384703e078c4` |
| T005 | SPEC-015 | Gate A approved; readiness revalidated and bounded demo release approved 2026-07-17; 64/64 tasks; release commit `7235d1770a92faa6fbb794b41f2cc80b8387a1b7` | `8ff2fb774753d56851799741ccb0140a33f3b438b71c920a04b6424bfb46427d`, `3b5f843c38b74a7a287ee219f9d44989ef81b2bff559cedfab74aa6efbfecc52`, `a6e10ac657a2eabf85daae463f2e19374d65aa1b4f6b224a35ba048e356bc6d8`, `0814f0aa9a80ac17a5c48b93e1ca68eb594a610988cf1ae4a9e271859aa16648` |
| T006 | SPEC-018 | Gate A approved; readiness frozen PASS for dependency-ordered non-production work at `3505b84b814705d801fbef5275a0c1dd65a37cb0`; current foundation `4ac311825bf2c82ec382e488db1a4de145650c79`; release pending (59/74 tasks) | `9e0973c6670fec97733c2c0de15ffb4a5597889497216829c0a90f87fcd7118d`, `9dc035269854e7f142a9d4db323240360edcf43a187ca7af1b85e56215f6565a`, `940e519d5e9fad78908a00a40f9c0b905f4e41edc190ec8766e6220450814486`, `0423b7ab07d56a1e529a00378f24c0238a12bdcb628d107cbbb8eeee2dd07c6b` |

## T002 - SPEC-003 frontend contract

- STF-01 through STF-04 remain immutable design records owned by SPEC-003 and
  are implemented only by SPEC-016. Their hashes are respectively
  `6cf8d7a0cf16bb64a78f7c67ac8a0de060190d826176431dcdfe6aa98da44ff9`,
  `afedc46cea8b0a11647d5e7dd865c6be004413fe520111502118750236c3e884`,
  `dd2becc9e55335c9c9c4646173996c0499f16660905ff513362affe0b6b3ae19`,
  and `17e86752f9aef2d1faad1068755fb26b2fe8b92002ddea100fc41bd56d0dbd3e`.
- The current `design-only`/SPEC-016-not-pinned state permits test-first route
  work but is not release evidence. SPEC-016 must record its downstream pin
  only after contract, component, accessibility, and browser evidence passes.
- Shared AppShell, role navigation, state panels, group cards, pagination,
  calendar/list, form, validation, and focus patterns remain binding.

## T003 - SPEC-007 identity and context contract

- Staff use the shared staff login. The server derives Lecturer and
  TeachingAssistant roles and the active context; no client role claim is
  accepted.
- SPEC-016 consumes the authenticated session/user identifier, Context.Read,
  and assigned-teaching-resource scoping. It does not add a second login or a
  broad staff permission.
- Downstream Student/Admin permission additions in current RolePolicies do not
  change the Lecturer/TA Context.Read boundary consumed here.

## T004 - SPEC-010 Scheduling contract

- Scheduling exclusively owns GroupStaffAssignment, StaffTermAvailability,
  StaffAvailability, ScheduleImpactAlert, their EF mappings, and their
  database transaction/rowversion rules. StaffAdministration owns no duplicate
  entity, repository, mapping, or migration.
- The Admin availability surface is read-only. The validator currently reports
  forbidden Admin PUT/POST/PATCH/DELETE examples as unregistered endpoints;
  this is expected negative-contract evidence because only
  `GET /api/admin/staff-availability` is registered and mapped.
- A real bounded delivery gap remains: the repository has no narrow production
  Scheduling application port that returns the updated complete aggregate and
  impact-alert IDs atomically. T012 establishes that port contract; T056-T057
  and T060-T065 may implement and consume it without moving domain or
  persistence ownership out of Scheduling.
- SPEC-016 now consumes the canonical two-value `kind` vocabulary
  (`available | unavailable`) and Admin selection of aggregate ID plus
  rowversion. The former downstream `preferred`/range-copy wording was removed
  before implementation.
- SPEC-010's older abbreviated requirements API block remains inconsistent with
  its canonical `contracts/api.md`. SPEC-016 pins the contract hash above under
  the explicit exception in `checklists/implementation-readiness.md`; the
  exception expires before release traceability T083.

## T005 - SPEC-015 records boundary

- Lecturer/TA consume none of SPEC-015's Student/Admin endpoints or
  receipt/history/decision DTOs. SPEC-016 owns a separate assigned-group roster
  projection.
- The roster may read canonical active Enrollment rows by assigned GroupId;
  SPEC-014 remains their owner/writer. UniversityId and DisplayName come from
  identity/student sources, not SPEC-015.
- No drop, correction, receipt inspection, or seat mutation capability is
  exposed to staff.

## T006 - SPEC-018 quality boundary

- SPEC-016 inherits privacy-safe errors/correlation, exact scope-denial tests,
  PII-minimized audit/telemetry, isolated synthetic SQL evidence, WCAG 2.2 AA,
  keyboard and list/table equivalence, and the approved browser matrix.
- SPEC-016 retains its stricter 300 ms p95 read target and exact roster field
  allow-list.
- SPEC-018 release/Gate D is not inherited. Its manual staff-route
  accessibility, load/failover/recovery/coverage, cross-replica, traceability,
  and release-approval evidence remains pending and cannot be claimed here.

## Dependency result

The direct graph `SPEC-003/007/010/015/018 -> SPEC-016` is valid and acyclic.
Ownership remains consistent and no distributed component or duplicate model
is introduced. The accepted planning contracts permit dependency-ordered
test-first work; the narrow Scheduling port gap is explicit implementation
work, not an ownership ambiguity.
