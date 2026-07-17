# SPEC-017 Dependency Baseline

**Recorded:** 2026-07-17  
**Feature:** SPEC-017 Admin Operations, Audit, and Reporting  
**Repository source baseline:** `6bf65e9c230443d6fa365ec0a0168d8a7ef5766c`  
**Result:** SCOPED PASS for dependency-ordered non-production demo work

This baseline freezes only the approved contracts consumed by SPEC-017. It
does not claim completion of SPEC-003 or SPEC-018 release work, does not repair
upstream documentation inside this feature, and does not authorize production,
Gate B-D, or official AASTMT use.

Hashes are SHA-256 for `requirements.md`, `plan.md`, `data-model.md`, and
`contracts/api.md`, in that order.

| Task | Dependency | Accepted status/version | Artifact hashes |
|---|---|---|---|
| T002 | SPEC-003 | Gate A approved; ADM-01..ADM-09 Page Design Records approved; 199/276 tasks; current artifact commit `41e0fb6af4cd8b98b5a9b9c86c26ec244389c810` | `0461b4d864a5898cc02ee124103d3199517754173400df6c280bb8dba19c3491`, `283c7efd72ae5a3e0a55abc7d81d45e471b4a9bc9a58a0cad81daba34d9d281f`, `e0af1871709e4e28de2cf99eee7f31ebf63344d1b71e0864879ec7e1cfb566df`, `03218fb4aadae2e2b9420559316816776aa4530eeec117fcedd9b98798a9c31e` |
| T002 | SPEC-004 | Gate A approved; 50/50 tasks; current artifact commit `c3e26a1e8b237bbe66b8ae4a5f98b2c1cdc4a82c` | `c4f45f1a6a6fccc4088b2a1e71486b0ca725578e8fb571538a779f5c5829d3e7`, `acef65fb1f3ff08b37a35118eeeee479de5cd8ca963c4fde1f6cceb9b4bad582`, `1fe5a62ba7cf43001c75be27cf184470a706e428acab15c54b02d772e2b3c5fb`, `851faf6dd2b0ecefb2ee3c22fbd1f16c20c94f03f96cce1973983c11228daca7` |
| T003 | SPEC-007 | Gate A approved; 133/133 tasks; current artifact commit `c3e26a1e8b237bbe66b8ae4a5f98b2c1cdc4a82c` | `da64ac6585e09fcb67105d3b755f095d969f76aeecb693dcf0214de828fb1fbc`, `3ded6edf4763a5c21c1396dedd08d404650e5f4c551cdf329a6d5c2ece610192`, `c0821084dbe77b8354c4e94b3cd615a2463141b7b99f4088682eed17406d0ab4`, `28b2cd5b9e1ecc3152cd6bf5ba2ba48bb85255a94c7ad44fdce72a9c04997e5b` |
| T004 | SPEC-008 | Gate A approved; 91/91 tasks; release commit `1aabed25105bf0fb8fbe7facd25d6f468baa6331` | `152f102d24c3c7f44b7b57400e76c590082306cea5e0fdbed20773dcd312c5b5`, `ba6c8fd308b85fe3db8d51d2d87b0ecc72871cb9a82f57f0ac6f7257c392949f`, `6bbf406a1f9d3afb55424e4dc5091bfe3162a01d464671b19c02afbfc8d49678`, `2e4d6177c66ab60c2214e9fd008c8531c7fecb28dfccfcc804cb50debc61e257` |
| T005 | SPEC-009 | Gate A approved; 100/100 tasks; release commit `12cfecfab1787cb719bda44318bb604bca603a22` | `d2bc3706c37711267c90dad7f3b025f3d432f51468b2357914f93b65e55f0e9e`, `170059f5b7a9247bd97fc291cbf940b9eb79a7957c5cfb6259c1edbdc7685764`, `428e30f129cf5af000b69596f2348fd8fc7f3ae47e9ebf57df413ad3678314e6`, `1a809fec654af45577b4ce73407690dddd678dfdffc4f943658c06ef4c10fb58` |
| T006 | SPEC-010 | Gate A approved; 110/110 tasks; release commit `99c25eee2fc1078605a426d19325351e69916140` | `7f5d133195dab8ac0d17092cd096eeea1300e8ed11f95689231f042243ed0e41`, `423dc929bebc756db37efdd77eee8244b621e9c7f2d9cb38385a85430c28cd0d`, `83862f7065e299cc622648fd9dfe1b7cab0e4532207e96a339835a5bc0e5f739`, `e17932d1088b28ac9738cc874bc269103cd0609e0af35d012a00384703e078c4` |
| T007 | SPEC-014 | Gate A approved and reconciled; 123/123 tasks; release commit `9510aeea58dab6b6f98e2cff1075e306644d6159` | `b853cd5885b4bd859b6d563ea76b1b1dd1a8b473f7ff9614da4ffd415af578f9`, `2a1912f7aebfd1854e962f84eabc0f4bb8c270dbdb7f64f36f89752f6090a861`, `fb62f2be5ed8ea8b60b5e14779bc67b0ad6e579053abad350e5c39e0e5c97231`, `9f222078261d4be486de20e9cb101e6ea102ac5bcf63dc52c90bc70ef25cfb07` |
| T008 | SPEC-015 | Gate A approved; 64/64 tasks; release commit `7235d1770a92faa6fbb794b41f2cc80b8387a1b7` | `8ff2fb774753d56851799741ccb0140a33f3b438b71c920a04b6424bfb46427d`, `3b5f843c38b74a7a287ee219f9d44989ef81b2bff559cedfab74aa6efbfecc52`, `a6e10ac657a2eabf85daae463f2e19374d65aa1b4f6b224a35ba048e356bc6d8`, `0814f0aa9a80ac17a5c48b93e1ca68eb594a610988cf1ae4a9e271859aa16648` |
| T009 | SPEC-016 | Gate A approved; 84/84 tasks; release commit `6bf65e9c230443d6fa365ec0a0168d8a7ef5766c` | `e4821b9ff979b16d2ad7dca346ed8944e0d6f4b50094f16f6167100fad729fb7`, `a12082a60c282e610f12e59ff7f9d7b4411aa292fffaf6e36c17b014140e9b0b`, `d458ae13fb4fd59e833b078480e71beba1c42a2c820089530b5fbaaeaa133a6a`, `d63754d9f624e9ffc77d78c6e81ec890f20b0d4179fdb2374a0b81864c740eeb` |
| T010 | SPEC-018 | Gate A approved; quality/operations foundation present; 59/74 tasks; current artifact commit `4ac311825bf2c82ec382e488db1a4de145650c79` | `9e0973c6670fec97733c2c0de15ffb4a5597889497216829c0a90f87fcd7118d`, `9dc035269854e7f142a9d4db323240360edcf43a187ca7af1b85e56215f6565a`, `940e519d5e9fad78908a00a40f9c0b905f4e41edc190ec8766e6220450814486`, `0423b7ab07d56a1e529a00378f24c0238a12bdcb628d107cbbb8eeee2dd07c6b` |

## Consumed boundaries

### T002 - Frontend and architecture

- SPEC-003 owns the approved ADM-01 through ADM-09 Page Design Records and
  shared accessible components. SPEC-017 implements ADM-01, ADM-08, and ADM-09
  and contributes bounded data/action contracts to ADM-02 through ADM-07.
  Pending SPEC-003 runtime/accessibility/visual checks are not claimed as
  complete; SPEC-017 owns the downstream evidence named in its task ledger.
- SPEC-004 owns the modular-monolith rules, shared DbContext, AuditEvent, and
  `IAuditEventWriter`. That writer joins the caller's EF transaction, never
  commits independently, and propagates failure so business state rolls back.

### T003 - Identity and final-Admin safety

- SPEC-007 exclusively owns ApplicationUser, RoleAssignment, SecurityEvent,
  AdminSecurityGuard, role/status commands, and FINAL_ADMIN_REQUIRED.
- Admin pages call the canonical Identity endpoint. A reducing change locks
  the singleton guard, re-counts active Admins, mutates, and writes SecurityEvent
  plus shared AuditEvent atomically. SPEC-017 adds no competing writer.

### T004 - Time, terms, student records, and holds

- SPEC-008 owns authoritative server time, terms/windows, academic profiles,
  and holds. Admin mutations remain on its permission-scoped, versioned,
  reasoned owner endpoints. SPEC-017 only links, observes, and audits them.

### T005 - Catalogue, policy, preview, and import

- SPEC-009 owns ImportBatch, catalogue/policy drafts, typed rules, validation,
  preview binding, publication, and their audit transaction. SPEC-017 consumes
  the canonical `contracts/api.md` hash above.
- The repository validator reports declaration drift between SPEC-009
  `requirements.md` and `contracts/api.md`. This bounded documentation-lint
  exception permits SPEC-017 implementation only; it expires at T100. No
  divergent contract is copied and release traceability must re-check the pin.

### T006 - Scheduling, capacity, and availability

- SPEC-010 exclusively owns offerings/groups/resources, capacity and timetable
  invariants, StaffTermAvailability, StaffAvailability, ScheduleImpactAlert,
  publication preview/version locks, and all mutations.
- Capacity may never be reduced below active enrollment. Admin availability is
  only `GET /api/admin/staff-availability` plus immutable planning input;
  mutation/correction/override routes and controls remain absent.
- The validator reports an abbreviated API declaration plus deliberate
  forbidden Admin availability mutation literals used as negative examples.
  SPEC-017 pins the canonical `contracts/api.md`; this bounded exception
  expires at T100 and cannot support a production or global-pass claim.

### T007 - Registration concurrency

- SPEC-014 exclusively owns registration writes, capacity allocation,
  idempotency, reconciliation, and repair. SPEC-017 may show timestamped
  observation/support references but exposes no repair, correction, drop,
  withdrawal, or seat-decrement action.

### T008 - Registration records

- SPEC-015 owns read projections for receipt, current timetable, history, and
  decision snapshots. Admin inspection remains permission- and scope-checked,
  audited, bounded, and read-only.

### T009 - Staff workspace

- SPEC-016 consumes Scheduling-owned assignments, roster, availability, and
  alerts. Its completed runtime confirms staff-owned availability replacement.
  SPEC-017 consumes only the bounded Admin view and impact/revalidation state.

### T010 - Quality and operations

- SPEC-018 owns OperationalMetric, observation timestamps, health semantics,
  safe errors/correlation, telemetry privacy, SQL Server evidence conventions,
  accessibility, security, load, recovery, and release gates.
- The 15 pending SPEC-018 tasks, including later manual/load/failover/recovery
  and Gate D evidence, remain deferred and are not represented as complete.

## Dependency result

The direct graph `003/004/007/008/009/010/014/015/016/018 -> 017` is valid and
acyclic. Model ownership, module direction, persistence ownership, security,
concurrency, availability, and excluded-action boundaries are consistent.
The two documentation-lint exceptions are exact, time-bounded, and accepted by
Ahmed Elbamby for this demo under the current instruction; they do not relax
runtime behavior or release evidence.
