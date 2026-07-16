using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class ReleaseEvidenceTests
{
    [Fact]
    public void Scope_review_records_and_checks_all_four_exclusions()
    {
        var evidence = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-scope-review.md");
        var registrationSource = ReadSourceTree(
            "src/StudentRegistration.Registration");
        var featureSource = ReadSourceTree(
            "src/StudentRegistration.Client/Features/Registration");

        RepositoryFiles.ContainsAll(
            evidence,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "No recommendations",
            "No cross-college or cross-term search",
            "No client-authoritative eligibility",
            "No advisor approval workflow",
            "**Result: PASS.**");

        var combined = registrationSource + featureSource;
        Assert.DoesNotContain("RecommendationService", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("CrossCollegeSearch", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ClientEligibilityEvaluator", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("AdvisorApprovalWorkflow", combined, StringComparison.Ordinal);
    }

    [Fact]
    public void Traceability_contains_the_complete_spec011_inventory()
    {
        var trace = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-traceability.md");

        AssertInventory(trace, "FR", 8);
        AssertInventory(trace, "NFR", 4);
        AssertInventory(trace, "AC", 5);
        AssertInventory(trace, "EC", 4);
        AssertInventory(trace, "SC", 3);

        foreach (var route in new[] { "STU-02", "STU-03" })
        {
            Assert.Contains($"| {route} |", trace, StringComparison.Ordinal);
        }

        foreach (var endpoint in new[] { "01", "02" })
        {
            Assert.Contains($"| Endpoint {endpoint} |", trace, StringComparison.Ordinal);
        }

        foreach (var entity in new[]
                 {
                     "OfferingEligibility",
                     "EligibilityReason",
                     "GroupSummary"
                 })
        {
            Assert.Contains($"`{entity}`", trace, StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            trace,
            "NFR-001",
            "NFR-006",
            "NFR-004",
            "NFR-005",
            "T045",
            "T046",
            "T047",
            "T048",
            "T049",
            "T050",
            "T051",
            "**Result: PASS — complete SPEC-011 inventory and evidence mapping.**");
    }

    private static string ReadSourceTree(string relativeDirectory) =>
        string.Join(
            "\n",
            Directory.EnumerateFiles(
                    RepositoryFiles.PathTo(relativeDirectory),
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(path =>
                    path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));

    private static void AssertInventory(string trace, string prefix, int count)
    {
        for (var number = 1; number <= count; number++)
        {
            Assert.Contains(
                $"| {prefix}-{number} |",
                trace,
                StringComparison.Ordinal);
        }
    }
}
