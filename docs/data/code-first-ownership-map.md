# Code First Ownership Map

**Contract:** `code-first-ownership-map/1.0`<br>
**Requirement:** `FR-1`<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

This document governs the planned Code First composition. It does not claim
that a downstream model, mapping, database profile, or migration exists or has
passed runtime verification. Every contribution and migration remains planned
until its canonical owner is implemented and its real SQL tests pass.

## Composition and schema ownership

Infrastructure.SqlServer owns the sole DbContext and migrations. Canonical
feature owners own mappings and seed rows; SPEC-005 owns only this
cross-module conformance contract.

- Sole DbContext owner: SPEC-004
- Sole composition path:
  `src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs`
- Demo engine: SQL Server 2022 Developer
- Required database compatibility: compatibility level 160
- Development provisioner: Docker Development
- Testing provisioner: Testcontainers Testing
- Schema: auth
- Schema: academics
- Schema: scheduling
- Schema: registration
- Schema: audit

The schemas are logical ownership boundaries inside one SQL Server database;
they are not separate databases, services, or DbContexts. Cross-module writes
that require atomicity use the sole composed context.

## Planned mapping contributions

All rows below come from `.specify/persistence-manifest.json` version `2.1.0`.
They are planned contribution boundaries, not proof that their paths already
exist.

| Canonical owner | Mode | Planned EF contribution path |
|---|---|---|
| SPEC-004 | `append-only-cross-module` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AuditEventModelConfiguration.cs` |
| SPEC-007 | `writable` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs` |
| SPEC-008 | `writable` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` |
| SPEC-009 | `writable` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/CatalogueModelConfiguration.cs` |
| SPEC-010 | `writable` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs` |
| SPEC-011 | `keyless-read-only` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationDiscoveryModelConfiguration.cs` |
| SPEC-012 | `writable` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs` |
| SPEC-014 | `writable-transactional` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs` |
| SPEC-015 | `projection-on-014` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationReceiptModelConfiguration.cs` |
| SPEC-017 | `writable-reporting` | `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AdministrationAuditModelConfiguration.cs` |

Each owner defines its entity and mapping contribution. Infrastructure.SqlServer
composes those contributions without becoming a second domain owner.

## Planned non-production data profiles

These profiles are isolated demo plans. Migrations run before seed
contributors. Seed rows in migrations: prohibited.

| Profile | Database pattern | Allowed environment | Orchestrator | Identity contributor | Academic contributor | Reset/lifecycle mode |
|---|---|---|---|---|---|---|
| Development | `StudentRegistration_Development` | Allowed environment: Development | `src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs` | `src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs` | `src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs` | `explicit-command-only`; persists until guarded reset |
| Testing | `StudentRegistration_Test_{runId}` | Allowed environment: Testing | `tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerTestDatabaseFixture.cs` | `src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs` | `src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs` | `isolated-per-run-dispose`; disposed after the run |

Both profiles use a versioned deterministic logical fixture. Stable fixture
identifiers and ordinals govern each row. Reseeding the same profile version is
idempotent. Canonical feature owners write only their seed rows after the
schema has migrated.

Plaintext credentials are transient while the owning Identity component
hashes them. SQL contains ASP.NET Core Identity hashes only. Checked-in
fixtures, migrations, snapshots, logs, traces, and test evidence contain no
plaintext PIN or password and no real institutional/student profile.

## Planned migration slices

Every row is a Planned migration contribution from the manifest. A slice may
be generated only after all prerequisite owner mappings are active and their
tests pass; one shared snapshot is advanced in dependency order.

| Kind | Slice | Planned migration contribution owner | Planned path |
|---|---|---|---|
| Initial | `S1IdentityAcademicFoundation` | SPEC-008 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs` |
| Incremental | `S2CatalogueScheduling` | SPEC-010 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs` |
| Incremental | `S4DiscoveryPlanning` | SPEC-012 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713040000_DiscoveryPlanning.cs` |
| Incremental | `S6Registration` | SPEC-014 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs` |
| Incremental | `S7StaffAdminOperations` | SPEC-017 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713070000_StaffAdminOperations.cs` |
| Incremental performance | `S8Spec018RegistrationReadPerformance` | SPEC-018 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260717222551_Spec018RegistrationReadPerformance.cs` |
| Incremental constraint | `S9Spec009CourseCreditsExactlyThree` | SPEC-009 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260720182228_Spec009CourseCreditsExactlyThree.cs` |
| Incremental registration | `S10Spec014ApprovalSeatHoldsAndFirstTermAutomation` | SPEC-014 | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260720185102_Spec014ApprovalSeatHoldsAndFirstTermAutomation.cs` |

## Environment and production boundary

Bootstrap first verifies both the environment name and the resolved connection
target. Unknown environment or connection targets reject bootstrap/reset
without mutation. A partial migrate/seed operation never marks a database
ready. Reset is a separately invoked, guarded non-production operation; it is
never an application-startup behavior.

Production bootstrap/reset path: prohibited. Production migrations require a
reviewed script or bundle, approved operational authority, backup, rehearsal,
and rollback evidence. This demo contract does not approve a production SQL
Server edition or topology. Until those decisions and evidence exist,
production deployment and release remain fail closed.
