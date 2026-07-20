using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Api.Composition;
using StudentRegistration.Api.Development;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec008;

public sealed class NFR_1EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-008-NFR-1.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-1EvidenceTests.cs";
    private static readonly DateTime BoundaryOpenUtc =
        new(2042, 9, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime BoundaryCloseUtc = BoundaryOpenUtc.AddHours(1);
    private static readonly Guid RegistrationTermId =
        Guid.Parse("00000000-0000-0000-0000-000000008001");

    [Fact]
    public void Every_spec008_current_time_dependency_uses_the_injected_authoritative_clock()
    {
        Type[] timeDependentComponents =
        [
            typeof(AcademicContextResolver),
            typeof(RegistrationWindowService),
            typeof(StudentAcademicProfileService),
            typeof(AcademicStore),
            typeof(DemoDatabaseInitializer)
        ];
        Assert.All(timeDependentComponents, component =>
        {
            var constructor = Assert.Single(component.GetConstructors());
            Assert.Contains(
                constructor.GetParameters(),
                parameter => parameter.ParameterType == typeof(TimeProvider));
        });

        string[] currentTimeSources =
        [
            "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs",
            "src/StudentRegistration.Academics/Application/RegistrationWindowService.cs",
            "src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs",
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/AcademicStore.cs",
            "src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs"
        ];
        string[] forbiddenWallClocks =
        [
            "DateTime.UtcNow",
            "DateTime.Now",
            "DateTime.Today",
            "DateTimeOffset.UtcNow",
            "DateTimeOffset.Now"
        ];
        Assert.All(currentTimeSources, path =>
        {
            var source = RepositoryFiles.Read(path);
            Assert.All(forbiddenWallClocks, token =>
                Assert.DoesNotContain(token, source, StringComparison.Ordinal));
        });

        var fakeClock = new MutableTimeProvider(new DateTimeOffset(BoundaryOpenUtc));
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(fakeClock);
        services.AddAuthoritativeTime();
        using var provider = services.BuildServiceProvider();
        Assert.Same(fakeClock, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public async Task Fake_clock_owns_opening_and_closing_half_open_boundaries_not_device_time()
    {
        var fakeClock = new MutableTimeProvider(new DateTimeOffset(BoundaryOpenUtc));
        var reader = ReadyReader(
            Window(
                Guid.Parse("00000000-0000-0000-0000-000000008011"),
                BoundaryOpenUtc,
                BoundaryCloseUtc));
        var resolver = Resolver(reader, fakeClock);
        var deviceBeforeUtc = DateTime.UtcNow;

        var atOpening = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        fakeClock.UtcNow = new DateTimeOffset(BoundaryCloseUtc);
        var atClosing = await resolver.ResolveAsync(new AcademicStudentScope("AI", "2026"));
        var deviceAfterUtc = DateTime.UtcNow;

        Assert.Equal(BoundaryOpenUtc, atOpening.ServerTimeUtc);
        Assert.Equal(RegistrationWindowState.Open, atOpening.RegistrationWindowState);
        Assert.Equal(BoundaryCloseUtc, atClosing.ServerTimeUtc);
        Assert.Equal(RegistrationWindowState.Closed, atClosing.RegistrationWindowState);
        Assert.False(atOpening.ServerTimeUtc >= deviceBeforeUtc &&
            atOpening.ServerTimeUtc <= deviceAfterUtc);
        Assert.Equal(2, reader.ReadCount);
    }

    [Fact]
    public async Task Ambiguous_terms_and_open_windows_fail_closed_independent_of_input_order()
    {
        var clock = new MutableTimeProvider(new DateTimeOffset(BoundaryOpenUtc.AddMinutes(30)));
        var firstTerm = RegistrationTerm(
            Guid.Parse("00000000-0000-0000-0000-000000008001"));
        var secondTerm = RegistrationTerm(
            Guid.Parse("00000000-0000-0000-0000-000000008002"));

        var forwardTerms = await ResolveFailureAsync(
            new FakeAcademicContextReader([firstTerm, secondTerm], []),
            clock);
        var reverseTerms = await ResolveFailureAsync(
            new FakeAcademicContextReader([secondTerm, firstTerm], []),
            clock);
        Assert.Equal("CONTEXT_UNAVAILABLE", forwardTerms.Code);
        Assert.Equal(forwardTerms.Code, reverseTerms.Code);
        Assert.Equal(forwardTerms.Message, reverseTerms.Message);
        Assert.Null(forwardTerms.PartialContext);
        Assert.Null(reverseTerms.PartialContext);

        var firstWindow = Window(
            Guid.Parse("00000000-0000-0000-0000-000000008021"),
            BoundaryOpenUtc,
            BoundaryCloseUtc);
        var secondWindow = Window(
            Guid.Parse("00000000-0000-0000-0000-000000008022"),
            BoundaryOpenUtc.AddMinutes(10),
            BoundaryCloseUtc.AddMinutes(10));
        var forwardWindows = await ResolveFailureAsync(
            new FakeAcademicContextReader([firstTerm], [firstWindow, secondWindow]),
            clock);
        var reverseWindows = await ResolveFailureAsync(
            new FakeAcademicContextReader([firstTerm], [secondWindow, firstWindow]),
            clock);
        Assert.Equal("CONTEXT_UNAVAILABLE", forwardWindows.Code);
        Assert.Equal(forwardWindows.Code, reverseWindows.Code);
        Assert.Equal(forwardWindows.Message, reverseWindows.Message);
        Assert.Null(forwardWindows.PartialContext);
        Assert.Null(reverseWindows.PartialContext);
    }

    [Fact]
    public async Task Profile_hold_and_registration_boundary_use_the_same_fake_clock()
    {
        var fakeClock = new MutableTimeProvider(new DateTimeOffset(BoundaryOpenUtc));
        var store = new CapturingProfileStore(ProfileSnapshot());
        var service = new StudentAcademicProfileService(store, fakeClock);

        var atOpening = await service.ReadOwnAsync(store.ApplicationUserId);
        fakeClock.UtcNow = new DateTimeOffset(BoundaryCloseUtc);
        var atClosing = await service.ReadOwnAsync(store.ApplicationUserId);
        var boundary = await service.ExecuteRegistrationBoundaryAsync(
            store.StudentId,
            RegistrationTermId,
            Convert.ToBase64String([2]),
            _ => Task.CompletedTask);

        Assert.Equal(AcademicProfileOutcome.Succeeded, atOpening.Outcome);
        Assert.Single(atOpening.Profile!.ActiveHolds);
        Assert.Equal(AcademicProfileOutcome.Succeeded, atClosing.Outcome);
        Assert.Empty(atClosing.Profile!.ActiveHolds);
        Assert.Equal(
            [BoundaryOpenUtc, BoundaryCloseUtc],
            store.ReadRequests.Select(request => request.ServerNowUtc).ToArray());
        Assert.Equal(RegistrationBoundaryOutcome.Committed, boundary.Outcome);
        Assert.Equal(BoundaryCloseUtc, store.BoundaryCommand!.ServerReceivedAtUtc);
    }

    [Fact]
    public async Task Term_owner_stamps_audit_with_the_injected_fake_clock()
    {
        var fakeClock = new MutableTimeProvider(new DateTimeOffset(BoundaryOpenUtc));
        var store = new CapturingRegistrationWindowStore();
        var service = new RegistrationWindowService(store, fakeClock);
        var request = new CreateTermRequest(
            Guid.Parse("00000000-0000-0000-0000-000000008031"),
            "Create a deterministic quality-gate term.",
            "SPEC-008-NFR-1",
            new TermInput(
                "2042-FALL",
                "Fall 2042",
                "Africa/Cairo",
                new DateOnly(2042, 9, 1),
                new DateOnly(2043, 1, 15),
                TermState.Draft),
            []);

        var result = await service.CreateTermAsync(
            request,
            new AcademicCommandContext("admin:nfr-1", "nfr-1-correlation"));

        Assert.Equal(AcademicTermCommandOutcome.StorageUnavailable, result.Outcome);
        Assert.NotNull(store.CreateCommand);
        Assert.Equal(BoundaryOpenUtc, store.CreateCommand!.AuditEvent.OccurredAtUtc);
        Assert.Equal("academic-term-created", store.CreateCommand.AuditEvent.Action);
        Assert.Equal("2042-FALL", store.CreateCommand.Term.Code);
    }

    [Fact]
    public void Evidence_is_bound_to_this_executable_suite_and_keeps_nfr2_pending()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var source = RepositoryFiles.Read(TestPath).Replace("\r\n", "\n", StringComparison.Ordinal);
        var sourceHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));

        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-008 NFR-1 Fake-Clock Boundary Evidence",
            "TimeProvider",
            "opening instant is inclusive",
            "closing instant is exclusive",
            "device clock",
            "CONTEXT_UNAVAILABLE",
            "6 passed",
            "0 failed",
            "NFR-2 status: PENDING",
            "180,000",
            "ten-minute",
            sourceHash,
            "**Result: PASS.**");
        Assert.DoesNotContain(
            "NFR-2 status: PASS",
            evidence,
            StringComparison.OrdinalIgnoreCase);
    }

    private static AcademicContextResolver Resolver(
        IAcademicContextReader reader,
        TimeProvider clock) =>
        new(reader, clock, new AcademicContextOptions("Africa/Cairo"));

    private static FakeAcademicContextReader ReadyReader(
        params RegistrationWindowContextRecord[] windows) =>
        new([RegistrationTerm(RegistrationTermId)], windows);

    private static AcademicTermContextRecord RegistrationTerm(Guid id) =>
        new(
            id,
            $"TERM-{id.ToString("N")[^4..]}",
            "Registration term",
            "Africa/Cairo",
            new DateOnly(2042, 9, 1),
            new DateOnly(2043, 1, 15),
            TermState.RegistrationOpen,
            [1]);

    private static RegistrationWindowContextRecord Window(
        Guid id,
        DateTime opensAtUtc,
        DateTime closesAtUtc) =>
        new(
            id,
            RegistrationTermId,
            RegistrationWindowScopeType.Program,
            "AI",
            opensAtUtc,
            closesAtUtc,
            [1]);

    private static async Task<AcademicContextUnavailableException> ResolveFailureAsync(
        IAcademicContextReader reader,
        TimeProvider clock) =>
        await Assert.ThrowsAsync<AcademicContextUnavailableException>(
            () => Resolver(reader, clock).ResolveAsync(new AcademicStudentScope("AI", "2026")));

    private static AcademicProfileSnapshot ProfileSnapshot()
    {
        var applicationUserId =
            Guid.Parse("00000000-0000-0000-0000-000000008041");
        var studentId = Guid.Parse("00000000-0000-0000-0000-000000008042");
        var student = new Student(
            studentId,
            applicationUserId,
            "AI",
            "2026",
            3.25m,
            45m,
            "active",
            true,
            "synthetic-quality",
            "SPEC-008-NFR-1",
            "SPEC008-NFR1-V1",
            BoundaryOpenUtc,
            BoundaryOpenUtc);
        var state = new StudentTermAcademicState(
            Guid.Parse("00000000-0000-0000-0000-000000008043"),
            studentId,
            RegistrationTermId,
            3,
            3.25m,
            45m,
            "active",
            "synthetic-quality",
            "SPEC-008-NFR-1",
            "SPEC008-NFR1-V1",
            BoundaryOpenUtc);
        var hold = new StudentHold(
            Guid.Parse("00000000-0000-0000-0000-000000008044"),
            studentId,
            RegistrationTermId,
            "BOUNDARY",
            "Fake-clock boundary hold.",
            true,
            BoundaryOpenUtc,
            BoundaryCloseUtc,
            "synthetic-quality",
            "SPEC-008-NFR-1",
            BoundaryOpenUtc);
        return new AcademicProfileSnapshot(
            "AI2608041",
            student,
            state,
            [1],
            [2],
            new TranscriptSummaryDto(0, 0, 0),
            new Page<TranscriptAttemptDto>([], 1, 20, 0, "importedAtUtc,id"),
            [hold],
            new Page<AcademicProvenanceDto>(
                [
                    new AcademicProvenanceDto(
                        "synthetic-quality",
                        "SPEC-008-NFR-1",
                        BoundaryOpenUtc)
                ],
                1,
                20,
                1,
                "importedAtUtc,id"));
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    private sealed class FakeAcademicContextReader(
        IReadOnlyList<AcademicTermContextRecord> terms,
        IReadOnlyList<RegistrationWindowContextRecord> windows)
        : IAcademicContextReader
    {
        public int ReadCount { get; private set; }

        public Task<AcademicContextSnapshot> ResolveContextAsync(
            AcademicStudentScope? studentScope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            var applicable = windows
                .Where(window => studentScope is null || window.AppliesTo(studentScope))
                .ToArray();
            return Task.FromResult(new AcademicContextSnapshot(terms, applicable));
        }
    }

    private sealed class CapturingRegistrationWindowStore : IRegistrationWindowStore
    {
        public CreateAcademicTermStoreCommand? CreateCommand { get; private set; }

        public Task<AcademicTermCreationStoreResult> CreateOrReplayTermAsync(
            CreateAcademicTermStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateCommand = command;
            return Task.FromResult(new AcademicTermCreationStoreResult(
                AcademicTermCreationOutcome.StorageUnavailable));
        }

        public Task<AcademicTermMutationStoreResult> UpdateTermAsync(
            UpdateAcademicTermStoreCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicTermMutationStoreResult(
                AcademicTermMutationOutcome.StorageUnavailable));

        public Task<AcademicTermMutationStoreResult> PublishRegistrationWindowAsync(
            PublishRegistrationWindowStoreCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicTermMutationStoreResult(
                AcademicTermMutationOutcome.StorageUnavailable));
    }

    private sealed class CapturingProfileStore(AcademicProfileSnapshot profile)
        : IStudentAcademicProfileStore
    {
        private readonly AcademicProfileSnapshot _profile = profile;

        public Guid ApplicationUserId => _profile.Student.ApplicationUserId;

        public Guid StudentId => _profile.Student.Id;

        public List<AcademicProfileReadRequest> ReadRequests { get; } = [];

        public RegistrationBoundaryStoreCommand? BoundaryCommand { get; private set; }

        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(ApplicationUserId, applicationUserId);
            ReadRequests.Add(request);
            return Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.Succeeded,
                _profile));
        }

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.Succeeded,
                _profile));

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.Succeeded,
                _profile));

        public Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BoundaryCommand = command;
            return CompleteAsync(commitCallback, cancellationToken);
        }

        private async Task<AcademicProfileStoreResult> CompleteAsync(
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken)
        {
            await commitCallback(cancellationToken);
            return new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.Succeeded,
                _profile);
        }
    }
}
