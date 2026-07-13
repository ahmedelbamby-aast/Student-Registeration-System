# Data Model: Product Charter and RBAC

## Owned Governance Artifacts

- **RoleDefinition**: Governed role vocabulary owned by SPEC-001.
- **PermissionDefinition**: Governed capability/data-scope definition owned by SPEC-001.
- **RbacMatrix**: Governed role-to-permission mapping owned by SPEC-001.

Runtime `RoleAssignment` is referenced from SPEC-007 and is not owned here.

## Detailed Model

| Concept | Artifact role | Runtime owner | Required values |
|---|---|---|---|
| RoleDefinition | Governed vocabulary artifact | SPEC-001; consumed by SPEC-007 | Student, Admin, Lecturer, TeachingAssistant |
| PermissionDefinition | Governed capability/data-scope artifact | SPEC-001; consumed by SPEC-007 | Stable server policy name and allowed operations |
| RbacMatrix | Governed mapping artifact | SPEC-001; consumed by SPEC-007 | Role, permission, scope rule, denied operations |

## Governance Rules

- The role vocabulary and permission matrix are versioned planning artifacts.
- Every protected capability maps to one or more server policies owned by SPEC-007.
- Client routes and controls never create roles or extend data scope.
- Runtime identity constraints, effective dates, concurrency, and retention are
  defined by SPEC-007 and SPEC-005 rather than duplicated here.
