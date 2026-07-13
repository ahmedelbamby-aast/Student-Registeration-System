# SPEC-004 NFR-2 Stateless Replica and Lifecycle Evidence

**Requirement:** `SPEC-004/NFR-2`
**Measured:** 2026-07-13
**Configuration:** Release evidence contract, .NET 10, SQL Server 2022 Developer demo profile
**Automated test:** `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-2EvidenceTests.cs`
**Result:** PASS (non-production demo)
**Production authority:** FAIL CLOSED

## Method

One repository quality test maps every NFR-2 clause to the smallest existing
executable or governed proof. It checks the cross-replica acceptance and
integration fixtures, the SQL-backed Data Protection implementation, the
disposable SQL Server fixture, the persistent Development volume, the approved
synthetic-only lifecycle contract, and the repository-bounded artifact cleanup.
It also checks the actual Production startup guard rather than treating a demo
configuration as institutional deployment approval.

## Evidence matrix

| NFR-2 control | Enforced evidence | Measured outcome |
|---|---|---|
| Requests may cross **two replicas** | `AC-2Tests.cs` requires cross-instance authentication and server-authoritative plan state. `DataProtectionRegistrationTests.cs` rejects sticky state and `DistributedMemoryCache`; both instances use the same application name and SQL key ring. | Authorization and plan state are designed to remain valid **without sticky sessions**. PASS |
| Shared keys are encrypted and rotated | `DataProtectionRegistration.cs` uses `SetApplicationName`, SQL persistence, an **external certificate**, and ephemeral certificate loading. `SqlDataProtectionKeyRepository.cs`, `StudentRegistrationDbContext.cs`, and `DataProtectionKeyModelConfiguration.cs` bind the shared `DataProtectionKeys` table. `ops/runbooks/data-protection-keys.md` governs rotation and recovery. | One encrypted-at-rest, SQL-backed key ring is shared by the replicas. PASS |
| Key access is least privilege | The governed requirement states that keys are readable by **only the application identity**. The application has one Infrastructure-owned key repository and exposes no key endpoint. Exact Production SQL grants, certificate custody, and secret-provider authority are not inferred from the demo. | Demo architecture boundary PASS; Production grant authority remains FAIL CLOSED |
| Database lifecycle is bounded | `SqlServerContainerFixture.cs` creates and drops an isolated database and disposes its Testcontainer. `compose.development.yml` uses the named `development-sql-data` volume. `research.md` fixes a **per-run Testing database**, persistence until a **guarded Development reset**, and **synthetic-only** records. | Testing is disposable; Development is persistent until an explicit guarded reset; real institutional data is forbidden. PASS |
| Local artifacts expire | `.gitignore` excludes `.local`, **credentials**, **logs**, and **exports**. `Remove-ExpiredLocalArtifacts.ps1` defaults to **seven days**, constrains targets to the repository and approved roots, and deletes individual expired files. `LocalArtifactRetentionTests.cs` proves an eight-day file is removed while a one-day file remains. | Four ignored roots checked; one expired/current behavioral boundary checked. PASS |
| Production authority is explicit | `DataProtectionRegistration.cs` requires both repository and encryption approval flags in Production. `Production_without_repository_and_encryption_approval_fails_closed` expects `PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED`. | Missing **Security/DevOps approval** blocks startup. PASS (fail-closed behavior) |

## Least-privilege and production boundary

The non-production demo proves the application-side ownership boundary and
encrypted key persistence. It does not claim that a Production database role,
secret provider, certificate custodian, backup topology, or rotation authority
has been approved. Before Production can be considered ready, Security/DevOps
must approve an application identity whose data-plane grant is limited to the
shared key table and other explicitly required application tables. Until then,
both approval flags remain false and startup fails closed.

## Focused execution

The focused 2026-07-13 run used the repository-pinned .NET 10 toolchain:

| Suite/filter | Passed | Failed |
|---|---:|---:|
| Quality `SPEC004.NFR_2EvidenceTests` | 1 | 0 |
| Acceptance `SPEC004.AC_2Tests` | 1 | 0 |
| Integration `SPEC004.DataProtectionRegistrationTests` | 3 | 0 |
| Quality `SPEC004.LocalArtifactRetentionTests` | 1 | 0 |
| **Total** | **6** | **0** |

## Measured result

| Measure | Value |
|---|---:|
| Automated T040 quality methods | 1 |
| NFR-2 control groups mapped | 6/6 |
| Cross-replica acceptance/integration fixtures referenced | 2 |
| Shared-key implementation artifacts checked | 4 |
| Testing/Development lifecycle artifacts checked | 3 |
| Git-ignored artifact roots checked | 4/4 |
| Expired/current cleanup boundaries checked | 2/2 |
| Missing-authority Production rejection paths checked | 1/1 |

**Result: PASS for the approved non-production demo profile.** The production
repository, least-privilege SQL role, certificate custody, encryption, backup,
recovery, and rotation authorities remain explicitly unapproved and therefore
fail closed; this evidence does not represent AASTMT Production approval.
