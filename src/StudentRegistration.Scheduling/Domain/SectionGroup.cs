namespace StudentRegistration.Scheduling.Domain;

public enum SectionGroupState
{
    Draft = 1,
    Published = 2,
    Closed = 3,
    Cancelled = 4,
}

public sealed class SectionGroup
{
    private IReadOnlyList<MeetingSlot> _meetings = [];
    private IReadOnlyList<GroupStaffAssignment> _staffAssignments = [];

    private SectionGroup()
    {
    }

    public SectionGroup(
        Guid id,
        Guid offeringId,
        string groupCode,
        int capacity,
        int enrolledCount,
        SectionGroupState state,
        bool registrationPaused,
        int heldSeatCount = 0)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(offeringId, nameof(offeringId));
        SchedulingDomainValue.Defined(state, nameof(state));
        ValidateCapacity(capacity, enrolledCount, heldSeatCount);

        Id = id;
        OfferingId = offeringId;
        GroupCode = SchedulingDomainValue.Code(groupCode, nameof(groupCode));
        Capacity = capacity;
        EnrolledCount = enrolledCount;
        HeldSeatCount = heldSeatCount;
        State = state;
        RegistrationPaused = registrationPaused;
    }

    public Guid Id { get; private set; }

    public Guid OfferingId { get; private set; }

    public string GroupCode { get; private set; } = string.Empty;

    public int Capacity { get; private set; }

    public int EnrolledCount { get; private set; }

    public int HeldSeatCount { get; private set; }

    public int OccupiedSeatCount => EnrolledCount + HeldSeatCount;

    public int AvailableSeatCount => Capacity - OccupiedSeatCount;

    public SectionGroupState State { get; private set; }

    public bool RegistrationPaused { get; private set; }

    public IReadOnlyList<MeetingSlot> Meetings => _meetings;

    public IReadOnlyList<GroupStaffAssignment> StaffAssignments => _staffAssignments;

    public byte[] Version { get; private set; } = [];

    public bool IsSelectable =>
        State is SectionGroupState.Published
        && !RegistrationPaused
        && OccupiedSeatCount < Capacity;

    public void ReplaceSchedule(
        IReadOnlyList<MeetingSlot> meetings,
        IReadOnlyList<GroupStaffAssignment> staffAssignments)
    {
        ArgumentNullException.ThrowIfNull(meetings);
        ArgumentNullException.ThrowIfNull(staffAssignments);
        if (meetings.Any(meeting => meeting is null || meeting.GroupId != Id))
        {
            throw new ArgumentException(
                "Every meeting must belong to this group.",
                nameof(meetings));
        }

        var meetingIds = meetings.Select(meeting => meeting.Id).ToHashSet();
        if (meetingIds.Count != meetings.Count)
        {
            throw new ArgumentException("Meeting identifiers must be unique.", nameof(meetings));
        }

        if (staffAssignments.Any(assignment =>
            assignment is null
            || assignment.GroupId != Id
            || !meetingIds.Contains(assignment.MeetingSlotId)))
        {
            throw new ArgumentException(
                "Every staff assignment must target a meeting in this group.",
                nameof(staffAssignments));
        }

        var duplicateAssignment = staffAssignments
            .GroupBy(assignment => new
            {
                assignment.MeetingSlotId,
                assignment.StaffId,
                assignment.TeachingRole
            })
            .Any(group => group.Count() > 1);
        if (duplicateAssignment)
        {
            throw new ArgumentException(
                "Meeting, staff, and teaching-role assignments must be unique.",
                nameof(staffAssignments));
        }

        _meetings = Array.AsReadOnly(meetings.ToArray());
        _staffAssignments = Array.AsReadOnly(staffAssignments.ToArray());
    }

    public void ChangeCapacity(int capacity)
    {
        ValidateCapacity(capacity, EnrolledCount, HeldSeatCount);
        Capacity = capacity;
    }

    public void Rename(string groupCode) =>
        GroupCode = SchedulingDomainValue.Code(groupCode, nameof(groupCode));

    public void AllocateSeat()
    {
        if (!IsSelectable)
        {
            throw new InvalidOperationException("The group is not selectable.");
        }

        EnrolledCount++;
    }

    public void HoldSeat()
    {
        if (!IsSelectable)
        {
            throw new InvalidOperationException("The group is not selectable.");
        }

        HeldSeatCount++;
    }

    public void ConsumeHeldSeat()
    {
        EnsureHeldSeatExists();
        HeldSeatCount--;
        EnrolledCount++;
    }

    public void ReleaseHeldSeat()
    {
        EnsureHeldSeatExists();
        HeldSeatCount--;
    }

    public void SetRegistrationPaused(bool paused) => RegistrationPaused = paused;

    public void MarkPublished()
    {
        if (State is not SectionGroupState.Draft)
        {
            throw new InvalidOperationException("Only a draft group can be published.");
        }

        State = SectionGroupState.Published;
    }

    public void Close()
    {
        if (State is not SectionGroupState.Published)
        {
            throw new InvalidOperationException("Only a published group can be closed.");
        }

        State = SectionGroupState.Closed;
    }

    public void Cancel()
    {
        if (State is SectionGroupState.Cancelled)
        {
            throw new InvalidOperationException("The group is already cancelled.");
        }

        State = SectionGroupState.Cancelled;
    }

    private void EnsureHeldSeatExists()
    {
        if (HeldSeatCount <= 0)
        {
            throw new InvalidOperationException("The group has no held seat.");
        }
    }

    private static void ValidateCapacity(
        int capacity,
        int enrolledCount,
        int heldSeatCount)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (enrolledCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(enrolledCount));
        }

        if (heldSeatCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heldSeatCount));
        }

        if (enrolledCount + heldSeatCount > capacity)
        {
            throw new ArgumentException(
                "Enrolled and held seats cannot exceed capacity.",
                nameof(enrolledCount));
        }
    }
}
