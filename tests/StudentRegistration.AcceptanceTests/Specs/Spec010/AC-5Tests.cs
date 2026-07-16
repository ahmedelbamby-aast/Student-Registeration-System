using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_5Tests
{
    [Fact]
    public async Task Two_admins_contending_for_one_room_have_exactly_one_winner()
    {
        var valid = Spec010Scenario.Publication();
        var conflict = Spec010Scenario.Publication(roomOverlap: true);
        var store = new PublicationStoreFake();
        store.Queue(valid, conflict);
        var transaction = new PublicationTransactionFake(store);
        var service = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            transaction);

        var results = await Task.WhenAll(
            service.PublishAsync(
                Spec010Scenario.PublishCommand(valid, "admin-1-request")),
            service.PublishAsync(
                Spec010Scenario.PublishCommand(valid, "admin-2-request")));

        Assert.Single(results, result =>
            result.Outcome is OfferingPublicationOutcome.Published);
        var loser = Assert.Single(results, result =>
            result.Outcome is OfferingPublicationOutcome.ValidationFailed);
        Assert.Equal("ROOM_CONFLICT", loser.ErrorCode);
        Assert.Equal(1, transaction.CommitCount);
        Assert.Single(transaction.Audits);
        Assert.Equal(2, store.ValidationCallsInsideTransaction);
    }
}
