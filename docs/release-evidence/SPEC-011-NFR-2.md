# SPEC-011 NFR-2 Query Safety Evidence

## Requirement and mapping

This is the canonical evidence for SPEC-011 NFR-2, release alias NFR-006,
and T046. Search input must be bounded, NFKC-normalized, trimmed, and treated
as literal data without ever being interpolated into SQL.

## Executable evidence

`NFR-2EvidenceTests` proves all of the following:

- exactly 100 characters are accepted and 101 characters return
  `VALIDATION_ERROR` before evaluation;
- page size 101 returns `PAGE_SIZE_INVALID`, keeping result payloads bounded;
- the SQL-metacharacter string `DS413%' OR 1=1 --` is matched as literal
  course text in the evaluated in-memory projection and never enters a SQL
  command, so it cannot broaden the result set;
- the adapter's term-scoped root translates to a declared `@termId`
  parameter and predicate `[c].[TermId] = @termId`;
- the authenticated-student root translates to a declared
  `@applicationUserId` parameter and predicate
  `[s].[ApplicationUserId] = @applicationUserId`;
- both SQL query roots contain the EF Core `AsNoTracking` operator even when
  the context default is deliberately set to `TrackAll`; and
- requested course IDs are distinct and capped with `Take(100)`, while group,
  meeting, and staff reads are restricted to IDs obtained from the bounded
  parent query.

The SQL assertions use EF Core `ToQueryString()` against the SQL Server
provider. No database connection or string interpolation is involved in this
translation proof.

## Adapter and persistence evidence

T043 and T044 are now delivered at:

- `tests/StudentRegistration.IntegrationTests/Specs/Spec011/RegistrationDiscoveryModelConfigurationTests.cs`; and
- `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationDiscoveryQueryAdapter.cs`.

The focused integration suite independently verifies the read-only/keyless
model, required indexes, narrow read-port registrations, parameterized
predicates, and no-tracking behavior.

**Result: PASS.**
