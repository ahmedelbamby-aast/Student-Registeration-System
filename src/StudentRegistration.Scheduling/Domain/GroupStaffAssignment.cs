namespace StudentRegistration.Scheduling.Domain;

public enum TeachingRole
{
    Lecturer = 1,
    TeachingAssistant = 2,
}

public sealed class GroupStaffAssignment
{
    private GroupStaffAssignment()
    {
    }

    public GroupStaffAssignment(
        Guid groupId,
        Guid meetingSlotId,
        ActivityType activityType,
        Guid staffId,
        TeachingRole teachingRole)
    {
        SchedulingDomainValue.Identifier(groupId, nameof(groupId));
        SchedulingDomainValue.Identifier(meetingSlotId, nameof(meetingSlotId));
        SchedulingDomainValue.Identifier(staffId, nameof(staffId));
        SchedulingDomainValue.Defined(activityType, nameof(activityType));
        SchedulingDomainValue.Defined(teachingRole, nameof(teachingRole));
        if (activityType is ActivityType.Lecture
            && teachingRole is not TeachingRole.Lecturer
            || activityType is ActivityType.Tutorial or ActivityType.Laboratory
            && teachingRole is not TeachingRole.TeachingAssistant)
        {
            throw new ArgumentException(
                "The teaching role does not cover the declared activity.",
                nameof(teachingRole));
        }

        GroupId = groupId;
        MeetingSlotId = meetingSlotId;
        ActivityType = activityType;
        StaffId = staffId;
        TeachingRole = teachingRole;
    }

    public Guid GroupId { get; private set; }

    public Guid MeetingSlotId { get; private set; }

    public ActivityType ActivityType { get; private set; }

    public Guid StaffId { get; private set; }

    public TeachingRole TeachingRole { get; private set; }
}
