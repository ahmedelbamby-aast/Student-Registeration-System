# DTO Isolation and Safe Error Boundary

## DTO projection only

Every endpoint projects a feature-owned response DTO. EF entities never cross
the application-service or HTTP boundary, and public contracts contain no EF
navigation, lazy-loading proxy, DbContext, provider type, or persistence-only
foreign key. A rowversion is exposed only as the opaque contract field required
for an authorized concurrency workflow; persistence-specific representation is
not exposed.

## Credential and identity safety

No public DTO contains a password, PIN, password hash, security stamp,
concurrency stamp, reset secret, token, connection value, or internal identity
key. Public identifiers are the minimum opaque values required by the owning
feature contract. Logs and support correlation never become response
diagnostics.

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
Framework-owned bad requests and a cancellation caused by the disconnected
request are also declined by the unexpected-error handler; their owning ASP.NET
Core/request pipeline remains responsible for the applicable 4xx or aborted
connection behavior. Real endpoint proof remains gated by T025-T028.
