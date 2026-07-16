using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_8Tests
{
    [Fact]
    public async Task Schedule_change_invalidates_captured_group_version()
    {
        var offeringStore = new OfferingStoreFake();
        var group = Assert.Single(offeringStore.Snapshot.Groups);
        var updated = await new OfferingService(offeringStore).UpdateGroupAsync(
            new UpdateGroupCommand(
                group.Id,
                ExpectedOfferingRowVersion: [1],
                ExpectedGroupRowVersion: [1],
                GroupCode: group.GroupCode,
                Capacity: group.Capacity,
                RegistrationPaused: group.RegistrationPaused,
                Meetings: [],
                StaffAssignments: [],
                ActorId: "admin-1",
                Reason: "replace schedule"));

        var capacityGroup = new SectionGroup(
            group.Id,
            offeringStore.Snapshot.Id,
            group.GroupCode,
            group.Capacity,
            group.EnrolledCount,
            SectionGroupState.Published,
            group.RegistrationPaused);
        var capacityStore = new CapacityStoreFake(
            capacityGroup,
            updated.GroupRowVersion);
        var allocation = await new SectionGroupCapacityService(capacityStore)
            .AllocateSeatAsync(group.Id, [1]);

        Assert.Equal(OfferingOutcome.Updated, updated.Outcome);
        Assert.Equal([2], updated.GroupRowVersion);
        Assert.Equal(GroupCapacityOutcome.StaleVersion, allocation);
    }

    [Fact]
    public async Task Availability_change_is_revalidated_on_staff_term_version()
    {
        var availabilityStore = new ResourceAvailabilityStoreFake();
        var availability = await new ResourceAvailabilityService(
                availabilityStore)
            .ReplaceOwnAvailabilityAsync(
                availabilityStore.OwnerStaffId,
                availabilityStore.AvailabilityId,
                [1],
                [new(1, new(8, 0), new(12, 0), "unavailable")],
                new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc),
                "staff update");

        var publicationStore = new PublicationStoreFake
        {
            Snapshot = Spec010Scenario.Publication(staffVersion: [2])
        };
        var publication = await new OfferingPublicationValidator()
            .ValidateAsync(
                Spec010Scenario.ValidateCommand(
                    publicationStore.Snapshot,
                    expectedStaffVersion: [1]),
                publicationStore);

        Assert.Equal(ResourceAvailabilityOutcome.Applied, availability.Outcome);
        Assert.False(publication.Valid);
        Assert.Contains("STALE_DEPENDENCY", publication.ReasonCodes);
    }
}
