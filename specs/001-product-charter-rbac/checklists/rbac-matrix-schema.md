# RbacMatrix Schema and Version Target

**Target schema:** `rbac-matrix/1.0`
**Recorded:** 2026-07-13 by Ahmed ELbamby
**Publication dependency:** T019, T021, T023, and T027 red tests observed

- [x] Each human-role row names the exact case-sensitive role token.
- [x] Each row lists governed permission tokens from `permissions.md`.
- [x] Each row states a maximum server-enforced data-scope rule.
- [x] Each row states material explicit denials.
- [x] Exceptional outcomes cover dual Lecturer/TeachingAssistant, no supported
  role, direct-route/client-role manipulation, and service-only reconciliation.
- [x] The artifact names SPEC-007 as runtime authorization owner.
- [x] The artifact creates no role entity, claim, endpoint, UI route, or policy
  source.

Version 1.0 changes require a reviewed SPEC-001 contract amendment. Runtime
implementation remains deferred to SPEC-007.
