namespace StudentRegistration.Academics.Domain;

public sealed class StudentTermAcademicState
{
    private StudentTermAcademicState()
    {
    }

    public StudentTermAcademicState(
        Guid id,
        Guid studentId,
        Guid termId,
        int programTermOrdinal,
        decimal gpaAtStart,
        decimal earnedCreditsAtStart,
        string standingAtStart,
        string source,
        string sourceReference,
        string dataVersion,
        DateTime dataAsOfUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A student-term state identifier is required.",
                nameof(id));
        }

        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("A student identifier is required.", nameof(studentId));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term is required.", nameof(termId));
        }

        if (programTermOrdinal <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(programTermOrdinal),
                "The program-term ordinal must be positive.");
        }

        if (gpaAtStart is < 0m or > 4m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gpaAtStart),
                "Starting GPA must be between zero and four.");
        }

        if (earnedCreditsAtStart < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(earnedCreditsAtStart),
                "Starting earned credits cannot be negative.");
        }

        EnsureUtc(dataAsOfUtc, nameof(dataAsOfUtc));

        Id = id;
        StudentId = studentId;
        TermId = termId;
        ProgramTermOrdinal = programTermOrdinal;
        GpaAtStart = gpaAtStart;
        EarnedCreditsAtStart = earnedCreditsAtStart;
        StandingAtStart = Required(standingAtStart, nameof(standingAtStart));
        Source = Required(source, nameof(source));
        SourceReference = Required(sourceReference, nameof(sourceReference));
        DataVersion = Required(dataVersion, nameof(dataVersion));
        DataAsOfUtc = dataAsOfUtc;
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid TermId { get; private set; }

    public int ProgramTermOrdinal { get; private set; }

    public decimal GpaAtStart { get; private set; }

    public decimal EarnedCreditsAtStart { get; private set; }

    public string StandingAtStart { get; private set; } = string.Empty;

    public string Source { get; private set; } = string.Empty;

    public string SourceReference { get; private set; } = string.Empty;

    public string DataVersion { get; private set; } = string.Empty;

    public DateTime DataAsOfUtc { get; private set; }

    public byte[] Version { get; private set; } = [];

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
