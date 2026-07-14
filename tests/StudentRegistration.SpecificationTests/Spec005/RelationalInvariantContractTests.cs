using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec005;

public sealed class RelationalInvariantContractTests
{
    private const string UniquePath = "specs/005-erd-data-lifecycle/contracts/unique-invariants.md";
    private const string CheckPath = "specs/005-erd-data-lifecycle/contracts/check-constraints.md";
    private const string ConcurrencyPath = "specs/005-erd-data-lifecycle/contracts/concurrency-tokens.md";
    private const string HistoryPath = "specs/005-erd-data-lifecycle/contracts/immutable-history.md";
    private const string SummaryPath = "docs/data/relational-invariants.md";

    [Fact]
    public void Fr2_contract_captures_uniqueness_and_alternate_keys()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(UniquePath);
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "unique-invariants/1.0",
            "FR-2",
            "Staff.StaffNumber", "CatalogueVersion.VersionCode",
            "AcademicTerm.Code", "Room.Code",
            "RoleAssignment(UserId, RoleCode, EffectiveFromUtc)",
            "StudentActivation(UserId)", "AccountRecoveryChallenge(TokenHash)",
            "AuthenticationAbuseState(SubjectKeyHash)", "IdentityImportBatch(SourceHash)",
            "RegistrationSubmission.Reference is unique when non-null and accepted",
            "StudentTermAcademicState(StudentId, TermId)",
            "StaffTermAvailability(StaffId, TermId)",
            "RegistrationPlanItem(PlanId, OfferingId)",
            "Composite keys for curriculum, prerequisite, and staff assignment bridges",
            "Re-registration changes the same logical Enrollment row state");
        Spec005ContractTestSupport.AssertMirrorsErd(
            contract,
            "Unique filtered normalized ApplicationUser.UniversityId",
            "Unique Program(CatalogueVersionId, Code)",
            "Course(CatalogueVersionId, Code)",
            "Unique CourseOffering(TermId, CourseId)",
            "Unique SectionGroup(OfferingId, GroupCode)",
            "Alternate key SectionGroup(Id, OfferingId)",
            "Unique Enrollment(StudentId, OfferingId)",
            "Unique RegistrationSubmission(StudentId, TermId, ClientRequestId)",
            "Unique StudentTermRegistrationGuard(StudentId, TermId)");
    }

    [Fact]
    public void Fr3_contract_captures_capacity_and_temporal_checks()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(CheckPath);
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "check-constraints/1.0",
            "FR-3",
            "SectionGroup.Capacity >= 0",
            "0 <= SectionGroup.EnrolledCount <= SectionGroup.Capacity",
            "MeetingSlot.EndLocal > MeetingSlot.StartLocal",
            "StaffAvailability.EndLocal > StaffAvailability.StartLocal",
            "AcademicTerm.TeachingEndsOn > AcademicTerm.TeachingStartsOn",
            "RegistrationWindow.ClosesAtUtc > RegistrationWindow.OpensAtUtc",
            "StudentHold.EffectiveToUtc IS NULL OR StudentHold.EffectiveToUtc > StudentHold.EffectiveFromUtc",
            "Reducing capacity below EnrolledCount is rejected",
            "RegistrationPaused = false is an atomic allocation predicate, not a check constraint",
            "Invalid capacity, temporal, or normalized scope values are rejected, never clamped or auto-corrected");
        Spec005ContractTestSupport.AssertMirrorsErd(
            contract,
            "Check Capacity >= 0 and 0 <= EnrolledCount <= Capacity",
            "SectionGroup.RegistrationPaused = false",
            "Check EndLocal > StartLocal",
            "SQL constraints cannot express arbitrary overlapping time ranges",
            "Scheduling separately validates meeting, staff, and room overlaps before");
    }

    [Fact]
    public void Fr4_contract_catalogues_every_erd_rowversion_boundary()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(ConcurrencyPath);
        var erd = RepositoryFiles.Read("docs/diagrams/ERD.md");
        var rowVersionEntities = Regex.Matches(
                erd,
                @"(?ms)^\s{2}(?<entity>[A-Z][A-Z0-9_]*)\s+\{\r?\n(?<body>.*?)^\s{2}\}\s*$")
            .Where(match => match.Groups["body"].Value.Contains("rowversion", StringComparison.Ordinal))
            .Select(match => match.Groups["entity"].Value)
            .ToArray();

        Assert.NotEmpty(rowVersionEntities);
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "concurrency-tokens/1.0",
            "FR-4",
            "409 STALE_VERSION", "current version", "no lost update",
            "ExportJob uses compare-and-set rowversion plus an expiring lease",
            "Final-Admin mutation locks AdminSecurityGuard",
            "rowversion is not the capacity allocator",
            "rowversion is not the sole student-term serialization mechanism");
        foreach (var entity in rowVersionEntities)
        {
            Spec005ContractTestSupport.AssertContainsNormalized(
                contract,
                $"{entity}: rowversion");
        }

        Spec005ContractTestSupport.AssertMirrorsErd(
            contract,
            "rowversion on mutable aggregate roots and admin records",
            "Every group-state, MeetingSlot, room, and GroupStaffAssignment mutation locks and advances its owning SectionGroup.Version",
            "Every availability range replacement locks and advances the owning StaffTermAvailability.Version");
    }

    [Fact]
    public void Fr5_contract_preserves_historical_meaning_by_append_or_supersession()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(HistoryPath);
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "immutable-history/1.0",
            "FR-5",
            "Historical correction uses append or supersession; in-place rewriting is prohibited",
            "CatalogueDraft is mutable only before publication",
            "RegistrationSubmission final result and snapshots remain replayable",
            "RegistrationReceipt is a projection, not a second table or write path",
            "Testing disposal, Development guarded reset, and seven-day local artifact cleanup do not imply production retention",
            "Hard deletion and retention for real or production data remain unapproved and fail closed");
        Spec005ContractTestSupport.AssertMirrorsErd(
            contract,
            "No transcript attempt is overwritten",
            "Enrollments retain successful registration history",
            "Published policy sets are immutable and superseded",
            "Published catalogue versions are immutable",
            "Decision snapshots retain the exact rule version and input summary used",
            "Audit events are append-only");
    }

    [Fact]
    public void Fr6_summary_binds_all_invariant_contracts_to_owner_specs()
    {
        var summary = Spec005ContractTestSupport.ReadBoundedContract(SummaryPath);
        Spec005ContractTestSupport.AssertWorkstream(
            "Relational invariants",
            SummaryPath,
            "tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs",
            "FR-2", "FR-3", "FR-4", "FR-5", "FR-6");

        Spec005ContractTestSupport.AssertContainsNormalized(
            summary,
            "FR-2", "FR-3", "FR-4", "FR-5", "FR-6",
            "relational-invariants/1.0",
            UniquePath, CheckPath, ConcurrencyPath, HistoryPath,
            "Owner specifications implement persistence mappings",
            "SPEC-005 owns conformance, not downstream runtime models",
            "Enrollment(GroupId, OfferingId) -> SectionGroup(Id, OfferingId)",
            "same-catalogue-version composite foreign keys",
            "Database constraint", "Transactional application validation",
            "Future real-SQL release evidence",
            "Runtime model, mapping, migration, and SQL verification remain deferred");
        Spec005ContractTestSupport.AssertMirrorsErd(
            summary,
            "Alternate key SectionGroup(Id, OfferingId), referenced by Enrollment, so a group cannot be paired with another offering");
    }
}
