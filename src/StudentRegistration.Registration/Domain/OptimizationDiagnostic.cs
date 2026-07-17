namespace StudentRegistration.Registration.Domain;

public sealed record OptimizationMeetingInterval
{
    public OptimizationMeetingInterval(
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new ArgumentException(
                "A canonical .NET DayOfWeek value from zero through six is required.",
                nameof(dayOfWeek));
        }

        if (endLocal <= startLocal)
        {
            throw new ArgumentException(
                "A meeting interval must end after it starts.",
                nameof(endLocal));
        }

        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
    }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartLocal { get; }

    public TimeOnly EndLocal { get; }
}

public sealed record OptimizationDiagnosticMember
{
    private static readonly string[] AllowedActions =
        ["change-group", "remove-course"];

    public OptimizationDiagnosticMember(
        Guid courseId,
        Guid? groupId,
        OptimizationMeetingInterval? interval,
        string action)
    {
        if (courseId == Guid.Empty)
        {
            throw new ArgumentException(
                "A course identifier is required.",
                nameof(courseId));
        }

        if (groupId == Guid.Empty)
        {
            throw new ArgumentException(
                "A supplied group identifier cannot be empty.",
                nameof(groupId));
        }

        var normalizedAction = action?.Trim() ?? string.Empty;
        if (!AllowedActions.Contains(normalizedAction, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "A diagnostic action must be change-group or remove-course.",
                nameof(action));
        }

        if (normalizedAction == "change-group" && !groupId.HasValue)
        {
            throw new ArgumentException(
                "Changing a group requires the affected group identifier.",
                nameof(groupId));
        }

        CourseId = courseId;
        GroupId = groupId;
        Interval = interval;
        Action = normalizedAction;
    }

    public Guid CourseId { get; }

    public Guid? GroupId { get; }

    public OptimizationMeetingInterval? Interval { get; }

    public string Action { get; }
}

public sealed record OptimizationDiagnostic
{
    public const string InclusionMinimal = "inclusion-minimal";

    public OptimizationDiagnostic(
        string diagnosticId,
        string reasonCode,
        IReadOnlyList<OptimizationDiagnosticMember> members,
        OptimizationMeetingInterval? conflictingInterval = null)
    {
        DiagnosticId = Required(diagnosticId, nameof(diagnosticId));
        ReasonCode = StableCode(reasonCode);

        ArgumentNullException.ThrowIfNull(members);
        if (members.Count == 0)
        {
            throw new ArgumentException(
                "An inclusion-minimal diagnostic requires at least one member.",
                nameof(members));
        }

        if (members.Any(member => member is null))
        {
            throw new ArgumentException(
                "A diagnostic cannot contain a null member.",
                nameof(members));
        }

        var duplicateMember = members
            .GroupBy(member => (member.CourseId, member.GroupId))
            .Any(group => group.Count() > 1);
        if (duplicateMember)
        {
            throw new ArgumentException(
                "A diagnostic member may appear only once.",
                nameof(members));
        }

        var orderedMembers = members
            .OrderBy(member => member.CourseId)
            .ThenBy(member => member.GroupId ?? Guid.Empty)
            .ThenBy(member => member.Action, StringComparer.Ordinal)
            .ToArray();

        if (ReasonCode == "MEETING_OVERLAP")
        {
            ValidateMeetingOverlap(orderedMembers, conflictingInterval);
        }

        Members = Array.AsReadOnly(orderedMembers);
        ConflictingInterval = conflictingInterval;
    }

    public string DiagnosticId { get; }

    public string ReasonCode { get; }

    public IReadOnlyList<OptimizationDiagnosticMember> Members { get; }

    public OptimizationMeetingInterval? ConflictingInterval { get; }

    public string Minimality => InclusionMinimal;

    private static void ValidateMeetingOverlap(
        IReadOnlyList<OptimizationDiagnosticMember> members,
        OptimizationMeetingInterval? conflictingInterval)
    {
        if (members.Count < 2 ||
            conflictingInterval is null ||
            members.Any(member =>
                !member.GroupId.HasValue ||
                member.Interval is null))
        {
            throw new ArgumentException(
                "A meeting-overlap diagnostic requires both affected groups, their intervals, and the overlap interval.",
                nameof(members));
        }

        foreach (var member in members)
        {
            var interval = member.Interval!;
            if (interval.DayOfWeek != conflictingInterval.DayOfWeek ||
                conflictingInterval.StartLocal < interval.StartLocal ||
                conflictingInterval.EndLocal > interval.EndLocal)
            {
                throw new ArgumentException(
                    "The conflicting interval must be contained in every affected meeting interval.",
                    nameof(conflictingInterval));
            }
        }
    }

    private static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException(
                "A non-empty value is required.",
                parameterName)
            : normalized;
    }

    private static string StableCode(string? value)
    {
        var normalized = Required(value, nameof(value));
        if (!normalized.All(character =>
                character is >= 'A' and <= 'Z' ||
                character is >= '0' and <= '9' ||
                character == '_'))
        {
            throw new ArgumentException(
                "A stable reason code may contain only uppercase letters, digits, and underscores.",
                nameof(value));
        }

        return normalized;
    }
}
