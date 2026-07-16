namespace StudentRegistration.Scheduling.Domain;

public enum ActivityType
{
    Lecture = 1,
    Tutorial = 2,
    Laboratory = 3,
}

public sealed class MeetingSlot
{
    private MeetingSlot()
    {
    }

    public MeetingSlot(
        Guid id,
        Guid groupId,
        Guid roomId,
        ActivityType activityType,
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(groupId, nameof(groupId));
        SchedulingDomainValue.Identifier(roomId, nameof(roomId));
        SchedulingDomainValue.Defined(activityType, nameof(activityType));
        SchedulingDomainValue.Defined(dayOfWeek, nameof(dayOfWeek));
        if (endLocal <= startLocal)
        {
            throw new ArgumentException(
                "A meeting must end after it starts and cannot cross midnight.",
                nameof(endLocal));
        }

        Id = id;
        GroupId = groupId;
        RoomId = roomId;
        ActivityType = activityType;
        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
    }

    public Guid Id { get; private set; }

    public Guid GroupId { get; private set; }

    public Guid RoomId { get; private set; }

    public ActivityType ActivityType { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartLocal { get; private set; }

    public TimeOnly EndLocal { get; private set; }
}
