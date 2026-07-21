# Canonical RBAC Matrix

**Schema:** `rbac-matrix/1.0`
**Artifact owner:** SPEC-001
**Runtime authorization owner: SPEC-007**

All permission tokens are defined once in [`permissions.md`](permissions.md).
The matrix states maximum human-role grants; owning feature policies may narrow
them further and may never widen them from browser input.

## Human role grants

| Role | Workspace | Governed permission grants | Data-scope rule | Explicit denials |
|---|---|---|---|---|
| `Student` | student workspace | `Context.Read`, `AcademicProfile.ReadOwn`, `Catalogue.ReadAvailable`, `RegistrationPlan.ManageOwn`, `Registration.SubmitOwn`, `RegistrationRecords.ReadOwn` | self and server-filtered published catalogue | Staff context, another student's data, unpublished data, and every academic/capacity override |
| `Admin` | admin workspace | `Context.Read`, `IdentityAccess.Manage`, `AcademicTerms.Manage`, `AcademicProfiles.Manage`, `CataloguePolicy.Manage`, `Offerings.Manage`, `RegistrationRecords.Read`, `Audit.Read`, `Reports.Export` | institutional-admin, narrowed by each named permission and requested resource; academic-profile location requires AcademicTermId plus a bounded University ID/name query and returns minimal locator fields, while detail/correction requires named StudentId plus AcademicTermId | Implicit superuser access, permission inference from the Admin role, unrestricted student listing, own-role escalation, final-Admin removal, staff availability mutation, service reconciliation, and policy/invariant bypass |
| `Lecturer` | lecturer workspace | `Context.Read`, `TeachingAssignments.ReadOwn`, `AssignedRosters.Read`, `Availability.ManageOwn` | self and assigned-groups where a Lecturer assignment is effective | Admin mutations, unrelated groups/students, capacity change, grade/attendance scope, and TA context without an effective TA role |
| `TeachingAssistant` | teaching-assistant workspace | `Context.Read`, `TeachingAssignments.ReadOwn`, `AssignedRosters.Read`, `Availability.ManageOwn` | self and assigned-groups where a TA assignment is effective | Admin mutations, unrelated groups/students, capacity change, grade/attendance scope, and Lecturer context without an effective Lecturer role |

## Exceptional outcomes

| Situation | Server outcome |
|---|---|
| Combined Lecturer and TeachingAssistant assignment | Reject authentication as invalid role configuration; require separate single-role accounts |
| No supported staff role | deny staff workspace access with a safe no-role message and support reference |
| Direct route or client role change | deny the unauthorized API request; route state never grants a role or permission |
| Missing permission or resource scope | deny without disclosing protected resource existence |
| Admin has `AcademicTerms.Manage` but not `AcademicProfiles.Manage`, or the reverse | authorize only the independently granted capability; never infer the missing permission from the Admin role or the other grant |
| Service reconciliation request | `Registration.Reconcile` is never granted to a human role; only the restricted operations service identity may satisfy it |

## Implementation boundary

SPEC-007 consumes this matrix when it defines executable authorization policy
constants and handlers. Identity persistence, RoleAssignment, claim issuance,
session context, and policy source remain outside SPEC-001. Frontend visibility
is never accepted as authorization evidence.
