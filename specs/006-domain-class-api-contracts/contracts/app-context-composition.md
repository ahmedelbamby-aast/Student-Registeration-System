# Application Context Composition Contract

## Ownership

- SPEC-006 owns the response schemas `AppContextDto`, `TermSummaryDto`,
  `RegistrationWindowSummaryDto`, and `PublicContextDto`; it owns no handler
  or contributor runtime.
- SPEC-007 owns identity, session, and authorized-role contribution for the
  authenticated response.
- SPEC-008 owns authoritative time, term, window, and service contribution.
- SPEC-008 owns both context handlers: `GET /api/context` and
  `GET /api/public/context`.

The browser never composes, fills, or upgrades context data. It consumes one
complete server response and treats a safe error as an unavailable context.

## Complete authenticated response

`GET /api/context` requires successful SPEC-007 and SPEC-008 contributions.
The server intersects role candidates with its authorization rules and emits
only the server-authorized role set. A client-supplied role never grants
authorization. `activeRole` is a member of that set for an active or expiring
session. When multiple roles require a choice, the session state is
`role-selection-required` and `activeRole is null`; choosing a role requires a
separate server-validated identity workflow owned by SPEC-007.

The response includes authoritative UTC server time, timezone ID, teaching and
registration terms, registration-window state, the matched window's ID, UTC
opening/closing instants and row version, service state, display name,
authorized roles, active role/session state, expiry, and the canonical safe
support path. A missing or failed contributor returns 503
`CONTEXT_UNAVAILABLE` as `ApiError` with no partial success response.

## Authoritative absence versus failure

A successful SPEC-008 contribution may return an authoritative null teaching
term or registration term when no applicable term exists. An authoritative
null registration term requires `registrationWindowState = none` and a null
`registrationWindow`. A present `registrationWindow` requires a non-null
registration term and its `state` must equal `registrationWindowState`; a null
window is allowed only for the `none` state. The public term labels follow the
same authoritative-absence rule.

A missing or failed contributor is not an authoritative null. The handler must
not use null to hide a contributor failure, reuse stale browser state, or
return a partially populated DTO. The response is the safe unavailable error
and its correlation ID.

## Public response

`GET /api/public/context` does not require or call the SPEC-007 identity
contributor. It returns exactly the six fields in `PublicContextDto`: server
time, timezone ID, nullable public teaching and registration term labels,
registration-window state, and service state. It contains no display name,
user or student identifier, role, session, matched-window identifier,
opening/closing interval, row version, capacity, internal health, or diagnostic
detail.

## Activation boundary

These are schema and composition rules only. Runtime integration remains
blocked until SPEC-007 and SPEC-008 contributor contracts, fixtures, handlers,
and immutable versions are approved and pinned. A hand-composed DTO or mock
that bypasses the real handler is not integration or release evidence.
