using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class AC_5Tests
{
    [Fact]
    public void Release_candidate_meets_the_product_quality_boundary()
    {
        foreach (var evidencePath in new[]
                 {
                     "docs/release-evidence/SPEC-001-NFR-1.md",
                     "docs/release-evidence/SPEC-001-NFR-2.md",
                     "docs/release-evidence/SPEC-001-NFR-3.md",
                     "docs/release-evidence/SPEC-001-NFR-4.md"
                 })
        {
            Assert.Contains(
                "PASS",
                RepositoryFiles.Read(evidencePath),
                StringComparison.Ordinal);
        }

        using var accessibility = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-8-browser-matrix.json"));
        Assert.Equal("PASS", accessibility.RootElement.GetProperty("result").GetString());
        Assert.Equal(
            accessibility.RootElement.GetProperty("browserRouteCombinations").GetInt32(),
            accessibility.RootElement.GetProperty("passedCombinations").GetInt32());
        Assert.Equal(
            0,
            accessibility.RootElement.GetProperty("failedCombinations").GetInt32());

        using var load = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-load-results.json"));
        Assert.Equal(
            0,
            load.RootElement.GetProperty("target")
                .GetProperty("unexpectedFailures").GetInt32());
        Assert.Equal(
            0,
            load.RootElement.GetProperty("target")
                .GetProperty("invariants")
                .GetProperty("totalViolations").GetInt32());
    }
}
