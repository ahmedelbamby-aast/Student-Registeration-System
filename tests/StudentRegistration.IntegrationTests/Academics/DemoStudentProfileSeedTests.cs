using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class DemoStudentProfileSeedTests
{
    [Fact]
    public void Shared_profile_version_and_fixture_ordinal_rebuild_the_complete_same_graph()
    {
        var input = new AcademicSeedInput(
            SqlServerTestDatabaseFixture.SeedProfileVersion,
            FixtureOrdinal: 42,
            ApplicationUserId: Guid.Parse("00000000-0000-0000-0000-000000000042"),
            UniversityId: "AI2600042");

        var first = DemoStudentProfileSeedDouble.Build("Testing", input, syntheticOnly: true);
        var second = DemoStudentProfileSeedDouble.Build("Testing", input, syntheticOnly: true);

        Assert.Equal(first.LogicalFingerprint, second.LogicalFingerprint);
        Assert.Equal(first.Student, second.Student);
        Assert.Equal(first.TranscriptAttempts, second.TranscriptAttempts);
        Assert.Equal(first.Holds, second.Holds);
        Assert.Equal(first.StudentTermState, second.StudentTermState);
        Assert.Equal(first.Provenance, second.Provenance);
        Assert.Equal(input.ApplicationUserId, first.Student.ApplicationUserId);
        Assert.Equal(input.UniversityId, first.UniversityId);
        Assert.Equal("AI", first.Student.ProgramCode);
        Assert.False(string.IsNullOrWhiteSpace(first.Student.Cohort));
        Assert.InRange(first.Student.CurrentGpa, 0m, 4m);
        Assert.True(first.Student.EarnedCredits >= 0);
        Assert.False(string.IsNullOrWhiteSpace(first.Student.Standing));
        Assert.NotEmpty(first.TranscriptAttempts);
        Assert.Contains(first.Holds, hold => hold.BlocksRegistration);
        Assert.Contains(first.Holds, hold => !hold.BlocksRegistration);
        Assert.Equal(first.Student.Id, first.StudentTermState.StudentId);
        Assert.Equal(input.SeedProfileVersion, first.Student.DataVersion);
        Assert.Equal(DateTimeKind.Utc, first.Student.DataAsOfUtc.Kind);
        Assert.All(first.Provenance, value => Assert.Contains("synthetic", value));
    }

    [Fact]
    public void Reseeding_the_same_profile_and_ordinal_is_idempotent()
    {
        var store = new AcademicSeedStoreDouble();
        var input = new AcademicSeedInput(
            SqlServerTestDatabaseFixture.SeedProfileVersion,
            7,
            Guid.Parse("00000000-0000-0000-0000-000000000007"),
            "AI2600007");

        var first = store.Reconcile(input);
        var replay = store.Reconcile(input);

        Assert.Equal(first, replay);
        Assert.Equal(1, store.GraphCount);
        Assert.Equal(first.LogicalFingerprint, replay.LogicalFingerprint);
    }

    [Fact]
    public void Profile_or_ordinal_change_produces_a_different_logical_graph()
    {
        var original = DemoStudentProfileSeedDouble.Build(
            "Development",
            new AcademicSeedInput(
                "synthetic-fixture/1.0",
                1,
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "AI2600001"),
            syntheticOnly: true);
        var nextOrdinal = DemoStudentProfileSeedDouble.Build(
            "Development",
            new AcademicSeedInput(
                "synthetic-fixture/1.0",
                2,
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                "AI2600002"),
            syntheticOnly: true);
        var nextProfile = DemoStudentProfileSeedDouble.Build(
            "Development",
            new AcademicSeedInput(
                "synthetic-fixture/2.0",
                1,
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "AI2600001"),
            syntheticOnly: true);

        Assert.NotEqual(original.LogicalFingerprint, nextOrdinal.LogicalFingerprint);
        Assert.NotEqual(original.LogicalFingerprint, nextProfile.LogicalFingerprint);
    }

    [Fact]
    public void Production_or_non_synthetic_input_is_rejected_without_importing_personal_data()
    {
        var input = new AcademicSeedInput(
            "synthetic-fixture/1.0",
            1,
            Guid.NewGuid(),
            "AI2600001");

        Assert.Throws<InvalidOperationException>(() =>
            DemoStudentProfileSeedDouble.Build("Production", input, syntheticOnly: true));
        Assert.Throws<InvalidOperationException>(() =>
            DemoStudentProfileSeedDouble.Build("Testing", input, syntheticOnly: false));
        Assert.Equal(
            ["ApplicationUserId", "FixtureOrdinal", "SeedProfileVersion", "UniversityId"],
            typeof(AcademicSeedInput)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.DoesNotContain(
            typeof(SyntheticStudent).GetProperties(),
            property => property.Name is "Email" or "Phone" or "Address" or "BirthDate");
    }

    [Fact]
    public async Task Contributor_accepts_only_the_deterministic_shared_identity_link()
    {
        const int fixtureOrdinal = 42;
        const string seedProfileVersion = "synthetic-fixture/1.0";
        const string universityId = "AI2600042";
        var applicationUserId = StableIdentityId(universityId);
        var termId = Guid.NewGuid();
        var store = new CapturingSeedStore();
        var contributor = new DemoStudentProfileSeedContributor(store);

        var created = await contributor.ContributeAsync(
            "Testing",
            seedProfileVersion,
            fixtureOrdinal,
            applicationUserId,
            universityId,
            termId);
        var replayed = await contributor.ContributeAsync(
            "Testing",
            seedProfileVersion,
            fixtureOrdinal,
            applicationUserId,
            universityId.ToLowerInvariant(),
            termId);

        Assert.True(created.Created);
        Assert.False(replayed.Created);
        Assert.Equal(created.StudentId, replayed.StudentId);
        Assert.Equal(2, store.Commands.Count);
        Assert.All(store.Commands, command =>
        {
            Assert.Equal(seedProfileVersion, command.SeedProfileVersion);
            Assert.Equal(fixtureOrdinal, command.FixtureOrdinal);
            Assert.Equal(universityId, command.UniversityId);
            Assert.Equal(applicationUserId, command.Student.ApplicationUserId);
        });
    }

    [Fact]
    public async Task Contributor_rejects_identity_link_mismatch_before_reconcile()
    {
        const int fixtureOrdinal = 42;
        const string seedProfileVersion = "synthetic-fixture/1.0";
        const string universityId = "AI2600042";
        var applicationUserId = StableIdentityId(universityId);
        var termId = Guid.NewGuid();
        var store = new CapturingSeedStore();
        var contributor = new DemoStudentProfileSeedContributor(store);

        await Assert.ThrowsAsync<ArgumentException>(() => contributor.ContributeAsync(
            "Testing",
            seedProfileVersion,
            fixtureOrdinal,
            applicationUserId,
            "AI2600043",
            termId));
        await Assert.ThrowsAsync<ArgumentException>(() => contributor.ContributeAsync(
            "Development",
            seedProfileVersion,
            fixtureOrdinal,
            Guid.NewGuid(),
            universityId,
            termId));
        await Assert.ThrowsAsync<InvalidOperationException>(() => contributor.ContributeAsync(
            "Production",
            seedProfileVersion,
            fixtureOrdinal,
            applicationUserId,
            universityId,
            termId));

        Assert.Empty(store.Commands);
    }

    [Fact]
    public void Production_seed_contributor_must_accept_the_shared_version_and_ordinal_contract()
    {
        var contributor = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.DemoStudentProfileSeedContributor");

        Assert.True(
            contributor is not null,
            "DemoStudentProfileSeedContributor has not delivered the complete deterministic academic graph.");
        Assert.Contains(
            contributor!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method =>
            {
                var parameters = method.GetParameters()
                    .Select(parameter => parameter.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                return parameters.Contains("seedProfileVersion") &&
                    parameters.Contains("fixtureOrdinal");
            });

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs");
        RepositoryFiles.ContainsAll(
            source,
            "SeedProfileVersion",
            "FixtureOrdinal",
            "StudentTermAcademicState",
            "TranscriptAttempt",
            "StudentHold",
            "Development",
            "Testing",
            "Production",
            "Synthetic");
        Assert.DoesNotContain("Random.Shared", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }

    private static Guid StableIdentityId(string universityId)
    {
        var normalized = universityId.Trim().Normalize().ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record AcademicSeedInput(
        string SeedProfileVersion,
        int FixtureOrdinal,
        Guid ApplicationUserId,
        string UniversityId);

    private sealed record SyntheticStudent(
        Guid Id,
        Guid ApplicationUserId,
        string ProgramCode,
        string Cohort,
        decimal CurrentGpa,
        int EarnedCredits,
        string Standing,
        string Source,
        string DataVersion,
        DateTime DataAsOfUtc);

    private sealed record SyntheticTranscriptAttempt(
        Guid Id,
        Guid StudentId,
        Guid TermId,
        string CourseCode,
        int Credits,
        string Source);

    private sealed record SyntheticHold(
        Guid Id,
        Guid StudentId,
        Guid TermId,
        bool BlocksRegistration,
        string Source);

    private sealed record SyntheticStudentTermState(
        Guid Id,
        Guid StudentId,
        Guid TermId,
        string Source);

    private sealed record SyntheticAcademicGraph(
        string UniversityId,
        SyntheticStudent Student,
        IReadOnlyList<SyntheticTranscriptAttempt> TranscriptAttempts,
        IReadOnlyList<SyntheticHold> Holds,
        SyntheticStudentTermState StudentTermState,
        IReadOnlyList<string> Provenance,
        string LogicalFingerprint);

    private static class DemoStudentProfileSeedDouble
    {
        public static SyntheticAcademicGraph Build(
            string environmentName,
            AcademicSeedInput input,
            bool syntheticOnly)
        {
            if (environmentName is not ("Development" or "Testing") || !syntheticOnly)
            {
                throw new InvalidOperationException(
                    "Academic demo profiles are synthetic Development/Testing data only.");
            }

            if (string.IsNullOrWhiteSpace(input.SeedProfileVersion) ||
                input.FixtureOrdinal < 1 ||
                input.ApplicationUserId == Guid.Empty ||
                string.IsNullOrWhiteSpace(input.UniversityId))
            {
                throw new ArgumentException("A complete shared seed input is required.");
            }

            var identity = $"{input.SeedProfileVersion}:{input.FixtureOrdinal}";
            var studentId = StableGuid($"student:{identity}");
            var termId = StableGuid($"term:{input.SeedProfileVersion}");
            var asOfUtc = new DateTime(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc)
                .AddMinutes(input.FixtureOrdinal);
            var source = $"synthetic:{identity}";
            var student = new SyntheticStudent(
                studentId,
                input.ApplicationUserId,
                "AI",
                $"20{24 + input.FixtureOrdinal % 4}",
                2.5m + (input.FixtureOrdinal % 15) / 10m,
                30 + input.FixtureOrdinal % 90,
                "Active",
                source,
                input.SeedProfileVersion,
                asOfUtc);
            var attempts = new[]
            {
                new SyntheticTranscriptAttempt(
                    StableGuid($"attempt:{identity}:1"),
                    studentId,
                    termId,
                    "CC214",
                    3,
                    source),
                new SyntheticTranscriptAttempt(
                    StableGuid($"attempt:{identity}:2"),
                    studentId,
                    termId,
                    "AI201",
                    3,
                    source)
            };
            var holds = new[]
            {
                new SyntheticHold(
                    StableGuid($"hold:{identity}:blocking"),
                    studentId,
                    termId,
                    true,
                    source),
                new SyntheticHold(
                    StableGuid($"hold:{identity}:advisory"),
                    studentId,
                    termId,
                    false,
                    source)
            };
            var state = new SyntheticStudentTermState(
                StableGuid($"state:{identity}"),
                studentId,
                termId,
                source);
            var fingerprint = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    $"{input.UniversityId}|{student}|{string.Join("|", attempts)}|{string.Join("|", holds)}|{state}")));

            return new SyntheticAcademicGraph(
                input.UniversityId,
                student,
                attempts,
                holds,
                state,
                [$"synthetic seed {input.SeedProfileVersion}", $"synthetic ordinal {input.FixtureOrdinal}"],
                fingerprint);
        }

        private static Guid StableGuid(string value)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return new Guid(hash.AsSpan(0, 16));
        }
    }

    private sealed class AcademicSeedStoreDouble
    {
        private readonly Dictionary<(string Version, int Ordinal), SyntheticAcademicGraph> _graphs = [];

        public int GraphCount => _graphs.Count;

        public SyntheticAcademicGraph Reconcile(AcademicSeedInput input)
        {
            var key = (input.SeedProfileVersion, input.FixtureOrdinal);
            if (_graphs.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var graph = DemoStudentProfileSeedDouble.Build(
                "Testing",
                input,
                syntheticOnly: true);
            _graphs.Add(key, graph);
            return graph;
        }
    }

    private sealed class CapturingSeedStore : IDemoStudentProfileSeedStore
    {
        private readonly Dictionary<(string Version, int Ordinal), Guid> _students = [];

        public List<DemoStudentProfileSeedCommand> Commands { get; } = [];

        public Task<DemoStudentProfileSeedResult> ReconcileAsync(
            DemoStudentProfileSeedCommand command,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command);
            var key = (command.SeedProfileVersion, command.FixtureOrdinal);
            if (_students.TryGetValue(key, out var studentId))
            {
                return Task.FromResult(new DemoStudentProfileSeedResult(
                    studentId,
                    Created: false));
            }

            _students.Add(key, command.Student.Id);
            return Task.FromResult(new DemoStudentProfileSeedResult(
                command.Student.Id,
                Created: true));
        }
    }
}
