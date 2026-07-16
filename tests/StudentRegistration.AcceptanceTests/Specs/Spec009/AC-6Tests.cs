using StudentRegistration.Academics.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_6Tests
{
    [Fact]
    public async Task Edited_draft_invalidates_preview_and_same_key_replays_the_rejection()
    {
        var store = new Spec009PublicationStore();
        store.Seed("catalogue:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(Spec009PublicationScenario.PreviewRequest());
        store.Edit("catalogue:AI-DS", "v2", "hash-2");
        var command = Spec009PublicationScenario.ConfirmationRequest(preview.Token);

        var first = await service.ConfirmAsync(command);
        var replay = await service.ConfirmAsync(command);

        Assert.Equal(PublicationConfirmationOutcome.StalePreview, first.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, replay.Outcome);
        Assert.True(replay.IsReplay);
        Assert.Equal(0, store.CommitCount);
        Assert.Empty(store.AuditEntries);
    }
}
