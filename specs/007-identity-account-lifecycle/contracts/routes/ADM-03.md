# ADM-03 Identity Route Contribution

**Route:** `/admin/users`  
**Design owner:** SPEC-003  
**Identity implementation owner:** SPEC-007  
**Contributor:** SPEC-017 may consume the route contract but does not write identity state.

This contribution pins the Identity-side data, actions, and states consumed by
the approved ADM-03 Page Design Record. The canonical
`UserAdministrationPage.razor` is delivered only after the Phase 7 identity
commands exist.

## Data and actions

- `GET /api/admin/users` provides a bounded, paginated, minimized user list.
- `POST /api/admin/users/imports` validates a provenance-bearing,
  idempotent pre-provisioning batch without accepting plaintext credentials.
- `GET /api/admin/users/imports/{importId}` returns bounded status and safe row
  errors only inside the caller's authorized scope.
- `POST /api/admin/users/imports/{importId}/publish` publishes a validated
  batch atomically and replays only a same-key/same-payload result.
- `PATCH /api/admin/users/{userId}/status` requires a reason and the aggregate
  `expectedRowVersion`.
- `PUT /api/admin/users/{userId}/roles` replaces only allow-listed roles and
  requires a reason and the same aggregate `expectedRowVersion`.

## Route states

| State | Identity contribution |
|---|---|
| loading | Disable duplicate commands and preserve the current focus. |
| empty | Explain that no authorized results match; never present it as command success. |
| success | Show the server-returned status, roles, row version, import totals, and safe row results. |
| validation-error | Preserve `VALIDATION_FAILED`, `IMPORT_INVALID`, `IMPORT_NOT_VALIDATED`, or `FINAL_ADMIN_REQUIRED` with linked correction guidance. |
| stale | Preserve `STALE_VERSION`; refresh the selected user/import before another command. |
| unauthorized | Remove protected content and expose only a safe authorized-home action. |
| session-expired | Remove protected content and offer the shared `/staff/login` route. |
| service-error | Show a generic message, correlation reference, retry, and support action. |
| offline | Never queue or claim an identity mutation. |

## Security and concurrency boundary

The page never decides permissions, grants roles, guesses current versions, or
bypasses the Identity-owned final-enabled-Admin guard. A reducing command may
return `FINAL_ADMIN_REQUIRED`; the UI explains that the final enabled Admin
must remain and leaves every control unchanged. A `STALE_VERSION` result
requires refresh and review. Success is shown only from a server-accepted
result. Every mutation includes the same-origin antiforgery request token, and
the route stores no credential or long-lived browser token.
