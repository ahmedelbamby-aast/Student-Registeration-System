using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_6Tests
{
    [Fact]
    public void Ci_runs_the_approved_gate_order_with_one_isolated_database_per_run()
    {
        var workflow = RepositoryFiles.Read(".github/workflows/ci.yml");
        string[] orderedGates =
        [
            "Gate 01 - Restore",
            "Gate 02 - Format",
            "Gate 03 - Warnings-as-errors build",
            "Gate 04 - Unit tests",
            "Gate 05 - Architecture tests",
            "Gate 06 - SQL Server runtime",
            "Gate 07 - Migration readiness",
            "Gate 08 - Versioned seed lifecycle",
            "Gate 09 - SQL integration tests",
            "Gate 10 - E2E browser matrix",
            "Gate 11 - Automated and manual accessibility",
            "Gate 12 - Security and threat model"
        ];
        var previousIndex = -1;
        foreach (var gate in orderedGates)
        {
            var index = workflow.IndexOf(gate, StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"CI gate is absent or out of order: {gate}");
            previousIndex = index;
        }
        Assert.DoesNotContain("continue-on-error: true", workflow, StringComparison.Ordinal);

        using var lifecycle = Spec018AcceptanceEvidence.ReadJson(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/non-production-lifecycle.json");
        var root = lifecycle.RootElement;
        var testing = root.GetProperty("testing");
        Assert.Equal("ready", root.GetProperty("upstreamReadiness").GetProperty("status").GetString());
        Assert.Equal("Testing", testing.GetProperty("environment").GetString());
        Assert.True(testing.GetProperty("uniquePerRun").GetBoolean());
        Assert.True(testing.GetProperty("migrationsBeforeSeed").GetBoolean());
        Assert.True(testing.GetProperty("readinessRequired").GetBoolean());
        Assert.True(testing.GetProperty("disposeAfterRun").GetBoolean());
        Assert.False(testing.GetProperty("sharedDevelopmentOrProductionConnectionAllowed").GetBoolean());

        var traceability = Spec018AcceptanceEvidence.RequirePassingTraceability();
        Spec018AcceptanceEvidence.ContainsAll(traceability, "AC-6", "CI");
    }
}
