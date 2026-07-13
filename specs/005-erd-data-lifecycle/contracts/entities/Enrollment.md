# Enrollment ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### Enrollment

- Canonical entity: Enrollment
- Canonical owner: SPEC-014
- Canonical source path: `src/StudentRegistration.Registration/Domain/Enrollment.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs` (`writable-transactional`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier OfferingId FK`
- `uniqueidentifier GroupId FK`
- `uniqueidentifier SubmissionId FK`
- `string State`
- `datetime2 RegisteredAtUtc`
- `rowversion Version`

#### Relationships and invariants

- Enrollment belongs to Student, CourseOffering, and RegistrationSubmission.
- `SECTION_GROUP ||--o{ ENROLLMENT : allocates`
- `Alternate key SectionGroup(Id, OfferingId), referenced by Enrollment` prevents a group from a different offering being selected.
- `Unique Enrollment(StudentId, OfferingId)`; re-registration changes the state of the same logical record.
- Active group/state indexing supports capacity checks, and rowversion protects mutable enrollment state.
- `Enrollments retain successful registration history`; drop, withdrawal, and correction require separately approved workflows.
- Seat counters and enrollment rows commit atomically with the accepted submission.

#### Ownership boundary

This is a design-time reference only. SPEC-014 alone may implement or change the runtime entity and its EF mapping.
