using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_7Tests
{
    [Fact]
    public void Data_release_gate_has_passing_plan_migration_restore_and_privacy_evidence()
    {
        AssertPass("docs/release-evidence/SPEC-005-NFR-1-plans.json");
        AssertPass("docs/release-evidence/SPEC-005-NFR-2-rehearsal.json");
        AssertPass("docs/release-evidence/SPEC-018-NFR-7-recovery.json");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-005-NFR-4.md"),
            "**Result: PASS.**",
            "password hashes",
            "generated plaintext credentials",
            "4 passed",
            "0 failed");
    }

    private static void AssertPass(string path)
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(path));
        Assert.Equal("pass", document.RootElement.GetProperty("status").GetString());
    }
}
