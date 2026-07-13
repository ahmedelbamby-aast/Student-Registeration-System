# Identity Entry Boundary

**Version:** `identity-boundary/1.0`  
**Runtime owner:** SPEC-007  
**Design owner:** SPEC-003

Student and staff identity use visibly distinct entry pages. All forms send
secrets only to SPEC-007 endpoints and display privacy-safe outcomes. The demo
uses no 2FA flow.

| Route | Actor and purpose | Required fields/actions |
|---|---|---|
| `/student/login` | Existing student sign-in | University ID, password/PIN, show-password control, sign in, activation and recovery links |
| `/student/activate` | Pre-provisioned student first activation | University ID, issued one-time PIN, new password/confirmation, activate |
| `/account/recovery` | Student or staff recovery | Safe identifier/contact step, recovery proof/code, new password; responses do not disclose account existence |
| `/staff/login` | Shared Admin, Lecturer, or Teaching Assistant sign-in | Staff identity, password, sign in, recovery link; no client role picker |

Labels remain visible during autofill, password values are never logged or
echoed, and validation explains format without confirming whether an account
exists. Pending submission disables the command and presents one result.

## Staff role boundary

The staff form asks for identity credentials only. The server returns
server-authorized roles. A single role becomes the active role directly; when
SPEC-007 explicitly returns `role-selection-required`, the authenticated
context-selection design may show only those authorized roles. A query string,
hidden field, browser storage value, or staff login role dropdown can never
grant or change authorization.

## Navigation and focus

The public gateway links separately to student and staff sign-in. Each page has
one H1, a labelled form, visible error summary, and links back to the gateway.
Failed submission focuses the error summary; successful authentication follows
the server-authorized destination. Session errors retain no password or PIN.

Demo development/test University IDs and PINs are synthetic. Seed output may
show a one-time issued value to the approved developer workflow, but databases
and logs store only password hashes; the UI never retrieves a stored plaintext
credential.
