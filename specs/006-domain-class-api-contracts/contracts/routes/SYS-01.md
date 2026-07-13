# SPEC-006 SYS-01 Route Contributor Contract

**Contributor contract:** `SPEC-006/SYS-01/1.0`<br>
**Canonical Page Design Record:** `SPEC-003/SYS-01/1.0`<br>
**Canonical Page Design Record path:** `specs/003-ux-storyboard-accessibility/design/pages/SYS-01.md`<br>
**SPEC-003 governance commit:** `8ee8f724af3b60bbbc464532bc68de6f173247e5`<br>
**SPEC-003 design contract pin:** `frontend-design-index/1.1`<br>
**Page Design Record governance pin:** `page-design-record/1.1`<br>
**Route manifest pin:** `2.1.0`<br>
**Page/API manifest pin:** `1.1.0`<br>
**Route:** `SYS-01 /status/{code}`<br>
**Route requirement/criterion:** `SPEC-006 FR-3 / AC-1`<br>
**Route readiness:** `design-only`

## Ownership and evidence boundary

SPEC-003 remains the sole design and implementation owner for SYS-01.
SPEC-006 contributes only the safe error/status transport rules, stable reason
mapping, privacy constraints, and stale/concurrent interpretation recorded
here. SPEC-006 does not own or edit `SystemStatusPage.razor`, either endpoint
handler, the health model, authentication/session behavior, or route
navigation.

| Contribution | Exact state |
|---|---|
| SPEC-003 Page Design Record | `SPEC-003/SYS-01/1.0`, governed by `page-design-record/1.1` and `frontend-design-index/1.1` |
| SPEC-006 contributor document | `SPEC-006/SYS-01/1.0` |
| SPEC-006 runtime error contribution | `not-pinned` |
| SPEC-006 component/Razor consumption | `not-pinned` |
| SPEC-007 session contribution | `not-pinned` |
| SPEC-008 public-context handler | `not-pinned` |
| SPEC-018 health handler and HealthSummary | `not-pinned` |

`not-pinned` is a blocking state, not a wildcard or approval of the latest
implementation. This document does not promote SYS-01 to
`implementation-ready`. No Razor, component, browser, accessibility, visual, or E2E evidence is claimed by this contract. Those claims remain blocked until
Ahmed ELbamby records exact immutable contributor versions, their tests pass,
and SPEC-003 reconciles the Page Design Record.

## Safe data contract

SYS-01 consumes the exact design-time API list from the page/API manifest:

- `GET /api/public/context`, owned at runtime by SPEC-008; and
- `GET /api/health`, owned at runtime by SPEC-018.

For an error, the page may present only the stable `ApiError.code`, the
user-safe `ApiError.message`, and `ApiError.correlationId` as the support
reference. `fieldErrors` are not general diagnostics and are not rendered on
this non-form route. `currentVersion` is inapplicable to this read-only route
and is never disclosed for an unauthorized request.

For health/status display, the design permits only the public status, safe
version, and timestamp already allowed by SPEC-018's HealthSummary contract.
The public-context contribution remains limited to the public server
time/timezone, public term labels, registration-window state, and service
state defined by SPEC-006 FR-9. Neither response authorizes the browser to
derive a more detailed state or to expose internal diagnostics.

## Stable reason mapping and actions

The route parameter and any server-returned stable reason are matched against
this allow-list. Matching is case-insensitive at the presentation boundary;
the displayed copy remains user-safe. An unknown value always uses the safe
fallback and never becomes success.

| Stable reason family | Canonical SYS-01 state | Safe presentation data | Allowed next actions |
|---|---|---|---|
| `healthy` | `success` | Healthy heading; safe health status, version, and timestamp | Open public gateway |
| `degraded`, `unhealthy`, `SERVICE_UNAVAILABLE`, `MAINTENANCE` | `service-error` | Safe unavailable/maintenance explanation and correlation reference; safe health summary only when supplied | Retry; Open safe support path |
| `UNAUTHORIZED`, `FORBIDDEN` | `unauthorized` | Access-denied explanation and correlation reference without protected content | Return to authorized home; Open appropriate sign-in |
| `SESSION_EXPIRED` | `session-expired` | Session-expired explanation without cached protected content | Open appropriate sign-in |
| `NOT_FOUND`, `404` | `service-error` | Not-found explanation and correlation reference without echoing a protected identifier | Open public gateway; Return to authorized home |
| `offline` | `offline` | Offline explanation with no success or queued-write claim | Retry when online |
| Unknown or unexpected reason | `service-error` | Generic safe explanation and correlation reference; no raw exception data | Retry; Open safe support path; Open public gateway |

The canonical action vocabulary is therefore: Retry, Open public gateway,
Open appropriate sign-in, Return to authorized home, and Open safe support
path. The route does not select an authenticated destination from untrusted
browser role data; server-validated session context controls any authorized
home action.

## Authorization and privacy

SYS-01 is publicly reachable so that a visitor can recover safely, but public
reachability does not make protected failure context public. A 401/403 response
must not include `currentVersion`, a protected resource identifier, cached
user/role/student data, or an authorization explanation that confirms whether
a protected resource exists.

No state may render a stack trace, SQL text, connection information, a secret or credential, a password/hash/token, a raw payload, an internal health detail,
topology, capacity, host/database name, or any other protected diagnostic. The
correlation reference is opaque and is the only support linkage exposed by
SPEC-006. Unknown reason codes use the generic safe fallback rather than
echoing the untrusted route value.

## Stale and concurrent behavior

SYS-01 is read-only and has no versioned update, delete, or durable command.
The canonical `stale` page state is `not-applicable`; the route must not invent
a `STALE_VERSION` workflow, expose a rowversion, or offer a mutation retry.

If service, health, session, or connectivity state changes while the page is open, a user-initiated retry refetches and renders the latest complete safe response. The page never merges diagnostics from different responses, never
retains protected content after an unauthorized/session-expired result, and
keeps each correlation reference bound to the response that produced it. It
also never claims that an offline or failed write was queued or accepted.

Concurrent response arrival must not allow an older response to overwrite a
newer completed retry. Until SPEC-003 pins and tests an implementation strategy,
this remains a required design behavior rather than a runtime or component
claim.

## Promotion rule

This contributor document may be version-pinned in the SPEC-003 contributor
baseline only after its focused contract tests pass. SYS-01 itself remains
`design-only` until SPEC-003, SPEC-006, SPEC-007, SPEC-008, and SPEC-018 each
have an exact immutable implementation/contribution pin, all applicable tests
pass, and Ahmed ELbamby approves the reconciled route from the required review
perspectives.
