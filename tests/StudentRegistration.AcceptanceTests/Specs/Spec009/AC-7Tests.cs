using System.Diagnostics;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_7Tests
{
    [Fact]
    public async Task Catalogue_and_policy_quality_gate_is_measurable_deterministic_atomic_and_audited()
    {
        var provenance = new CatalogueFieldProvenance(
            "synthetic-load-fixture",
            new DateOnly(2026, 7, 13),
            CatalogueSourceKind.SyntheticDemo,
            []);
        var content = new CatalogueDraftContent(
            "AI-DS",
            Enumerable.Range(1, 10_000)
                .Select(index => new CatalogueCourseDefinition(
                    $"DEMO-{index:00000}",
                    $"Synthetic course {index}",
                    3m,
                    true,
                    ((index - 1) % 8) + 1,
                    (index - 1) % 8 == 0
                        ? []
                        : [$"DEMO-{index - 1:00000}"],
                    null,
                    null,
                    provenance))
                .ToArray());
        var timer = Stopwatch.StartNew();
        var validation = new CataloguePublicationService().Validate(content);
        timer.Stop();

        Assert.True(validation.IsValid);
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(30));

        var policy = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());
        var input = new PolicySimulationInput(
            1.99m,
            95m,
            "Active",
            true,
            false,
            13,
            ["DS413"],
            ["GN111", "GN112"],
            true,
            false);
        var policyService = new PolicyAdministrationService();
        Assert.Equal(
            policyService.Simulate(policy, input).RuleResults,
            policyService.Simulate(policy, input).RuleResults);

        var failedStore = new Spec009PublicationStore { FailBeforeCommit = true };
        failedStore.Seed("catalogue:AI-DS");
        var confirmation = Spec009PublicationScenario.Service(failedStore);
        var preview = confirmation.CreatePreview(Spec009PublicationScenario.PreviewRequest());
        var failed = await confirmation.ConfirmAsync(
            Spec009PublicationScenario.ConfirmationRequest(preview.Token));

        Assert.Equal(PublicationConfirmationOutcome.StorageUnavailable, failed.Outcome);
        Assert.Equal(0, failedStore.CommitCount);
        Assert.Empty(failedStore.AuditEntries);

        var store = new Spec009PublicationStore();
        store.Seed("catalogue:AI-DS");
        confirmation = Spec009PublicationScenario.Service(store);
        preview = confirmation.CreatePreview(Spec009PublicationScenario.PreviewRequest());
        var published = await confirmation.ConfirmAsync(
            Spec009PublicationScenario.ConfirmationRequest(preview.Token));

        Assert.Equal(PublicationConfirmationOutcome.Published, published.Outcome);
        var audit = Assert.Single(store.AuditEntries);
        Assert.Contains("admin-1", audit, StringComparison.Ordinal);
        Assert.Contains("Publish approved draft", audit, StringComparison.Ordinal);
        Assert.Contains("SRC-DATA-SCIENCE", audit, StringComparison.Ordinal);
    }
}
