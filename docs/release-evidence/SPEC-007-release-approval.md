# SPEC-007 Demo Release Approval

**Decision date:** 2026-07-14<br>
**Sole decision authority and developer:** Ahmed ELbamby<br>
**Decision:** APPROVED for the Development and Testing design-capability demo

This is Ahmed ELbamby's human decision record. It is not a substitute for
running the cited suites on the current commit, and it grants no institutional
or Production authority.

| Perspective | Demo decision | Evidence boundary reviewed |
|---|---|---|
| Product | APPROVED | AC-1..AC-10 and SC-1..SC-3 mappings backed by identity behavior, rendered bUnit journeys, and the explicit scope review. |
| Security | APPROVED | Cookie/antiforgery configuration; TestServer missing-token rejection and logout/session behavior; keyed abuse control; enumeration evidence; Production startup guards. |
| Identity owner | APPROVED | Activation, login, recovery, session context, password/session invalidation, and Admin lifecycle behavior. |
| QA | APPROVED | Contract, client unit, authorization, security, integration, acceptance, E2E-component, real-SQL, and quality projects. A changed commit requires rerun. |
| Accessibility | APPROVED FOR COMPONENT-SEMANTICS SCOPE | [FormFieldTests](../../tests/StudentRegistration.Client.UnitTests/Components/FormFieldTests.cs), [ConfirmationDialogTests](../../tests/StudentRegistration.Client.UnitTests/Components/ConfirmationDialogTests.cs), and [ExecutableIdentityJourneys](../../tests/StudentRegistration.E2ETests/Specs/Spec007/ExecutableIdentityJourneys.cs) cover rendered labels/errors/live regions, native keyboard-operable controls, secret-reveal semantics, and dialog escape/focus-restoration contracts. No axe scan or real-browser keyboard audit is claimed. |
| Data / concurrency | APPROVED | EF model checks and real-SQL uniqueness, single-use/savepoint rollback, import handoff/audit atomicity, and final-enabled-Admin race coverage. |
| Operations | APPROVED FOR DEMO SCOPE | Bounded local artifacts, shared-state/key boundaries, service-level NFR-1 evidence, runtime composition, hosted-WASM smoke coverage, and fail-closed Production guards. No full Production topology is approved. |

## Evidence interpretation

- bUnit journeys render and interact with the real Razor components while a
  controlled HTTP handler records and answers requests. They are not a
  real-browser or live-network substitute.
- TestServer evidence executes ASP.NET Core middleware/HTTP behavior for the
  identity session/security paths it names. The compiled authorization matrix
  executes positive and negative policy decisions for all protected endpoint
  metadata; it does not claim dedicated HTTP handler execution for every
  endpoint.
- Real-SQL tests are required to run when their SQL prerequisite is available;
  a missing prerequisite cannot be represented as a passing SQL result.
- The SPEC-007 NFR-1 measurement is service-level. Full HTTP/SQL multi-host
  load and operational evidence remains governed by SPEC-018.

## Environment decision

- Development: APPROVED for synthetic demo data and bounded Git-ignored local
  credential/recovery artifacts.
- Testing: APPROVED for isolated synthetic fixtures, controlled delivery
  adapters, TestServer verification, and real-SQL integration verification.
- **Production NOT APPROVED.** The institutional credential policy,
  identity/recovery/provisioning providers, secret custody, deployment
  topology, operational authority, real-browser accessibility audit, and
  official AASTMT authorization remain unresolved Production gates and must
  fail closed where configured.

This decision completes only the SPEC-007 Development/Testing demo release
gate. It does not approve later feature specs, real AASTMT data, an official
institutional identity integration, or a complete-system release.
