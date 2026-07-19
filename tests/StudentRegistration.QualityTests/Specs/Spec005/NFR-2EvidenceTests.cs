using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class NFR_2EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-005-NFR-2-rehearsal.json";

    [Fact]
    public void Numeric_window_approval_and_fail_closed_rules_are_recorded()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-NFR-2.md");
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-005 NFR-2 Controlled Migration Evidence",
            "600 seconds",
            "480 seconds",
            "80%",
            "Ahmed ELbamby",
            "Operations review perspective",
            "non-production POC",
            "fail closed",
            "not official AASTMT production authorization",
            "tested rollback",
            "SPEC-005-NFR-2-rehearsal.json",
            "**Result: PASS.**");
    }

    [Fact]
    public void Recorded_rehearsal_is_inside_threshold_and_restores_prior_state()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(EvidencePath));
        var root = document.RootElement;
        Assert.Equal("spec005-migration-rehearsal/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("SPEC-005", root.GetProperty("ownerSpec").GetString());
        Assert.Equal(600, root.GetProperty("approvedWindowSeconds").GetInt32());
        Assert.Equal(480, root.GetProperty("maximumAllowedSeconds").GetInt32());
        Assert.InRange(root.GetProperty("forwardDurationSeconds").GetDouble(), 0, 480);
        Assert.InRange(root.GetProperty("rollbackDurationSeconds").GetDouble(), 0, 480);
        Assert.Matches("^[A-F0-9]{64}$", root.GetProperty("migrationArtifactSha256").GetString());
        Assert.True(root.GetProperty("backupVerified").GetBoolean());
        Assert.True(root.GetProperty("rollbackTested").GetBoolean());
        Assert.True(root.GetProperty("priorStateRestored").GetBoolean());
        Assert.True(root.GetProperty("modelParityVerified").GetBoolean());
        Assert.Equal(7, root.GetProperty("appliedMigrationCount").GetInt32());
        Assert.Equal("pass", root.GetProperty("status").GetString());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvedBy").GetString());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
    }
}
