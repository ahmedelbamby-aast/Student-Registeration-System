namespace StudentRegistration.Registration.Domain;

public sealed record ScheduleConflictParticipant
{
    public ScheduleConflictParticipant(
        Guid groupId,
        string groupCode,
        string courseCode,
        string subjectTitle,
        TimeOnly startLocal,
        TimeOnly endLocal)
    {
        RegistrationPlanDomainGuard.Identifier(groupId, nameof(groupId));
        if (endLocal <= startLocal)
        {
            throw new ArgumentException(
                "A participant interval must end after it starts.",
                nameof(endLocal));
        }

        GroupId = groupId;
        GroupCode = RegistrationPlanDomainGuard.Required(
            groupCode,
            nameof(groupCode));
        CourseCode = RegistrationPlanDomainGuard.Required(
            courseCode,
            nameof(courseCode));
        SubjectTitle = RegistrationPlanDomainGuard.Required(
            subjectTitle,
            nameof(subjectTitle));
        StartLocal = startLocal;
        EndLocal = endLocal;
    }

    public Guid GroupId { get; }

    public string GroupCode { get; }

    public string CourseCode { get; }

    public string SubjectTitle { get; }

    public TimeOnly StartLocal { get; }

    public TimeOnly EndLocal { get; }
}

public sealed record ScheduleConflictAction
{
    private static readonly HashSet<string> ActionKinds =
        new(StringComparer.Ordinal)
        {
            "change-group",
            "remove-group",
        };

    public ScheduleConflictAction(
        string action,
        Guid targetGroupId,
        string label,
        string route)
    {
        var normalizedAction = RegistrationPlanDomainGuard.Required(
            action,
            nameof(action));
        if (!ActionKinds.Contains(normalizedAction))
        {
            throw new ArgumentException(
                "A canonical conflict action is required.",
                nameof(action));
        }

        RegistrationPlanDomainGuard.Identifier(
            targetGroupId,
            nameof(targetGroupId));
        Action = normalizedAction;
        TargetGroupId = targetGroupId;
        Label = RegistrationPlanDomainGuard.Required(label, nameof(label));
        Route = RegistrationPlanDomainGuard.Required(route, nameof(route));
    }

    public string Action { get; }

    public Guid TargetGroupId { get; }

    public string Label { get; }

    public string Route { get; }
}

public sealed record ScheduleConflict
{
    private static readonly HashSet<string> Codes =
        new(StringComparer.Ordinal)
        {
            "MEETING_OVERLAP",
            "TRAVEL_BUFFER",
        };

    public ScheduleConflict(
        string code,
        ScheduleConflictParticipant first,
        ScheduleConflictParticipant second,
        DayOfWeek dayOfWeek,
        TimeOnly overlapStartLocal,
        TimeOnly overlapEndLocal,
        string message,
        IReadOnlyList<ScheduleConflictAction> actions)
    {
        var normalizedCode = RegistrationPlanDomainGuard.Required(
            code,
            nameof(code));
        if (!Codes.Contains(normalizedCode))
        {
            throw new ArgumentException(
                "A canonical conflict code is required.",
                nameof(code));
        }

        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        RegistrationPlanDomainGuard.Defined(dayOfWeek, nameof(dayOfWeek));
        if (first.GroupId == second.GroupId)
        {
            throw new ArgumentException(
                "A conflict must identify two different groups.",
                nameof(second));
        }

        var exactStart = first.StartLocal > second.StartLocal
            ? first.StartLocal
            : second.StartLocal;
        var exactEnd = first.EndLocal < second.EndLocal
            ? first.EndLocal
            : second.EndLocal;
        if (exactStart >= exactEnd ||
            overlapStartLocal != exactStart ||
            overlapEndLocal != exactEnd)
        {
            throw new ArgumentException(
                "The overlap must be the exact intersection of both intervals.");
        }

        ArgumentNullException.ThrowIfNull(actions);
        var copiedActions = actions.ToArray();
        if (copiedActions.Any(action => action is null) ||
            copiedActions.Select(action => (action.Action, action.TargetGroupId))
                .Distinct()
                .Count() != copiedActions.Length ||
            !HasCompleteActions(copiedActions, first.GroupId) ||
            !HasCompleteActions(copiedActions, second.GroupId) ||
            copiedActions.Any(action =>
                action.TargetGroupId != first.GroupId &&
                action.TargetGroupId != second.GroupId))
        {
            throw new ArgumentException(
                "Change and remove actions are required for both groups.",
                nameof(actions));
        }

        Code = normalizedCode;
        First = first;
        Second = second;
        DayOfWeek = dayOfWeek;
        OverlapStartLocal = overlapStartLocal;
        OverlapEndLocal = overlapEndLocal;
        Message = RegistrationPlanDomainGuard.Required(message, nameof(message));
        Actions = Array.AsReadOnly(copiedActions);
    }

    public string Code { get; }

    public ScheduleConflictParticipant First { get; }

    public ScheduleConflictParticipant Second { get; }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly OverlapStartLocal { get; }

    public TimeOnly OverlapEndLocal { get; }

    public string Message { get; }

    public IReadOnlyList<ScheduleConflictAction> Actions { get; }

    private static bool HasCompleteActions(
        IReadOnlyList<ScheduleConflictAction> actions,
        Guid groupId) =>
        actions.Any(action =>
            action.TargetGroupId == groupId && action.Action == "change-group") &&
        actions.Any(action =>
            action.TargetGroupId == groupId && action.Action == "remove-group");
}
