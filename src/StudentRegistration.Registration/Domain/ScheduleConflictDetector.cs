using System.Globalization;

namespace StudentRegistration.Registration.Domain;

public sealed class SelectedScheduleMeeting
{
    public SelectedScheduleMeeting(
        Guid meetingId,
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal)
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
        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
    }

    public Guid MeetingId { get; }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartLocal { get; }

    public TimeOnly EndLocal { get; }
}

public sealed class SelectedScheduleGroup
{
    public SelectedScheduleGroup(
        Guid groupId,
        string groupCode,
        string courseCode,
        string subjectTitle,
        IReadOnlyList<SelectedScheduleMeeting> meetings)
    {
        if (groupId == Guid.Empty)
        {
            throw new ArgumentException(
                "A group identifier is required.",
                nameof(groupId));
        }

        ArgumentNullException.ThrowIfNull(meetings);
        if (meetings.Any(meeting => meeting is null))
        {
            throw new ArgumentException(
                "Selected meetings cannot contain null values.",
                nameof(meetings));
        }

        GroupId = groupId;
        GroupCode = EligibilityDomainGuard.Required(groupCode, nameof(groupCode));
        CourseCode = EligibilityDomainGuard.Required(courseCode, nameof(courseCode));
        SubjectTitle = EligibilityDomainGuard.Required(subjectTitle, nameof(subjectTitle));
        Meetings = Array.AsReadOnly(meetings.ToArray());
    }

    public Guid GroupId { get; }

    public string GroupCode { get; }

    public string CourseCode { get; }

    public string SubjectTitle { get; }

    public IReadOnlyList<SelectedScheduleMeeting> Meetings { get; }
}

public sealed class ScheduleConflictDetector
{
    private const string MeetingOverlapCode = "MEETING_OVERLAP";
    private const string ChangeGroupAction = "change-group";
    private const string RemoveGroupAction = "remove-group";
    private const string ChangeGroupRoute = "/student/subjects";
    private const string RemoveGroupRoute = "/student/schedule";

    public IReadOnlyList<ScheduleConflict> Detect(
        IReadOnlyList<SelectedScheduleGroup> selectedGroups)
    {
        ArgumentNullException.ThrowIfNull(selectedGroups);
        if (selectedGroups.Any(group => group is null))
        {
            throw new ArgumentException(
                "Selected groups cannot contain null values.",
                nameof(selectedGroups));
        }

        var groups = selectedGroups
            .OrderBy(group => group.CourseCode, StringComparer.Ordinal)
            .ThenBy(group => group.GroupCode, StringComparer.Ordinal)
            .ThenBy(group => group.GroupId)
            .GroupBy(group => group.GroupId)
            .Select(group => group.First())
            .ToArray();
        var conflicts = new List<ScheduleConflict>();

        for (var firstIndex = 0; firstIndex < groups.Length; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1;
                 secondIndex < groups.Length;
                 secondIndex++)
            {
                AddConflicts(groups[firstIndex], groups[secondIndex], conflicts);
            }
        }

        return Array.AsReadOnly(conflicts.ToArray());
    }

    private static void AddConflicts(
        SelectedScheduleGroup firstGroup,
        SelectedScheduleGroup secondGroup,
        ICollection<ScheduleConflict> conflicts)
    {
        var firstMeetings = OrderedDistinctMeetings(firstGroup.Meetings);
        var secondMeetings = OrderedDistinctMeetings(secondGroup.Meetings);

        foreach (var firstMeeting in firstMeetings)
        {
            foreach (var secondMeeting in secondMeetings)
            {
                if (firstMeeting.DayOfWeek != secondMeeting.DayOfWeek ||
                    firstMeeting.StartLocal >= secondMeeting.EndLocal ||
                    secondMeeting.StartLocal >= firstMeeting.EndLocal)
                {
                    continue;
                }

                var overlapStart = firstMeeting.StartLocal > secondMeeting.StartLocal
                    ? firstMeeting.StartLocal
                    : secondMeeting.StartLocal;
                var overlapEnd = firstMeeting.EndLocal < secondMeeting.EndLocal
                    ? firstMeeting.EndLocal
                    : secondMeeting.EndLocal;
                var first = Participant(firstGroup, firstMeeting);
                var second = Participant(secondGroup, secondMeeting);
                var message =
                    $"Conflict: {first.CourseCode} {first.GroupCode} overlaps " +
                    $"{second.CourseCode} {second.GroupCode} from " +
                    $"{overlapStart.ToString("HH:mm", CultureInfo.InvariantCulture)} to " +
                    $"{overlapEnd.ToString("HH:mm", CultureInfo.InvariantCulture)}.";

                conflicts.Add(
                    new ScheduleConflict(
                        MeetingOverlapCode,
                        first,
                        second,
                        firstMeeting.DayOfWeek,
                        overlapStart,
                        overlapEnd,
                        message,
                        Actions(firstGroup, secondGroup)));
            }
        }
    }

    private static IReadOnlyList<SelectedScheduleMeeting> OrderedDistinctMeetings(
        IReadOnlyList<SelectedScheduleMeeting> meetings) =>
        meetings
            .OrderBy(meeting => meeting.DayOfWeek)
            .ThenBy(meeting => meeting.StartLocal)
            .ThenBy(meeting => meeting.EndLocal)
            .ThenBy(meeting => meeting.MeetingId)
            .GroupBy(meeting => meeting.MeetingId)
            .Select(group => group.First())
            .ToArray();

    private static ScheduleConflictParticipant Participant(
        SelectedScheduleGroup group,
        SelectedScheduleMeeting meeting) =>
        new(
            group.GroupId,
            group.GroupCode,
            group.CourseCode,
            group.SubjectTitle,
            meeting.StartLocal,
            meeting.EndLocal);

    private static IReadOnlyList<ScheduleConflictAction> Actions(
        SelectedScheduleGroup first,
        SelectedScheduleGroup second) =>
        [
            new(
                ChangeGroupAction,
                first.GroupId,
                $"Change {first.CourseCode} group {first.GroupCode}",
                ChangeGroupRoute),
            new(
                RemoveGroupAction,
                first.GroupId,
                $"Remove {first.CourseCode} group {first.GroupCode}",
                RemoveGroupRoute),
            new(
                ChangeGroupAction,
                second.GroupId,
                $"Change {second.CourseCode} group {second.GroupCode}",
                ChangeGroupRoute),
            new(
                RemoveGroupAction,
                second.GroupId,
                $"Remove {second.CourseCode} group {second.GroupCode}",
                RemoveGroupRoute)
        ];
}
