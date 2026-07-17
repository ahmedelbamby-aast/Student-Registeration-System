# STU-06 Atomic Registration Result Contribution

**Route:** `/student/registration/result/{id}`  
**Canonical page owner:** SPEC-015  
**Contributor:** SPEC-014  
**Contract version:** `spec014-stu06/1.0`

SPEC-014 contributes the atomic command outcome and private lost-response
recovery contract. It does not own or edit `RegistrationResultPage.razor`, the
student history projection, or the canonical receipt query.

## Data and actions

- `GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}`
  recovers only the authenticated Student's term-scoped result. A 200 result
  carries the immutable submission ID, `accepted` or `rejected` status,
  stable result code, received/completed server times, policy set/version,
  plan version, and the accepted reference/receipt snapshot.
- A bounded 202 `processing` response is retry guidance only. STU-06 offers
  **Retry result lookup** and never issues another registration POST.
- 404 `REQUEST_NOT_FOUND` covers a nonexistent or rolled-back key, another
  Student's key, and a key outside the authorized term without disclosing an
  owner, term, submission ID, or current version.
- A final 200 then permits SPEC-015 to load the owned receipt through
  `GET /api/student/registrations/{submissionId}`. The page may navigate to
  STU-07 history, STU-04 edit plan after rejection, or STU-01 dashboard.

## Stable outcomes and presentation

Accepted `REGISTERED` displays one complete receipt/reference and the explicit
no-partial guarantee. Rejected `GROUP_FULL`, `PLAN_CHANGED`, `POLICY_CHANGED`,
`WINDOW_CLOSED`, `SCHEDULE_CONFLICT`, `PREREQUISITE_NOT_MET`,
`CREDIT_LIMIT_EXCEEDED`, `HOLD_BLOCK`, and `IDEMPOTENCY_KEY_REUSED` preserve the
server code and privacy-safe message exactly; they never display a partial
schedule or offer an override. Unknown codes use a safe fallback while keeping
the correlation reference.

Concurrent or uncertain delivery is a `stale` presentation until the private
GET returns one authoritative result. A replay displays the same result,
reference, and immutable receipt snapshot; it never implies a second seat or
submission.

## Authorization and ownership boundary

The recovery GET requires authenticated `Student` plus exact
`Registration.SubmitOwn`; the receipt GET separately follows SPEC-015's exact
`RegistrationRecords.ReadOwn` policy. Authorization runs before lookup or
identifier/version disclosure. Missing session, wrong role, missing permission,
another owner, or another term exposes no receipt, group, submission, or
existence oracle.

SPEC-015 remains the sole canonical page and receipt/history implementation
owner. SPEC-014 contributes only these result/recovery semantics.
