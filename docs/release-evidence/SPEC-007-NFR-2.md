# SPEC-007 NFR-2 Enumeration-Resistance Evidence

**Scope:** non-production identity demo  
**Evidence date:** 2026-07-14  
**Result: PASS.**

Public contract: same public status and same public error code for every login
denial class.

## Verified public outcomes

The authentication services always run ASP.NET Core Identity password
verification, using a process-local dummy user and dummy hash when the account
does not exist. The HTTP boundary maps an unknown account, wrong credential,
disabled account, locked account, inactive student, missing staff record, and
missing supported staff role to the same public status and same public error
code: `401 AUTHENTICATION_FAILED`. Internal failure reasons are not serialized.

Recovery request returns the same generic 202 response for known, unknown,
disabled, and delivery-rejected subjects. Its response contains no recovery
proof, delivery reference, existence flag, or account state. A proof crosses
only the narrow delivery port and only its SHA-256 digest is stored with the
challenge.

## Executable evidence

- `StudentAuthenticationService` and `StaffAuthenticationService` exercise a
  dummy Identity hash for unknown identifiers and return one generic outcome.
- `Spec007Endpoints` publishes one generic authentication error and one generic
  recovery-accepted response.
- `SC-3OutcomeTests`, `AC-5Tests`, `StudentLoginTests`,
  `StaffPasswordLoginTests`, `SessionLifecycleTests`, and
  `IdentitySecurityBoundaryTests` cover the same-boundary behavior and proof
  exclusion.
- `NFR-2EvidenceTests` rejects missing outcome classes, different public
  statuses/codes, or a response-carried recovery proof.

Timing equality is not claimed: network, runtime, and datastore variance may
exist. The implemented constant-work dummy verification and identical public
contract remove the application-level account-existence oracle required by
this demo gate.
