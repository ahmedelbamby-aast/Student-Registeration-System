using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class TraceabilityEvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-012-traceability.md";

    [Fact]
    public void Evidence_contains_every_required_trace_identifier()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var identifiers = Enumerable.Range(1, 8).Select(number => $"FR-{number}")
            .Concat(Enumerable.Range(1, 4).Select(number => $"NFR-{number}"))
            .Concat(Enumerable.Range(1, 5).Select(number => $"AC-{number}"))
            .Concat(Enumerable.Range(1, 4).Select(number => $"EC-{number}"))
            .Concat(Enumerable.Range(1, 3).Select(number => $"SC-{number}"))
            .Concat(["STU-04", "STU-05"])
            .Concat([
                "ENTITY-RegistrationPlan",
                "ENTITY-RegistrationPlanItem",
                "ENTITY-ScheduleConflict",
                "ENTITY-ValidationSnapshot",
                "API-Endpoint01",
                "API-Endpoint02",
                "API-Endpoint03"
            ])
            .Concat(Enumerable.Range(1, 53).Select(number => $"T{number:000}"));

        Assert.All(
            identifiers,
            identifier => Assert.Contains(identifier, evidence, StringComparison.Ordinal));
    }

    [Fact]
    public void Every_mapped_artifact_exists_and_is_named_in_the_evidence()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        string[] artifacts =
        [
            "specs/012-schedule-builder-conflicts/spec.md",
            "specs/012-schedule-builder-conflicts/requirements.md",
            "specs/012-schedule-builder-conflicts/tasks.md",
            "specs/012-schedule-builder-conflicts/dependency-baseline.md",
            "specs/012-schedule-builder-conflicts/contracts/api.md",
            "specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md",
            "src/StudentRegistration.Registration/Domain/RegistrationPlan.cs",
            "src/StudentRegistration.Registration/Domain/RegistrationPlanItem.cs",
            "src/StudentRegistration.Registration/Domain/ScheduleConflict.cs",
            "src/StudentRegistration.Registration/Domain/ValidationSnapshot.cs",
            "src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs",
            "src/StudentRegistration.Registration/Application/RegistrationPlanService.cs",
            "src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs",
            "src/StudentRegistration.Contracts/Registration/RegistrationPlanContracts.cs",
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs",
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationPlanSqlServerAdapter.cs",
            "src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs",
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-2Tests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-3Tests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-5Tests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-1OutcomeTests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-2OutcomeTests.cs",
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-3OutcomeTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-1Tests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-2Tests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-3Tests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-4Tests.cs",
            "tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs",
            "tests/StudentRegistration.ApplicationTests/Registration/ScheduleConflictDetectorTests.cs",
            "tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs",
            "tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint01ContractTests.cs",
            "tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint02ContractTests.cs",
            "tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint03ContractTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec012/RegistrationReviewPageContributorTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanItemModelTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/ScheduleConflictModelTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/ValidationSnapshotModelTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs",
            "tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-1EvidenceTests.cs",
            "tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-2EvidenceTests.cs",
            "tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-3EvidenceTests.cs",
            "tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-4EvidenceTests.cs",
            "docs/release-evidence/SPEC-012-NFR-1.md",
            "docs/release-evidence/SPEC-012-NFR-2.md",
            "docs/release-evidence/SPEC-012-NFR-3.md",
            "docs/release-evidence/SPEC-012-NFR-4.md",
            "docs/release-evidence/SPEC-012-scope-review.md"
        ];

        Assert.All(
            artifacts,
            artifact =>
            {
                Assert.Contains($"`{artifact}`", evidence, StringComparison.Ordinal);
                Assert.False(string.IsNullOrWhiteSpace(RepositoryFiles.Read(artifact)));
            });
    }

    [Fact]
    public void Evidence_has_no_placeholders_and_does_not_claim_release_approval()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER)\b",
            evidence);
        RepositoryFiles.ContainsAll(
            evidence,
            "T054 is not claimed",
            "Gate B-D",
            "release approval remains separate");
        Assert.DoesNotContain(
            "SPEC-012-release-approval.md",
            evidence,
            StringComparison.Ordinal);
    }
}
