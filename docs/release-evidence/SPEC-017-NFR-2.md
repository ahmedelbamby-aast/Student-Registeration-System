# SPEC-017 NFR-2 Audit First-Page Latency Evidence

**Requirement:** The first audit-log page SHOULD load within one second at
the 95th percentile under the approved demo profile.  
**Source-under-test commit:** `e3c567f43e608597a66b9d8e5f1f0bcfc4b04594`  
**Measured:** 2026-07-17T20:38:49.9198756Z  
**Result: PASS.**

## Environment and command

- Microsoft Windows 10.0.26200, X64, 20 logical processors
- .NET 10.0.9, target framework `net10.0`
- Pinned SQL Server 2022 CU25 Developer container, product 16.0.4255.1,
  database compatibility level 160
- Synthetic demo data only; fixture seed 170017

```text
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter FullyQualifiedName~Spec017.NFR_2EvidenceTests
```

## Approved demo profile

The real `SqlAdminAuditReader` queried 100,000 merged rows: 50,000 append-only
audit events and 50,000 identity security events. The first page contained 20
rows. Five warm-up queries were excluded, followed by 30 measured queries.
Fixture creation and database seeding were outside the timed region.

## Raw measurements and calculation

The machine-readable artifact is
`docs/release-evidence/SPEC-017-NFR-2-raw.json`. It records the exact container
image, environment, fixture volume, all 30 measured queries, calculation, and
result.

The p95 uses the nearest-rank method:

- rank = `ceil(30 * 0.95) = 29`
- sorted sample at rank 29 = `47.4728 ms`
- threshold = `1,000 ms`
- margin = `1,000 - 47.4728 = 952.5272 ms`

Therefore `47.4728 ms <= 1,000 ms` and NFR-2 passes for this approved,
repeatable demo profile. This is engineering evidence for the documented demo
fixture, not a production SLA claim.
