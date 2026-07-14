# SPEC-007 NFR-4 Endpoint Authorization Matrix

**Scope:** the complete 16-endpoint SPEC-007 identity route inventory<br>
**Evidence date:** 2026-07-14<br>
**Protected endpoints governed by NFR-4:** 11<br>
**Anonymous lifecycle endpoints:** 5<br>
**Documentation state:** COMPLETE; runtime status comes from executing the cited tests

`Anonymous lifecycle` identifies a deliberately unauthenticated entry point;
its credential/proof and abuse-control outcomes are security behavior, not a
positive authorization principal. Every `Protected` row has route-specific
compiled metadata evidence and an executed positive/negative authorization
policy pair. Client route visibility is never an authorization boundary.

## Endpoint inventory and authorization matrix

| ID | Method | Route | Authorization surface | Positive case | Negative case | Route-specific evidence |
|---|---|---|---|---|---|---|
| Endpoint01 | POST | `/api/auth/student/login` | Anonymous lifecycle | Not applicable: deliberately anonymous | Invalid/disabled/locked/inactive credential receives generic 401 | [Endpoint01ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint01ContractTests.cs); [StudentLoginTests](../../tests/StudentRegistration.IntegrationTests/Identity/StudentLoginTests.cs) |
| Endpoint02 | POST | `/api/auth/student/activate` | Anonymous lifecycle | Not applicable: deliberately anonymous | Unknown/claimed/stale proof receives generic safe failure | [Endpoint02ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint02ContractTests.cs); [StudentActivationConcurrencyTests](../../tests/StudentRegistration.IntegrationTests/Identity/StudentActivationConcurrencyTests.cs) |
| Endpoint03 | POST | `/api/auth/staff/login` | Anonymous lifecycle | Not applicable: deliberately anonymous | Invalid/disabled/locked/no-supported-role receives generic 401 | [Endpoint03ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint03ContractTests.cs); [StaffPasswordLoginTests](../../tests/StudentRegistration.IntegrationTests/Identity/StaffPasswordLoginTests.cs) |
| Endpoint04 | POST | `/api/auth/logout` | Protected: Authenticated | Any authenticated enabled session | Anonymous principal denied | [Endpoint04ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint04ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint05 | POST | `/api/auth/recovery/request` | Anonymous lifecycle | Not applicable: deliberately anonymous | Every subject receives the same generic 202; abuse controls remain server-side | [Endpoint05ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint05ContractTests.cs); [SessionLifecycleTests](../../tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs) |
| Endpoint06 | POST | `/api/auth/recovery/complete` | Anonymous lifecycle | Not applicable: deliberately anonymous | Missing/expired/used/stale proof receives generic safe failure | [Endpoint06ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint06ContractTests.cs); [SessionLifecycleTests](../../tests/StudentRegistration.IntegrationTests/Identity/SessionLifecycleTests.cs) |
| Endpoint07 | POST | `/api/auth/password/change` | Protected: Authenticated | Any authenticated enabled session | Anonymous principal denied; wrong current password is a separate generic lifecycle failure | [Endpoint07ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint07ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint08 | POST | `/api/auth/sessions/revoke-all` | Protected: Authenticated | Any authenticated enabled session | Anonymous principal denied | [Endpoint08ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint08ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint09 | GET | `/api/auth/session` | Protected: Authenticated | Any authenticated enabled session with current stamp | Anonymous principal denied; stale/disabled session rejected by session validation | [Endpoint09ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint09ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint10 | PUT | `/api/auth/session/context` | Protected: StaffContext | Admin, Lecturer, or TeachingAssistant available-role claim | Anonymous, Student-only, or unassigned role denied | [Endpoint10ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint10ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs); [IdentityRuntimeCompositionTests](../../tests/StudentRegistration.SecurityTests/IdentityRuntimeCompositionTests.cs) |
| Endpoint11 | GET | `/api/admin/users` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint11ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint11ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint12 | POST | `/api/admin/users/imports` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint12ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint12ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint13 | GET | `/api/admin/users/imports/{importId}` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint13ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint13ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint14 | POST | `/api/admin/users/imports/{importId}/publish` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint14ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint14ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint15 | PATCH | `/api/admin/users/{userId}/status` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint15ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint15ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |
| Endpoint16 | PUT | `/api/admin/users/{userId}/roles` | Protected: IdentityManagement | Active Admin with server-issued IdentityManagement permission | Anonymous, Student, Lecturer, TeachingAssistant, or permission-less Admin denied | [Endpoint16ContractTests](../../tests/StudentRegistration.ContractTests/Specs/Spec007/Endpoint16ContractTests.cs); [IdentityEndpointAuthorizationMatrixTests](../../tests/StudentRegistration.AuthorizationTests/IdentityEndpointAuthorizationMatrixTests.cs) |

## Executable evidence layers

- `Endpoint01ContractTests` through `Endpoint16ContractTests` inspect the real
  compiled route metadata. They distinguish all five anonymous entry points
  from all 11 protected routes, verify the named StaffContext or
  IdentityManagement policy where applicable, and verify antiforgery metadata
  on mutations.
- `IdentityEndpointAuthorizationMatrixTests` discovers the 11 protected
  endpoints from `MapSpec007Endpoints`, combines their actual authorization
  metadata through ASP.NET Core's policy provider, and executes one permitted
  and one denied principal for every route.
- `StaffContextAuthorizationTests` and `IdentityResourceScopeTests` execute the
  selected-role, IdentityManagement, and resource-scope rules independently.
- `IdentityRuntimeCompositionTests` adds direct TestServer HTTP evidence for
  Endpoint10: Student is forbidden, a valid staff context succeeds, unavailable
  roles cannot be added, and antiforgery rejection occurs before the handler.
- `IdentityHttpSessionEvidenceTests` uses TestServer for missing-antiforgery
  rejection across every state-changing route, real logout-cookie expiry, and
  old-cookie rejection on two replicas after security-stamp rotation. These
  checks complement authorization; they do not claim dedicated positive and
  negative HTTP handler execution for every protected route.
- `AdminUserLifecycleTests` and `AdminUserLifecycleStorePersistenceTests`
  exercise final-enabled-Admin, concurrency, idempotency, and audit invariants
  after authorization. Those domain/SQL controls are defense in depth, not a
  replacement for endpoint authorization.

## Interpretation

The NFR-4 evidence is complete at the compiled endpoint-metadata plus executed
policy layer for all 11 protected routes. Direct TestServer handler-level
positive/negative authorization is evidenced for Endpoint10 only; no broader
HTTP claim is made. The quality validator verifies inventory, mappings, linked
test source, class declarations, and the executable matrix structure. It does
not treat this document's wording as proof that those suites passed on an
arbitrary commit.
