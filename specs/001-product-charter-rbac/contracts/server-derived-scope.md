# Server-Derived Role and Data Scope

**Contract:** `server-derived-scope/1.0`
**Runtime owner:** SPEC-007 and each protected resource owner

## Authorization decision

For every protected request the API must derive and validate, in order:

1. the authenticated identity from server-validated credentials/session;
2. the effective server role set from authenticated server claims and current
   role assignments;
3. the active role, which must be a member of that effective server role set;
4. the governed permission required by the owning endpoint;
5. resource ownership or the permission's declared institutional scope; and
6. current state, version, and feature invariants required by the owner spec.

Any missing, stale, unsupported, or contradictory contributor must fail closed.

## Untrusted client input

A client-supplied role, route, hidden-control value, student identifier, group
identifier, cached permission, or active-role claim is untrusted input. It may
identify the requested resource but can never establish authorization. A role
selection is accepted only when it names an already effective supported staff
role; all other selections deny without changing the session.

## Data minimization

Queries must apply server-side resource scope before projection. Student data is
self-scoped; Lecturer and TeachingAssistant data is limited to own assignments
and effective assigned groups; Admin data is limited to the explicit governed
permission. Logs and errors must not disclose denied resource existence or
sensitive student data.
