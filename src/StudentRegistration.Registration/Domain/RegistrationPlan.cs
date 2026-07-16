namespace StudentRegistration.Registration.Domain;

public enum RegistrationPlanState
{
    Draft = 1,
    ReviewBlocked = 2,
}

public sealed class RegistrationPlan
{
    private readonly List<RegistrationPlanItem> _items = [];

    private RegistrationPlan()
    {
    }

    public RegistrationPlan(
        Guid id,
        Guid studentId,
        Guid termId,
        decimal totalCredits,
        RegistrationPlanState state,
        IReadOnlyList<ScheduleConflict>? conflicts = null,
        ValidationSnapshot? validation = null)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(termId, nameof(termId));
        RegistrationPlanDomainGuard.Defined(state, nameof(state));
        if (totalCredits < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCredits));
        }

        Id = id;
        StudentId = studentId;
        TermId = termId;
        TotalCredits = totalCredits;
        State = state;
        Conflicts = conflicts is null
            ? []
            : Array.AsReadOnly(conflicts.ToArray());
        if (Conflicts.Any(conflict => conflict is null))
        {
            throw new ArgumentException(
                "Plan conflicts cannot contain null values.",
                nameof(conflicts));
        }

        Validation = validation;
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid TermId { get; private set; }

    public decimal TotalCredits { get; private set; }

    public RegistrationPlanState State { get; private set; }

    public bool ReviewBlocked => State is RegistrationPlanState.ReviewBlocked;

    public IReadOnlyList<ScheduleConflict> Conflicts { get; private set; } = [];

    public ValidationSnapshot? Validation { get; private set; }

    public byte[] Version { get; private set; } = [];

    public IReadOnlyList<RegistrationPlanItem> Items => _items;

    public void ReplaceSelections(
        IReadOnlyList<RegistrationPlanSelection> selections,
        decimal totalCredits,
        RegistrationPlanState state,
        IReadOnlyList<ScheduleConflict> conflicts,
        ValidationSnapshot validation)
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentNullException.ThrowIfNull(conflicts);
        ArgumentNullException.ThrowIfNull(validation);
        RegistrationPlanDomainGuard.Defined(state, nameof(state));
        if (totalCredits < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCredits));
        }

        if (selections.Any(selection => selection is null) ||
            selections.Select(selection => selection.OfferingId).Distinct().Count() !=
                selections.Count ||
            selections.Select(selection => selection.GroupId).Distinct().Count() !=
                selections.Count)
        {
            throw new ArgumentException(
                "Plan selections must contain one unique group per offering.",
                nameof(selections));
        }

        if (conflicts.Any(conflict => conflict is null))
        {
            throw new ArgumentException(
                "Plan conflicts cannot contain null values.",
                nameof(conflicts));
        }

        _items.Clear();
        _items.AddRange(selections.Select(selection => new RegistrationPlanItem(
            selection.ItemId,
            Id,
            selection.OfferingId,
            selection.GroupId,
            selection.OfferingVersion,
            selection.GroupVersion)));
        TotalCredits = totalCredits;
        State = state;
        Conflicts = Array.AsReadOnly(conflicts.ToArray());
        Validation = validation;
    }
}

internal static class RegistrationPlanDomainGuard
{
    public static void Identifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "A non-empty identifier is required.",
                parameterName);
        }
    }

    public static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException(
                "A non-empty value is required.",
                parameterName)
            : normalized;
    }

    public static void Defined<T>(T value, string parameterName)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
