namespace StudentRegistration.Academics.Domain;

public sealed class Student
{
    private Student()
    {
    }

    public Student(
        Guid id,
        Guid applicationUserId,
        string programCode,
        string cohort,
        decimal currentGpa,
        decimal earnedCredits,
        string standing,
        bool isActive,
        string source,
        string sourceReference,
        string dataVersion,
        DateTime dataAsOfUtc,
        DateTime importedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A student identifier is required.", nameof(id));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "An application user identifier is required.",
                nameof(applicationUserId));
        }

        if (currentGpa is < 0m or > 4m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentGpa),
                "GPA must be between zero and four.");
        }

        if (earnedCredits < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(earnedCredits),
                "Earned credits cannot be negative.");
        }

        EnsureUtc(dataAsOfUtc, nameof(dataAsOfUtc));
        EnsureUtc(importedAtUtc, nameof(importedAtUtc));

        Id = id;
        ApplicationUserId = applicationUserId;
        ProgramCode = Required(programCode, nameof(programCode));
        Cohort = Required(cohort, nameof(cohort));
        CurrentGpa = currentGpa;
        EarnedCredits = earnedCredits;
        Standing = Required(standing, nameof(standing));
        IsActive = isActive;
        Source = Required(source, nameof(source));
        SourceReference = Required(sourceReference, nameof(sourceReference));
        DataVersion = Required(dataVersion, nameof(dataVersion));
        DataAsOfUtc = dataAsOfUtc;
        ImportedAtUtc = importedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ApplicationUserId { get; private set; }

    public string ProgramCode { get; private set; } = string.Empty;

    public string Cohort { get; private set; } = string.Empty;

    public decimal CurrentGpa { get; private set; }

    public decimal EarnedCredits { get; private set; }

    public string Standing { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string SourceReference { get; private set; } = string.Empty;

    public string DataVersion { get; private set; } = string.Empty;

    public DateTime DataAsOfUtc { get; private set; }

    public DateTime ImportedAtUtc { get; private set; }

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
