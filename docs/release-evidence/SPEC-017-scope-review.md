# SPEC-017 Out-of-Scope Review

Date: 2026-07-17  
Reviewed implementation commit: `5550bf19427fbc4d390f665ea751384f015c8234`  
Scope: SPEC-017 source, contracts, migrations, routes, permissions, controls,
and focused tests

## Method

The review inspected the five SPEC-017 API handlers, Admin pages and client,
permission definitions, application/domain ports, the staff-admin migration
and model snapshot, and the focused authorization, contract, architecture,
integration, and UI tests. Searches included the normalized terms
`super-admin`, `break-glass`, `warehouse`, `report replica`, `seat-decrement`,
`enrollment correction`, `withdrawal`, and Admin availability
mutation/correction/override variants.

Positive absence evidence is executable in:

- `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs`;
- `tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs`;
- `tests/StudentRegistration.ContractTests/Registration/RegistrationConflictTests.cs`;
- `tests/StudentRegistration.ContractTests/Specs/Spec010/AdminAvailabilityMutationAbsenceContractTests.cs`;
- `tests/StudentRegistration.ArchitectureTests/Spec017CompositionBoundaryTests.cs`; and
- `tests/StudentRegistration.E2ETests/Specs/Spec017/RegistrationAdministrationPageFeatureTests.cs`.

The only source hit for excluded registration actions is
`RegistrationRecordActionPolicy.cs`, which is a deny-only policy returning
false for drop, withdrawal, and correction. It does not expose an action.

## OS-1 — unrestricted super-admin and unaudited database edits

**Result: EXCLUDED (PASS).**

- SPEC-017 defines four narrow permissions: metrics read, audit read, audit
  read-all, and audit export. It defines no super-admin permission or bypass.
- Every SPEC-017 endpoint has a named authorization policy; export creation
  additionally has anti-forgery metadata. There is no endpoint accepting SQL,
  table names, arbitrary entity names, or an unrestricted command envelope.
- Writes are limited to the durable `ExportJob` lifecycle and its audit facts.
  The SQL store uses fixed parameterized statements and the shared
  transaction-aware audit writer; there is no direct-database-edit route or
  UI control.
- `AdminCommandInvariantTests` proves the generic Admin facade and bypass are
  absent, while endpoint authorization tests cover positive and negative
  policy decisions.

## OS-2 — bypasses, enrollment repair, and Admin availability mutation

**Result: EXCLUDED (PASS).**

- No break-glass capacity/conflict override permission, command, endpoint, or
  control exists. Feature-owner capacity and timetable invariants remain in
  their owning modules.
- Registration administration is read-only monitoring. The application
  exposes no enrollment correction, drop, withdrawal, or seat-decrement
  command. The deny-only registration action policy explicitly rejects those
  names.
- Admin availability is consumed only through the bounded read-only owner
  view/import contract. There is no Admin availability mutation, correction,
  override, permission, editable control, notification workflow, or
  correction-audit path.
- The absence is covered by `RegistrationConflictTests`,
  `AdminAvailabilityMutationAbsenceContractTests`,
  `AdminCommandInvariantTests`, and the ADM-08 E2E/component contracts.

## OS-3 — business-intelligence warehouse

**Result: EXCLUDED (PASS).**

- No warehouse project, schema, migration, adapter, connection setting,
  endpoint, DTO, route, or background synchronization job was added.
- Metrics remain bounded operational projections, audit search reads the
  primary application stores, and export artifacts are short-lived files.
- The SPEC-017 composition-boundary test constrains delivery to the existing
  API, client, StaffAdministration, IdentityAccess, Contracts, and SQL Server
  modules; no analytics subsystem is referenced.

## OS-4 — long-term report replica

**Result: EXCLUDED (PASS).**

- No read-replica connection string, routing policy, replication worker,
  replica health contract, migration target, or report-store adapter exists.
- `SqlAdminAuditReader` and the export store use the existing primary
  `StudentRegistrationDbContext`. The demo's two replicas are application
  workers competing through one primary SQL lease, not database replicas.
- NFR-2 measures first-page impact on the approved primary-store fixture.
  NFR-3 keeps exports bounded to 10,000 rows and 10 MiB. A long-term report
  replica remains deferred until primary impact warrants a separate approved
  specification.

## Conclusion

OS-1, OS-2, OS-3, and OS-4 remain excluded. No implementation, permission,
route, contract, migration, control, or hidden workflow expands SPEC-017 into
those areas. This review is limited to the demo implementation and does not
authorize future scope.
