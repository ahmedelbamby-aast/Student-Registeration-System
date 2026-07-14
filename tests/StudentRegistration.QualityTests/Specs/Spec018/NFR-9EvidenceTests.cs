using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr9EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-018-NFR-9.md";

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
    public void Versioned_evidence_is_pending_and_never_substitutes_coverage_for_behavior()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-018 NFR-9 Release Evidence",
            "**Artifact version:** 1.0.0",
            "**Requirement:** NFR-9",
            "**Release result:** PENDING",
            "Coverage measurement result: NOT EXECUTED",
            ">= 90% branch coverage",
            "Eligibility",
            "conflict",
            "capacity",
            "coverage never replaces behavior tests",
            "boundary, authorization, real-SQL concurrency, and invariant tests",
            "Runtime execution result: PENDING",
            "Activation condition");
        Assert.DoesNotContain("Runtime execution result: PASS", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip =
        "Activation condition: SPEC-011 eligibility, SPEC-012 conflict, and SPEC-014 capacity implementations plus their behavior suites must exist before branch coverage can be collected and evaluated.")]
    public void Downstream_rule_owners_reach_90_percent_branch_coverage_with_behavior_tests_passing()
    {
    }

    private static bool CoveragePasses(int coveredBranches, int totalBranches) =>
        coveredBranches >= 0 &&
        totalBranches > 0 &&
        coveredBranches <= totalBranches &&
        ((decimal)coveredBranches / totalBranches) >= 0.90m;
}
