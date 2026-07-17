using StudentRegistration.Academics.Application;
using StudentRegistration.AcceptanceTests.Specs.Spec009;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_6Tests
{
    [Fact]
    public async Task Stale_owner_preview_replays_the_same_rejection_without_effects()
    {
        // Given an Admin previewed the canonical SPEC-009 catalogue publication.
        var store = new Spec009PublicationStore();
        store.Seed("catalogue:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(Spec009PublicationScenario.PreviewRequest());

        // And the owning dependency changed before confirmation.
        store.Edit("catalogue:AI-DS", "v2", "hash-2");
        var command = Spec009PublicationScenario.ConfirmationRequest(
            preview.Token,
            clientRequestId: "spec017-stale-confirmation");

        // When the same payload-bound idempotency key is confirmed twice.
        var first = await service.ConfirmAsync(command);
        var replay = await service.ConfirmAsync(command);

        // Then the owner returns the replayable STALE_PREVIEW outcome twice and
        // commits neither publication nor an audit event.
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, first.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, replay.Outcome);
        Assert.True(replay.IsReplay);
        Assert.Equal(0, store.CommitCount);
        Assert.Empty(store.AuditEntries);

        var contract = RepositoryFiles.Read(
            "specs/009-catalog-prerequisites-policy-admin/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "POST /api/admin/catalogue/imports/{importId}/publish",
            "409 STALE_PREVIEW",
            "audit or storage failure rolls back all effects");
    }
}
