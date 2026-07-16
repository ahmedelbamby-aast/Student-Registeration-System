# SPEC-005 NFR-4 Sensitive Data Evidence

**Requirement:** SPEC-005/NFR-4

**Recorded:** 2026-07-16

**Owner:** Ahmed ELbamby

## Verified boundary

The non-production SQL bootstrap persists ASP.NET Identity password hashes,
never generated plaintext credentials. The SQL-backed
`Spec008SqlServerTestDatabaseBootstrapperTests` fixture reads every persisted
password hash and proves none equals a generated credential secret. Credentials
remain in process memory for the Testing run and are not written to migrations,
the EF model snapshot, checked-in fixtures, logs, traces, or test reports.

The canonical identity quality test constructs
`PasswordHasher<ApplicationUser>` in
`PasswordHasherCompatibilityMode.IdentityV3` and verifies credentials through
`VerifyHashedPassword`; it does not compare deterministic hash bytes.

Repository checks scan the generated migrations and EF model snapshot for the
synthetic credential values used by executable tests and for seeded University
ID-shaped values. No such value is embedded in schema artifacts.

Local reveal-once credentials, logs, and exports are rooted under `.local/`,
`credentials/`, `logs/`, and `exports/`. All are Git-ignored.
`LocalArtifactLifecycle.MaximumAgeDays` is `7`, and its executable boundary
marks artifacts expired at seven days.

## Executable evidence

- `Spec008SqlServerTestDatabaseBootstrapperTests.Testing_database_migrates_then_seeds_is_idempotent_ready_and_disposable`
  proves real SQL rows contain password hashes and exclude generated plaintext
  credentials.
- `StudentRegistration.QualityTests.Specs.Spec018.NFR_1EvidenceTests`
  proves ASP.NET Identity verification without deterministic hash assertions.
- `StudentRegistration.IntegrationTests.Infrastructure.NonProductionSeedLifecycleTests`
  proves Git-ignore coverage and the seven-day cleanup boundary.
- `StudentRegistration.QualityTests.Specs.Spec005.NFR_4EvidenceTests`
  binds this record to those executable artifacts and scans generated schema
  files.

## Focused quality result

- 4 passed
- 0 failed
- Configuration: Release

**Result: PASS.**
