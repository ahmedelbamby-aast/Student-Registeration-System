using StudentRegistration.Academics.Application;
using StudentRegistration.IntegrationTests.Specs.Spec009;

namespace StudentRegistration.IntegrationTests.Specs.Spec009.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public async Task Audit_failure_rolls_back_publication_and_activation()
    {
        var store = new Spec009PublicationStore { FailBeforeCommit = true };
        store.Seed("catalogue:AI-DS");
        var service = Spec009PublicationScenario.Service(store);
        var preview = service.CreatePreview(Spec009PublicationScenario.PreviewRequest());

        var result = await service.ConfirmAsync(
            Spec009PublicationScenario.ConfirmationRequest(preview.Token));

        Assert.Equal(PublicationConfirmationOutcome.StorageUnavailable, result.Outcome);
        Assert.Equal(0, store.CommitCount);
        Assert.Empty(store.AuditEntries);
    }
}
