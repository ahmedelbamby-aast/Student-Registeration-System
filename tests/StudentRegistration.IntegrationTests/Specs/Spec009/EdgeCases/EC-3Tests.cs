using StudentRegistration.Academics.Application;
using StudentRegistration.IntegrationTests.Specs.Spec009;

namespace StudentRegistration.IntegrationTests.Specs.Spec009.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public async Task Concurrent_policy_publish_has_one_winner_and_one_stale_result()
    {
        var store = new Spec009PublicationStore();
        store.Seed("policy:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(
            Spec009PublicationScenario.PreviewRequest(PublicationScopeKind.Policy));

        var results = await Task.WhenAll(
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token, PublicationScopeKind.Policy, "policy-a")),
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token, PublicationScopeKind.Policy, "policy-b")));

        Assert.Single(results, item => item.Outcome == PublicationConfirmationOutcome.Published);
        Assert.Single(results, item => item.Outcome == PublicationConfirmationOutcome.StalePreview);
    }
}
