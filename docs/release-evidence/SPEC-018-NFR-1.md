# SPEC-018 NFR-1 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-1

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

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
artifact. When SPEC-007 supplies its canonical bootstrap, salted password
hashes are never byte-compared; generated input must be verified through the
approved ASP.NET Identity hasher.

Runtime execution result: PENDING. This contract does not prove that SQL has
been migrated/seeded, that 25,000 canonical Identity accounts exist, or that
5,000 authenticated sessions have run.

## Automated contract evidence

| Check | Result |
|---|---|
| Fixture version is explicit | PASS |
| Logical accounts | PASS: 25,000 |
| Logical sessions | PASS: 5,000 |
| Rebuild equality uses logical values | PASS |
| Credential/full-profile shape absent | PASS |
| Canonical ASP.NET Identity hash verification | PENDING |
| Migrated SQL fixture and authenticated-session execution | PENDING |

## Activation condition

Activation condition: SPEC-007 must deliver ApplicationUser persistence, the
canonical Development/Testing synthetic bootstrap, generated-credential
hashing/verification, and executable sessions. The same logical fixture version
must then be contributed to the migrated SQL profile and exercised at 25,000
accounts/5,000 sessions before NFR-1 can pass for release.
