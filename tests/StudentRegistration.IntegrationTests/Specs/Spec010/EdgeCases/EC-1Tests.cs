using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task One_invalid_meeting_rejects_the_entire_multi_slot_group()
    {
        var offering = new CourseOffering(
            Id(90),
            Id(91),
            Id(92),
            CourseOfferingState.Draft);
        var group = new SectionGroup(
            Id(1),
            offering.Id,
            "G01",
            30,
            0,
            SectionGroupState.Draft,
            false);
        var lecture = Meeting(group.Id, Id(4), ActivityType.Lecture, 9);
        var tutorialWithUnavailableRoom =
            Meeting(group.Id, Id(5), ActivityType.Tutorial, 11);
        group.ReplaceSchedule(
            [lecture, tutorialWithUnavailableRoom],
            [
                new(
                    group.Id,
                    lecture.Id,
                    ActivityType.Lecture,
                    Id(20),
                    TeachingRole.Lecturer),
                new(
                    group.Id,
                    tutorialWithUnavailableRoom.Id,
                    ActivityType.Tutorial,
                    Id(21),
                    TeachingRole.TeachingAssistant)
            ]);
        var store = new PublicationStoreFake(
            new OfferingPublicationSnapshot(
                offering.Id,
                OfferingVersion: [1],
                group.Id,
                GroupVersion: [1],
                GroupCapacity: group.Capacity,
                tutorialWithUnavailableRoom.RoomId,
                RoomCapacity: 40,
                RoomAvailable: false,
                RoomOverlap: false,
                CurrentRoomVersion: [1],
                StaffTermAvailabilityId: Id(2),
                StaffAvailable: true,
                StaffOverlap: false,
                StaffVersion: [1],
                SlotValid: true,
                BundleComplete: true));
        var validator = new OfferingPublicationValidator();

        var result = await validator.ValidateAsync(
            CurrentCommand(store.Snapshot),
            store);

        Assert.Equal(2, group.Meetings.Count);
        Assert.False(result.Valid);
        Assert.Contains("ROOM_UNAVAILABLE", result.ReasonCodes);
        Assert.Equal(CourseOfferingState.Draft, offering.State);
        Assert.Equal(SectionGroupState.Draft, group.State);
    }

    private static ValidateOfferingCommand CurrentCommand(
        OfferingPublicationSnapshot snapshot) =>
        new(
            snapshot.OfferingId,
            snapshot.OfferingVersion,
            new Dictionary<Guid, byte[]>
            {
                [snapshot.GroupId] = snapshot.GroupVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.RoomId] = snapshot.CurrentRoomVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.StaffTermAvailabilityId] = snapshot.StaffVersion
            });

    private static MeetingSlot Meeting(
        Guid groupId,
        Guid roomId,
        ActivityType activity,
        int hour) =>
        new(
            Guid.NewGuid(),
            groupId,
            roomId,
            activity,
            DayOfWeek.Monday,
            new TimeOnly(hour, 0),
            new TimeOnly(hour + 1, 0));

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private sealed class PublicationStoreFake(OfferingPublicationSnapshot snapshot)
        : IOfferingPublicationStore
    {
        public OfferingPublicationSnapshot Snapshot { get; } = snapshot;

        public Task<PublicationDependencies> GetDependenciesAsync(
            Guid offeringId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new PublicationDependencies(
                    offeringId,
                    [Snapshot.GroupId],
                    [Snapshot.RoomId],
                    [Snapshot.StaffTermAvailabilityId]));

        public Task<OfferingPublicationSnapshot> LockAndLoadAsync(
            PublicationLockRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(Snapshot);
    }
}
