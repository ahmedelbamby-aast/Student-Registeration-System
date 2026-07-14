using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class TraceabilityEvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-007-traceability.md";

    private static readonly string[] RequiredIds =
    [
        .. Enumerable.Range(1, 14).Select(number => $"FR-{number}"),
        .. Enumerable.Range(1, 4).Select(number => $"NFR-{number}"),
        .. Enumerable.Range(1, 10).Select(number => $"AC-{number}"),
        .. Enumerable.Range(1, 6).Select(number => $"EC-{number}"),
        .. Enumerable.Range(1, 3).Select(number => $"SC-{number}"),
        "AUTH-02", "AUTH-03", "AUTH-04", "AUTH-05", "STU-08", "ADM-03", "SYS-01",
        "ApplicationUser", "Staff", "StudentActivation", "AccountRecoveryChallenge",
        "RoleAssignment", "AuthenticationAbuseState", "IdentityImportBatch",
        "IdentityImportCandidateRow", "SecurityEvent", "AdminSecurityGuard",
        .. Enumerable.Range(1, 16).Select(number => $"Endpoint{number:00}")
    ];

    private static readonly Regex EvidenceLink = new(
        @"\[(?<class>[A-Za-z_][A-Za-z0-9_]*)\]\((?<path>\.\./\.\./tests/[^)]+\.cs)\)",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Matrix_is_complete_unique_and_links_existing_test_classes()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var rows = ParseRows(evidence);

        Assert.Equal(RequiredIds.Length, rows.Length);
        Assert.Equal(rows.Length, rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.True(
            RequiredIds.ToHashSet(StringComparer.Ordinal).SetEquals(rows.Select(row => row.Id)),
            $"Traceability IDs differ. Actual: {string.Join(", ", rows.Select(row => row.Id).Order())}");

        foreach (var row in rows)
        {
            Assert.False(string.IsNullOrWhiteSpace(row.Boundary));
            Assert.DoesNotContain("pending", row.Evidence + row.Boundary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("TBD", row.Evidence + row.Boundary, StringComparison.OrdinalIgnoreCase);

            var links = EvidenceLink.Matches(row.Evidence);
            Assert.NotEmpty(links);
            Assert.Equal(links.Count, links.Select(link => link.Value).Distinct().Count());
            foreach (Match link in links)
            {
                AssertLinkedTestClassExists(link, row.Id);
            }
        }

        Assert.DoesNotContain("| PASS |", evidence, StringComparison.Ordinal);
        Assert.DoesNotContain("**Result: PASS.**", evidence, StringComparison.Ordinal);
        var normalizedEvidence = Regex.Replace(evidence, @"\s+", " ");
        RepositoryFiles.ContainsAll(
            normalizedEvidence,
            "does not turn a Markdown label into runtime proof",
            "not a real-browser",
            "No axe scan",
            "Full HTTP/SQL multi-host topology remains a SPEC-018 release concern",
            "Production deployment");
    }

    [Fact]
    public void Scope_and_human_decision_preserve_accessibility_and_production_boundaries()
    {
        var scope = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-007-scope-review.md");
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-007-release-approval.md");

        RepositoryFiles.ContainsAll(
            scope,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "Development",
            "Testing",
            "Production",
            "fail closed");
        var normalizedApproval = Regex.Replace(approval, @"\s+", " ");
        RepositoryFiles.ContainsAll(
            normalizedApproval,
            "Ahmed ELbamby",
            "Product",
            "Security",
            "Identity owner",
            "QA",
            "Accessibility",
            "Data / concurrency",
            "Operations",
            "not a substitute for running the cited suites",
            "No axe scan or real-browser keyboard audit is claimed",
            "Production NOT APPROVED");
    }

    private static TraceRow[] ParseRows(string markdown)
    {
        var section = RepositoryFiles.Section(markdown, "Traceability matrix");
        return section
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
            .Select(line => line.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries))
            .Where(cells => cells.Length == 3
                && !string.Equals(cells[0], "ID", StringComparison.Ordinal)
                && !cells[0].StartsWith("---", StringComparison.Ordinal))
            .Select(cells => new TraceRow(cells[0], cells[1], cells[2]))
            .ToArray();
    }

    private static void AssertLinkedTestClassExists(Match link, string rowId)
    {
        var className = link.Groups["class"].Value;
        var relativePath = link.Groups["path"].Value[6..];
        Assert.False(
            className is nameof(TraceabilityEvidenceTests) or nameof(NFR_4EvidenceTests),
            $"{rowId} must not cite a circular release-document validator.");
        Assert.True(
            RepositoryFiles.Exists(relativePath),
            $"{rowId} references a missing test source: {relativePath}");

        var source = RepositoryFiles.Read(relativePath);
        Assert.Matches(
            $@"\bpublic\s+(?:sealed\s+)?class\s+{Regex.Escape(className)}\b",
            source);
    }

    private sealed record TraceRow(string Id, string Evidence, string Boundary);
}
