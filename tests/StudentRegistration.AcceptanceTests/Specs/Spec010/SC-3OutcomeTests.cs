using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public async Task Capacity_never_commits_below_active_enrollment()
    {
        var group = new SectionGroup(
            Spec010Scenario.Id(4),
            Spec010Scenario.Id(1),
            "G01",
            30,
            20,
            SectionGroupState.Published,
            registrationPaused: false);
        var store = new CapacityStoreFake(group);
        var service = new SectionGroupCapacityService(store);

        var rejected = await service.ChangeCapacityAsync(
            group.Id,
            [1],
            19,
            "admin-1",
            "invalid");
        var accepted = await service.ChangeCapacityAsync(
            group.Id,
            [1],
            20,
            "admin-1",
            "valid");
        var full = await service.AllocateSeatAsync(group.Id, [2]);

        Assert.Equal(
            GroupCapacityOutcome.CapacityBelowEnrolled,
            rejected);
        Assert.Equal(GroupCapacityOutcome.Applied, accepted);
        Assert.Equal(GroupCapacityOutcome.GroupFull, full);
        Assert.True(store.Capacity >= store.EnrolledCount);
        Assert.Equal(1, store.CommitCount);
    }
}
