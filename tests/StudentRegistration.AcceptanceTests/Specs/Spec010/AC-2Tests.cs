using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_2Tests
{
    [Fact]
    public async Task Same_room_overlap_blocks_publication_after_locking_both_groups()
    {
        var roomId = Spec010Scenario.Id(6);
        var first = new MeetingSlot(
            Spec010Scenario.Id(5),
            Spec010Scenario.Id(4),
            roomId,
            ActivityType.Lecture,
            DayOfWeek.Sunday,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0));
        var second = new MeetingSlot(
            Spec010Scenario.Id(15),
            Spec010Scenario.Id(14),
            roomId,
            ActivityType.Lecture,
            DayOfWeek.Sunday,
            new TimeOnly(9, 30),
            new TimeOnly(10, 30));
        var store = new PublicationStoreFake
        {
            Snapshot = Spec010Scenario.Publication(roomOverlap: true),
            Dependencies = new(
                Spec010Scenario.Id(1),
                [Spec010Scenario.Id(4), Spec010Scenario.Id(14)],
                [Spec010Scenario.Id(6)],
                [Spec010Scenario.Id(11)])
        };

        var result = await new OfferingPublicationValidator().ValidateAsync(
            Spec010Scenario.ValidateCommand(store.Snapshot),
            store);

        Assert.False(result.Valid);
        Assert.Contains("ROOM_CONFLICT", result.ReasonCodes);
        var ordered = Assert.Single(store.LockRequests).OrderedResourceIds;
        Assert.Contains(Spec010Scenario.Id(4), ordered);
        Assert.Contains(Spec010Scenario.Id(14), ordered);
        Assert.Equal(first.RoomId, second.RoomId);
        Assert.Equal(new TimeOnly(9, 30),
            first.StartLocal > second.StartLocal
                ? first.StartLocal
                : second.StartLocal);
        Assert.Equal(new TimeOnly(10, 0),
            first.EndLocal < second.EndLocal
                ? first.EndLocal
                : second.EndLocal);
    }
}
