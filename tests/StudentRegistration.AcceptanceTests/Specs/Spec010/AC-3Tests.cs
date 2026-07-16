using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_3Tests
{
    [Fact]
    public async Task Full_group_is_server_marked_nonselectable_with_stable_reason()
    {
        var group = new SectionGroup(
            Spec010Scenario.Id(4),
            Spec010Scenario.Id(1),
            "G01",
            30,
            30,
            SectionGroupState.Published,
            registrationPaused: false);
        var store = new CapacityStoreFake(group);

        var result = await new SectionGroupCapacityService(store)
            .ReadSelectionAsync(group.Id);

        Assert.False(result.Selectable);
        Assert.Equal(["GROUP_FULL"], result.ReasonCodes);
        Assert.Equal(30, store.Capacity);
        Assert.Equal(30, store.EnrolledCount);
    }
}
