using StudentRegistration.Academics.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_5Tests
{
    [Fact]
    public async Task Two_current_admin_confirmations_have_one_immutable_winner()
    {
        var store = new Spec009PublicationStore();
        store.Seed("policy:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(
            Spec009PublicationScenario.PreviewRequest(PublicationScopeKind.Policy));

        var results = await Task.WhenAll(
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token,
                PublicationScopeKind.Policy,
                "policy-publish-1")),
            service.ConfirmAsync(Spec009PublicationScenario.ConfirmationRequest(
                preview.Token,
                PublicationScopeKind.Policy,
                "policy-publish-2")));

        Assert.Single(results, result =>
            result.Outcome == PublicationConfirmationOutcome.Published);
        Assert.Single(results, result =>
            result.Outcome == PublicationConfirmationOutcome.StalePreview);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }
}
