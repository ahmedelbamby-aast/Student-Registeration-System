# Policy Source Resolution Contract

**Contract:** `source-resolution/1.0`
**Review perspective:** Registrar/Policy SME

## Resolution protocol

A source record states URL/reference, authority, access date, affected fields,
classification, conflict state, and approval. Public-source facts do not become
executable merely because a page is reachable. A conflict must be resolved for
the exact bounded profile by an approved version; otherwise it remains In Review
and must fail closed.

## DEMO-POC-2026.1 resolutions

| Conflict/gap | Demo resolution | Authority |
|---|---|---|
| General 9-credit minimum versus College usual 12-credit guidance | Enforce submitted minimum 9; suppress the 12-credit guidance | General source fact selected by Ahmed for demo |
| Normal target/maximum | Display target 18 and enforce maximum 18 | General source plus Ahmed demo approval |
| GPA below 2.0 | Enforce maximum 12; advisor workflow remains absent | General source plus fail-closed demo boundary |
| Withdrawal source disagreement | Withdrawal is out of scope; publish no deadline | UnresolvedInstitutional |
| Online repeat behavior/manual conditions | Return `REPEAT_POLICY_UNAVAILABLE` for any repeat interpretation | In Review; no allowance invented |
| College capacity policy absent | First successful commit; no waitlist/reservation/override | AhmedApprovedDemo |
| Automatic meeting conflict authority absent | Exact overlap blocks; no override; travel buffer zero | AhmedApprovedDemo |
| Public curriculum credits absent | Credits=3 is a field-level SyntheticDemo value | AhmedApprovedDemo |
| Missing/misordered curriculum references | Exclude invalid rows; never silently repair official-source fields | Import safety rule |

Every other out-of-profile `POLICY-Q` remains In Review and fail closed.

## Source outage

When a previously recorded source becomes source unavailable, mark its current
review status `review required`, retain the stored URL, access date, extracted
facts, approval, and content hash/reference, and alert Admin. Do not silently
replace its facts, unpublish a historical snapshot, or change a decision.

Published rulebooks and SPEC-015 decision snapshots are immutable. A source
outage can block a future publication/reapproval when current verification is
required, but it cannot rewrite history.

## Prohibited resolution shortcuts

- Client or Admin input cannot select which source wins.
- Newer access date does not automatically mean higher authority.
- A College page cannot silently override general rules or vice versa.
- `AhmedApprovedDemo` cannot be described as `OfficialAASTMT`.
- Unresolved values cannot fall back to a hard-coded constant.
