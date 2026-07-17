# SPEC-017 FR-1 Delegation Evidence

Date: 2026-07-17
Evidence test: `AdminCommandInvariantTests` (5/5 green)

SPEC-017 composes the Admin experience; it does not take command ownership
from a feature module. The current owner matrix is:

| Route | Canonical page owner | Canonical command/API owner | Current evidence |
|---|---|---|---|
| ADM-01 `/admin` | SPEC-017 | read-only SPEC-017 metrics query | Phase 5 route pending |
| ADM-02 `/admin/terms` | SPEC-008 | `Spec008Endpoints` / `AdminAcademicManagementService` | owner source and focused conformance green; SPEC-017 route contribution pending T075-T076 |
| ADM-03 `/admin/users` | SPEC-007 | `Spec007Endpoints` / `AdminUserLifecycleService` | owner source, final-Admin serialization, and focused conformance green; route contribution pending T077-T078 |
| ADM-04 `/admin/students` | SPEC-008 | `Spec008Endpoints` / academic profile owner | owner source and focused conformance green; route contribution pending T079-T080 |
| ADM-05 `/admin/catalogue` | SPEC-009 | `Spec009Endpoints` / catalogue services | owner source and publication conformance green; route contribution pending T081-T082 |
| ADM-06 `/admin/offerings` | SPEC-010 | `Spec010Endpoints` / scheduling services | owner source and version/validation conformance green; route contribution pending T083-T084 |
| ADM-07 `/admin/resources` | SPEC-010 | `Spec010Endpoints` / scheduling services | owner source and read-only availability conformance green; route contribution pending T085-T086 |
| ADM-08 `/admin/registrations` | SPEC-017 | bounded read-only registration projection | Phase 5 route pending T087-T088 |
| ADM-09 `/admin/audit` | SPEC-017 | read-only audit query and scoped export | Phase 5 route pending T089-T091 |

There is no `AdminCommandService` or `AdminConfirmationService`. T100 must
reject release until every Phase 5 pending row has green route evidence.
