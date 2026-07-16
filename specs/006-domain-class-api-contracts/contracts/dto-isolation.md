# DTO Isolation and Safe Error Boundary

## DTO projection only

Every endpoint projects a feature-owned response DTO. EF entities never cross
the application-service or HTTP boundary, and public contracts contain no EF
navigation, lazy-loading proxy, DbContext, provider type, or persistence-only
foreign key. A rowversion is exposed only as the opaque contract field required
for an authorized concurrency workflow; persistence-specific representation is
not exposed.

## Credential and identity safety

No response DTO contains a password, PIN, recovery proof, password hash,
security stamp, concurrency stamp, reset secret, connection value, or internal
identity key. The complete secret-bearing request allow-list is:

- `StudentLoginRequest.Password`
- `StaffLoginRequest.Password`
- `ActivateStudentRequest.InitialPassword`
- `ActivateStudentRequest.NewPassword`
- `RecoveryCompleteRequest.ChallengeToken`
- `RecoveryCompleteRequest.NewPassword`
- `ChangePasswordRequest.CurrentPassword`
- `ChangePasswordRequest.NewPassword`

These values are transient input: they are never echoed in a response,
persisted raw, written to logs, audit payloads, or support diagnostics. Adding
another secret-bearing contract member requires an approved contract revision
and an update to the executable allow-list. Public identifiers are the minimum
opaque values required by the owning feature contract.

## Authorization before disclosure

Authentication and data-scope authorization precedes existence, version, and
conflict disclosure. An unauthorized caller receives 403 without a protected
identifier, rowversion, or `ApiError.currentVersion`; the API does not confirm
whether the protected resource exists. Only an authorized stale mutation may
receive 409 `STALE_VERSION` and the safe current version declared by its feature
contract.

## Unexpected errors

The API composition pipeline converts an unexpected exception to HTTP 500 with
only `UNEXPECTED_ERROR`, a generic safe message, and an opaque correlation ID.
It never serializes the exception message, stack trace, SQL text, connection
information, secret, raw payload, or internal topology. Feature-owned expected
validation, authorization, concurrency, and business outcomes remain explicit
typed results and are not routed through this fallback.
Framework-owned malformed-body and unsupported-media failures are normalized
at the API boundary to safe 400 `VALIDATION_ERROR` and 415
`UNSUPPORTED_MEDIA_TYPE` responses without exposing parser or endpoint
internals. A cancellation caused by a disconnected request remains declined so
the request pipeline can preserve aborted-connection behavior. Real endpoint
proof for cancellation and replay remains gated by T027.
