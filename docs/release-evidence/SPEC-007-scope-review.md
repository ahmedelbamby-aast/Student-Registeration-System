# SPEC-007 Identity Scope Review

**Review date:** 2026-07-14  
**Decision authority:** Ahmed ELbamby  
**Scope:** non-production Student Registration System demo

## Included demo boundary

SPEC-007 provides pre-provisioned Student activation and University-ID login,
one password-only login for Admin/Lecturer/TeachingAssistant, server-derived
role context, secure cookie sessions, antiforgery, recovery/password/session
lifecycle, governed Admin user lifecycle, SQL Server EF Core mappings, and
Development/Testing synthetic identity bootstrap. It remains one module in the
approved modular monolith and uses the one shared SQL persistence boundary.

Development may write a reveal-once credential/recovery handoff only beneath a
Git-ignored local directory and purges it within seven days. Testing keeps
recovery proofs in memory and uses disposable databases. SQL stores hashes,
state, provenance, row versions, and safe events only—never plaintext PINs,
passwords, or recovery proofs.

## Verified exclusions

| ID | Exclusion | Verification |
|---|---|---|
| OS-1 | Social login and public staff registration | No social provider, public staff-create route, or staff self-registration endpoint/page exists. Staff identities are pre-provisioned or governed by Admin import. |
| OS-2 | Student-created institutional identity | Activation accepts only an existing normalized University ID and conditionally consumes its pre-provisioned activation row; it never creates `ApplicationUser`. |
| OS-3 | Authorization based only on Blazor visibility | Every protected API route uses server authorization and resource/role policies. Client routing and hidden controls are usability only. |
| OS-4 | Final AASTMT identity-provider integration | No institutional provider is claimed. Production startup rejects the demo credential policy or absent approved recovery provider. |

No 2FA/MFA flow is included or required for this approved demo. Initial Student
PIN/password material is a first-use credential, not a second factor.

## Environment decision

- **Development:** synthetic accounts and reveal-once local credential/recovery
  artifacts are allowed under the bounded ignored directory.
- **Testing:** synthetic accounts and an injected in-memory proof adapter are
  allowed; databases are isolated and disposed by the test lifecycle.
- **Production:** demo bootstrap and both local adapters are unavailable. The
  system must fail closed until an approved, versioned institutional credential
  policy, recovery provider, secret source, and Production operating authority
  are configured.

This review approves only the non-production demonstration of design and
engineering capability. It does not claim official AASTMT policy approval,
authorize real student/staff data, deploy Production infrastructure, or release
the complete system.
