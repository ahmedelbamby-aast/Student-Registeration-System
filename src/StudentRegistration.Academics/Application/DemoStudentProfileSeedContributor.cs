using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Application;

public sealed class DemoStudentProfileSeedContributor
{
    private const string SyntheticSource = "Synthetic";
    private const string UniversityIdSeedPrefix = "AI26";
    private static readonly DateTime SeedEpochUtc =
        new(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc);

    private readonly IDemoStudentProfileSeedStore _store;

    public DemoStudentProfileSeedContributor(IDemoStudentProfileSeedStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task<DemoStudentProfileSeedResult> ContributeAsync(
        string environmentName,
        string seedProfileVersion,
        int fixtureOrdinal,
        Guid applicationUserId,
        string universityId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(environmentName, "Development", StringComparison.Ordinal) &&
            !string.Equals(environmentName, "Testing", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Production data is never an input to the synthetic academic contributor.");
        }

        if (string.IsNullOrWhiteSpace(seedProfileVersion))
        {
            throw new ArgumentException(
                "A seed profile version is required.",
                nameof(seedProfileVersion));
        }

        if (fixtureOrdinal is < 1 or > 25_000)
        {
            throw new ArgumentOutOfRangeException(nameof(fixtureOrdinal));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "An application user identifier is required.",
                nameof(applicationUserId));
        }

        if (string.IsNullOrWhiteSpace(universityId))
        {
            throw new ArgumentException("A University ID is required.", nameof(universityId));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term is required.", nameof(termId));
        }

        var normalizedVersion = seedProfileVersion.Trim();
        var normalizedUniversityId = universityId.Trim().Normalize().ToUpperInvariant();
        var expectedUniversityId = $"{UniversityIdSeedPrefix}{fixtureOrdinal:00000}";
        if (!string.Equals(
                normalizedUniversityId,
                expectedUniversityId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The University ID does not match the shared demo identity fixture.",
                nameof(universityId));
        }

        var expectedApplicationUserId = StableGuid(expectedUniversityId);
        if (applicationUserId != expectedApplicationUserId)
        {
            throw new ArgumentException(
                "The application user does not match the shared demo identity fixture.",
                nameof(applicationUserId));
        }

        var identity = $"{normalizedVersion}:{fixtureOrdinal}";
        var dataAsOfUtc = SeedEpochUtc.AddMinutes(fixtureOrdinal);
        var importedAtUtc = dataAsOfUtc.AddHours(1);
        var sourceReference =
            $"SeedProfileVersion={normalizedVersion};FixtureOrdinal={fixtureOrdinal}";
        var studentId = StableGuid($"student:{identity}");
        var student = new Student(
            studentId,
            applicationUserId,
            "AI",
            "2026",
            2.50m + (fixtureOrdinal % 15) / 10m,
            30m + fixtureOrdinal % 90,
            "Active",
            true,
            SyntheticSource,
            sourceReference,
            normalizedVersion,
            dataAsOfUtc,
            importedAtUtc);
        var transcriptAttempts = new[]
        {
            new TranscriptAttempt(
                StableGuid($"attempt:{identity}:CC214"),
                studentId,
                termId,
                supersedesAttemptId: null,
                "CC214",
                3m,
                "B+",
                TranscriptAttemptStatus.Passed,
                SyntheticSource,
                $"{sourceReference};CourseCode=CC214",
                importedAtUtc),
            new TranscriptAttempt(
                StableGuid($"attempt:{identity}:AI201"),
                studentId,
                termId,
                supersedesAttemptId: null,
                "AI201",
                3m,
                "A",
                TranscriptAttemptStatus.Passed,
                SyntheticSource,
                $"{sourceReference};CourseCode=AI201",
                importedAtUtc)
        };
        var holds = new[]
        {
            new StudentHold(
                StableGuid($"hold:{identity}:blocking"),
                studentId,
                termId,
                "REGISTRATION-HOLD",
                "Resolve this synthetic demo hold before registration.",
                true,
                dataAsOfUtc.AddDays(-1),
                effectiveToUtc: null,
                SyntheticSource,
                $"{sourceReference};Hold=blocking",
                importedAtUtc),
            new StudentHold(
                StableGuid($"hold:{identity}:advisory"),
                studentId,
                termId,
                "ADVISORY",
                "Synthetic advising reminder.",
                false,
                dataAsOfUtc.AddDays(-1),
                effectiveToUtc: null,
                SyntheticSource,
                $"{sourceReference};Hold=advisory",
                importedAtUtc)
        };
        var studentTermState = new StudentTermAcademicState(
            StableGuid($"student-term:{identity}:{termId:D}"),
            studentId,
            termId,
            student.CurrentGpa,
            student.EarnedCredits,
            student.Standing,
            SyntheticSource,
            $"{sourceReference};TermId={termId:D}",
            normalizedVersion,
            dataAsOfUtc);
        var provenance = new[]
        {
            new AcademicProvenanceDto(
                SyntheticSource,
                sourceReference,
                importedAtUtc),
            new AcademicProvenanceDto(
                SyntheticSource,
                $"{sourceReference};AcademicGraph=complete",
                importedAtUtc)
        };

        return _store.ReconcileAsync(
            new DemoStudentProfileSeedCommand(
                SeedProfileVersion: normalizedVersion,
                FixtureOrdinal: fixtureOrdinal,
                normalizedUniversityId,
                student,
                transcriptAttempts,
                holds,
                StudentTermAcademicState: studentTermState,
                provenance),
            cancellationToken);
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
