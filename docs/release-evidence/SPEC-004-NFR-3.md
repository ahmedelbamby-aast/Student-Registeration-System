# SPEC-004 NFR-3 Release Evidence

**Requirement:** SPEC-004/NFR-3 - the architecture must support at least two application replicas.
**Evidence date:** 2026-07-13
**Scope:** Non-production demo architecture evidence; this is not approval of a
production SQL topology, certificate authority, or deployment platform.

## Automated gate

`NFR-3EvidenceTests` validates the measured topology below against the real API
composition and SQL Data Protection repository. It also injects invalid
one-replica, split-state, unprotected-key, and affinity samples to prove that
the validator rejects them.

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --filter "FullyQualifiedName~Specs.Spec004.NFR_3EvidenceTests"
```

## Measured two-replica topology

Both logical API replicas have the same durable-state coordinates. The API is
stateless between requests: academic plan state is in shared SQL, while
authentication protection uses shared Data Protection keys. Requests may move
between the replicas with no sticky sessions.

| Replica | Shared SQL database | Data Protection application name | Key repository | Key encryption | Request affinity |
|---|---|---|---|---|---|
| `api-01` | `StudentRegistration` | `AASTMT.StudentRegistration` | `StudentRegistrationDbContext` | `ExternalCertificate` | `None` |
| `api-02` | `StudentRegistration` | `AASTMT.StudentRegistration` | `StudentRegistrationDbContext` | `ExternalCertificate` | `None` |

Measured replica count: **2 distinct application replicas**. Both rows must
retain one SQL database, one application name, one SQL-backed key repository,
external-certificate key protection, and `None` request affinity.

## Source controls

- `DataProtectionRegistration` assigns one configured application name,
  persists the key ring to SQL, and protects keys with the configured external
  certificate.
- `SqlDataProtectionKeyRepository` persists keys through the single
  `StudentRegistrationDbContext`, making the same key ring readable by each
  replica using the same application identity and database configuration.
- The API composition contains no ASP.NET session registration, in-process
  memory state registration, or load-balancer affinity marker. Authorization
  and plan state therefore do not depend on the instance handling a request.
- The focused quality gate scans those source controls on every run and fails
  if the manifest drops below two replicas, shared coordinates diverge,
  encryption changes, or affinity is introduced.

This evidence proves NFR-3 at the architecture boundary. Environment-specific
replica orchestration and the production SQL/key authorities remain explicit
later release decisions and fail closed until approved.
