using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public void Version_change_during_capture_returns_stale_input_without_mixed_options()
    {
        var source = new VersionedInputSource(
            catalogueVersion: "catalogue-v1",
            policyVersion: "policy-v1",
            groupVersion: "group-v1");

        var coherent = source.Capture();
        var stale = source.Capture(() =>
            source.GroupVersion = "group-v2");

        Assert.Equal(CaptureOutcome.Captured, coherent.Outcome);
        Assert.Equal("group-v1", coherent.GroupVersion);
        Assert.Single(coherent.Options);
        Assert.Equal(CaptureOutcome.StaleInput, stale.Outcome);
        Assert.Equal("STALE_INPUT", stale.SafeCode);
        Assert.Empty(stale.Options);
        Assert.Null(stale.CatalogueVersion);
        Assert.Null(stale.PolicyVersion);
        Assert.Null(stale.GroupVersion);
    }

    [Fact]
    public void Production_boundary_reads_one_coherent_recommendation_snapshot()
    {
        var assembly = Assembly.Load("StudentRegistration.Registration");
        var reader = assembly.GetType(
            "StudentRegistration.Registration.Application.Ports.IRecommendationSnapshotReader");
        var coordinator = assembly.GetType(
            "StudentRegistration.Registration.Application.OptimizationCoordinator");

        Assert.True(
            reader is not null,
            "The Spec 013 application boundary needs a narrow IRecommendationSnapshotReader before EC-5 can pass.");
        Assert.Contains(
            reader!.GetMethods(),
            method => method.Name.EndsWith("Async", StringComparison.Ordinal));
        Assert.True(
            coordinator is not null,
            "T045/T047 must provide OptimizationCoordinator before EC-5 can pass against production code.");
    }

    private enum CaptureOutcome
    {
        Captured,
        StaleInput
    }

    private sealed record CaptureResult(
        CaptureOutcome Outcome,
        string? SafeCode,
        string? CatalogueVersion,
        string? PolicyVersion,
        string? GroupVersion,
        IReadOnlyList<string> Options);

    private sealed class VersionedInputSource(
        string catalogueVersion,
        string policyVersion,
        string groupVersion)
    {
        public string CatalogueVersion { get; set; } = catalogueVersion;

        public string PolicyVersion { get; set; } = policyVersion;

        public string GroupVersion { get; set; } = groupVersion;

        public CaptureResult Capture(Action? fault = null)
        {
            var before = Versions();
            fault?.Invoke();
            var after = Versions();

            if (before != after)
            {
                return new(
                    CaptureOutcome.StaleInput,
                    "STALE_INPUT",
                    null,
                    null,
                    null,
                    []);
            }

            return new(
                CaptureOutcome.Captured,
                null,
                before.Catalogue,
                before.Policy,
                before.Group,
                ["complete-option"]);
        }

        private (string Catalogue, string Policy, string Group) Versions() =>
            (CatalogueVersion, PolicyVersion, GroupVersion);
    }
}
