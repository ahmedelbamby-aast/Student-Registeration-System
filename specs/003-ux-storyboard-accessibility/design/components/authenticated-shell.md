# Authenticated Shell Context Contract

**Version:** `authenticated-shell/2.0`<br>
**Composed API owner:** SPEC-006 with SPEC-007/SPEC-008 contributions  
**Presentation owner:** SPEC-003

AppShell receives one `FrontendAppContextView` composed from the authenticated
context endpoint. Every displayed value is server-authoritative; the shell does
not calculate current time/term, infer roles, or cache authorization truth.

| Context value | Required presentation |
|---|---|
| Server date/time | Visible localized date and time from `serverDateTime`; never browser-now as academic truth |
| `timezone` | Visible beside Server date/time using the server timezone identifier |
| Teaching term | Nullable current teaching-term label/identifier; absence is rendered explicitly |
| Registration term | Nullable registration-term label/identifier, which may differ from teaching term; absence is rendered explicitly |
| registration window | Explicit server-derived `none`, `upcoming`, `open`, or `closed` state plus nullable canonical nested summary; times render only when the summary exists and the client never calculates openness |
| Display name | Authenticated user heading/menu context, safely encoded |
| Authorized roles | Navigation options already authorized by the server |
| Active role | Current server-issued context; nullable only during `role-selection-required` |
| Session state | Active, expiring, or session-expired presentation |
| session expiry | Absolute server expiry with accessible warning; browser timer is display-only |
| Service state | Available, degraded, maintenance, or safe unknown state |
| `supportReferencePath` | Application-relative support/status link, never arbitrary external markup |

## Layout and semantics

The shell provides a skip link, banner, labelled role navigation, main region,
and content-info/support region. On narrow widths navigation collapses without
changing its links or accessible name. Context details wrap or move into a
labelled disclosure; they are never removed because of viewport size.

At 1024 CSS px and above, navigation is a persistent 272px sidebar. Below
1024px, a labelled Menu button opens a navigation drawer. The button exposes
`aria-expanded`; opening moves focus into the drawer, Escape or outside click
dismisses it, and focus returns to the Menu button. Route links and active-page
semantics are supplied only by `WorkspaceNavigationCatalog`.

`AuthenticatedPage` is the presentation-only composition wrapper around
AppShell, shared navigation, PageHeader, route status/actions, and page body.
It receives `FrontendAppContextView`, `UiStatus`, loading state,
`WorkspaceKind`, current route ID, and `UiDensity`; it performs no fetching,
authorization, academic calculation, or role inference.

Student routes use comfortable density. Admin, Lecturer, and Teaching
Assistant routes use compact density. Density changes spacing only; component
shape, semantics, focus, status, and navigation behavior remain identical.
The shell is light-only and English-first/localization-ready.

Status updates are text plus semantic icon/color. Background time/session or
service updates use a polite live region and do not move focus. Expiry that
invalidates the current action moves to the session-expired route/status only
once and preserves only safe identifiers permitted by the owner contract.

## Failure states

- Loading shows stable shell landmarks and a named progress status.
- Unauthorized clears protected navigation/content and follows the safe status
  action supplied by SPEC-007.
- A session-expired context offers reauthentication, then refetches and
  revalidates rather than restoring cached editable state.
- Stale context marks data as stale, disables authority-sensitive commands, and
  refreshes the composed endpoint.
- Offline keeps last visible data explicitly labelled stale/offline and never
  claims a role, window, capacity, or submission change succeeded.
- Missing/invalid context fails closed to a privacy-safe status with the
  `supportReferencePath`; no stack trace, token, SQL, or protected identifier is
  rendered.
