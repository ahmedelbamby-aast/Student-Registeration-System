# SPEC-009 NFR-3 Transactional Publication Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T15:00:00Z

## Requirement and executable model

NFR-3 requires publication to be transactional. The focused gate executes the
delivered preview and confirmation service against an atomic store fixture
that holds one normalized scope lock while checking expected version,
idempotency, publication, and audit effects.

## Verified outcomes

| Scenario | Executable result |
|---|---|
| Two concurrent confirmations | Exactly one winner publishes and writes one audit fact; the loser receives stale preview/current-version evidence. |
| Same-key same-payload replay | The stored publication result is returned with replay state and no duplicate publication or audit. |
| Audit failure before commit | Storage failure is returned with 0 publication rows and 0 audit rows. |
| Scope serialization | Version check and commit occur while the same scope lock is held. |

The audit failure path is deliberately exercised before commit.

The fault fixture deliberately refuses the transaction before either durable
counter changes. It therefore proves all-or-nothing behavior rather than
cleaning up a partially reported success.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec009.NFR_3EvidenceTests" -p:TreatWarningsAsErrors=true
```

- Transaction/fault test: 1 passed
- 1 passed
- 0 failed

Quality test normalized-LF SHA-256: `280DFE09F74178E3C5AF1CB90C48FD455C332243E708B7DDDF89A2EF9D7562B7`

The combined `S2CatalogueScheduling` SQL migration remains SPEC-010-owned.
This gate verifies the SPEC-009 transactional command contract and fault
semantics without creating a competing migration.

**Result: PASS.**
