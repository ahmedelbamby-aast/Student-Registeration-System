# SPEC-007 NFR-3 Credential and Abuse-Control Evidence

**Scope:** approved non-production demo baseline  
**Policy version:** `DEMO-POC-2026.1`  
**Evidence date:** 2026-07-14  
**Result: PASS.**

## Pinned baseline

| Control | Verified setting |
|---|---|
| ASP.NET Core Identity mode | `IdentityV3` |
| PBKDF2 iteration count | 100,000 |
| Accepted password length | 15-128 characters |
| Composition | no character-class composition requirement |
| Blocklist | versioned common/context-specific blocklist, `DEMO-POC-2026.1` |
| Password-manager compatibility | paste, spaces, and Unicode remain allowed |
| Password-login lockout | five failures, five minutes |
| Activation/recovery proof attempts | at most five failures |
| Recovery proof lifetime | at most 15 minutes, hashed at rest and single-use |
| Browser token storage | secure same-origin cookie; no long-lived local-storage token |

`IdentitySecurityOptions.ValidateForEnvironment` prevents configuration from
weakening these bounds. Cookie and antiforgery composition use Secure,
SameSite cookies; the authentication cookie is HttpOnly. The framework
antiforgery cookie stays HttpOnly and only the request token is exposed through
the bounded `XSRF-TOKEN` cookie for echo in `X-XSRF-TOKEN`.

## Production boundary

The official AASTMT credential policy and institutional recovery provider are
not source-approved. Production fails closed while the demo credential policy
is active, while an approved version is absent, or while an approved recovery
provider is unavailable. Development and Testing adapters are selected only by
their explicit environment; neither is registered for Production.

## Executable evidence

- `IdentitySecurityOptions`, `IdentityPasswordValidator`, and
  `IdentitySecurityRegistration` implement the pinned values and startup
  guards.
- `IdentityRateLimitTests` and `IdentitySecurityBoundaryTests` cover the
  blocklist, length, no-composition, lockout, proof, cookie, antiforgery, and
  Production failure paths.
- `NFR-3EvidenceTests` rejects drift from the approved policy constants or
  ASP.NET Core Identity configuration.
