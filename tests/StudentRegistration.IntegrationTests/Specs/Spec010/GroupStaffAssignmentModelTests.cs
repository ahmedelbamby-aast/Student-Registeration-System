using System.Reflection;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class GroupStaffAssignmentModelTests
{
    [Theory]
    [InlineData(ActivityType.Lecture, TeachingRole.Lecturer)]
    [InlineData(ActivityType.Tutorial, TeachingRole.TeachingAssistant)]
    [InlineData(ActivityType.Laboratory, TeachingRole.TeachingAssistant)]
    public void Assignment_preserves_the_unique_meeting_staff_role_key(
        ActivityType activityType,
        TeachingRole teachingRole)
    {
        var groupId = Guid.NewGuid();
        var meetingSlotId = Guid.NewGuid();
        var staffId = Guid.NewGuid();

        var assignment = new GroupStaffAssignment(
            groupId,
            meetingSlotId,
            activityType,
            staffId,
            teachingRole);

        Assert.Equal(groupId, assignment.GroupId);
        Assert.Equal(meetingSlotId, assignment.MeetingSlotId);
        Assert.Equal(activityType, assignment.ActivityType);
        Assert.Equal(staffId, assignment.StaffId);
        Assert.Equal(teachingRole, assignment.TeachingRole);
        AssertPrivateSetter(nameof(GroupStaffAssignment.MeetingSlotId));
        AssertPrivateSetter(nameof(GroupStaffAssignment.StaffId));
        AssertPrivateSetter(nameof(GroupStaffAssignment.TeachingRole));
        Assert.Null(typeof(GroupStaffAssignment).GetProperty("Version"));
    }

    [Fact]
    public void Lecturer_covers_only_lecture_and_ta_covers_only_tutorial_or_laboratory()
    {
        Assert.Throws<ArgumentException>(() => Create(
            ActivityType.Tutorial,
            TeachingRole.Lecturer));
        Assert.Throws<ArgumentException>(() => Create(
            ActivityType.Laboratory,
            TeachingRole.Lecturer));
        Assert.Throws<ArgumentException>(() => Create(
            ActivityType.Lecture,
            TeachingRole.TeachingAssistant));
    }

    [Fact]
    public void Assignment_rejects_missing_identity_and_unknown_activity_or_role()
    {
        Assert.Throws<ArgumentException>(() => Create(groupId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(meetingSlotId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(staffId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create((ActivityType)999, TeachingRole.Lecturer));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(ActivityType.Lecture, (TeachingRole)999));
    }

    [Fact]
    public void Staff_assignment_mutation_is_owned_by_the_versioned_section_group_boundary()
    {
        Assert.Null(typeof(GroupStaffAssignment).GetProperty("Version"));
        Assert.DoesNotContain(
            typeof(GroupStaffAssignment).GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.DeclaredOnly),
            method => !method.IsSpecialName);

        var rootVersion = typeof(SectionGroup).GetProperty(
            nameof(SectionGroup.Version));
        Assert.NotNull(rootVersion);
        Assert.False(rootVersion.SetMethod?.IsPublic ?? false);

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

    private static GroupStaffAssignment Create(
        ActivityType activityType = ActivityType.Lecture,
        TeachingRole teachingRole = TeachingRole.Lecturer,
        Guid? groupId = null,
        Guid? meetingSlotId = null,
        Guid? staffId = null) =>
        new(
            groupId ?? Guid.NewGuid(),
            meetingSlotId ?? Guid.NewGuid(),
            activityType,
            staffId ?? Guid.NewGuid(),
            teachingRole);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(GroupStaffAssignment).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
