using System.Globalization;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class ThreatModelGateTests
{
    private const string ThreatModelPath = "docs/security/THREAT_MODEL.md";

    [Fact]
    public void Canonical_threat_model_has_versioned_review_metadata_and_scope()
    {
        var threatModel = RepositoryFiles.Read(ThreatModelPath);

        RepositoryFiles.ContainsAll(
            threatModel,
            "# STRIDE Threat Model",
            "**Artifact version:**",
            "**Status:** Reviewed",
            "**Scope:** Non-production Student Registration System demo",
            "**Owner:** Ahmed ELbamby",
            "**Reviewer:** Ahmed ELbamby",
            "**Reviewed on:**",
            "**Next review due:**",
            "**Production authority:** Not granted");
    }

    [Fact]
    public void Threat_model_covers_every_fr9_boundary_asset_and_security_domain()
    {
        var threatModel = RepositoryFiles.Read(ThreatModelPath);

        RepositoryFiles.ContainsAll(
            threatModel,
            "## Assets",
            "## Trust boundaries",
            "Browser to API",
            "API to SQL Server",
            "API replicas to shared key ring",
            "API to telemetry and exports",
            "Deployment operator to runtime configuration",
            "identity and session",
            "authorization and data scope",
            "protected option tokens",
            "registration races",
            "Admin, audit, and export",
            "SQL persistence",
            "telemetry",
            "secrets",
            "deployment");
    }

    [Fact]
    public void Every_registered_threat_has_mitigation_evidence_residual_risk_owner_and_review_state()
    {
        var threatModel = RepositoryFiles.Read(ThreatModelPath);
        var rows = threatModel
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| TM-", StringComparison.Ordinal))
            .ToArray();

        Assert.True(rows.Length >= 10, "The threat register must cover all FR-9 security domains.");
        foreach (var row in rows)
        {
            var cells = row
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(9, cells.Length);
            Assert.All(cells, cell => Assert.False(string.IsNullOrWhiteSpace(cell)));
            Assert.Equal("Ahmed ELbamby", cells[7]);
            Assert.Equal("Reviewed", cells[8]);
            Assert.DoesNotContain("TBD", row, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Review_freshness_is_machine_enforced()
    {
        var threatModel = RepositoryFiles.Read(ThreatModelPath);
        var reviewedOn = ReadDate(threatModel, "Reviewed on");
        var nextReviewDue = ReadDate(threatModel, "Next review due");
        var today = DateOnly.FromDateTime(DateTime.Now);

        Assert.True(reviewedOn <= today, "Threat-model review date cannot be in the future.");
        Assert.True(nextReviewDue > reviewedOn, "Next review must follow the completed review.");
        Assert.True(
            today <= nextReviewDue,
            $"Threat model is stale: review was due on {nextReviewDue:yyyy-MM-dd}.");
    }

    [Fact]
    public void Release_gate_blocks_every_fr9_security_and_usability_failure()
    {
        var threatModel = RepositoryFiles.Read(ThreatModelPath);
        var releaseGate = RepositoryFiles.Section(threatModel, "Release blockers");

        RepositoryFiles.ContainsAll(
            releaseGate,
            "unreviewed or stale threat model",
            "unresolved Critical or High security finding",
            "capacity, duplicate-enrollment, or atomicity invariant failure",
            "Critical or Major core-usability defect");
    }

    private static DateOnly ReadDate(string threatModel, string label)
    {
        var match = Regex.Match(
            threatModel,
            $@"(?m)^\*\*{Regex.Escape(label)}:\*\*\s+(?<date>\d{{4}}-\d{{2}}-\d{{2}})\s*$",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, $"Threat-model metadata is missing '{label}'.");
        return DateOnly.ParseExact(
            match.Groups["date"].Value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);
    }
}
