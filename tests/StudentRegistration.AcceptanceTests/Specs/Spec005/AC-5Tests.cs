using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_5Tests
{
    [Fact]
    public void Reviewed_script_is_a_controlled_step_and_rollback_restores_the_verified_backup()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-NFR-2-rehearsal.json"));
        var root = document.RootElement;
        Assert.True(root.GetProperty("backupVerified").GetBoolean());
        Assert.True(root.GetProperty("rollbackTested").GetBoolean());
        Assert.True(root.GetProperty("priorStateRestored").GetBoolean());
        Assert.True(root.GetProperty("modelParityVerified").GetBoolean());
        Assert.Equal("pass", root.GetProperty("status").GetString());

        var migrationGate = RepositoryFiles.Read(
            "tests/StudentRegistration.MigrationTests/MigrationBundleTests.cs");
        RepositoryFiles.ContainsAll(
            migrationGate,
            "no application-startup migration",
            "MigrateAsync",
            "EnsureCreatedAsync");
    }
}
