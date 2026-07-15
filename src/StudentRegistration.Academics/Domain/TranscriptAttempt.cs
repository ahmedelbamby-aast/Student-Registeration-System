namespace StudentRegistration.Academics.Domain;

public enum TranscriptAttemptStatus
{
    InProgress,
    Passed,
    Failed,
    Withdrawn
}

public sealed class TranscriptAttempt
{
    public TranscriptAttempt(
        Guid id,
        Guid studentId,
        Guid termId,
        Guid? supersedesAttemptId,
        string courseCode,
        decimal credits,
        string? gradeCode,
        TranscriptAttemptStatus status,
        string source,
        string sourceReference,
        DateTime importedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A transcript-attempt identifier is required.",
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

        if (supersedesAttemptId == Guid.Empty)
        {
            throw new ArgumentException(
                "A superseded attempt identifier cannot be empty.",
                nameof(supersedesAttemptId));
        }

        if (supersedesAttemptId == id)
        {
            throw new ArgumentException(
                "A transcript attempt cannot supersede itself.",
                nameof(supersedesAttemptId));
        }

        if (credits <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credits),
                "Attempted credits must be greater than zero.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "A declared transcript-attempt status is required.");
        }

        EnsureUtc(importedAtUtc, nameof(importedAtUtc));

        Id = id;
        StudentId = studentId;
        TermId = termId;
        SupersedesAttemptId = supersedesAttemptId;
        CourseCode = Required(courseCode, nameof(courseCode));
        Credits = credits;
        GradeCode = Optional(gradeCode, nameof(gradeCode));
        Status = status;
        Source = Required(source, nameof(source));
        SourceReference = Required(sourceReference, nameof(sourceReference));
        ImportedAtUtc = importedAtUtc;
    }

    public Guid Id { get; }

    public Guid StudentId { get; }

    public Guid TermId { get; }

    public Guid? SupersedesAttemptId { get; }

    public string CourseCode { get; }

    public decimal Credits { get; }

    public string? GradeCode { get; }

    public TranscriptAttemptStatus Status { get; }

    public string Source { get; }

    public string SourceReference { get; }

    public DateTime ImportedAtUtc { get; }

    public bool SupersedesCurrentLeaf(TranscriptAttempt prior)
    {
        ArgumentNullException.ThrowIfNull(prior);

        return SupersedesAttemptId == prior.Id &&
            StudentId == prior.StudentId &&
            TermId == prior.TermId &&
            string.Equals(CourseCode, prior.CourseCode, StringComparison.Ordinal);
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

    private static string? Optional(string? value, string parameterName)
    {
        if (value is null)
        {
            return null;
        }

        return Required(value, parameterName);
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC instant is required.", parameterName);
        }
    }
}
