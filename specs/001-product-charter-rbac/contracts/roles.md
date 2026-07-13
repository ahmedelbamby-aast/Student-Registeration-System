# Governed Role Vocabulary

**Schema:** `role-definition/1.0`
**Artifact owner:** SPEC-001
**Runtime owner: SPEC-007**

Role tokens are case-sensitive and are the only values that may cross identity,
session, API, audit, and authorization boundaries. UI copy may display
“Teaching Assistant” or “TA”, but those labels are never claim values.

## Canonical role definitions

| Token | Human meaning | Default data scope | Entry context |
|---|---|---|---|
| `Student` | Institutionally provisioned learner | The authenticated student's own records | Student |
| `Admin` | Explicitly permissioned institutional operator | Only capabilities and institutional data explicitly granted by server policy | Shared staff |
| `Lecturer` | Instructor assigned to lecture activities | Own staff record and assigned groups | Shared staff |
| `TeachingAssistant` | TA assigned to tutorial, section, or lab activities | Own staff record and assigned groups | Shared staff |

## Governance rules

- A role token describes a server-verified identity capability; it is not a
  client-selectable privilege.
- Student identity is institutionally provisioned. Public staff registration
  and student selection of a staff context are forbidden.
- A staff user with more than one effective supported role may select only one
  already-authorized context. Selection does not add a role.
- A staff identity with no supported effective role is denied with a safe
  no-role outcome.
- This artifact MUST NOT create a runtime role assignment, claim, database
  entity, or executable policy. Runtime RoleAssignment, effective dates,
  claims, and session context are exclusively owned by SPEC-007.
