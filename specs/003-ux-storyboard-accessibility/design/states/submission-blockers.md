# Submission Blocker Contract

**Version:** `submission-blockers/1.0`  
**Decision owners:** SPEC-009, SPEC-012, and SPEC-014  
**Presentation owner:** SPEC-003

Registration submission is available only from the latest server-validated
plan state. A hard conflict or any other blocking validation keeps the submit
control disabled. The client cannot override, dismiss, or downgrade a blocker.

## Required blocker presentation

- The validation summary lists every blocking reason in text, grouped by the
  affected subject/group when that safe detail is supplied.
- The disabled submit control uses `aria-describedby` to reference concise
  blocker text; the complete validation summary remains adjacent and linked.
- Each reason includes its stable code, safe explanation, and direct change,
  remove, refresh, or support action supplied by the owning contract.
- Icons and semantic colors supplement the text; they are never the sole way to
  identify a blocked plan.
- Focus moves to the validation summary only after an attempted review/submit
  transition. Background changes announce politely and do not steal focus.

## Pending and retry behavior

On activation, the submit control becomes pending and disabled immediately.
Rapid double activation cannot send a second client command. The owning SPEC-014
idempotency key is reused only according to its contract, and the UI presents
one authoritative result.

After any plan edit, stale response, capacity change, term/window change, or
session recovery, server revalidation is required before submission can be
enabled again. Cached client eligibility, an earlier available capacity, or a
locally resolved timetable is never sufficient.

## Result rules

- An accepted result may navigate to the receipt/result route.
- A rejected result retains the disabled state, every current blocker, stable
  reason code, reference path, and recovery actions.
- Offline or uncertain delivery does not display success. The client queries
  the idempotent result endpoint or asks the user to retry safely.
- Missing blocker detail fails to a safe service-error state; it never enables
  the command by omission.
