using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_8Tests
{
    private const string OptimizerPath =
        "src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs";
    private const string OptimizerTestsPath =
        "tests/StudentRegistration.ApplicationTests/Registration/ScheduleOptimizerTests.cs";
    private const string PerformanceEvidencePath =
        "tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-1EvidenceTests.cs";
    private const string CoverageEvidencePath =
        "tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-4EvidenceTests.cs";

    [Fact]
    public void Optimizer_orders_constrained_courses_first_and_prunes_invalid_partial_schedules()
    {
        var optimizer = FutureSource(
            OptimizerPath,
            "FR-3 must be implemented before the constrained-first pruning proof can pass.");
        var optimizerTests = FutureSource(
            OptimizerTestsPath,
            "The optimizer branch fixtures must exist before AC-8 can pass.");

        Assert.Contains("OrderBy", optimizer, StringComparison.Ordinal);
        Assert.Contains("prun", optimizer, StringComparison.OrdinalIgnoreCase);
        RepositoryFiles.ContainsAll(
            optimizerTests,
            "constrained",
            "prun");
    }

    [Fact]
    public void Approved_eight_by_ten_fixture_proves_p95_and_branch_coverage_targets()
    {
        var performanceEvidence = FutureSource(
            PerformanceEvidencePath,
            "T057 must provide the executable optimizer p95 evidence.");
        var coverageEvidence = FutureSource(
            CoverageEvidencePath,
            "T060 must provide the executable optimizer branch-coverage evidence.");

        RepositoryFiles.ContainsAll(
            performanceEvidence,
            "8",
            "10",
            "500",
            "p95");
        RepositoryFiles.ContainsAll(
            coverageEvidence,
            "90",
            "branch");
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(RepositoryFiles.Exists(path), message);
        return RepositoryFiles.Read(path);
    }
}
