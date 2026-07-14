# SYS-01 Identity Status Contribution

**Route:** `/status/{code}`  
**Canonical owner:** SPEC-003  
**Identity contributor:** SPEC-007

SPEC-003 owns the canonical page and its responsive, accessibility, and visual
behavior. SPEC-007 contributes only the following safe identity reason mapping
and navigation actions; it does not create or edit `SystemStatusPage.razor`.

## Identity reason mapping

| Reason | Safe presentation | Primary action |
|---|---|---|
| `SERVICE_UNAVAILABLE` | Identity services cannot be reached; show a correlation ID and retry/support guidance. | Retry the original safe destination. |
| `MAINTENANCE` | Sign-in or account service is temporarily paused; show approved timing only when the server supplies it. | Return later or open support. |
| `SESSION_EXPIRED` | The earlier session is no longer valid and protected content is removed. | Open `/student/login` or `/staff/login` as appropriate. |
| `RATE_LIMITED` | Too many attempts were received; do not reveal the subject, counters, or remaining attempts. | Wait for server guidance or open `/account/recovery`. |
| `UNAUTHORIZED` / `FORBIDDEN` | Access is unavailable without naming a protected resource. | Return to an authorized home or sign in. |
| unknown identity code | Use the generic unavailable message and correlation ID. | Retry or open the safe support path. |

## Privacy and ownership

The status contribution contains no credential, challenge, internal security
value, account-existence fact, raw request, protected identifier, role list, or
diagnostic topology. It preserves the server reason code and correlation ID,
never claims that a write succeeded, and never derives authorization from the
route or browser state.
