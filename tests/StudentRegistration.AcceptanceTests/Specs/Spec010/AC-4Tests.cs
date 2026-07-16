using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_4Tests
{
    [Fact]
    public async Task Publication_and_capacity_change_commit_atomically_or_not_at_all()
    {
        var publicationStore = new PublicationStoreFake();
        var publicationTransaction =
            new PublicationTransactionFake(publicationStore);
        var publication = await new OfferingPublicationService(
                new OfferingPublicationValidator(),
                publicationTransaction)
            .PublishAsync(
                Spec010Scenario.PublishCommand(publicationStore.Snapshot));

        Assert.Equal(
            OfferingPublicationOutcome.Published,
            publication.Outcome);
        Assert.Equal(1, publicationTransaction.CommitCount);
        Assert.Single(publicationTransaction.Audits);
        Assert.Equal(1, publicationStore.ValidationCallsInsideTransaction);

        var group = new SectionGroup(
            Spec010Scenario.Id(4),
            Spec010Scenario.Id(1),
            "G01",
            30,
            20,
            SectionGroupState.Published,
            registrationPaused: false);
        var capacityStore = new CapacityStoreFake(group);
        var capacity = new SectionGroupCapacityService(capacityStore);

        var invalid = await capacity.ChangeCapacityAsync(
            group.Id,
            [1],
            19,
            "admin-1",
            "invalid capacity");

        Assert.Equal(
            GroupCapacityOutcome.CapacityBelowEnrolled,
            invalid);
        Assert.Equal(30, capacityStore.Capacity);
        Assert.Equal(0, capacityStore.CommitCount);
        Assert.Equal([1], capacityStore.Version);
    }
}
