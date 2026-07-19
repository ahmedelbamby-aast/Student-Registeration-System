using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr9EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-018-NFR-9.md";
    private const string MeasurementPath =
        "docs/release-evidence/SPEC-018-NFR-9-coverage.json";

    [Theory]
    [InlineData(90, 100, true)]
    [InlineData(9, 10, true)]
    [InlineData(89, 100, false)]
    [InlineData(0, 0, false)]
    [InlineData(101, 100, false)]
    public void Branch_coverage_gate_requires_at_least_90_percent(
        int coveredBranches,
        int totalBranches,
        bool expected)
    {
        Assert.Equal(expected, CoveragePasses(coveredBranches, totalBranches));
    }

    [Fact]
    public void Versioned_evidence_passes_and_never_substitutes_coverage_for_behavior()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-018 NFR-9 Release Evidence",
            "**Artifact version:** 1.0.0",
            "**Requirement:** NFR-9",
            "**Release result:** PASS",
            "Coverage measurement result: EXECUTED AND PASSED",
            ">= 90% branch coverage",
            "Eligibility",
            "conflict",
            "capacity",
            "coverage never replaces behavior tests",
            "boundary, authorization, real-SQL concurrency, and invariant tests",
            "Runtime execution result: PASS");
        Assert.DoesNotContain("Runtime execution result: PENDING", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Measured_coverage_and_behavior_suite_pass_for_each_manifest_owner()
    {
        using var evidence = JsonDocument.Parse(RepositoryFiles.Read(MeasurementPath));
        var root = evidence.RootElement;

        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal(133, root.GetProperty("behaviorTestsPassed").GetInt32());
        Assert.Equal(0, root.GetProperty("behaviorTestsFailed").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("coverageReportSha256").GetString()));
        Assert.Equal(0.90m, root.GetProperty("minimumRequiredBranchRate").GetDecimal());
        Assert.True(root.GetProperty("coverageNeverReplacesBehaviorTests").GetBoolean());
        Assert.All(root.GetProperty("owners").EnumerateArray(), owner =>
        {
            var covered = owner.GetProperty("coveredBranches").GetInt32();
            var total = owner.GetProperty("totalBranches").GetInt32();
            Assert.True(CoveragePasses(covered, total));
            Assert.True(owner.GetProperty("branchRate").GetDecimal() >= 0.90m);
        });
    }

    private static bool CoveragePasses(int coveredBranches, int totalBranches) =>
        coveredBranches >= 0 &&
        totalBranches > 0 &&
        coveredBranches <= totalBranches &&
        ((decimal)coveredBranches / totalBranches) >= 0.90m;
}
