# SPEC-007 NFR-4 Endpoint Authorization Matrix

**Scope:** all 16 SPEC-007 identity endpoints  
**Evidence date:** 2026-07-14  
**Matrix rows:** 16  
**Result: PASS.**

`Anonymous` denotes a deliberately unauthenticated authentication/lifecycle
entry point. `Authenticated` denotes any enabled Student, Admin, Lecturer, or
TeachingAssistant session. Admin lifecycle commands additionally require the
Identity administration policy. Every protected row has positive and negative
coverage; client route visibility is never an authorization boundary.

| Endpoint | Route | Positive principal | Negative principal/outcome |
|---|---|---|---|
| Endpoint01 | `POST /api/auth/student/login` | Anonymous with valid student credential | Invalid/disabled/locked/inactive -> generic 401 |
| Endpoint02 | `POST /api/auth/student/activate` | Anonymous with pre-provisioned identity and valid initial credential | Unknown/claimed/stale -> generic 400 |
| Endpoint03 | `POST /api/auth/staff/login` | Anonymous with enabled provisioned staff credential | Invalid/no supported server role -> generic 401 |
| Endpoint04 | `POST /api/auth/logout` | Authenticated | Anonymous -> 401 |
| Endpoint05 | `POST /api/auth/recovery/request` | Anonymous | Every subject still receives generic 202; abuse controls apply |
| Endpoint06 | `POST /api/auth/recovery/complete` | Anonymous with valid single-use proof | Missing/expired/used/stale -> generic 400 |
| Endpoint07 | `POST /api/auth/password/change` | Authenticated | Anonymous -> 401; wrong current password -> generic 400 |
| Endpoint08 | `POST /api/auth/sessions/revoke-all` | Authenticated | Anonymous -> 401 |
| Endpoint09 | `GET /api/auth/session` | Authenticated | Anonymous, disabled, or stale security stamp -> 401 |
| Endpoint10 | `PUT /api/auth/session/context` | Admin, Lecturer, or TeachingAssistant selecting a server-returned role | Anonymous/Student/unassigned role -> denied |
| Endpoint11 | `GET /api/admin/users` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |
| Endpoint12 | `POST /api/admin/users/imports` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |
| Endpoint13 | `GET /api/admin/users/imports/{importId}` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |
| Endpoint14 | `POST /api/admin/users/imports/{importId}/publish` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |
| Endpoint15 | `PATCH /api/admin/users/{userId}/status` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |
| Endpoint16 | `PUT /api/admin/users/{userId}/roles` | Admin with identity-management permission | Anonymous, Student, Lecturer, TeachingAssistant -> 401/403 |

## Executable evidence

- Endpoint01-Endpoint16 contract tests inspect anonymous/protected metadata,
  antiforgery on every mutation, bounded DTOs, and policy assignment.
- `IdentityResourceScopeTests` proves the role/resource policy matrix.
- `AdminUserLifecycleTests` proves the final-enabled-Admin guard cannot be
  bypassed by a concurrent status or role request.
- `NFR-4EvidenceTests` rejects a missing or duplicate endpoint row and requires
  all five principal categories plus positive and negative outcomes.
