namespace StudentRegistration.Registration.Domain;

public sealed record SchedulePreferences
{
    public SchedulePreferences(
        IReadOnlyList<DayOfWeek>? avoidedWeekdays = null,
        TimeOnly? earliestPreferredStartLocal = null,
        TimeOnly? latestPreferredEndLocal = null)
    {
        var copiedWeekdays = avoidedWeekdays?.ToArray() ?? [];
        foreach (var weekday in copiedWeekdays)
        {
            if (!Enum.IsDefined(weekday))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(avoidedWeekdays),
                    weekday,
                    "Every avoided weekday must use the canonical DayOfWeek range.");
            }
        }

        if (copiedWeekdays.Distinct().Count() != copiedWeekdays.Length)
        {
            throw new ArgumentException(
                "Avoided weekdays must be unique.",
                nameof(avoidedWeekdays));
        }

        if (earliestPreferredStartLocal is { } earliest &&
            latestPreferredEndLocal is { } latest &&
            earliest > latest)
        {
            throw new ArgumentException(
                "The earliest preferred start must not be after the latest preferred end.");
        }

        Array.Sort(copiedWeekdays);
        AvoidedWeekdays = Array.AsReadOnly(copiedWeekdays);
        EarliestPreferredStartLocal = earliestPreferredStartLocal;
        LatestPreferredEndLocal = latestPreferredEndLocal;
    }

    public IReadOnlyList<DayOfWeek> AvoidedWeekdays { get; }

    public TimeOnly? EarliestPreferredStartLocal { get; }

    public TimeOnly? LatestPreferredEndLocal { get; }

    public bool Equals(SchedulePreferences? other) =>
        other is not null &&
        AvoidedWeekdays.SequenceEqual(other.AvoidedWeekdays) &&
        EarliestPreferredStartLocal == other.EarliestPreferredStartLocal &&
        LatestPreferredEndLocal == other.LatestPreferredEndLocal;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var weekday in AvoidedWeekdays)
        {
            hash.Add(weekday);
        }

        hash.Add(EarliestPreferredStartLocal);
        hash.Add(LatestPreferredEndLocal);
        return hash.ToHashCode();
    }
}
