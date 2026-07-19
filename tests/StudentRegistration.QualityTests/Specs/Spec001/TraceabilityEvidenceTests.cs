using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec001;

public sealed class TraceabilityEvidenceTests
{
    private static readonly string[] ExpectedRows =
    [
        "FR-1", "FR-2", "FR-3", "FR-4", "FR-5", "FR-6", "FR-7",
        "NFR-1", "NFR-2", "NFR-3", "NFR-4",
        "AC-1", "AC-2", "AC-3", "AC-4", "AC-5",
        "EC-1", "EC-2", "EC-3",
        "SC-1", "SC-2", "SC-3",
        "ROUTE-BOUNDARY"
    ];

    [Fact]
    public void Every_required_traceability_row_has_passing_executable_evidence()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-traceability.md");
        var rows = Regex.Matches(
                evidence,
                @"(?m)^\|\s*(?<id>(?:N?FR|AC|EC|SC)-\d+|ROUTE-BOUNDARY)\s*\|.*\|\s*PASS\s*\|\s*$")
            .Select(match => match.Groups["id"].Value)
            .ToArray();

        Assert.Equal(ExpectedRows.Order(StringComparer.Ordinal), rows.Order(StringComparer.Ordinal));
        Assert.DoesNotContain("| FAIL |", evidence, StringComparison.Ordinal);
        Assert.DoesNotContain("| PENDING |", evidence, StringComparison.Ordinal);
        Assert.DoesNotContain("| BLOCKED |", evidence, StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(
            Normalize(evidence),
            "missing row or non-PASS row rejects SPEC-001 release",
            "SPEC-001-release-approval.md",
            "broader system release remains independently governed by SPEC-018");
    }

    [Fact]
    public void All_applicable_review_perspectives_are_approved_by_the_demo_authority()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "**Status:** APPROVED",
            "**Decision authority:** Ahmed ELbamby",
            "| Product Owner |",
            "| Domain owner |",
            "| QA |",
            "| Security |",
            "| Accessibility |",
            "| Data / concurrency |",
            "| Operations |",
            "productionAuthorized: false",
            "officialAastmtApproval: false");

        var approvalRows = Regex.Matches(
            approval,
            @"(?m)^\|\s*(?:Product Owner|Domain owner|QA|Security|Accessibility|Data / concurrency|Operations)\s*\|.*\|\s*APPROVED(?: at SPEC-001 scope)?\s*\|\s*$");
        Assert.Equal(7, approvalRows.Count);
    }

    private static string Normalize(string value) => string.Join(
        " ",
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
