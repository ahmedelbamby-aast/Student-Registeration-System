# Research: Identity and Account Lifecycle

## Decisions

### Modular boundary
**Decision**: Own this capability in the IdentityAccess module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Effective-role permission claims
**Decision**: Identity owns one explicit allow-list from the effective active
role to governed permission tokens. Student receives `Context.Read` and
`AcademicProfile.ReadOwn`; Admin receives `IdentityAccess.Manage`,
`Context.Read`, `AcademicTerms.Manage`, and `AcademicProfiles.Manage`;
Lecturer and TeachingAssistant receive only `Context.Read`. Context switching
rebuilds the cookie claims from the selected effective role.
**Rationale**: The permission claim remains independently enforceable, while
the demo avoids a second permission database or client-asserted grants.
**Alternatives rejected**: Treating Admin as an implicit superuser, unioning
claims across available roles, and accepting permissions from request data.

### Feature contract
```typescript
interface StudentLoginRequest { universityId: string; password: string; }
interface StaffLoginRequest { userName: string; password: string; }
interface ActivateStudentRequest {
  universityId: string;
  initialPassword: string;
  newPassword: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant" | null;
  sessionState: "active" | "expiring" | "role-selection-required";
  expiresAtUtc: string;
}
```

Endpoints are split by lifecycle action so recovery request and completion,
password change, revoke-all, and role-context selection have
separate authorization, antiforgery, rate-limit, and replay semantics.

Activation sends the issued initial credential and the replacement password;
the confirmation field exists only in the client. The server verifies and
replaces the password hash in the same transition that consumes activation.

### Recovery proof delivery
**Decision**: Keep proof generation/consumption in IdentityAccess and deliver
the opaque proof through a narrow `IAccountRecoveryProofDelivery` application
port. Tests inject an in-memory adapter. Development may register a local,
Git-ignored adapter whose artifacts follow the seven-day cleanup contract.
Production startup fails closed until an approved institutional adapter is
configured. The request endpoint always returns the same 202 body and never
returns or logs the proof.
**Rationale**: Recovery remains demonstrable without inventing an AASTMT
email/SMS provider or coupling the identity service to a vendor.
**Alternatives rejected**: Returning the proof from the endpoint, logging it,
silently dropping a live challenge, or claiming an unapproved production
provider.

### Replica-safe session state
**Decision**: Use shared SQL-backed Identity state and security-stamp
validation, plus the SPEC-004 SQL-backed Data Protection foundation under
SPEC-018 security/operations governance. Rotate the
stamp for recovery/password/revoke-all and store challenge/rate-limit state in
the shared database.
**Rationale**: Either replica can validate a session or reject an invalidated
one without sticky routing.
**Alternatives rejected**: In-memory counters, per-node key rings, and sticky
sessions as correctness mechanisms.

### Demo credential baseline
**Decision**: Pin the .NET 10 demo to IdentityV3/PBKDF2 with at least 100,000
iterations, 15-128 character passwords, no character-class composition rules,
a versioned common/context-specific password blocklist, and password-manager,
paste, space, and Unicode support. Password login locks after five failed
attempts for five minutes. Activation/recovery proofs expire after 15 minutes
and allow at most five failed verifications. No periodic password rotation is
required without compromise. Official AASTMT credential policy is unverified;
Production fails closed until an approved version is reconciled.
**Rationale**: ASP.NET Core Identity 10 uses IdentityV3 and a 100,000-iteration
default, while NIST SP 800-63B-4 requires at least 15 characters for a
single-factor password, support for at least 64 characters, no composition
rule, blocked common/compromised values, rate limiting, and salted hashing.
**Sources**:
- https://learn.microsoft.com/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0
- https://github.com/dotnet/aspnetcore/blob/v10.0.9/src/Identity/Extensions.Core/src/PasswordHasherOptions.cs
- https://pages.nist.gov/800-63-4/sp800-63b.html#passwordver
**Alternatives rejected**: ASP.NET Core's permissive six-character password
default, arbitrary composition/rotation rules, deterministic hashes, unlimited
attempts, and claiming an unpublished AASTMT credential policy.

### Login performance profile
**Decision**: Measure 10 minutes at 25 password-login attempts/second against
25,000 synthetic accounts through at least two stateless replicas. The mix is
80% valid, 15% invalid credential, and 5% already locked. The p95 target is 500
ms and unexpected errors stay below 1%; expected generic denials are counted as
business outcomes, not service errors.
**Rationale**: SPEC-018's registration/read profile does not define login
traffic. This bounded profile measures password hashing and shared lockout
state without pretending registration throughput is authentication evidence.
**Alternatives rejected**: Reusing unrelated registration-load numbers, a
single-replica microbenchmark, and measuring only successful credentials.

### Institutional providers
**Decision**: For the non-production demo, generate pre-provisioned local
student/staff identities and passwords, persist only ASP.NET Core Identity
hashes, and use password-only authentication with no MFA/2FA or
pre-authentication/self-asserted role selection. After authentication, a
multi-role user may select only a role returned by the server. Any later real
institutional provider is a separately approved production integration.
**Rationale**: The demo remains self-contained and testable without claiming
official AASTMT identity integration or weakening credential storage.
**Alternatives rejected**: Public self-registration, plaintext database
credentials, second-factor scope not requested by Ahmed, and fake production
provider claims.

### Governed Admin user lifecycle
**Decision**: Identity owns pre-provisioned user import/list/status/role
commands, with aggregate `expectedRowVersion`, reason, audit facts, idempotent import
publication, and an Identity-owned singleton AdminSecurityGuard that
database-serializes every disabling or role-removal path that could reduce the
enabled-Admin set and therefore protects the final-enabled-Admin invariant.
**Rationale**: ApplicationUser and RoleAssignment have one writer and two
replicas cannot remove the last Admin through write skew.
**Alternatives rejected**: Duplicate SPEC-017 writers and check-then-update
role counts.

### Security-event direction
**Decision**: SPEC-007 owns append-only `SecurityEvent`; downstream SPEC-017
consumes its read contract.
**Rationale**: Dependency direction remains acyclic while identity/abuse facts
retain a canonical owner.
**Alternatives rejected**: SPEC-017 becoming a second identity writer or
Identity depending on a downstream audit implementation. The shared write
port is owned upstream by SPEC-004.



## Open Research

The official AASTMT credential policy and production identity/recovery
provider remain unverified external decisions. They are production/release
prerequisites with fail-closed configuration, not guessed demo defaults.
