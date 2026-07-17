namespace StudentRegistration.Registration.Domain;

public sealed record ScheduleOptionSelection
{
    public ScheduleOptionSelection(
        Guid courseId,
        Guid offeringId,
        Guid groupId)
    {
        CourseId = Required(courseId, nameof(courseId));
        OfferingId = Required(offeringId, nameof(offeringId));
        GroupId = Required(groupId, nameof(groupId));
    }

    public Guid CourseId { get; }

    public Guid OfferingId { get; }

    public Guid GroupId { get; }

    private static Guid Required(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException(
                "A non-empty identifier is required.",
                parameterName)
            : value;
}

public sealed record ScheduleOption
{
    public ScheduleOption(
        int rank,
        IReadOnlyList<ScheduleOptionSelection> selections,
        IReadOnlyList<ScoreComponent> scoreExplanation)
    {
        if (rank is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rank),
                rank,
                "A recommendation rank must be between one and three.");
        }

        var selectionCopy = CopyRequired(
            selections,
            nameof(selections),
            "A complete option requires at least one selection.");
        EnsureUnique(
            selectionCopy,
            selection => selection.CourseId,
            "A schedule option must contain exactly one group per course.",
            nameof(selections));
        EnsureUnique(
            selectionCopy,
            selection => selection.OfferingId,
            "A schedule option must contain exactly one group per offering.",
            nameof(selections));
        EnsureUnique(
            selectionCopy,
            selection => selection.GroupId,
            "A group cannot appear more than once in a schedule option.",
            nameof(selections));

        Rank = rank;
        Selections = Array.AsReadOnly(
            selectionCopy
                .OrderBy(selection => selection.CourseId)
                .ThenBy(selection => selection.OfferingId)
                .ThenBy(selection => selection.GroupId)
                .ToArray());
        ScoreExplanation = Array.AsReadOnly(
            CopyRequired(
                scoreExplanation,
                nameof(scoreExplanation),
                "A schedule option requires a score explanation."));
    }

    public int Rank { get; }

    public IReadOnlyList<ScheduleOptionSelection> Selections { get; }

    public IReadOnlyList<ScoreComponent> ScoreExplanation { get; }

    private static T[] CopyRequired<T>(
        IReadOnlyList<T> values,
        string parameterName,
        string emptyMessage)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        if (values.Count == 0)
        {
            throw new ArgumentException(emptyMessage, parameterName);
        }

        if (values.Any(value => value is null))
        {
            throw new ArgumentException(
                "A schedule option collection cannot contain null values.",
                parameterName);
        }

        return values.ToArray();
    }

    private static void EnsureUnique(
        IReadOnlyCollection<ScheduleOptionSelection> selections,
        Func<ScheduleOptionSelection, Guid> keySelector,
        string message,
        string parameterName)
    {
        if (selections.Select(keySelector).Distinct().Count() != selections.Count)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
