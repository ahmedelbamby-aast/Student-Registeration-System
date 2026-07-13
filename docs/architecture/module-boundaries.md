# Module Boundaries

```yaml
schemaVersion: 1.0
ownerSpec: SPEC-004
approvalVersion: Gate-A-2026-07-13
approvedBy: Ahmed ELbamby
```

This is the canonical project-reference contract for the Student Registration
modular monolith. `ModuleDependencyTests` compares this record with the real
project graph and will fail the build when a project, edge, cycle, or forbidden
module-internal dependency is introduced.

## Allowed project references

`None` means that the project has no source-project reference. References in a
row are an exact allowlist, not examples.

| Project | Allowed direct project references |
|---|---|
| StudentRegistration.Contracts | None |
| StudentRegistration.Client | StudentRegistration.Contracts |
| StudentRegistration.IdentityAccess | StudentRegistration.Contracts |
| StudentRegistration.Academics | StudentRegistration.Contracts |
| StudentRegistration.Scheduling | StudentRegistration.Contracts |
| StudentRegistration.Registration | StudentRegistration.Academics, StudentRegistration.Contracts, StudentRegistration.IdentityAccess, StudentRegistration.Scheduling |
| StudentRegistration.StaffAdministration | StudentRegistration.Academics, StudentRegistration.Contracts, StudentRegistration.Scheduling |
| StudentRegistration.Infrastructure.SqlServer | StudentRegistration.Academics, StudentRegistration.Contracts, StudentRegistration.IdentityAccess, StudentRegistration.Registration, StudentRegistration.Scheduling, StudentRegistration.StaffAdministration |
| StudentRegistration.Api | StudentRegistration.Academics, StudentRegistration.Client, StudentRegistration.Contracts, StudentRegistration.IdentityAccess, StudentRegistration.Infrastructure.SqlServer, StudentRegistration.Registration, StudentRegistration.Scheduling, StudentRegistration.StaffAdministration |

The graph is acyclic: Contracts is the stable base; Client and the three
provider modules depend on Contracts; Registration and StaffAdministration
orchestrate approved provider ports; Infrastructure.SqlServer supplies shared
persistence; and Api is the sole composition root.

## Ownership and exposed surfaces

| Module | Owned concerns | Exposed surface |
|---|---|---|
| Client | Blazor WebAssembly UI, route presentation, client-only view state | Static web assets and shared Contracts DTO consumption |
| Api | Host startup and explicit module composition | Composition only; it owns no business handler |
| Contracts | Stable cross-module identifiers, DTO conventions, and AuditWritePort | `StudentRegistration.Contracts` and its bounded subnamespaces |
| IdentityAccess | Accounts, credentials, roles, sessions, and authorization | `Application.Ports` to peers and `Endpoints` to Api |
| Academics | Terms, students, curricula, catalogue, transcript, GPA, holds, and policy | `Application.Ports` to peers and `Endpoints` to Api |
| Scheduling | Offerings, groups, rooms, staff assignments, availability, and meetings | `Application.Ports` to peers and `Endpoints` to Api |
| Registration | Plans, eligibility orchestration, optimizer, submissions, and enrollments | `Application.Ports` to peers and `Endpoints` to Api |
| StaffAdministration | Role-scoped staff queries, delegated commands, audit query/export, and reports | `Application.Ports` to peers and `Endpoints` to Api |
| Infrastructure.SqlServer | The single DbContext, mappings, migrations, SQL adapters, and shared key/audit persistence | Explicit composition and persistence adapters only |

## Enforced rules

- Business-module `Domain`, handler, and other module internals are never a
  cross-module surface. `InternalsVisibleTo` is not used to bypass the rule.
- A business module may consume another business module only through that
  provider's narrow `Application.Ports` contract. Stable shared DTOs and IDs
  belong in Contracts.
- Api is composition-only. It may call module `Endpoints` or composition
  extensions, but business behavior does not live in Api.
- Client references Contracts only and never references a server business
  module or Infrastructure.SqlServer.
- Infrastructure.SqlServer may reference module Domain models only for their
  explicit EF mapping contributions and may implement `Application.Ports`; a
  business module never references Infrastructure.SqlServer.
- All persistence uses the single Infrastructure.SqlServer-owned
  `StudentRegistrationDbContext`; an additional DbContext is not a module seam.
- A source-project edge not listed above, a tenth source project, or a change
  to deployment shape requires an approved ADR and an architecture-test update.

## EC-1: an additional read

When a module needs an additional read from another owner, add a narrow query interface
to the provider's `Application.Ports`. A direct table read, direct EF
entity use, or access to module internals is forbidden. This keeps a future
service or feature additive without creating a speculative service framework.
