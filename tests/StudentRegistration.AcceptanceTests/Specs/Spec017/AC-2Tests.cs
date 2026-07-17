using StudentRegistration.AcceptanceTests.Specs.Spec010;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_2Tests
{
    [Fact]
    public async Task Capacity_reduction_below_active_enrollment_is_rejected_without_state_change()
    {
        // Given a published group has active enrollments equal to its current capacity.
        var group = new SectionGroup(
            Spec010Scenario.Id(4),
            Spec010Scenario.Id(1),
            "G01",
            capacity: 20,
            enrolledCount: 20,
            SectionGroupState.Published,
            registrationPaused: false);
        var store = new CapacityStoreFake(group, version: [7]);
        var service = new SectionGroupCapacityService(store);
        var beforeVersion = store.Version.ToArray();

        // When Admin delegates a reduction below EnrolledCount to SPEC-010.
        var result = await service.ChangeCapacityAsync(
            group.Id,
            expectedVersion: [7],
            capacity: 19,
            actorId: "admin:spec017-ac2",
            reason: "demonstrate capacity invariant");

        // Then the owner rejects it and no capacity/enrollment/version state commits.
        Assert.Equal(GroupCapacityOutcome.CapacityBelowEnrolled, result);
        Assert.Equal(20, store.Capacity);
        Assert.Equal(20, store.EnrolledCount);
        Assert.Equal(beforeVersion, store.Version);
        Assert.Equal(0, store.CommitCount);

        var selection = await service.ReadSelectionAsync(group.Id);
        Assert.False(selection.Selectable);
        Assert.Equal(["GROUP_FULL"], selection.ReasonCodes);
    }
}
