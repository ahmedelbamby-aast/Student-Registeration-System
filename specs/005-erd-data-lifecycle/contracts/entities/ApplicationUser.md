# ApplicationUser ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### ApplicationUser

- Canonical entity: ApplicationUser
- Canonical owner: SPEC-007
- Canonical source path: `src/StudentRegistration.IdentityAccess/Domain/ApplicationUser.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `string UserName UK`
- `string NormalizedUserName UK`
- `string UniversityId UK`
- `string PasswordHash`
- `string SecurityStamp`
- `bool IsEnabled`
- `int AccessFailedCount`
- `datetime2 LockoutEndUtc`
- `rowversion Version`

#### Relationships and invariants

- `APPLICATION_USER ||--o| STUDENT : has_academic_profile`
- `APPLICATION_USER ||--o| STAFF : represents`
- ApplicationUser owns role assignments, recovery challenges, security events, identity imports, and activation. SPEC-017-owned ExportJob may reference the user without transferring ownership.
- Unique filtered normalized ApplicationUser.UniversityId for student identities.
- Passwords and generated PINs are persisted only as ASP.NET Core Identity hashes; plaintext credentials are prohibited.
- SecurityStamp, access-failure state, and lockout state are server-only
  security internals and never appear in a public DTO.
- Status and effective-role mutations advance ApplicationUser.Version as the
  one public aggregate concurrency token; internal RoleAssignment rowversions
  remain persistence safeguards.
- `rowversion on mutable aggregate roots and admin records` protects mutable identity state.

#### Ownership boundary

This is a design-time reference only. SPEC-007 alone may implement or change the runtime entity and its EF mapping.
