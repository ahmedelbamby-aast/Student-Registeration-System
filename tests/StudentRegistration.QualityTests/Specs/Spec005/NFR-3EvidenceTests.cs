using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Spec018_recovery_record_meets_the_inherited_rpo_rto_gate()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-7-recovery.json"));
        var root = document.RootElement;
        Assert.Equal("SPEC-018", root.GetProperty("ownerSpec").GetString());
        Assert.Equal("isolated-recovery", root.GetProperty("restoreEnvironment").GetString());
        Assert.Equal(300, root.GetProperty("rpoTargetSeconds").GetInt32());
        Assert.InRange(root.GetProperty("measuredRpoSeconds").GetInt32(), 0, 300);
        Assert.Equal(3_600, root.GetProperty("rtoTargetSeconds").GetInt32());
        Assert.InRange(root.GetProperty("measuredRtoSeconds").GetInt32(), 0, 3_600);
        Assert.Equal("pass", root.GetProperty("reconciliationStatus").GetString());
        Assert.Equal("pass", root.GetProperty("status").GetString());
        Assert.All(
            root.GetProperty("integrityChecks").EnumerateArray(),
            check => Assert.Equal("pass", check.GetProperty("status").GetString()));
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
    }

    [Fact]
    public void Spec005_record_binds_the_exact_executed_restore_fixture()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-NFR-3.md");
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-005 NFR-3 Backup and Restore Evidence",
            "SPEC-018-NFR-7-recovery.json",
            "RecoveryRehearsalTests.Real_sql_backup_restore_rehearsal_meets_rpo_rto_and_integrity_gates",
            "measured RPO was 1",
            "measured RTO was 2",
            "DBCC CHECKDB",
            "migration-history rows",
            "overbooking/duplicate-enrollment violations",
            "**Result: PASS.**");
    }
}
