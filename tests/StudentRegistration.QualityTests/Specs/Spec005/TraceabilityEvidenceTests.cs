using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class TraceabilityEvidenceTests
{
    [Fact]
    public void Matrix_covers_every_requirement_acceptance_edge_success_and_route_row()
    {
        var matrix = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-traceability.md");
        RepositoryFiles.ContainsAll(
            matrix,
            Enumerable.Range(1, 9).Select(value => $"FR-{value}").ToArray());
        RepositoryFiles.ContainsAll(
            matrix,
            Enumerable.Range(1, 4).Select(value => $"NFR-{value}").ToArray());
        RepositoryFiles.ContainsAll(
            matrix,
            Enumerable.Range(1, 7).Select(value => $"AC-{value}").ToArray());
        RepositoryFiles.ContainsAll(
            matrix,
            Enumerable.Range(1, 4).Select(value => $"EC-{value}").ToArray());
        RepositoryFiles.ContainsAll(
            matrix,
            "SC-1",
            "SC-2",
            "SC-3",
            "Frontend route ownership",
            "PASS / NOT APPLICABLE",
            "**Overall result: PASS.**");
    }

    [Fact]
    public void No_spec005_fixture_is_skipped_deferred_or_unimplemented()
    {
        var spec005Files = Directory.EnumerateFiles(
                RepositoryFiles.PathTo("tests"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains(
                "/Spec005/",
                StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith(
                nameof(TraceabilityEvidenceTests) + ".cs",
                StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(spec005Files);
        foreach (var path in spec005Files)
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotContain("Skip =", source, StringComparison.Ordinal);
            Assert.DoesNotContain("NotImplementedException", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DeferredReason", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Approval_records_every_applicable_review_perspective_and_scope_limit()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "Product owner",
            "Domain owner / Registrar-policy",
            "QA",
            "Security",
            "Accessibility",
            "Data / concurrency",
            "Operations",
            "APPROVED FOR NON-PRODUCTION POC",
            "not official AASTMT",
            "Production authorization remains fail closed",
            "SPEC005-SCAN-20260719",
            "2026-08-02",
            "**Result: APPROVED (non-production demo only).**");
    }
}
