# SPEC-018 NFR-1 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-1

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Logical fixture contract result: PASS. The executable
`SPEC018-LOAD-FIXTURE-1.0.0` generator creates exactly 25,000 minimal synthetic
logical accounts and 5,000 logical sessions. Two independent rebuilds produce
the same account IDs, logical account keys, academic profile values, session
IDs, and account/session relationships.

The harness model intentionally contains no credential, password hash, name,
email, phone, address, national identifier, or date-of-birth field. Generated
credential plaintext and full profiles therefore cannot enter this load-only
artifact. The executable test also invokes the canonical SPEC-007 seed
contributor and ASP.NET Identity V3 hasher: generated input verifies against
the stored salted password hash, while hashes are never byte-compared.

Runtime execution result: PASS. Two already-recorded, versioned downstream
measurements provide the runtime scale proof without re-labeling either run:

- `SPEC-014-load-results.json` records SQL Server 2022 Developer compatibility
  160 with 25,000 synthetic accounts, 5,000 logical authenticated sessions,
  two logical registration replicas, and zero privacy violations.
- `SPEC-008-NFR-2.md` records a migrated shared-SQL fixture containing 25,000
  synthetic student identities, real protected cookies accepted across two
  independently hosted API replicas, and 180,000 successful authenticated
  requests over ten minutes with zero unexpected failures.

Together with the SPEC-018 deterministic generator and canonical hash
verification tests, this proves the NFR-1 account/session capacity and data
handling boundary. It does not claim the separate SPEC-018 simultaneous mixed
75-submission/s plus 300-read/s gate, which remains governed by NFR-2.

## Automated contract evidence

| Check | Result |
|---|---|
| Fixture version is explicit | PASS |
| Logical accounts | PASS: 25,000 |
| Logical sessions | PASS: 5,000 |
| Rebuild equality uses logical values | PASS |
| Credential/full-profile shape absent | PASS |
| Canonical ASP.NET Identity hash verification | PASS |
| SQL profile: 25,000 accounts / 5,000 logical sessions | PASS |
| Real-cookie authenticated execution across two API replicas | PASS: 180,000 requests |

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec018.Nfr1EvidenceTests"
```

The ignored raw SPEC-008 load artifact remains local under
`load-test-results/spec008`; the committed evidence contains aggregate values
only and no credentials, cookies, connection strings, or full profiles.
