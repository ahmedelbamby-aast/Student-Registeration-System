# Controlled Migrations Contract

**Contract version:** `controlled-migrations/1.0`<br>
**Owner:** `StudentRegistration.Infrastructure.SqlServer`<br>
**Status:** Design-time procedure only; production authority remains fail closed

This document defines the evidence required for a controlled EF Core migration.
It does not create a migration, script, bundle, deployment command, or approval.
No production bundle or migration execution is approved by this document.

## Safety boundary

- Production deployment requires a reviewed script or bundle generated from a
  pinned source and migration version.
- There is no application-startup migration. The API must not call `Migrate`,
  `MigrateAsync`, `EnsureCreated`, or `EnsureCreatedAsync` during startup.
- There is no bootstrap, seed, or reset in production. Synthetic Development
  and Testing data operations remain separate, environment-guarded workflows.
- A generated artifact remains unusable until its artifact hash, reviewer
  approval, operator approval, backup reference, and restore verification are
  recorded by the authorized deployment process.
- Production SQL edition/topology, the numeric deployment window, production
  credentials, backup custody, and execution authorization are not supplied by
  this demo contract.

## Manifest-ordered planned slices

The following order mirrors `.specify/persistence-manifest.json`. Every entry
is planned only. Its path declares the future canonical migration location; it
does not assert that a migration or bundle exists.

| Order | Migration ID | Kind | Owner | Planned path | Prerequisite specs |
|---:|---|---|---|---|---|
| 1 | `S1IdentityAcademicFoundation` | initial | SPEC-008 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs` | SPEC-004, SPEC-007, SPEC-008 |
| 2 | `S2CatalogueScheduling` | incremental | SPEC-010 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs` | SPEC-009, SPEC-010 |
| 3 | `S4DiscoveryPlanning` | incremental | SPEC-012 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713040000_DiscoveryPlanning.cs` | SPEC-011, SPEC-012 |
| 4 | `S6Registration` | incremental | SPEC-015 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs` | SPEC-014, SPEC-015 |
| 5 | `S7StaffAdminOperations` | incremental | SPEC-017 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713070000_StaffAdminOperations.cs` | SPEC-017 |

All slices share the future
`StudentRegistrationDbContextModelSnapshot.cs` in this directory and must be
generated and reviewed in this exact dependency order. A later slice cannot
stand in for a missing prerequisite slice.

## Required release record

Before an authorized operator may execute a migration against any controlled
environment, the release record must contain:

1. the pinned source and migration version and the exact source commit;
2. the generated script or bundle identifier and reproducible artifact hash;
3. independent reviewer approval for schema safety and data impact;
4. operator approval for the named environment and scheduled window;
5. a verified backup reference created before the change;
6. restore verification against an isolated production-like target;
7. forward rehearsal duration, output, schema/model parity, and query checks;
8. rollback rehearsal duration, instructions, and verified prior-state result.

Missing, expired, mismatched, or environment-ambiguous evidence blocks the
operation. Approval for one artifact hash, migration range, or environment is
not authority for another.

## Rehearsal and execution sequence

1. Validate the target identity, approved migration range, current schema, and
   ordered prerequisites without changing the database.
2. Verify the backup reference and completed restore verification.
3. Execute the forward rehearsal on the isolated production-like copy and
   compare the resulting schema with the reviewed EF model.
4. Execute the rollback rehearsal using the reviewed rollback or restore path,
   then verify the prior schema and critical data invariants.
5. Recreate the rehearsal target and repeat the forward path to prove the
   artifact is deterministic and operational notes are complete.
6. Stop. Production execution requires separate institutional reviewer and
   operator authority plus the still-unresolved production configuration.

Application startup, Development bootstrap, Testing seed, destructive reset,
and production migration execution are deliberately separate concerns.
