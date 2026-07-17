using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec016;

public sealed class ReleaseEvidenceTests
{
    [Fact]
    public void Scope_review_keeps_all_four_exclusions_out_of_runtime_routes()
    {
        var review = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-016-scope-review.md");
        RepositoryFiles.ContainsAll(review, "OS-1", "OS-2", "OS-3", "OS-4", "PASS");
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs");
        Assert.DoesNotContain("/api/admin", endpoints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MapPost", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPatch", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapDelete", endpoints, StringComparison.Ordinal);
    }

    [Fact]
    public void Traceability_names_every_normative_identifier_and_route()
    {
        var trace = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-016-traceability.md");
        foreach (var prefix in new[] { "FR-", "AC-", "EC-", "NFR-", "SC-" })
        {
            var maximum = prefix switch
            {
                "FR-" => 10,
                "AC-" => 8,
                "EC-" => 5,
                "NFR-" => 4,
                _ => 3
            };
            for (var number = 1; number <= maximum; number++)
            {
                Assert.Contains($"{prefix}{number}", trace, StringComparison.Ordinal);
            }
        }
        RepositoryFiles.ContainsAll(trace, "STF-01", "STF-02", "STF-03", "STF-04", "Result:** PASS");
    }

    [Fact]
    public void Demo_approval_records_each_required_review_perspective_and_non_claim()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-016-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "Product owner",
            "Domain owner",
            "QA",
            "Security/privacy",
            "Accessibility",
            "Data/concurrency",
            "Operations",
            "not Gate D",
            "APPROVED");
    }
}
