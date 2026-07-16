using StudentRegistration.Academics.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public async Task Published_version_is_single_replayable_and_audited()
    {
        var store = new Spec009PublicationStore();
        store.Seed("catalogue:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(Spec009PublicationScenario.PreviewRequest());
        var command = Spec009PublicationScenario.ConfirmationRequest(preview.Token);

        var published = await service.ConfirmAsync(command);
        var replay = await service.ConfirmAsync(command);

        Assert.Equal(PublicationConfirmationOutcome.Published, published.Outcome);
        Assert.Equal(published.PublishedVersionId, replay.PublishedVersionId);
        Assert.True(replay.IsReplay);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }
}
