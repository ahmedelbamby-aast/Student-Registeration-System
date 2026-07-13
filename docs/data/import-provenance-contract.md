# Import and Synthetic Data Provenance Contract

**Contract:** `import-provenance-contract/1.0`<br>
**Requirement:** `FR-8`<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

This contract defines the provenance evidence that canonical import and seed
owners must persist or correlate. It creates no runtime entity and does not
claim that a planned owner mapping, importer, seed contributor, or database
profile already exists.

## Required import evidence

Every imported identity, academic, catalogue, curriculum, or policy record
must retain enough evidence to reproduce and review its origin:

- Import source/reference: required.
- Import source hash: required.
- Import batch: required.
- Import actor evidence: required.
- Import access/import time: required.
- The batch state and privacy-safe validation/error summary are required.
- Missing prerequisites reject preview and keep publication blocked.

Source/reference evidence includes a stable source name or reference, its
content hash, access/import timestamp, batch identity, and one of these
classifications:

| Classification | Meaning and publication boundary |
|---|---|
| `OfficialAASTMT` | Recorded AASTMT source provenance; it does not by itself grant production approval. |
| `AhmedApprovedDemo` | Ahmed-approved bounded demo interpretation, never represented as official institutional policy. |
| `SyntheticDemo` | Wholly synthetic fixture or explicitly labelled gap data used only by the demo profiles. |
| `UnresolvedInstitutional` | Institutional interpretation or authority is unresolved; publication and production use remain fail closed. |

An unknown classification is invalid. Provenance cannot be inferred from a
URL, file name, or seed location.

## Actor evidence without an invented catalogue field

The ERD defines `IdentityImportBatch.RequestedByUserId`; identity import actor
evidence uses that foreign key and the correlated audit/security evidence.

The catalogue `ImportBatch` intentionally has no requester field. Its actor is
proved by `AuditEvent.ActorReference` correlated to catalogue `ImportBatch`
through the import operation's correlation/entity references. Therefore,
`ImportBatch.RequestedByUserId` is not introduced. This preserves the approved
ERD while still making catalogue import actor evidence required and auditable.

## Planned deterministic seed profiles

Migrations run before seed contributors. Each synthetic row records or derives
its `SeedProfileVersion`, `FixtureOrdinal`, and `SyntheticDemo` provenance.
The same seed-profile version and fixture ordinal reproduce the same logical
identity and academic values. Reseeding the same profile version is
idempotent.

Stable logical IDs, University IDs, codes, relationships, GPA, earned credits,
standing, transcript facts, and catalogue values come from the versioned
fixture definition. Password hash bytes need not be deterministic because
ASP.NET Core Identity uses a per-hash salt. ASP.NET Core Identity verifies
generated credentials; SQL persists only its password hashes and never the
generated PIN/password plaintext.

The planned profiles from `.specify/persistence-manifest.json` version `2.1.0`
are:

| Profile | Database pattern | Reset/lifecycle | Identity contributor | Academic contributor |
|---|---|---|---|---|
| Development | `StudentRegistration_Development` | `explicit-command-only` | `src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs` | `src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs` |
| Testing | `StudentRegistration_Test_{runId}` | `isolated-per-run-dispose` | `src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs` | `src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs` |

Development data persists until an explicit guarded reset. Testing data is
isolated per run and disposed after that run. Neither profile is runtime proof
until its canonical contributors and real SQL provisioner tests are active.

## Reset, privacy, and fail-closed lifecycle

Reset is explicit and allowed only for Development or Testing. Every other
environment or connection target is rejected before mutation. A failed
migration, seed, or reset never marks the target ready, and no production
startup path seeds or resets data.

Real institutional or student data is prohibited in demo profiles. Generated
credential plaintext exists only transiently for its owning local demo flow;
it is never written to SQL, migrations, snapshots, logs, traces, exports, or
checked-in evidence. Git-ignored local credential, log, and export artifacts
are removed within seven days.

These lifecycle rules govern wholly synthetic POC data only. Production data
classification, retention, deletion, SQL edition/topology, and operational
authority require later institutional approval and remain fail closed.
