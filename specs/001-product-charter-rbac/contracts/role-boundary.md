# Role and Authorization Boundary

**Contract:** `role-boundary/1.0`
**Governance owner:** SPEC-001
**Runtime implementation owner:** SPEC-007

## Student entry

Student authentication and institutional activation use the dedicated student
entry. A student never selects, requests, or receives a staff role through that
flow. Student permissions are self-scoped unless an owning feature defines a
narrower public projection.

## Shared staff entry

Admin, Lecturer, and TeachingAssistant authenticate through one shared staff
entry with no public staff registration and no role picker. The authenticated
server-derived effective role set determines the allowed contexts.

## Boundary outcomes

- **Direct-route denial:** navigating to another role's URL or changing client
  state grants nothing; the API authenticates, authorizes, and scopes every
  protected request.
- **Invalid multi-role outcome:** a staff identity with more than one effective
  role is denied safely. Admin, Lecturer, and TeachingAssistant accounts each
  have exactly one server-derived role and there is no context-selection flow.
- **No-role outcome:** an authenticated staff identity with no supported
  effective role is denied with a privacy-safe support path.
- An Admin has no implicit superuser bypass. Every sensitive capability needs
  its named governed permission and server-enforced scope.

This contract defines vocabulary and outcomes only. It creates no claims,
sessions, policies, handlers, routes, persistence, or user interface.
