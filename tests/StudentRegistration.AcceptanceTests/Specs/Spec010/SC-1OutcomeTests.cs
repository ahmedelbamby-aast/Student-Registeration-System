using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public async Task Only_complete_conflict_free_offerings_become_visible()
    {
        var validStore = new PublicationStoreFake();
        var validTransaction = new PublicationTransactionFake(validStore);
        var valid = await new OfferingPublicationService(
                new OfferingPublicationValidator(),
                validTransaction)
            .PublishAsync(
                Spec010Scenario.PublishCommand(validStore.Snapshot));

        var incompleteStore = new PublicationStoreFake
        {
            Snapshot = Spec010Scenario.Publication(bundleComplete: false)
        };
        var incompleteTransaction =
            new PublicationTransactionFake(incompleteStore);
        var incomplete = await new OfferingPublicationService(
                new OfferingPublicationValidator(),
                incompleteTransaction)
            .PublishAsync(
                Spec010Scenario.PublishCommand(incompleteStore.Snapshot));

        Assert.Equal(
            OfferingPublicationOutcome.Published,
            valid.Outcome);
        Assert.Equal("published", validTransaction.OfferingState);
        Assert.Equal(
            OfferingPublicationOutcome.ValidationFailed,
            incomplete.Outcome);
        Assert.Equal("draft", incompleteTransaction.OfferingState);
        Assert.Equal(0, incompleteTransaction.CommitCount);
    }
}
