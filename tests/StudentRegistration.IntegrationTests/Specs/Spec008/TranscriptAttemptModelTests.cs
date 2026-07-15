using System.Reflection;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class TranscriptAttemptModelTests
{
    private static readonly DateTime ImportedAtUtc =
        new(2026, 7, 14, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_preserves_the_complete_sourced_attempt()
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var priorId = Guid.NewGuid();

        var attempt = new TranscriptAttempt(
            id,
            studentId,
            termId,
            priorId,
            "CC214",
            3m,
            "A",
            TranscriptAttemptStatus.Passed,
            "synthetic-transcript-import",
            "fixture-42/CC214",
            ImportedAtUtc);

        Assert.Equal(id, attempt.Id);
        Assert.Equal(studentId, attempt.StudentId);
        Assert.Equal(termId, attempt.TermId);
        Assert.Equal(priorId, attempt.SupersedesAttemptId);
        Assert.Equal("CC214", attempt.CourseCode);
        Assert.Equal(3m, attempt.Credits);
        Assert.Equal("A", attempt.GradeCode);
        Assert.Equal(TranscriptAttemptStatus.Passed, attempt.Status);
        Assert.Equal("synthetic-transcript-import", attempt.Source);
        Assert.Equal("fixture-42/CC214", attempt.SourceReference);
        Assert.Equal(ImportedAtUtc, attempt.ImportedAtUtc);
        Assert.Equal(DateTimeKind.Utc, attempt.ImportedAtUtc.Kind);
    }

    [Fact]
    public void Attempt_is_append_only_and_grade_and_supersession_are_explicitly_nullable()
    {
        var attempt = CreateAttempt(gradeCode: null, supersedesAttemptId: null);

        Assert.Null(attempt.GradeCode);
        Assert.Null(attempt.SupersedesAttemptId);
        Assert.True(typeof(TranscriptAttempt).IsSealed);
        Assert.All(
            typeof(TranscriptAttempt).GetProperties(BindingFlags.Instance | BindingFlags.Public),
            property => Assert.Null(property.SetMethod));
        Assert.Equal(
            [
                "CourseCode", "Credits", "GradeCode", "Id", "ImportedAtUtc",
                "Source", "SourceReference", "Status", "StudentId",
                "SupersedesAttemptId", "TermId"
            ],
            typeof(TranscriptAttempt).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            ["InProgress", "Passed", "Failed", "Withdrawn"],
            Enum.GetNames<TranscriptAttemptStatus>());
    }

    [Fact]
    public void Supersession_rejects_self_and_requires_the_same_student_course_and_term_chain()
    {
        var prior = CreateAttempt();
        var successor = CreateAttempt(
            studentId: prior.StudentId,
            termId: prior.TermId,
            supersedesAttemptId: prior.Id,
            courseCode: prior.CourseCode);

        Assert.True(successor.SupersedesCurrentLeaf(prior));
        Assert.False(CreateAttempt(supersedesAttemptId: null).SupersedesCurrentLeaf(prior));
        Assert.False(CreateAttempt(supersedesAttemptId: prior.Id).SupersedesCurrentLeaf(prior));
        Assert.False(CreateAttempt(
            studentId: prior.StudentId,
            termId: Guid.NewGuid(),
            supersedesAttemptId: prior.Id,
            courseCode: prior.CourseCode).SupersedesCurrentLeaf(prior));
        Assert.False(CreateAttempt(
            studentId: prior.StudentId,
            termId: prior.TermId,
            supersedesAttemptId: prior.Id,
            courseCode: "DIFFERENT").SupersedesCurrentLeaf(prior));

        Assert.Throws<ArgumentException>(() =>
            CreateAttempt(id: prior.Id, supersedesAttemptId: prior.Id));
    }

    [Fact]
    public void Constructor_rejects_missing_identity_invalid_credits_status_or_provenance_and_non_utc_import()
    {
        Assert.Throws<ArgumentException>(() => CreateAttempt(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateAttempt(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateAttempt(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateAttempt(supersedesAttemptId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateAttempt(courseCode: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAttempt(credits: 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAttempt(credits: -1m));
        Assert.Throws<ArgumentException>(() => CreateAttempt(gradeCode: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateAttempt(status: (TranscriptAttemptStatus)999));
        Assert.Throws<ArgumentException>(() => CreateAttempt(source: " "));
        Assert.Throws<ArgumentException>(() => CreateAttempt(sourceReference: " "));
        Assert.Throws<ArgumentException>(() => CreateAttempt(
            importedAtUtc: DateTime.SpecifyKind(ImportedAtUtc, DateTimeKind.Unspecified)));
    }

    private static TranscriptAttempt CreateAttempt(
        Guid? id = null,
        Guid? studentId = null,
        Guid? termId = null,
        Guid? supersedesAttemptId = null,
        string courseCode = "CC214",
        decimal credits = 3m,
        string? gradeCode = "B+",
        TranscriptAttemptStatus status = TranscriptAttemptStatus.Passed,
        string source = "synthetic-transcript-import",
        string sourceReference = "fixture-42/CC214",
        DateTime? importedAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            supersedesAttemptId,
            courseCode,
            credits,
            gradeCode,
            status,
            source,
            sourceReference,
            importedAtUtc ?? ImportedAtUtc);
}
