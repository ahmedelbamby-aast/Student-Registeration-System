# SPEC-018 Scope Review

**Reviewed on:** 2026-07-14<br>
**Reviewer and owner:** Ahmed ELbamby<br>
**Scope:** T069-T072 exclusion audit only<br>
**Boundary result:** OS-1 through OS-4 remain excluded or prohibited<br>
**Release decision:** None

> This is a working-tree scope audit, not immutable release evidence. It does
> not complete SPEC-018 traceability, approval, Gate D, production authority,
> or any measured browser, load, accessibility, security, or recovery gate.

## Inspection baseline

The review inspected the current tracked and untracked workspace, excluding
generated `.git`, `bin`, and `obj` content. The baseline is not pinned to a
release commit and therefore cannot authorize deployment.

| Area | Inspected paths | Scope observation |
|---|---|---|
| Governing scope | `specs/018-quality-security-scalability-operations/spec.md`, `requirements.md`, `plan.md`, `research.md`, `data-model.md`, and `contracts/api.md` | All four OS statements remain explicit. The plan keeps vendor hosting, Kubernetes, and actual Safari/macOS outside the POC. |
| Runtime source | `src/StudentRegistration.Api/Operations/SecurityConfiguration.cs`, `src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs`, `src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs`, and `src/StudentRegistration.Api/Endpoints/Spec018Endpoints.cs` | POC security inputs and privacy-safe operations surfaces are bounded; missing production authority fails closed. |
| Shared contracts | `src/StudentRegistration.Contracts/Operations/HealthSummary.cs` and `OperationalMetric.cs` | Health and metric DTOs are bounded. Metric allow-list enforcement remains at the only runtime collector that constructs endpoint metrics. |
| Migrations | `src/StudentRegistration.Infrastructure.SqlServer/Migrations/README.md` and the complete migrations directory | The directory contains only the controlled-procedure README and zero `.cs` migration files. It grants no production execution, hosting, or credential authority. |
| Routes | `.specify/route-manifest.json`, `.specify/page-api-manifest.json`, and `specs/018-quality-security-scalability-operations/contracts/api.md` | SPEC-018 is neither a design nor implementation owner for a UI route. It is linked to `SYS-01` for the safe health summary; route/API inspection found no hosting, orchestration, SLO, or PII-collection surface. Route reconciliation remains outside this audit. |
| Infrastructure and dependencies | `infra/docker/compose.development.yml`, `.github/workflows/ci.yml`, every source/test `.csproj`, and `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs` | Infrastructure contains only non-production Development Docker Compose plus CI. No production hosting/secret-store SDK selection or Kubernetes package/artifact was found. |
| Browser evidence | `tests/StudentRegistration.E2ETests/browser-matrix.json`, `tests/StudentRegistration.SpecificationTests/Frontend/BrowserMatrixContractTests.cs`, and `tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs` | WebKit is explicitly not Safari. Actual Safari/macOS is deferred and `not-passed`; other browser executions are still planned. |
| Telemetry governance and tests | `docs/operations/telemetry-contract.md`, `docs/security/THREAT_MODEL.md`, `tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs`, both SPEC-018 endpoint behavior test files, and the SPEC-018 structured-log/trace schema tests | Runtime names and dimensions are allow-listed, cardinality is bounded, sensitive inputs are rejected, and public/restricted outputs are minimized. |
| Remaining release evidence | SPEC-018 acceptance, integration edge, load, quality, recovery, release, and E2E test paths | Activation-gated and planned evidence remains visible. A skipped, planned, or deferred result is not treated as passing evidence. |

The filesystem searches also produced these exact boundary observations:

- Kubernetes/Helm artifact candidates: **0**.
- Migration code files under the governed migrations directory: **0**.
- SPEC-018 direct UI route design owners: **0**; implementation owners: **0**.
- Tracked `.env`, `secrets.json`, certificate, or private-key candidates: **0**.
- Infrastructure/workflow files: only `infra/docker/compose.development.yml`
  and `.github/workflows/ci.yml`.

## OS-1 - production hosting, vendor secret provider, and Safari/macOS

**Boundary result: PRESERVED - excluded from the POC.**

- `SecurityConfiguration.cs` accepts the approved POC inputs from .NET User
  Secrets or environment variables and delegates to the shared SQL-backed Data
  Protection registration. Its contract explicitly says the production secret
  provider remains undecided and grants no production authority.
- `DataProtectionRegistration.cs` requires both production repository and
  encryption approval flags before Production composition can continue. With
  either approval absent, startup throws
  `PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED` before certificate loading.
- `SecretAndDataProtectionTests.cs` verifies the POC certificate inputs, no
  tracked secret/certificate material, and the fail-closed Production branch.
  Its cross-replica session and protected-option-token checks remain skipped
  until their owning feature runtimes exist.
- `compose.development.yml` declares a local SQL Server Developer service and
  explicitly disclaims production edition or topology approval. No production
  deployment bundle, hosting selection, load-balancer/orchestrator definition,
  or vendor secret-store SDK was found in infrastructure, workflows, source
  projects, or test projects.
- `browser-matrix.json` labels the bundled target `Playwright WebKit (not
  Safari)`. The separate `Apple Safari` row says `status: deferred`, `result:
  not-passed`, and that a macOS runner is unavailable. Contract tests preserve
  that distinction; they are not actual Safari evidence.
- `THREAT_MODEL.md` records `Production authority: Not granted` and treats final
  hosting, secret provider, certificate custody, and AASTMT go-live as
  unresolved boundaries.

**Fail-closed activation boundary:** an approved specification/architecture
decision must name the production hosting platform, secret provider,
certificate custody, runtime topology, and accountable production authority.
Actual Safari must then execute on an identified macOS/Safari version with a
dated result. POC User Secrets, environment variables, a local certificate,
Docker Compose, or Playwright WebKit cannot be promoted into that evidence by
inference.

## OS-2 - Kubernetes by default

**Boundary result: PRESERVED - absent and default-excluded.**

- A repository-wide artifact scan found no `k8s`, `kubernetes`, `helm`, or
  chart directory; no `Chart.yaml`, Helm values file, `.helmignore`, or
  Kubernetes deployment/service/ingress manifest; and no Kubernetes client
  package.
- The only textual Kubernetes references are governance exclusions,
  architecture revisit criteria, and tests that guard against accidental
  introduction.
- `ProhibitedComplexityTests.cs` rejects `KubernetesClient` dependencies. The
  architecture contract requires measured need plus a separately approved ADR
  and specification before the boundary can change.
- Development Docker Compose and SQL Server Testcontainers are local POC/test
  facilities, not Kubernetes definitions or production orchestration.

**Fail-closed activation boundary:** Kubernetes remains absent unless measured
operational evidence shows simpler hosting is insufficient and Ahmed ELbamby
approves the corresponding specification, ADR, architecture-test update,
security review, and operations plan. Scaling goals alone do not select it.

## OS-3 - 24/7 SLO outside announced registration windows

**Boundary result: PRESERVED - no 24/7 commitment exists.**

- `docs/PROJECT_PLAN.md` limits the approved availability target to **99.9%
  during announced registration windows** and explicitly calls all listed
  values POC engineering targets, not production availability commitments or
  an AASTMT SLA.
- Repository-wide exact searches found `24/7` only in the SPEC-018 exclusion
  and its scope-review task. No runtime configuration, deployment artifact,
  alert rule, error budget, on-call policy, or evidence record establishes a
  24/7 objective.
- `OperationalHealthRegistry`, `/api/health`, and the restricted metrics API
  expose current operational signals. They do not define the measurement
  window, organizational support model, or contractual availability target.

**Fail-closed activation boundary:** any availability target outside announced
registration windows requires an approved production SLO definition with
measurement windows, exclusions, error-budget policy, monitoring source,
incident ownership, capacity evidence, and production authority. Health and
metrics implementation alone cannot create an SLA.

## OS-4 - arbitrary student PII in telemetry

**Boundary result: PRESERVED - arbitrary student PII collection is
prohibited.**

- `OperationalTelemetry` accepts only ten declared metric names and the seven
  dimension keys `code`, `method`, `module`, `operation`, `outcome`, `route`,
  and `statusClass`. A series may have at most four dimensions, values must be
  short controlled codes, and the in-memory snapshot is capped at 256 series.
- Correlation middleware ignores client-supplied correlation identifiers and
  always creates a server-generated GUID reference. It records safe
  method/status-class dimensions and does not read raw request bodies or query
  strings.
- `/api/operations/metrics` is restricted to an authenticated server-derived
  Admin role, is paginated with a maximum page size of 100, and returns
  `Cache-Control: no-store`. `/api/health` returns only status, application
  version, and server UTC time.
- The API contract prohibits secrets, credentials, identities,
  student/profile values, connection data, host/replica names, and internal
  topology. Unknown metric or dimension names are rejected rather than
  reflected.
- The structured-log and trace schemas use `additionalProperties: false` with
  closed attribute sets. `telemetry-contract.md` also prohibits credentials,
  full student profiles, raw request/response bodies, query strings, personal
  identifiers, connection details, and topology.
- Executable tests reject a metric named `student.password`, a `studentId`
  dimension, email-shaped method input, and numeric student-like `code`
  values. They also verify the series cap and scan the observability source for
  raw request, connection-string, password, and University ID access.

**Fail-closed activation boundary:** arbitrary PII is not a deferred feature.
Any new telemetry name or attribute must be added to the closed allow-list and
schema through approved privacy/security review, with bounded cardinality,
redaction, retention, authorization, and negative leakage tests. Unknown or
unapproved fields remain rejected; production telemetry retention and exporter
authority remain unresolved.

## Non-release conclusion

This inspection records only that the four excluded boundaries remain intact
in the current workspace. It does not claim that SPEC-018 is implemented or
released. In particular, T073 traceability and T074 approval remain outside
this review, actual Safari/macOS remains not passed, mandatory measured load
and failover evidence remains activation-gated, the recovery runbook is not a
completed recovery rehearsal, and Gate D and production authorization remain
closed.
