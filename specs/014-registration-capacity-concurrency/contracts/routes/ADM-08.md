# ADM-08 Registration Monitoring Contribution

**Route:** `/admin/registrations`  
**Canonical page owner:** SPEC-017  
**Contributor:** SPEC-014  
**Contract version:** `spec014-adm08/1.0`

SPEC-014 contributes privacy-safe atomic-registration and reconciliation
signals to the read-only monitoring page. It does not own or edit
`RegistrationAdministrationPage.razor`, create an Admin registration command,
or expose a capacity/conflict override.

## Data and actions

The SPEC-017-owned `GET /api/admin/operations/metrics` projection may consume:

- accepted/rejected/processing counts and stable result-code counts;
- group fill/capacity values, collision counts, lock-wait summaries, and the
  last server evaluation time;
- zero-overbooking, zero-partial-commit, and duplicate-active-enrollment
  invariant status; and
- a privacy-safe reconciliation alert with affected group support reference,
  `registrationPaused` state, evidence hash, and verified/resumed timestamp.

ADM-08 may filter registrations, clear filters, open an authorized submission
detail, open the scoped Student profile, refresh monitoring data, or open the
safe support reference. It has no repair, correction, enrollment, resume,
capacity, conflict, or reconciliation command. `Registration.Reconcile` is
restricted to the operations service identity and is never granted to Admin.

## Stable states and concurrent changes

The route preserves `GROUP_FULL`, `PLAN_CHANGED`, `POLICY_CHANGED`,
`WINDOW_CLOSED`, `SCHEDULE_CONFLICT`, and reconciliation mismatch codes as
server-authored aggregate/detail reasons. A collision displays winners and
rejections as complete atomic outcomes and never suggests a partial commit.

`availabilityState: stale` and its server timestamp/reason render as stale,
not live. A reconciliation mismatch renders degraded/paused with a safe
support reference; a later verified snapshot must be refreshed before the
page claims the group resumed. Unknown reasons remain privacy-safe and retain
the correlation reference.

## Authorization and ownership boundary

The route requires authenticated `Admin` with the exact SPEC-017 owner-policy
permissions: `RegistrationRecords.Read` for scoped registration rows and
`Audit.Read` for operational evidence. Role alone never implies permission.
Student queries require named StudentId plus TermId and return only the bounded
fields authorized by SPEC-015/SPEC-017; denied and not-found outcomes reveal no
Student, submission, receipt, group, or version oracle.

SPEC-017 remains the sole canonical page, metrics-query, filtering, and detail
implementation owner. SPEC-014 contributes only the invariant, race-result,
and reconciliation presentation contract.
