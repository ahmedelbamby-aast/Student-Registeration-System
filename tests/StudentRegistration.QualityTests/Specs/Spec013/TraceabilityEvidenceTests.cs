using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class TraceabilityEvidenceTests
{
    private const string TracePath =
        "docs/release-evidence/SPEC-013-traceability.md";
    private const string ApprovalPath =
        "docs/release-evidence/SPEC-013-release-approval.md";

    [Fact]
    public void Every_normative_identifier_and_release_boundary_is_traced()
    {
        var trace = RepositoryFiles.Read(TracePath);
        foreach (var identifier in Enumerable.Range(1, 10).Select(value => $"FR-{value}")
            .Concat(Enumerable.Range(1, 4).Select(value => $"NFR-{value}"))
            .Concat(Enumerable.Range(1, 3).Select(value => $"SC-{value}"))
            .Concat(Enumerable.Range(1, 8).Select(value => $"AC-{value}"))
            .Concat(Enumerable.Range(1, 5).Select(value => $"EC-{value}"))
            .Append("STU-04"))
        {
            Assert.Contains(identifier, trace, StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            trace,
            "NFR-1EvidenceTests.cs",
            "NFR-2EvidenceTests.cs",
            "NFR-3EvidenceTests.cs",
            "NFR-4EvidenceTests.cs",
            "SPEC-013-scope-review.md",
            "SPEC-013-release-approval.md",
            "Release is rejected");
    }

    [Fact]
    public void Approval_records_every_required_perspective_and_demo_limit()
    {
        var approval = string.Join(
            " ",
            RepositoryFiles.Read(ApprovalPath).Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries));
        RepositoryFiles.ContainsAll(
            approval,
            "Product owner",
            "Domain owner",
            "QA",
            "Security",
            "Accessibility",
            "Data and concurrency",
            "Operations",
            "Ahmed ELbamby",
            "non-production demo",
            "not production approval");
    }
}
