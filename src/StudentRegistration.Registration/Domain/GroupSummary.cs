namespace StudentRegistration.Registration.Domain;

public sealed record GroupNonSelectableReason
{
    public GroupNonSelectableReason(string code, string message)
    {
        Code = EligibilityDomainGuard.Required(code, nameof(code));
        Message = EligibilityDomainGuard.Required(message, nameof(message));
    }

    public string Code { get; }

    public string Message { get; }
}

public sealed record GroupMeetingStaffSummary
{
    public GroupMeetingStaffSummary(string role, string name)
    {
        Role = EligibilityDomainGuard.Required(role, nameof(role));
        Name = EligibilityDomainGuard.Required(name, nameof(name));
    }

    public string Role { get; }

    public string Name { get; }
}

public sealed record GroupMeetingSummary
{
    public GroupMeetingSummary(
        Guid meetingId,
        string activity,
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal,
        string roomCode,
        string location,
        IReadOnlyList<GroupMeetingStaffSummary> staff)
    {
        if (meetingId == Guid.Empty)
        {
            throw new ArgumentException(
                "A meeting identifier is required.",
                nameof(meetingId));
        }

        if (!Enum.IsDefined(dayOfWeek) || endLocal <= startLocal)
        {
            throw new ArgumentException(
                "A valid same-day meeting interval is required.");
        }

        MeetingId = meetingId;
        Activity = EligibilityDomainGuard.Required(activity, nameof(activity));
        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
        RoomCode = EligibilityDomainGuard.Required(roomCode, nameof(roomCode));
        Location = EligibilityDomainGuard.Required(location, nameof(location));
        Staff = EligibilityDomainGuard.Copy(staff, nameof(staff));
    }

    public Guid MeetingId { get; }

    public string Activity { get; }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartLocal { get; }

    public TimeOnly EndLocal { get; }

    public string RoomCode { get; }

    public string Location { get; }

    public IReadOnlyList<GroupMeetingStaffSummary> Staff { get; }

    public bool Equals(GroupMeetingSummary? other) =>
        other is not null &&
        MeetingId == other.MeetingId &&
        string.Equals(Activity, other.Activity, StringComparison.Ordinal) &&
        DayOfWeek == other.DayOfWeek &&
        StartLocal == other.StartLocal &&
        EndLocal == other.EndLocal &&
        string.Equals(RoomCode, other.RoomCode, StringComparison.Ordinal) &&
        string.Equals(Location, other.Location, StringComparison.Ordinal) &&
        Staff.SequenceEqual(other.Staff);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MeetingId);
        hash.Add(Activity, StringComparer.Ordinal);
        hash.Add(DayOfWeek);
        hash.Add(StartLocal);
        hash.Add(EndLocal);
        hash.Add(RoomCode, StringComparer.Ordinal);
        hash.Add(Location, StringComparer.Ordinal);
        foreach (var member in Staff)
        {
            hash.Add(member);
        }

        return hash.ToHashCode();
    }
}

public sealed record GroupSummary
{
    private static readonly HashSet<string> States =
        new(StringComparer.Ordinal)
        {
            "draft",
            "published",
            "closed",
            "cancelled"
        };

    public GroupSummary(
        Guid groupId,
        string groupCode,
        string state,
        bool selectable,
        int capacity,
        int enrolledCount,
        IReadOnlyList<GroupNonSelectableReason> nonSelectableReasons,
        IReadOnlyList<GroupMeetingSummary> meetings,
        string rowVersion,
        int heldSeatCount = 0)
    {
        if (groupId == Guid.Empty)
        {
            throw new ArgumentException(
                "A group identifier is required.",
                nameof(groupId));
        }

        var normalizedState = EligibilityDomainGuard.Required(
            state,
            nameof(state)).ToLowerInvariant();
        if (!States.Contains(normalizedState))
        {
            throw new ArgumentException(
                "A canonical group lifecycle is required.",
                nameof(state));
        }

        if (capacity < 0 || enrolledCount < 0 || heldSeatCount < 0 ||
            enrolledCount + heldSeatCount > capacity)
        {
            throw new ArgumentException(
                "Group capacity and enrollment values are inconsistent.");
        }

        GroupId = groupId;
        GroupCode = EligibilityDomainGuard.Required(groupCode, nameof(groupCode));
        State = normalizedState;
        Selectable = selectable;
        Capacity = capacity;
        EnrolledCount = enrolledCount;
        HeldSeatCount = heldSeatCount;
        SeatsRemaining = capacity - enrolledCount - heldSeatCount;
        NonSelectableReasons = EligibilityDomainGuard.Copy(
            nonSelectableReasons,
            nameof(nonSelectableReasons));
        Meetings = EligibilityDomainGuard.Copy(meetings, nameof(meetings));
        RowVersion = EligibilityDomainGuard.Required(
            rowVersion,
            nameof(rowVersion));
    }

    public Guid GroupId { get; }

    public string GroupCode { get; }

    public string State { get; }

    public bool Selectable { get; }

    public int Capacity { get; }

    public int EnrolledCount { get; }

    public int HeldSeatCount { get; }

    public int SeatsRemaining { get; }

    public IReadOnlyList<GroupNonSelectableReason> NonSelectableReasons { get; }

    public IReadOnlyList<GroupMeetingSummary> Meetings { get; }

    public string RowVersion { get; }

    public bool Equals(GroupSummary? other) =>
        other is not null &&
        GroupId == other.GroupId &&
        string.Equals(GroupCode, other.GroupCode, StringComparison.Ordinal) &&
        string.Equals(State, other.State, StringComparison.Ordinal) &&
        Selectable == other.Selectable &&
        Capacity == other.Capacity &&
        EnrolledCount == other.EnrolledCount &&
        HeldSeatCount == other.HeldSeatCount &&
        SeatsRemaining == other.SeatsRemaining &&
        NonSelectableReasons.SequenceEqual(other.NonSelectableReasons) &&
        Meetings.SequenceEqual(other.Meetings) &&
        string.Equals(RowVersion, other.RowVersion, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GroupId);
        hash.Add(GroupCode, StringComparer.Ordinal);
        hash.Add(State, StringComparer.Ordinal);
        hash.Add(Selectable);
        hash.Add(Capacity);
        hash.Add(EnrolledCount);
        hash.Add(HeldSeatCount);
        hash.Add(SeatsRemaining);
        foreach (var reason in NonSelectableReasons)
        {
            hash.Add(reason);
        }

        foreach (var meeting in Meetings)
        {
            hash.Add(meeting);
        }

        hash.Add(RowVersion, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}
