using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_6Tests
{
    [Fact]
    public async Task Capacity_reduction_and_last_seat_share_one_group_version()
    {
        var group = new SectionGroup(
            Spec010Scenario.Id(4),
            Spec010Scenario.Id(1),
            "G01",
            30,
            29,
            SectionGroupState.Published,
            registrationPaused: false);
        var store = new CapacityStoreFake(group);
        var service = new SectionGroupCapacityService(store);

        var outcomes = await Task.WhenAll(
            service.ChangeCapacityAsync(
                group.Id,
                [1],
                29,
                "admin-1",
                "reduce capacity"),
            service.AllocateSeatAsync(group.Id, [1]));

        Assert.Single(
            outcomes,
            outcome => outcome is GroupCapacityOutcome.Applied);
        Assert.Single(
            outcomes,
            outcome => outcome is GroupCapacityOutcome.StaleVersion
                or GroupCapacityOutcome.GroupFull);
        Assert.InRange(store.EnrolledCount, 0, store.Capacity);
        Assert.Equal(1, store.CommitCount);
        Assert.Equal([2], store.Version);
    }
}
