# SPEC-008 Scope Review

**Review date:** 2026-07-16

**Decision authority:** Ahmed ELbamby

**Result:** OS-1 through OS-4 remain excluded from the non-production demo.

## Delivered boundary inspected

The review compared the approved SPEC-008 requirements with the delivered
Academics domain, application services and ports, HTTP/client facades, API-side
session adapter, SQL adapter, EF mapping and composition. The delivered slice
contains six Academics entities (`AcademicTerm`, `RegistrationWindow`,
`Student`, `StudentTermAcademicState`, `TranscriptAttempt`, and `StudentHold`),
four use-case services, one demo seed contributor, six persistence ports, one
API session-composition port, and one `AcademicStore` over the existing shared
`StudentRegistrationDbContext`.

The reconciled [class diagram](../diagrams/CLASS_DIAGRAM.md) records those
dependencies without presenting future-spec types as delivered code.

## Verified exclusions

| ID | Exclusion | Actual-source verification | Future ownership |
|---|---|---|---|
| OS-1 | Computing official grades from assessment events | `Student`, `StudentTermAcademicState`, and `TranscriptAttempt` persist sourced GPA, standing, credits, grade/status, provenance and correction history. No assessment-event entity, assessment ingestion port, weighting formula, or official-grade computation service exists in the SPEC-008 domain or application source. | A future assessment/official-grade feature requires its own approved spec and data authority. It is not silently assigned to the profile module. |
| OS-2 | Inferring a term from month/date alone | `AcademicContextResolver` selects at most one explicitly persisted `Teaching` term and one explicitly persisted `RegistrationOpen` term, fails closed on ambiguity, and uses dates only as term metadata. No month switch, academic-year heuristic, or browser-selected term establishes current state. | Term lifecycle remains explicitly administered by SPEC-008. Any automated lifecycle policy requires a separately approved change to SPEC-008 rather than client inference. |
| OS-3 | Browser clock as an authority | `AcademicContextResolver`, `RegistrationWindowService`, `StudentAcademicProfileService`, and `AcademicStore` receive authoritative server `TimeProvider` values. `AcademicApiClient` sends no current-time value and only deserializes the server result. | SPEC-012 through SPEC-014 must consume the authoritative term/window instant; they may not reintroduce browser time. |
| OS-4 | SIS synchronization mechanism until integration is specified | The production path contains no SIS connector, polling job, webhook, message consumer, synchronization checkpoint, or institutional-data credential. `DemoStudentProfileSeedContributor` is synthetic Development/Testing bootstrap, and Admin correction is an audited application command rather than SIS sync. | The SIS mechanism, source authority, reconciliation rules, credentials, operations and production approval require a future integration spec. |

## Downstream and complexity exclusions

The source and diagram inspection also confirms these ownership boundaries:

- SPEC-009 owns runtime catalogue/prerequisite/policy administration.
- SPEC-010 owns offerings, groups, staff/room resources, and recurring
  `DayOfWeek`/`TimeOnly` meetings.
- SPEC-011 owns eligibility and subject discovery.
- SPEC-012 and SPEC-013 own schedule conflicts and recommendations.
- SPEC-014 owns registration submissions, atomic capacity, seats and
  enrollments; SPEC-008 supplies only the tested student-term guard protocol.
- SPEC-015 through SPEC-017 own registration records, staff workspaces, and
  admin operations/reporting contributions.
- SPEC-018 retains the later mixed-load, security and operational release gate.

No event bus, message broker, microservice boundary, distributed lock,
distributed transaction, second DbContext, generic repository, or additional
unit-of-work abstraction was introduced. `AcademicStore` implements the six
consumer-oriented ports as one scoped SQL adapter so local profile, term and
audit work can share the existing EF/SQL transaction boundary.

## Dependency-direction review

The delivered direction is:

```text
Client -> Contracts
Client --same-origin HTTP--> HTTP endpoint facade
Endpoint facade -> Academics application services and ports
API composition adapter -> Academics composition port + SPEC-007 session service
Academics application -> Academics domain + ports + TimeProvider
SQL infrastructure -> Academics ports/domain + shared DbContext/audit port
```

The six domain entities do not reference ASP.NET Core, Blazor, EF Core, SQL
Server, the API project, or IdentityAccess implementation types.
`AcademicSessionContextAdapter` is deliberately in the API composition project,
which prevents an Academics-to-Identity implementation dependency. Program and
course references remain bounded sourced codes rather than premature foreign
keys to downstream runtime entities.

## Deterministic drift binding

The implementation manifest below was sorted with ordinal path comparison.
For each path, the verifier concatenated `relative-path + LF + normalized-LF
file-content + LF`, encoded the complete stream as UTF-8 without a BOM, and
calculated SHA-256.

Implementation manifest normalized-LF SHA-256 (24 files):
`77EEEFBD26254C580E5DF56EC77A8A715294792F3C2FA7718242C5E8F965FA95`

Class diagram normalized-LF SHA-256:
`289F53A3F85A84038B56EB2DF16303A9971FAC1E32A86642FB5D353238C80F8F`

SPEC-008 requirements normalized-LF SHA-256:
`152F102D24C3C7F44B7B57400E76C590082306CEA5E0FDBED20773DCD312C5B5`

Manifest paths:

```text
src/StudentRegistration.Academics/Application/AcademicContextResolver.cs
src/StudentRegistration.Academics/Application/AdminAcademicManagementService.cs
src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs
src/StudentRegistration.Academics/Application/Ports/IAcademicContextReader.cs
src/StudentRegistration.Academics/Application/Ports/IAcademicSessionContextAdapter.cs
src/StudentRegistration.Academics/Application/Ports/IAdminAcademicStore.cs
src/StudentRegistration.Academics/Application/Ports/IRegistrationWindowStore.cs
src/StudentRegistration.Academics/Application/Ports/IStudentAcademicProfileStore.cs
src/StudentRegistration.Academics/Application/RegistrationWindowService.cs
src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs
src/StudentRegistration.Academics/Domain/AcademicTerm.cs
src/StudentRegistration.Academics/Domain/RegistrationWindow.cs
src/StudentRegistration.Academics/Domain/Student.cs
src/StudentRegistration.Academics/Domain/StudentHold.cs
src/StudentRegistration.Academics/Domain/StudentTermAcademicState.cs
src/StudentRegistration.Academics/Domain/TranscriptAttempt.cs
src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs
src/StudentRegistration.Api/Composition/AcademicModuleRegistration.cs
src/StudentRegistration.Api/Composition/AcademicSessionContextAdapter.cs
src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs
src/StudentRegistration.Infrastructure.SqlServer/Persistence/AcademicSqlServerRegistration.cs
src/StudentRegistration.Infrastructure.SqlServer/Persistence/AcademicStore.cs
src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs
src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs
```

Any hash mismatch invalidates this review until the implementation, diagram,
ownership boundary and evidence are reconciled together. This review approves
only the delivered non-production SPEC-008 demo slice; it does not authorize
production SIS integration or claim completion of future specs.

**Result: PASS.**
