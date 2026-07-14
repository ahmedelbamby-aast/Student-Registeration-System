# SPEC-001 Implementation Readiness

**Baseline state:** FROZEN FOR DEMO IMPLEMENTATION
**Frozen:** 2026-07-13 by Ahmed ELbamby

- [x] `spec.md` and `requirements.md` are approved and contain no unresolved
  clarification marker.
- [x] `plan.md`, `research.md`, `data-model.md`, `contracts/api.md`, and
  `quickstart.md` are present and mutually consistent.
- [x] The governed entity vocabulary is fixed to RoleDefinition,
  PermissionDefinition, and RbacMatrix.
- [x] The four role tokens are fixed to Student, Admin, Lecturer, and
  TeachingAssistant.
- [x] SPEC-001 owns no runtime API, UI route, persistence model, or policy
  implementation.
- [x] The executable task baseline contains T001-T042 in dependency order.
- [x] Institutional decisions and non-production-demo limitations are recorded
  in the project plan and approval record.
- [x] The dependency baseline and versioned architecture contract are frozen.

Changes to a normative requirement, ownership boundary, role token, or task
dependency require a reviewed specification amendment. Additional governed
permission definitions may be versioned without transferring runtime policy
ownership away from SPEC-007.

## 2026-07-14 amendment revalidation

**State:** APPROVED FOR THE BOUNDED NON-PRODUCTION DEMO AMENDMENT<br>
**Approved by:** Ahmed ELbamby

- [x] `AcademicProfiles.Manage` now records the bounded term/query locator and
  named student/term detail-correction scopes without granting a broad dump.
- [x] SPEC-001 still owns permission vocabulary only; SPEC-007 owns executable
  policy and the downstream academic feature owns its endpoints/data access.
- [x] T001-T042 and the current 27/42 completion truth are preserved; no new
  task or checked completion is invented by this amendment.
- [x] Gates B-D, release evidence, real institutional data, and production
  authorization remain outside this approval.
