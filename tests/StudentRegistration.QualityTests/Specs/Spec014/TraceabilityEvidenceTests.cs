using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class TraceabilityEvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-014-traceability.md";
    private const string TaskLedgerPath =
        "specs/014-registration-capacity-concurrency/tasks.md";

    private static readonly string[] RequiredArtifacts =
    [
        "specs/014-registration-capacity-concurrency/requirements.md",
        "specs/014-registration-capacity-concurrency/concurrency-matrix.md",
        "specs/014-registration-capacity-concurrency/contracts/api.md",
        "specs/014-registration-capacity-concurrency/contracts/routes/STU-06.md",
        "specs/014-registration-capacity-concurrency/contracts/routes/ADM-08.md",
        "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs",
        "src/StudentRegistration.Registration/Application/RegistrationCommandFactory.cs",
        "src/StudentRegistration.Registration/Application/RegistrationConflictMapper.cs",
        "src/StudentRegistration.Registration/Application/RegistrationEndpointService.cs",
        "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationEndpointStore.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-1Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-2Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-3Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-4Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-5Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-6Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-7Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-8Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-9Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-10Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-11Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-12Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec014/AC-13Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-1Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-2Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-3Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-4Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-5Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-6Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-7Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-8Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-9Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/EdgeCases/EC-10Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR01Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR02Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR03Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR04Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR05Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR06Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR07Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR08Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR09Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR10Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR11Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR12Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR13Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR14Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec014/ConcurrencyMatrix/RaceR15Tests.cs",
        "tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint01ContractTests.cs",
        "tests/StudentRegistration.ContractTests/Specs/Spec014/Endpoint02ContractTests.cs",
        "tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint01BehaviorTests.cs",
        "tests/StudentRegistration.ApplicationTests/Specs/Spec014/Endpoint02BehaviorTests.cs",
        "tests/StudentRegistration.SecurityTests/Specs/Spec014/RegistrationEndpointSecurityBoundaryTests.cs",
        "tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationReviewPageFeatureTests.cs",
        "tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationResultPageContributorTests.cs",
        "tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationAdministrationPageContributorTests.cs",
        "tests/StudentRegistration.AccessibilityTests/Routes/RegistrationReviewPageAccessibilityTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-1EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-2EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-3EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-4EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-5EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-6EvidenceTests.cs",
        "docs/release-evidence/SPEC-014-NFR-1.md",
        "docs/release-evidence/SPEC-014-NFR-2.md",
        "docs/release-evidence/SPEC-014-NFR-3.md",
        "docs/release-evidence/SPEC-014-NFR-4.md",
        "docs/release-evidence/SPEC-014-NFR-5.md",
        "docs/release-evidence/SPEC-014-NFR-6.md",
        "docs/release-evidence/SPEC-014-load-results.json",
        "docs/release-evidence/SPEC-014-accessibility.md",
        "docs/release-evidence/SPEC-014-scope-review.md"
    ];

    [Fact]
    public void Every_normative_identifier_and_boundary_is_traced()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var identifiers = Enumerable.Range(1, 18).Select(number => $"FR-{number}")
            .Concat(Enumerable.Range(1, 6).Select(number => $"NFR-{number}"))
            .Concat(Enumerable.Range(1, 3).Select(number => $"SC-{number}"))
            .Concat(Enumerable.Range(1, 13).Select(number => $"AC-{number}"))
            .Concat(Enumerable.Range(1, 10).Select(number => $"EC-{number}"))
            .Concat(Enumerable.Range(1, 15).Select(number => $"Race R{number:00}"))
            .Concat([
                "POST /api/student/terms/{termId}/registrations",
                "GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
                "STU-05",
                "STU-06",
                "ADM-08",
                "Registration.SubmitOwn",
                "100 concurrent submissions",
                "30 active enrollments",
                "WCAG 2.2 AA",
                "OS-1", "OS-2", "OS-3", "OS-4", "OS-5"
            ]);

        Assert.All(
            identifiers,
            identifier => Assert.Contains(identifier, evidence, StringComparison.Ordinal));
    }

    [Fact]
    public void Every_mapped_artifact_exists_and_is_named_in_the_matrix()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        Assert.All(
            RequiredArtifacts,
            artifact =>
            {
                Assert.Contains($"`{artifact}`", evidence, StringComparison.Ordinal);
                Assert.False(string.IsNullOrWhiteSpace(RepositoryFiles.Read(artifact)));
            });
    }

    [Fact]
    public void Nfr_rows_require_passing_documents_and_fresh_machine_evidence()
    {
        foreach (var number in Enumerable.Range(1, 6))
        {
            var document = RepositoryFiles.Read(
                $"docs/release-evidence/SPEC-014-NFR-{number}.md");
            Assert.Matches(
                @"(?im)^\s*\*\*Result:\*\*\s+PASS\b",
                document);
        }

        var measured = Spec014LoadEvidence.ReadCheckedIn();
        Assert.Equal(
            Spec014RegistrationLoadHarness.CalculateSourceFingerprint(),
            measured.SourceFingerprint);
        Assert.Equal(75, measured.Target.ConfiguredSubmissionsPerSecond);
        Assert.Equal(600, measured.Target.DurationSeconds);
        Assert.Equal(45_000, measured.Target.CompletedRequests);
        Assert.InRange(measured.Target.SubmissionP95Milliseconds, 0, 2_000);
        Assert.Equal(200, measured.Spike.ConfiguredSubmissionsPerSecond);
        Assert.Equal(60, measured.Spike.DurationSeconds);
        Assert.Equal(12_000, measured.Spike.CompletedRequests);
        Assert.Equal(2, measured.Spike.ReplicaCount);
        Assert.Equal(0, measured.Target.Invariants.TotalViolations);
        Assert.Equal(0, measured.Spike.Invariants.TotalViolations);
        Assert.Equal(100, measured.Collision.ConcurrentRequests);
        Assert.Equal(30, measured.Collision.GroupCapacity);
        Assert.Equal(30, measured.Collision.AcceptedRequests);
        Assert.Equal(30, measured.Collision.ActiveEnrollments);
        Assert.Equal(30, measured.Collision.FinalEnrolledCount);
        Assert.InRange(measured.Target.UnexpectedFailureRatePercent, 0, 0.099999999);
        Assert.Equal(0, measured.RemoteCallsInsideTransactions);
        Assert.Equal(0, measured.PrivacyViolations);
    }

    [Fact]
    public void Matrix_has_no_placeholders_and_keeps_release_approval_separate()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var normalizedEvidence = Regex.Replace(evidence, @"\s+", " ");

        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER)\b",
            evidence);
        RepositoryFiles.ContainsAll(
            normalizedEvidence,
            "Release is rejected",
            "T123 is not claimed",
            "release approval remains separate",
            "non-production demo",
            "not production authorization");
        Assert.DoesNotContain(
            "SPEC-014-release-approval.md",
            evidence,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Every_task_through_T123_is_checked_and_names_existing_evidence()
    {
        var ledger = RepositoryFiles.Read(TaskLedgerPath);
        var taskLines = ledger.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => Regex.IsMatch(line, @"^- \[[ xX]\] T\d{3}\b"))
            .Where(line => int.Parse(Regex.Match(line, @"T(?<number>\d{3})").Groups["number"].Value) <= 123)
            .ToArray();

        Assert.Equal(123, taskLines.Length);
        foreach (var number in Enumerable.Range(1, 123))
        {
            var prefix = $"- [x] T{number:000}";
            var line = Assert.Single(
                taskLines,
                candidate => candidate.StartsWith(prefix, StringComparison.Ordinal));
            var paths = Regex.Matches(
                    line,
                    @"(?<path>(?:specs|src|tests|docs)/[A-Za-z0-9_./-]+\.(?:cs|md|json|razor|css))")
                .Select(match => match.Groups["path"].Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Assert.NotEmpty(paths);
            Assert.All(
                paths,
                path => Assert.False(
                    string.IsNullOrWhiteSpace(RepositoryFiles.Read(path)),
                    $"{prefix} names missing or empty evidence '{path}'."));
        }

        Assert.DoesNotContain(
            taskLines,
            line => line.StartsWith("- [ ]", StringComparison.Ordinal));
    }

    [Fact]
    public void Release_approval_is_explicit_bounded_and_non_production()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-014-release-approval.md");

        RepositoryFiles.ContainsAll(
            approval,
            "Release decision:** APPROVED",
            "Approver:** Ahmed ELbamby",
            "Product owner | Approved",
            "Domain owner | Approved",
            "QA | Approved",
            "Security | Approved",
            "Accessibility | Approved",
            "Data and concurrency | Approved",
            "Operations | Approved",
            "bounded non-production SPEC-014 demo release only",
            "cannot authorize production deployment",
            "institutional AASTMT");
    }
}
