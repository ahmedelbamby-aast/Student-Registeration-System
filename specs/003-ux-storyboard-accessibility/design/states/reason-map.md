# UI State and Authoritative Reason Map

**Version:** `ui-state-reason-map/1.0`  
**Runtime owner:** SPEC-003 presentation only

The mapper converts an already-authoritative server or browser outcome into a
designed route state. It never calculates eligibility, authorization, current
time/term, conflict, capacity, or submission success. The stable reason code
and safe reference ID are preserved for display/support.

## State contract

| State | Heading focus/live region | Primary next action |
|---|---|---|
| loading | Keep focus; polite status only when loading is not immediate | Wait or cancel only when the owner contract supports cancellation |
| empty | Route heading; `none` unless caused by a user action | Clear filters, create, or return according to route ownership |
| success | Result heading; polite command result | Continue to the server-provided destination |
| validation-error | Focus validation summary; assertive only for submitted form failure | Review linked fields/blockers and retry after changes |
| service-error | Keep focus for background failure or focus error heading after navigation failure; polite | Retry and safe support/reference path |
| unauthorized | Safe status heading; no protected identifier; assertive on navigation | Return to an authorized home or sign in |
| session-expired | Session heading; assertive once | Reauthenticate, refetch, and revalidate server state |
| stale | Concurrent-change heading; polite without stealing focus | Refresh authoritative data, review differences, then retry |
| offline | Keep focus; polite connection status | Retry after connectivity returns; do not claim queued success |

## Stable reason families

The code is rendered exactly; the localized heading/message comes from an
external text provider keyed by the safe family. Codes not listed here follow
the unknown-code rule below.

| Stable codes | Route state | Authority-safe behavior |
|---|---|---|
| `UNAUTHORIZED`, `FORBIDDEN` | unauthorized | Hide no data optimistically; use only server-authorized context/actions |
| `SESSION_EXPIRED` | session-expired | Retain only a safe plan/reference ID, then refetch and revalidate |
| `STALE_VERSION`, `STALE_PREVIEW`, `PLAN_CHANGED`, `POLICY_CHANGED`, `WINDOW_CHANGED`, `WINDOW_CLOSED`, `REGISTRATION_WINDOW_CLOSED`, `GROUP_FULL`, `RESOURCE_CONFLICT`, `AVAILABILITY_DEADLINE_PASSED` | stale | Never reuse cached eligibility/capacity/window success; show the code and refresh action |
| `ACADEMIC_STANDING_UNAVAILABLE`, `REGISTRATION_HOLD`, `PREREQUISITE_NOT_COMPLETED`, `LOAD_ABOVE_NORMAL_MAXIMUM`, `PROBATION_LOAD_EXCEEDED`, `REPEAT_POLICY_UNAVAILABLE`, `MEETING_CONFLICT`, `SCHEDULE_CONFLICT` | validation-error | Present every server-provided safe blocker; no client override or success conversion |
| `SERVICE_UNAVAILABLE`, `MAINTENANCE` | service-error | Safe generic retry/support action, reference ID, no stack/SQL/payload text |
| explicit accepted result with no blocking reason | success | Success appears only when `serverAccepted` is true |

## Unknown reason and precedence

An unknown nonblank code maps to `service-error`, preserves the code and safe
reference ID, and uses `Ui.State.UnknownReason.Heading` and
`Ui.State.UnknownReason.Message`. The mapper does not display an arbitrary
server payload, log secrets, or guess success.

Offline, session-expired, unauthorized, stale/concurrent, service failure, and
validation rejection take precedence over a requested success view. A
`serverAccepted: false` input can never produce `success`. Browser time, cached
capacity, cached policy, or local role state never changes this rule.

## Localization boundary

Mapper source contains stable resource keys, not user-facing prose. An
`IUiTextProvider` resolves them from the approved language resources. Missing
translations fail to a safe externalized fallback rather than exposing code,
stack trace, SQL text, credentials, or unauthorized identifiers as message
content.
