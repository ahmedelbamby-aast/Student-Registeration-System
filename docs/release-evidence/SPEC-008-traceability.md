# SPEC-008 Complete Traceability Evidence

**Artifact version:** 1.0.0

**Requirement:** T090 / TRACE / SC-1 / SC-2 / SC-3

**Recorded UTC:** 2026-07-16T10:35:00Z

**Owner:** Ahmed ELbamby

## Scope and completeness

This matrix is derived from the approved SPEC-008 requirements, frozen API
contract, task ledger, and delivered repository files. The approved
requirements contain FR-1 through FR-11; FR-11 is included even though the
minimum requested inventory said FR-1 through FR-10, because omitting the
approved Admin requirement would leave routes 04 through 10 untraced.

| Inventory | Expected | Traced here |
|---|---:|---:|
| Functional requirements | 11 | 11 |
| Non-functional requirements | 4 | 4 |
| Acceptance criteria | 9 | 9 |
| Edge cases | 7 | 7 |
| Success criteria | 3 | 3 |
| API endpoints | 10 | 10 |
| Feature-owned entities | 6 | 6 |
| Frontend pages | 4 | 4 |
| Shared client facade | 1 | 1 |
| Foundation migration | 1 | 1 |

`Delivered` below means implemented and covered inside the approved SPEC-008
Gate A non-production demo boundary. It does not grant production authority or
claim delivery of work assigned to later specifications.

## Functional requirement trace matrix

| ID | Approved behavior | Implementation | Tasks | Executable verification |
|---|---|---|---|---|
| FR-1 | Authoritative UTC time and institutional IANA timezone | `AcademicContextResolver`, `RegistrationWindowService`, `TimeProviderRegistration`, context DTOs | T034, T041, T051, T061, T075-T078, T085 | `AC-1Tests.cs`, `AC-8Tests.cs`, `AcademicContextBoundaryTests.cs`, `Endpoint01ContractTests.cs`, `Endpoint02ContractTests.cs`, `NFR-1EvidenceTests.cs`; `SPEC-008-NFR-1.md` |
| FR-2 | Distinct teaching and registration terms | `AcademicContextResolver`, `AcademicStore`, `AcademicApiClient` | T035, T041, T051, T061, T075-T078 | `AC-2Tests.cs`, `AC-8Tests.cs`, `SC-1OutcomeTests.cs`, `AcademicContextBoundaryTests.cs`, `Endpoint02ContractTests.cs` |
| FR-3 | Explicit term/window dates, lifecycle, scope, creation replay metadata, rowversion | `AcademicTerm`, `RegistrationWindow`, `RegistrationWindowService`, `AcademicStore`, term contracts | T008-T009, T037, T039, T052, T055-T056, T062, T079-T080, T083-T084 | `AcademicTermModelTests.cs`, `RegistrationWindowModelTests.cs`, `AC-4Tests.cs`, `AC-6Tests.cs`, `RegistrationWindowConcurrencyTests.cs`, endpoint 05-07 contract tests |
| FR-4 | No overlapping Published windows; singleton current context; deterministic open/upcoming/closed selection | `AcademicContextResolver`, `RegistrationWindowService`, SQL constraints/indexes and stable locking in `AcademicStore` | T035, T039, T048, T051-T052, T061-T062, T075-T080, T083-T084 | `AC-2Tests.cs`, `AC-6Tests.cs`, `AC-8Tests.cs`, `EC-1Tests.cs`, `EC-6Tests.cs`, `EC-7Tests.cs`, `SC-1OutcomeTests.cs`, `SC-3OutcomeTests.cs` |
| FR-5 | Complete sourced profile, bounded pages/holds, immutable transcript correction, complete synthetic seed | `Student`, `TranscriptAttempt`, `StudentHold`, `StudentTermAcademicState`, `StudentAcademicProfileService`, `DemoStudentProfileSeedContributor`, `AcademicStore` | T010-T013, T036, T044, T049, T053, T057-T060, T063, T077-T078, T081-T084 | `StudentModelTests.cs`, `TranscriptAttemptModelTests.cs`, `StudentHoldModelTests.cs`, `StudentTermAcademicStateModelTests.cs`, `AC-3Tests.cs`, `AC-7Tests.cs`, `SC-2OutcomeTests.cs`, `DemoStudentProfileSeedTests.cs`, `Endpoint03ContractTests.cs`, `Endpoint09ContractTests.cs` |
| FR-6 | Registration boundary re-resolves time, term, window, state, and holds | `StudentAcademicProfileService.ExecuteRegistrationBoundaryAsync`, `IStudentAcademicProfileStore`, `AcademicStore` | T034, T036, T038, T047, T051, T061, T063, T083-T084 | `AC-1Tests.cs`, `AC-3Tests.cs`, `AC-5Tests.cs`, `EC-5Tests.cs`, `ProfileHoldConcurrencyTests.cs`, `AcademicContextBoundaryTests.cs` |
| FR-7 | Authorized, reasoned, sourced, versioned, audited Admin profile corrections | `AdminAcademicManagementService`, `StudentAcademicProfileService`, correction contracts, `AcademicStore`, `RolePolicies` | T037, T042, T053-T054, T063-T064, T074, T079-T082, T083-T084, T088 | `AC-4Tests.cs`, `AC-9Tests.cs`, `AdminAcademicJourneyTests.cs`, `ProfileHoldConcurrencyTests.cs`, `AcademicPermissionPolicyTests.cs`, `Endpoint10ContractTests.cs`, `NFR-4EvidenceTests.cs` |
| FR-8 | One database-backed student-term serialization/version boundary | `StudentTermAcademicState`, registration-boundary store command and serializable `AcademicStore` transaction | T013, T038, T053, T060, T063, T081-T084 | `AC-5Tests.cs`, `StudentTermAcademicStateModelTests.cs`, `ProfileHoldConcurrencyTests.cs`, `AcademicContextModelConfigurationTests.cs`; real seat/enrollment conformance remains SPEC-014-owned |
| FR-9 | Stable term/window locks, overlap recheck, bounded versions, stale rejection | `RegistrationWindowService`, `AcademicStore.PublishRegistrationWindowAsync`, `RegistrationWindow` | T026-T027, T039, T046, T052, T056, T062, T070-T071, T079-T080, T083-T084 | `AC-6Tests.cs`, `AC-9Tests.cs`, `EC-1Tests.cs`, `EC-4Tests.cs`, `RegistrationWindowConcurrencyTests.cs`, endpoint 06-07 contract tests |
| FR-10 | Authenticated AppContext composes canonical identity/session and academic context; ambiguity returns 503 without partial data | `Spec008Endpoints.GetAuthenticatedContextAsync`, `AcademicContextResolver`, `IAcademicSessionContextAdapter`, `AcademicSessionContextAdapter`, `AcademicApiClient` | T016-T017, T041, T051, T061, T065-T066, T075-T078 | `AC-8Tests.cs`, `Endpoint02ContractTests.cs`, `SharedAppContextOwnerContractTests.cs`, `AcademicContextBoundaryTests.cs`, `RoleGatewayPageContractTests.cs`, `StudentDashboardPageContractTests.cs` |
| FR-11 | Bounded Admin term/profile APIs, exact permissions, validation, audit and stable outcomes | `AdminAcademicManagementService`, `RegistrationWindowService`, `StudentAcademicProfileService`, `RolePolicies`, `Spec008Endpoints`, `AcademicApiClient` | T020-T033, T042, T054, T064, T068-T074, T079-T082, T088 | `AC-9Tests.cs`, endpoint 04-10 contract tests, `AdminAcademicJourneyTests.cs`, `AcademicPermissionPolicyTests.cs`, `IdentityRuntimeCompositionTests.cs`, `NFR-4EvidenceTests.cs` |

## Non-functional requirement trace matrix

| ID | Gate | Implementation and fixture | Tasks | Executable evidence |
|---|---|---|---|---|
| NFR-1 | Injected authoritative clock and boundary tests | `TimeProvider` in resolver/services/store/bootstrap | T040, T085 | `NFR-1EvidenceTests.cs`; `SPEC-008-NFR-1.md` |
| NFR-2 | 25,000 identities, two shared-SQL replicas, 600 seconds, 300 authenticated reads/s, 180,000 attempts, p95 at most 300 ms, failures below 0.1% | `AcademicContextLoadProfile.RequiredGate`, `AcademicContextLoadRunner`, `Spec008TwoReplicaSharedSqlFixture` | T040, T086 | `AcademicContextLoadTests.cs`, `NFR-2EvidenceTests.cs`; `SPEC-008-NFR-2.md`. This read-only gate does not replace the later SPEC-018 mixed-load gate. |
| NFR-3 | UTC `datetime2` instants and valid IANA term timezone | EF mappings, migration, `AcademicTerm`, timezone validation | T040, T083-T084, T087 | `NFR-3EvidenceTests.cs`, `S1IdentityAcademicFoundationMigrationTests.cs`; `SPEC-008-NFR-3.md`. Recurring meeting persistence remains SPEC-010-owned. |
| NFR-4 | Student self/approved Admin scope; synthetic-only fixtures; no full-profile evidence leakage | `RolePolicies`, resource handlers, endpoint policies, synthetic bootstraps | T040, T083-T084, T088 | `NFR-4EvidenceTests.cs`; `SPEC-008-NFR-4.md` |

## Acceptance criterion trace matrix

| ID | Requirement linkage and outcome | Tasks | Primary executable tests | Supporting implementation/evidence |
|---|---|---|---|---|
| AC-1 | FR-1/FR-6; device clock cannot affect half-open window boundary | T034, T075-T078, T085 | `AC-1Tests.cs` | `AcademicContextResolver`; `NFR-1EvidenceTests.cs` |
| AC-2 | FR-2/FR-4; no active term gives authoritative unavailable/read-only state | T035, T075-T078 | `AC-2Tests.cs` | `AcademicContextResolver`, `StudentDashboardPage.razor` |
| AC-3 | FR-5/FR-6; hold added before test-consumer commit blocks and callback is not invoked | T036, T053, T063 | `AC-3Tests.cs` | `StudentAcademicProfileService`; real enrollment remains SPEC-014-owned |
| AC-4 | FR-3/FR-7; governed term/profile edit, audit, stale rejection, payload-bound creation replay | T037, T079-T082 | `AC-4Tests.cs` | `RegistrationWindowService`, `AdminAcademicManagementService`, `AcademicStore` |
| AC-5 | FR-6/FR-8; hold mutation versus test consumer has one serial order and loser revalidates | T038, T081-T082 | `AC-5Tests.cs` | `StudentTermAcademicState`, `ProfileHoldConcurrencyTests.cs`; real enrollment remains SPEC-014-owned |
| AC-6 | FR-3/FR-4/FR-9; concurrent overlap publication has one winner and stable loser reason | T039, T079-T080 | `AC-6Tests.cs` | `RegistrationWindowConcurrencyTests.cs`, endpoint 07 |
| AC-7 | NFR-1..NFR-4; clock, reproducible synthetic graph, load, persistence and authorization quality gate | T040, T083-T088 | `AC-7Tests.cs` | four `NFR-*EvidenceTests.cs` suites and four `SPEC-008-NFR-*.md` records |
| AC-8 | FR-1/FR-2/FR-4/FR-10; canonical composed context and fail-closed ambiguity | T041, T075-T078 | `AC-8Tests.cs` | `AcademicSessionContextAdapter`, endpoint 02, `AcademicApiClient` |
| AC-9 | FR-3/FR-7/FR-9/FR-11; complete bounded ADM-02/ADM-04 journeys and stable failure outcomes | T042, T079-T082 | `AC-9Tests.cs` | `AdminAcademicJourneyTests.cs`, endpoint 04-10 contract tests and frontend evidence |

## Edge-case trace matrix

| ID | Approved edge behavior | Task | Executable test | Implementation |
|---|---|---|---|---|
| EC-1 | Any same-term Published overlap fails regardless of scope | T043 | `EdgeCases/EC-1Tests.cs` | `RegistrationWindowService`, `AcademicStore` |
| EC-2 | Missing GPA/provenance/incomplete seed or over-100 active holds fails closed | T044 | `EdgeCases/EC-2Tests.cs` | `StudentAcademicProfileService`, `DemoStudentProfileSeedContributor`, `AcademicStore` |
| EC-3 | Timezone rule change preserves unambiguous UTC and library-based display | T045, T087 | `EdgeCases/EC-3Tests.cs`, `NFR-3EvidenceTests.cs` | `AcademicTerm`, UTC EF converters |
| EC-4 | Stale Admin edit returns 409 and current version | T046 | `EdgeCases/EC-4Tests.cs` | `RegistrationWindowService`, `AdminAcademicManagementService`, `AcademicStore` |
| EC-5 | Server-received scheduled cutoff and emergency version change govern in-flight request | T047 | `EdgeCases/EC-5Tests.cs` | `StudentAcademicProfileService`, registration boundary |
| EC-6 | Multiple RegistrationOpen or Teaching terms fail `CONTEXT_UNAVAILABLE` | T048 | `EdgeCases/EC-6Tests.cs` | `AcademicContextResolver`, singleton SQL indexes |
| EC-7 | No open window selects earliest upcoming, otherwise latest closed, then none | T048 | `EdgeCases/EC-7Tests.cs` | `AcademicContextResolver` deterministic ordering |

## Success criterion trace matrix

| ID | Approved success measure | Task | Executable outcome | Status boundary |
|---|---|---|---|---|
| SC-1 | Availability depends only on authoritative institutional time and approved windows | T048, T085 | `SC-1OutcomeTests.cs`, `NFR-1EvidenceTests.cs` | Delivered for SPEC-008 context resolution |
| SC-2 | Every decision uses a complete sourced profile; same seed version reproduces logical values without production data | T049, T083-T084 | `SC-2OutcomeTests.cs`, `DemoStudentProfileSeedTests.cs`, bootstrap tests | Delivered for synthetic Development/Testing fixtures; production data source is not approved here |
| SC-3 | No two Published same-term windows overlap, preventing overlapping active context | T050, T052, T083-T084 | `SC-3OutcomeTests.cs`, `RegistrationWindowConcurrencyTests.cs`, `AcademicContextModelConfigurationTests.cs` | Delivered for term/window publication |

## Ten-route contract and outcome matrix

All routes are implemented in `Spec008Endpoints.cs`. Contract rows below retain
the approved success, authorization, and non-success outcomes; unexplained
omission of a status is not permitted.

| # | Route | Success and authorization | Approved non-success outcomes | Tasks and executable contract |
|---:|---|---|---|---|
| 01 | `GET /api/public/context` | `200 PublicContextDto`; anonymous | `503 CONTEXT_UNAVAILABLE`; `500 INTERNAL_ERROR`; validation, 401/403 and conflict not applicable | T014-T015, T065; `Endpoint01ContractTests.cs` |
| 02 | `GET /api/context` | `200 AppContextDto`; authenticated + `Context.Read` | `401`; `403`; `503 CONTEXT_UNAVAILABLE`; `500`; validation/conflict not applicable | T016-T017, T066; `Endpoint02ContractTests.cs`, `SharedAppContextOwnerContractTests.cs` |
| 03 | `GET /api/students/me/academic-context` | `200 StudentAcademicContextDto`; Student + `AcademicProfile.ReadOwn` + owner | `400 PAGE_SIZE_INVALID`; `401`; `403`; `404 PROFILE_NOT_FOUND`; `409 PROFILE_NOT_READY`; `503`; `500` | T018-T019, T067; `Endpoint03ContractTests.cs` |
| 04 | `GET /api/admin/terms` | `200 Page<AdminTermDto>`; `AcademicTerms.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503`; `500`; conflict not applicable | T020-T021, T068; `Endpoint04ContractTests.cs` |
| 05 | `POST /api/admin/terms` | `201 AdminTermDto`; `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; `409 TERM_CODE_EXISTS/TERM_STATE_CONFLICT/IDEMPOTENCY_KEY_REUSED`; `503`; `500` | T022-T023, T069; `Endpoint05ContractTests.cs` |
| 06 | `PUT /api/admin/terms/{termId}` | `200 AdminTermDto`; `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404`; `409 STALE_VERSION/TERM_STATE_CONFLICT/WINDOW_OVERLAP`; `503`; `500` | T024-T025, T070; `Endpoint06ContractTests.cs` |
| 07 | `POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish` | `200 AdminTermDto`; `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404`; `409 STALE_VERSION/WINDOW_OVERLAP`; `503`; `500` | T026-T027, T071; `Endpoint07ContractTests.cs` |
| 08 | `GET /api/admin/students` | `200 Page<AdminStudentLocatorDto>`; `AcademicProfiles.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503`; `500`; conflict not applicable | T028-T029, T072; `Endpoint08ContractTests.cs` |
| 09 | `GET /api/admin/students/{studentId}/academic-context` | `200 AdminStudentAcademicContextDto`; `AcademicProfiles.Manage` + named StudentId/TermId | `400`; `401`; `403`; authorized `404`; `409 PROFILE_NOT_READY`; `503`; `500`; other conflict not applicable | T030-T031, T073; `Endpoint09ContractTests.cs` |
| 10 | `PATCH /api/admin/students/{studentId}/academic-profile` | `200 AdminStudentAcademicContextDto`; `AcademicProfiles.Manage` + named StudentId/TermId + antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404`; `409 STALE_VERSION/INVALID_SUPERSESSION/PROFILE_NOT_READY`; `503`; `500` | T032-T033, T074; `Endpoint10ContractTests.cs` |

## Entity, persistence, and migration trace matrix

| Entity/artifact | Delivered source and invariant | Tasks | Executable verification |
|---|---|---|---|
| `AcademicTerm` | `Domain/AcademicTerm.cs`; lifecycle, timezone, unique code/client request, payload hash, rowversion | T008, T055, T083-T084 | `AcademicTermModelTests.cs`, `AcademicContextModelConfigurationTests.cs`, migration tests |
| `RegistrationWindow` | `Domain/RegistrationWindow.cs`; scope/range/lifecycle/computed state/version | T009, T056, T083-T084 | `RegistrationWindowModelTests.cs`, `RegistrationWindowConcurrencyTests.cs`, migration tests |
| `Student` | `Domain/Student.cs`; identity link, program/cohort/profile/provenance/version | T010, T057, T083-T084 | `StudentModelTests.cs`, seed/bootstrap and mapping tests |
| `TranscriptAttempt` | `Domain/TranscriptAttempt.cs`; immutable append-only supersession and provenance | T011, T058, T083-T084 | `TranscriptAttemptModelTests.cs`, `ProfileHoldConcurrencyTests.cs`, mapping tests |
| `StudentHold` | `Domain/StudentHold.cs`; term/effective interval/blocking/source | T012, T059, T083-T084 | `StudentHoldModelTests.cs`, `ProfileHoldConcurrencyTests.cs`, mapping tests |
| `StudentTermAcademicState` | `Domain/StudentTermAcademicState.cs`; unique student/term rowversion serialization guard | T013, T060, T083-T084 | `StudentTermAcademicStateModelTests.cs`, `AC-5Tests.cs`, mapping tests |
| SQL mapping/store | `AcademicContextModelConfiguration.cs`, `AcademicStore.cs`, `AcademicSqlServerRegistration.cs` | T083-T084 | `AcademicContextModelConfigurationTests.cs`, `Spec008NonProductionBootstrapTests.cs` |
| `S1IdentityAcademicFoundation` migration | `20260713010000_IdentityAcademicFoundation.cs` plus model snapshot; migration first, no seed operations | T083-T084 | `S1IdentityAcademicFoundationMigrationTests.cs`, `Spec008SqlServerTestDatabaseBootstrapperTests.cs` |

## Frontend page and facade trace matrix

| Route ID | Delivered page/facade | Requirement links | Tasks | Executable UI verification and evidence |
|---|---|---|---|---|
| AUTH-01 `/` | `RoleGatewayPage.razor` using `AcademicApiClient` | FR-1, FR-2, FR-4, FR-10; AC-1, AC-2, AC-8 | T075-T076 | contract, component, E2E, accessibility and visual `RoleGatewayPage*Tests`; `SPEC-008-AUTH-01-frontend.md` |
| STU-01 `/student` | `StudentDashboardPage.razor` using `AcademicApiClient` | FR-1, FR-2, FR-4, FR-5, FR-10; AC-1, AC-2, AC-7, AC-8 | T077-T078 | contract, component, E2E, accessibility and visual `StudentDashboardPage*Tests`; `SPEC-008-STU-01-frontend.md` |
| ADM-02 `/admin/terms` | `TermAdministrationPage.razor` using `AcademicApiClient` | FR-3, FR-4, FR-7, FR-9, FR-11; AC-4, AC-6, AC-9 | T079-T080 | contract, component, E2E, accessibility and visual `TermAdministrationPage*Tests`; `SPEC-008-ADM-02-frontend.md` |
| ADM-04 `/admin/students` | `StudentAdministrationPage.razor` using `AcademicApiClient` | FR-5, FR-7, FR-8, FR-11; AC-4, AC-5, AC-9 | T081-T082 | contract, component, E2E, accessibility and visual `StudentAdministrationPage*Tests`; `SPEC-008-ADM-04-frontend.md` |
| Shared facade | `Features/Academics/AcademicApiClient.cs`, registered once in client `Program.cs` | all ten route DTO/request boundaries used by the four pages | T076, T078, T080, T082 | four client-contract suites and four component suites |

STU-01 intentionally retains the downstream timetable contributor as
unavailable until SPEC-015 supplies its contract. That is an honest future
state, not a delivered timetable feature.

## Future ownership and authority boundary

| Owner | Not delivered by SPEC-008 | Delivered SPEC-008 boundary available to it |
|---|---|---|
| SPEC-010 | Recurring class meeting DayOfWeek/TimeOnly persistence | UTC academic term/window and IANA timezone foundation |
| SPEC-014 | Real seat/enrollment writes and end-to-end registration conformance | Student-term lock/version protocol proven with a test consumer |
| SPEC-015 | Timetable catalogue, optimization and student schedule contributor | Authoritative term/profile/context and STU-01 unavailable contributor state |
| SPEC-017 | Audit/report orchestration and views | Privacy-safe atomic owner audit records and Admin owner APIs |
| SPEC-018 | Mixed read/write target, spike, soak, failover and release operations gate | SPEC-008 read-only authenticated context result only |
| Institutional/operations approvers | Production data source, production deployment, Gate B-D and official AASTMT go-live | Non-production Gate A demo evidence only |

## Deterministic verification and source binding

Normalized-LF SHA-256 bindings:

- `spec.md`: `62CB9AEB2BC0B14B0FD06EF851CAB7F3B877A852EA88BC00B1FF089006EFA8DB`
- `requirements.md`: `152F102D24C3C7F44B7B57400E76C590082306CEA5E0FDBED20773DCD312C5B5`
- `contracts/api.md`: `2E4D6177C66AB60C2214E9FD008C8531C7FECB28DFCCFCC804CB50DEBC61E257`

The inventory is reproducible from the repository root with this read-only
PowerShell check:

```powershell
$requirements = Get-Content specs/008-academic-term-student-profile/requirements.md -Raw
$spec = Get-Content specs/008-academic-term-student-profile/spec.md -Raw
$contract = Get-Content specs/008-academic-term-student-profile/contracts/api.md -Raw
$trace = Get-Content docs/release-evidence/SPEC-008-traceability.md -Raw

$expected = @{
  FR = 1..11
  NFR = 1..4
  AC = 1..9
  EC = 1..7
  SC = 1..3
  Route = 1..10
}

foreach ($kind in 'FR','NFR','AC','EC','SC') {
  foreach ($number in $expected[$kind]) {
    if ($trace -notmatch [regex]::Escape("| $kind-$number |")) {
      throw "Missing trace row: $kind-$number"
    }
  }
}
foreach ($number in $expected.Route) {
  $routeNumber = $number.ToString('00')
  if ($trace -notmatch "(?m)^\| $routeNumber \|") {
    throw "Missing route trace row: $routeNumber"
  }
}

$sourceCounts = [ordered]@{
  FR = ([regex]::Matches($requirements, '(?m)^- FR-\d+:')).Count
  NFR = ([regex]::Matches($requirements, '(?m)^- NFR-\d+:')).Count
  AC = ([regex]::Matches($requirements, '(?m)^### AC-\d+:')).Count
  EC = ([regex]::Matches($requirements, '(?m)^- EC-\d+:')).Count
  SC = ([regex]::Matches($spec, '(?m)^- \*\*SC-\d\*\*:')).Count
  Route = ([regex]::Matches($contract, '(?m)^\| (?:0[1-9]|10) \|')).Count
}
if (($sourceCounts.Values -join ',') -ne '11,4,9,7,3,10') {
  throw "Approved source inventory drift: $($sourceCounts.Values -join ',')"
}
$sourceCounts
```

## Executed checks

Run on 2026-07-16 from the repository root with Release configuration and
warnings treated as errors:

```powershell
dotnet test tests/StudentRegistration.ContractTests/StudentRegistration.ContractTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~Specs.Spec008" --logger "console;verbosity=minimal" -p:TreatWarningsAsErrors=true

dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~Specs.Spec008" --logger "console;verbosity=minimal" -p:TreatWarningsAsErrors=true
```

| Suite | Passed | Failed | Skipped |
|---|---:|---:|---:|
| SPEC-008 contract checks | 37 | 0 | 0 |
| SPEC-008 acceptance and success-criterion checks | 27 | 0 | 0 |

Every required matrix row is complete, and no future-owned work is reported as
delivered.

**Result: PASS.**
