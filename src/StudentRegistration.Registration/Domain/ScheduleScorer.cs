using System.Globalization;

namespace StudentRegistration.Registration.Domain;

public sealed class ScheduleCandidateMeeting
{
    public ScheduleCandidateMeeting(
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal,
        string location)
    {
        if (!Enum.IsDefined(dayOfWeek) || endLocal <= startLocal)
        {
            throw new ArgumentException(
                "A valid same-day meeting interval is required.");
        }

        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
        Location = Required(location, nameof(location));
    }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartLocal { get; }

    public TimeOnly EndLocal { get; }

    public string Location { get; }

    private static string Required(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(
                "A non-empty value is required.",
                parameterName)
            : value.Trim();
}

public sealed class ScheduleCandidateGroup
{
    public ScheduleCandidateGroup(
        Guid courseId,
        Guid offeringId,
        Guid groupId,
        decimal credits,
        string state,
        bool viable,
        IReadOnlyList<ScheduleCandidateMeeting> meetings)
    {
        if (courseId == Guid.Empty ||
            offeringId == Guid.Empty ||
            groupId == Guid.Empty)
        {
            throw new ArgumentException(
                "Course, offering, and group identifiers are required.");
        }

        if (credits <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credits),
                credits,
                "Credits must be positive.");
        }

        ArgumentNullException.ThrowIfNull(meetings);
        if (meetings.Any(meeting => meeting is null))
        {
            throw new ArgumentException(
                "Candidate meetings cannot contain null values.",
                nameof(meetings));
        }
        var orderedMeetings = meetings
            .OrderBy(meeting => meeting.DayOfWeek)
            .ThenBy(meeting => meeting.StartLocal)
            .ThenBy(meeting => meeting.EndLocal)
            .ToArray();
        for (var index = 1; index < orderedMeetings.Length; index++)
        {
            var previous = orderedMeetings[index - 1];
            var current = orderedMeetings[index];
            if (previous.DayOfWeek == current.DayOfWeek &&
                current.StartLocal < previous.EndLocal)
            {
                throw new ArgumentException(
                    "Meetings inside one candidate group cannot overlap.",
                    nameof(meetings));
            }
        }

        CourseId = courseId;
        OfferingId = offeringId;
        GroupId = groupId;
        Credits = credits;
        State = string.IsNullOrWhiteSpace(state)
            ? throw new ArgumentException(
                "A group state is required.",
                nameof(state))
            : state.Trim().ToLowerInvariant();
        Viable = viable;
        Meetings = Array.AsReadOnly(meetings.ToArray());
    }

    public Guid CourseId { get; }

    public Guid OfferingId { get; }

    public Guid GroupId { get; }

    public decimal Credits { get; }

    public string State { get; }

    public bool Viable { get; }

    public IReadOnlyList<ScheduleCandidateMeeting> Meetings { get; }
}

public sealed class ScheduleScorer
{
    private readonly OptimizerConfiguration _configuration;

    public ScheduleScorer(OptimizerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    public string ConfigurationVersion => _configuration.Version;

    public IReadOnlyList<ScoreComponent> Score(
        IReadOnlyList<ScheduleCandidateGroup> schedule,
        SchedulePreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(preferences);

        var vector = Vector(schedule, preferences);
        return
        [
            new(
                ScoreFactor.PreferenceViolations,
                vector.PreferenceViolations.ToString(CultureInfo.InvariantCulture),
                "Meetings outside the approved day and time preferences."),
            new(
                ScoreFactor.IdleMinutes,
                vector.IdleMinutes.ToString(CultureInfo.InvariantCulture),
                "Idle minutes between meetings on the same teaching day."),
            new(
                ScoreFactor.TeachingDays,
                vector.TeachingDays.ToString(CultureInfo.InvariantCulture),
                "Number of distinct teaching days."),
            new(
                ScoreFactor.StableGroupTuple,
                vector.StableGroupTuple,
                "Stable group identifier tie-break.")
        ];
    }

    public int Compare(
        IReadOnlyList<ScheduleCandidateGroup> first,
        IReadOnlyList<ScheduleCandidateGroup> second,
        SchedulePreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(preferences);

        var firstVector = Vector(first, preferences);
        var secondVector = Vector(second, preferences);
        foreach (var factor in _configuration.FactorOrder)
        {
            var comparison = factor switch
            {
                ScoreFactor.PreferenceViolations =>
                    firstVector.PreferenceViolations.CompareTo(
                        secondVector.PreferenceViolations),
                ScoreFactor.IdleMinutes =>
                    firstVector.IdleMinutes.CompareTo(secondVector.IdleMinutes),
                ScoreFactor.TeachingDays =>
                    firstVector.TeachingDays.CompareTo(secondVector.TeachingDays),
                ScoreFactor.StableGroupTuple =>
                    StringComparer.Ordinal.Compare(
                        firstVector.StableGroupTuple,
                        secondVector.StableGroupTuple),
                _ => throw new InvalidOperationException(
                    "The optimizer configuration contains an unknown factor.")
            };
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    private static ScoreVector Vector(
        IReadOnlyList<ScheduleCandidateGroup> schedule,
        SchedulePreferences preferences)
    {
        if (schedule.Any(group => group is null))
        {
            throw new ArgumentException(
                "A schedule cannot contain null groups.",
                nameof(schedule));
        }

        var meetings = schedule.SelectMany(group => group.Meetings).ToArray();
        var preferenceViolations = meetings.Count(meeting =>
            preferences.AvoidedWeekdays.Contains(meeting.DayOfWeek));
        preferenceViolations += meetings.Count(meeting =>
            preferences.EarliestPreferredStartLocal is { } earliest &&
            meeting.StartLocal < earliest);
        preferenceViolations += meetings.Count(meeting =>
            preferences.LatestPreferredEndLocal is { } latest &&
            meeting.EndLocal > latest);

        var idleMinutes = meetings
            .GroupBy(meeting => meeting.DayOfWeek)
            .Sum(day =>
            {
                var ordered = day
                    .OrderBy(meeting => meeting.StartLocal)
                    .ThenBy(meeting => meeting.EndLocal)
                    .ToArray();
                var idle = 0;
                var runningEnd = ordered[0].EndLocal;
                for (var index = 1; index < ordered.Length; index++)
                {
                    idle += Math.Max(
                        0,
                        (int)(ordered[index].StartLocal -
                            runningEnd).TotalMinutes);
                    if (ordered[index].EndLocal > runningEnd)
                    {
                        runningEnd = ordered[index].EndLocal;
                    }
                }

                return idle;
            });
        var tuple = string.Join(
            "|",
            schedule
                .OrderBy(group => group.CourseId)
                .ThenBy(group => group.GroupId)
                .Select(group => group.GroupId.ToString("N")));

        return new(
            preferenceViolations,
            idleMinutes,
            meetings.Select(meeting => meeting.DayOfWeek).Distinct().Count(),
            tuple);
    }

    private sealed record ScoreVector(
        int PreferenceViolations,
        int IdleMinutes,
        int TeachingDays,
        string StableGroupTuple);
}
