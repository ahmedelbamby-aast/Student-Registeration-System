using System.Reflection;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class MeetingSlotModelTests
{
    [Theory]
    [InlineData(ActivityType.Lecture)]
    [InlineData(ActivityType.Tutorial)]
    [InlineData(ActivityType.Laboratory)]
    public void Meeting_uses_only_the_canonical_activity_values(ActivityType activityType)
    {
        var groupId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var meeting = new MeetingSlot(
            Guid.NewGuid(),
            groupId,
            roomId,
            activityType,
            DayOfWeek.Sunday,
            new TimeOnly(9, 0),
            new TimeOnly(10, 30));

        Assert.Equal(groupId, meeting.GroupId);
        Assert.Equal(roomId, meeting.RoomId);
        Assert.Equal(activityType, meeting.ActivityType);
        Assert.Equal(DayOfWeek.Sunday, meeting.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), meeting.StartLocal);
        Assert.Equal(new TimeOnly(10, 30), meeting.EndLocal);
        AssertPrivateSetter(nameof(MeetingSlot.GroupId));
        Assert.Null(typeof(MeetingSlot).GetProperty("Version"));
    }

    [Fact]
    public void Meeting_rejects_missing_parent_room_invalid_activity_day_and_time_range()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(groupId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(roomId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(activityType: (ActivityType)999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(dayOfWeek: (DayOfWeek)999));
        Assert.Throws<ArgumentException>(
            () => Create(startLocal: new TimeOnly(9, 0), endLocal: new TimeOnly(9, 0)));
        Assert.Throws<ArgumentException>(
            () => Create(startLocal: new TimeOnly(23, 0), endLocal: new TimeOnly(1, 0)));
    }

    [Fact]
    public void Meeting_mutation_is_owned_by_the_versioned_section_group_boundary()
    {
        Assert.Null(typeof(MeetingSlot).GetProperty("Version"));
        Assert.DoesNotContain(
            typeof(MeetingSlot).GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.DeclaredOnly),
            method => !method.IsSpecialName);

        var rootVersion = typeof(SectionGroup).GetProperty(
            nameof(SectionGroup.Version));
        Assert.NotNull(rootVersion);
        Assert.False(rootVersion.SetMethod?.IsPublic ?? false);

        AssertReplaceScheduleContract();
    }

    private static MeetingSlot Create(
        Guid? id = null,
        Guid? groupId = null,
        Guid? roomId = null,
        ActivityType activityType = ActivityType.Lecture,
        DayOfWeek dayOfWeek = DayOfWeek.Sunday,
        TimeOnly? startLocal = null,
        TimeOnly? endLocal = null) =>
        new(
            id ?? Guid.NewGuid(),
            groupId ?? Guid.NewGuid(),
            roomId ?? Guid.NewGuid(),
            activityType,
            dayOfWeek,
            startLocal ?? new TimeOnly(9, 0),
            endLocal ?? new TimeOnly(10, 30));

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(MeetingSlot).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }

    private static void AssertReplaceScheduleContract()
    {
        var method = typeof(SectionGroup).GetMethod(
            "ReplaceSchedule",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            [
                typeof(IReadOnlyList<MeetingSlot>),
                typeof(IReadOnlyList<GroupStaffAssignment>)
            ],
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }
}
