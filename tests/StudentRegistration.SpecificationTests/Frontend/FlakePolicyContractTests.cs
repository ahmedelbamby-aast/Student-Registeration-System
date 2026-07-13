using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed class FlakePolicyContractTests
{
    private const string PolicyPath =
        "tests/StudentRegistration.E2ETests/flake-policy.json";

    [Fact]
    public void First_run_failure_remains_failed_and_retry_is_diagnostics_only()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(PolicyPath));
        var root = document.RootElement;
        var retry = root.GetProperty("diagnosticsOnlyRetry");

        Assert.Equal("flake-policy/1.0.0", root.GetProperty("version").GetString());
        Assert.True(root.GetProperty("firstRunFailureStaysFailed").GetBoolean());
        Assert.False(root.GetProperty("silentPassAllowed").GetBoolean());
        Assert.True(retry.GetProperty("enabled").GetBoolean());
        Assert.Equal(1, retry.GetProperty("maxRetries").GetInt32());
        Assert.False(retry.GetProperty("canChangeGateResult").GetBoolean());

        var artifacts = retry.GetProperty("artifacts").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            new[] { "trace", "video", "screenshot", "console", "network" },
            artifact => Assert.Contains(artifact, artifacts));
    }

    [Fact]
    public void Critical_journeys_cannot_be_quarantined_and_flakes_require_owned_correction()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(PolicyPath));
        var root = document.RootElement;
        var criticalJourneys = root.GetProperty("criticalJourneys");
        var correction = root.GetProperty("flakeCorrection");

        Assert.False(criticalJourneys.GetProperty("quarantineAllowed").GetBoolean());
        Assert.NotEmpty(criticalJourneys.GetProperty("journeyIds").EnumerateArray());
        Assert.True(correction.GetProperty("releaseBlockedUntilCorrected").GetBoolean());
        Assert.Equal("Ahmed ELbamby", correction.GetProperty("defaultOwner").GetString());

        var requiredFields = correction.GetProperty("requiredFields").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            new[] { "owner", "issue", "cause", "fixBy", "correctionEvidence" },
            field => Assert.Contains(field, requiredFields));
    }
}
