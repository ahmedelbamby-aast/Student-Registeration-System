# Unique and Alternate-Key Invariants

**Contract version:** `unique-invariants/1.0`<br>
**Requirement:** FR-2<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

This catalogue defines the uniqueness shape that canonical owner
specifications must implement. It is not an EF mapping, migration, database
inspection result, or production approval.

## Identity, catalogue, and term keys

- Unique filtered normalized `ApplicationUser.UniversityId` for student
  identities; a null staff value is outside that filtered key.
- `Staff.StaffNumber`, `CatalogueVersion.VersionCode`, `AcademicTerm.Code`,
  `AcademicTerm.CreationClientRequestId`, and `Room.Code` are unique. The
  globally unique term-creation request ID is bound to the canonical create
  payload through required `AcademicTerm.CreationPayloadHash`; a replay with a
  different hash is rejected rather than creating or changing a term.
- Unique `Program(CatalogueVersionId, Code)` and
  `Course(CatalogueVersionId, Code)` preserve version-scoped codes.
- `RoleAssignment(UserId, RoleCode, EffectiveFromUtc)`,
  `StudentActivation(UserId)`, `AccountRecoveryChallenge(TokenHash)`,
  `AuthenticationAbuseState(SubjectKeyHash)`, and
  `IdentityImportBatch(SourceHash)` are unique.
- Composite keys for curriculum, prerequisite, and staff assignment bridges
  prevent duplicate membership rows.

## Scheduling and registration keys

- Unique `CourseOffering(TermId, CourseId)`.
- Unique `SectionGroup(OfferingId, GroupCode)`.
- Alternate key `SectionGroup(Id, OfferingId)` is the relationship target that
  binds a group to its offering.
- Unique `Enrollment(StudentId, OfferingId)`. Re-registration changes the same
  logical Enrollment row state; it does not insert a second logical enrollment.
- Unique `RegistrationSubmission(StudentId, TermId, ClientRequestId)` binds an
  idempotency key to one student and term.
- `RegistrationSubmission.Reference` is unique when non-null and accepted;
  rejected and processing results do not invent a receipt reference.
- Unique `StudentTermRegistrationGuard(StudentId, TermId)` supplies one
  serialization boundary per student and term.
- `StudentTermAcademicState(StudentId, TermId)`,
  `StaffTermAvailability(StaffId, TermId)`, and
  `RegistrationPlanItem(PlanId, OfferingId)` are unique.
- `TranscriptAttempt.SupersedesAttemptId` has a filtered unique index when
  non-null, so an immutable attempt can have at most one direct successor.

No seventh SPEC-008 entity or generic idempotency table is introduced.
Term creation uses the two AcademicTerm creation fields above. Term/window
publication and academic-profile corrections instead require their governed
`expectedRowVersion` tokens.

Canonical owner specifications implement these persistence mappings and future
real-SQL tests verify the resulting indexes, alternate keys, and duplicate-key
failure mapping. Until then, this document is conformance intent only.
