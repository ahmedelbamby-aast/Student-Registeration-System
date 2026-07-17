using StudentRegistration.Academics.Application;
using StudentRegistration.IntegrationTests.Specs.Spec009;

namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public async Task Concurrent_owner_feature_admin_edit_returns_stale_and_loses_no_update()
    {
        var upstream = new Spec009.EdgeCases.EC_3Tests();
        await upstream.Concurrent_policy_publish_has_one_winner_and_one_stale_result();

        var store = new Spec009PublicationStore();
        store.Seed("policy:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(
            Spec009PublicationScenario.PreviewRequest(PublicationScopeKind.Policy));

        var results = await Task.WhenAll(
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token,
                PublicationScopeKind.Policy,
                "policy-ec3-a")),
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token,
                PublicationScopeKind.Policy,
                "policy-ec3-b")));

        Assert.Single(
            results,
            result => result.Outcome == PublicationConfirmationOutcome.Published);
        var stale = Assert.Single(
            results,
            result => result.Outcome == PublicationConfirmationOutcome.StalePreview);
        Assert.Equal("v1-next", stale.CurrentVersion);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }
}
