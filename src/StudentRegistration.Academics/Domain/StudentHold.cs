namespace StudentRegistration.Academics.Domain;

public sealed class StudentHold
{
    public StudentHold(
        Guid id,
        Guid studentId,
        Guid termId,
        string code,
        string message,
        bool blocksRegistration,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string source,
        string sourceReference,
        DateTime importedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A student-hold identifier is required.", nameof(id));
        }

        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("A student identifier is required.", nameof(studentId));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term is required.", nameof(termId));
        }

        EnsureUtc(effectiveFromUtc, nameof(effectiveFromUtc));
        if (effectiveToUtc is { } endUtc)
        {
            EnsureUtc(endUtc, nameof(effectiveToUtc));
            if (endUtc <= effectiveFromUtc)
            {
                throw new ArgumentException(
                    "The effective end must be after the effective start.",
                    nameof(effectiveToUtc));
            }
        }

        EnsureUtc(importedAtUtc, nameof(importedAtUtc));

        Id = id;
        StudentId = studentId;
        TermId = termId;
        Code = Required(code, nameof(code));
        Message = Required(message, nameof(message));
        BlocksRegistration = blocksRegistration;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        Source = Required(source, nameof(source));
        SourceReference = Required(sourceReference, nameof(sourceReference));
        ImportedAtUtc = importedAtUtc;
    }

    public Guid Id { get; }

    public Guid StudentId { get; }

    public Guid TermId { get; }

    public string Code { get; }

    public string Message { get; }

    public bool BlocksRegistration { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public string Source { get; }

    public string SourceReference { get; }

    public DateTime ImportedAtUtc { get; }

    public bool IsActiveAt(DateTime instantUtc)
    {
        EnsureUtc(instantUtc, nameof(instantUtc));

        return EffectiveFromUtc <= instantUtc &&
            (EffectiveToUtc is null || instantUtc < EffectiveToUtc.Value);
    }

    private static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return normalized;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC instant is required.", parameterName);
        }
    }
}
