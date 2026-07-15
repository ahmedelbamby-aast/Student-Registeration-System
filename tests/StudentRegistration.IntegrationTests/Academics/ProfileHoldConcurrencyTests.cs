using System.Reflection;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class ProfileHoldConcurrencyTests
{
    private static readonly DateTime NowUtc =
        new(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Transcript_and_provenance_page_independently_while_active_holds_remain_complete()
    {
        var profile = AcademicProfileDouble.Create(
            attempts: Enumerable.Range(1, 45)
                .Select(index => Attempt.Create($"COURSE-{index:000}")),
            provenance: Enumerable.Range(1, 27)
                .Select(index => new Provenance($"source-{index:000}")),
            holds: Enumerable.Range(1, 3)
                .Select(index => Hold.Active(blocksRegistration: index == 1)));

        var result = profile.Read(
            transcriptPage: 2,
            transcriptPageSize: 20,
            provenancePage: 3,
            provenancePageSize: 10,
            NowUtc);

        Assert.Equal(ProfileReadCode.Ready, result.Code);
        Assert.NotNull(result.Profile);
        Assert.Equal(20, result.Profile.TranscriptAttempts.Items.Count);
        Assert.Equal(45, result.Profile.TranscriptAttempts.TotalCount);
        Assert.Equal(7, result.Profile.Provenance.Items.Count);
        Assert.Equal(27, result.Profile.Provenance.TotalCount);
        Assert.Equal(3, result.Profile.ActiveHolds.Count);
        Assert.Contains(result.Profile.ActiveHolds, hold => hold.BlocksRegistration);

        Assert.Equal(
            ProfileReadCode.PageSizeInvalid,
            profile.Read(1, 101, 1, 20, NowUtc).Code);
        Assert.Equal(
            ProfileReadCode.PageSizeInvalid,
            profile.Read(1, 20, 1, 101, NowUtc).Code);
    }

    [Fact]
    public void More_than_one_hundred_active_holds_fails_closed_without_a_partial_profile()
    {
        var oneHundred = AcademicProfileDouble.Create(
            holds: Enumerable.Range(1, 100).Select(_ => Hold.Active(true)));
        var oneHundredOne = AcademicProfileDouble.Create(
            holds: Enumerable.Range(1, 101).Select(_ => Hold.Active(true)));

        var allowed = oneHundred.Read(1, 20, 1, 20, NowUtc);
        var blocked = oneHundredOne.Read(1, 20, 1, 20, NowUtc);

        Assert.Equal(100, allowed.Profile?.ActiveHolds.Count);
        Assert.Equal(ProfileReadCode.ProfileNotReady, blocked.Code);
        Assert.Null(blocked.Profile);
    }

    [Fact]
    public void Correction_requires_both_versions_and_stale_input_changes_nothing()
    {
        var profile = AcademicProfileDouble.Create();

        var accepted = profile.SetGpa(
            currentGpa: 3.5m,
            expectedStudentVersion: 1,
            expectedStudentTermVersion: 1);
        var stale = profile.SetGpa(
            currentGpa: 3.9m,
            expectedStudentVersion: 1,
            expectedStudentTermVersion: 1);

        Assert.Equal(CorrectionCode.Applied, accepted);
        Assert.Equal(CorrectionCode.StaleVersion, stale);
        Assert.Equal(3.5m, profile.CurrentGpa);
        Assert.Equal(2, profile.StudentVersion);
        Assert.Equal(2, profile.StudentTermVersion);
        Assert.Equal(1, profile.SuccessfulAuditCount);
    }

    [Fact]
    public void Transcript_correction_appends_one_successor_and_summary_counts_only_current_leaves()
    {
        var prior = Attempt.Create("CC214", credits: 3m, passed: false);
        var profile = AcademicProfileDouble.Create(attempts: [prior]);
        var immutableSnapshot = prior with { };

        var result = profile.SupersedeAttempt(
            prior.Id,
            credits: 4m,
            passed: true,
            expectedStudentVersion: 1,
            expectedStudentTermVersion: 1);

        Assert.Equal(CorrectionCode.Applied, result);
        Assert.Equal(2, profile.Attempts.Count);
        Assert.Equal(immutableSnapshot, profile.Attempts[0]);
        var successor = Assert.Single(
            profile.Attempts,
            attempt => attempt.SupersedesAttemptId == prior.Id);
        Assert.NotEqual(prior.Id, successor.Id);
        Assert.Equal("CC214", successor.CourseCode);
        var current = profile.CurrentTranscriptLeaves();
        Assert.Single(current);
        Assert.Equal(successor.Id, current[0].Id);
        Assert.Equal(4m, current.Sum(attempt => attempt.Credits));
    }

    [Fact]
    public async Task Hold_that_wins_the_student_term_boundary_prevents_consumer_callback()
    {
        var studentId = Guid.NewGuid();
        var blockedTermId = Guid.NewGuid();
        var otherTermId = Guid.NewGuid();
        var boundary = new StudentTermBoundaryDouble();
        var observedBlockedVersion = boundary.ReadVersion(studentId, blockedTermId);
        var observedOtherVersion = boundary.ReadVersion(studentId, otherTermId);
        var blockedCommits = 0;
        var otherCommits = 0;

        await boundary.AddBlockingHoldAsync(studentId, blockedTermId);
        var blocked = await boundary.RevalidateAndConsumeAsync(
            studentId,
            blockedTermId,
            observedBlockedVersion,
            () =>
            {
                Interlocked.Increment(ref blockedCommits);
                return Task.CompletedTask;
            });
        var unrelated = await boundary.RevalidateAndConsumeAsync(
            studentId,
            otherTermId,
            observedOtherVersion,
            () =>
            {
                Interlocked.Increment(ref otherCommits);
                return Task.CompletedTask;
            });

        Assert.Equal(RegistrationBoundaryCode.HoldBlocked, blocked);
        Assert.Equal(0, blockedCommits);
        Assert.Equal(RegistrationBoundaryCode.Committed, unrelated);
        Assert.Equal(1, otherCommits);
    }

    [Fact]
    public async Task Duplicate_scalar_corrections_are_rejected_before_store_or_audit()
    {
        IReadOnlyList<IReadOnlyList<AcademicProfileCorrectionOperation>> cases =
        [
            [new SetGpaOperation(3.1m, "gpa-1"), new SetGpaOperation(3.2m, "gpa-2")],
            [
                new SetEarnedCreditsOperation(60m, "credits-1"),
                new SetEarnedCreditsOperation(63m, "credits-2")
            ],
            [
                new SetStandingOperation("Active", "standing-1"),
                new SetStandingOperation("Probation", "standing-2")
            ]
        ];

        foreach (var operations in cases)
        {
            await AssertCorrectionRejectedBeforeStoreAsync(operations);
        }
    }

    [Fact]
    public async Task Contradictory_actions_for_one_hold_are_rejected_before_store_or_audit()
    {
        var holdId = Guid.NewGuid();
        var upsert = new UpsertHoldOperation(
            holdId.ToString("D"),
            "REGISTRATION-HOLD",
            "Resolve the governed hold.",
            true,
            NowUtc,
            null,
            "hold-upsert");

        await AssertCorrectionRejectedBeforeStoreAsync(
            [upsert, new RemoveHoldOperation(holdId.ToString("B"), "hold-remove")]);
        await AssertCorrectionRejectedBeforeStoreAsync(
            [upsert, upsert with { }]);
    }

    [Fact]
    public async Task Two_successors_for_one_attempt_are_rejected_before_store_or_audit()
    {
        var priorAttemptId = Guid.NewGuid();
        var first = new UpsertTranscriptAttemptOperation(
            priorAttemptId.ToString("D"),
            "CC214",
            "2026-1",
            3m,
            "B+",
            "passed",
            "attempt-1");
        var second = new UpsertTranscriptAttemptOperation(
            priorAttemptId.ToString("B"),
            "CC214",
            "2026-1",
            3m,
            "A",
            "passed",
            "attempt-2");

        await AssertCorrectionRejectedBeforeStoreAsync([first, second]);
    }

    [Fact]
    public void Production_profile_boundary_and_store_are_required_before_green()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");
        var infrastructure = Assembly.Load("StudentRegistration.Infrastructure.SqlServer");
        var service = academics.GetType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");

        Assert.True(
            service is not null,
            "StudentAcademicProfileService has not delivered the bounded profile, correction, and registration-boundary protocol.");
        var publicMethods = service!.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(publicMethods, method => method.Name.Contains("Read", StringComparison.Ordinal));
        Assert.Contains(publicMethods, method => method.Name.Contains("Correct", StringComparison.Ordinal));
        Assert.Contains(
            publicMethods,
            method => method.Name.Contains("Registration", StringComparison.Ordinal));
        Assert.True(
            infrastructure.GetType(
                "StudentRegistration.Infrastructure.SqlServer.Persistence.AcademicStore")
                is not null,
            "AcademicStore has not delivered the per-student/per-term SQL serialization boundary.");

        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs");
        RepositoryFiles.ContainsAll(
            source,
            "MaximumPageSize = 100",
            "MaximumActiveHolds = 100",
            "PROFILE_NOT_READY",
            "StudentTermAcademicState",
            "HOLD_BLOCKED",
            "SupersedesAttemptId");
    }

    private static async Task AssertCorrectionRejectedBeforeStoreAsync(
        IReadOnlyList<AcademicProfileCorrectionOperation> operations)
    {
        var store = new CapturingProfileStore();
        var service = new StudentAcademicProfileService(store, TimeProvider.System);
        var request = new AcademicProfileCorrectionRequest(
            Guid.NewGuid().ToString("D"),
            Convert.ToBase64String([1]),
            Convert.ToBase64String([1]),
            "Correct the synthetic academic profile.",
            "governed-test",
            operations);

        var result = await service.CorrectProfileAsync(
            Guid.NewGuid(),
            request,
            new AcademicCommandContext("admin-test", "correlation-test"));

        Assert.Equal(AcademicProfileOutcome.ValidationError, result.Outcome);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
        Assert.Equal(0, store.CorrectCalls);
        Assert.Empty(store.Commands);
    }

    private enum ProfileReadCode
    {
        Ready,
        PageSizeInvalid,
        ProfileNotReady
    }

    private enum CorrectionCode
    {
        Applied,
        StaleVersion,
        InvalidSupersession
    }

    private enum RegistrationBoundaryCode
    {
        Committed,
        HoldBlocked,
        StaleVersion
    }

    private sealed record PageDouble<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record ProfileProjection(
        PageDouble<Attempt> TranscriptAttempts,
        IReadOnlyList<Hold> ActiveHolds,
        PageDouble<Provenance> Provenance);

    private sealed record ProfileReadResult(
        ProfileReadCode Code,
        ProfileProjection? Profile);

    private sealed record Provenance(string Source);

    private sealed record Hold(
        Guid Id,
        bool BlocksRegistration,
        DateTime EffectiveFromUtc,
        DateTime? EffectiveToUtc)
    {
        public static Hold Active(bool blocksRegistration) =>
            new(Guid.NewGuid(), blocksRegistration, NowUtc.AddDays(-1), null);

        public bool IsActiveAt(DateTime instantUtc) =>
            EffectiveFromUtc <= instantUtc &&
            (EffectiveToUtc is null || instantUtc < EffectiveToUtc.Value);
    }

    private sealed record Attempt(
        Guid Id,
        Guid? SupersedesAttemptId,
        string CourseCode,
        decimal Credits,
        bool Passed)
    {
        public static Attempt Create(
            string courseCode,
            decimal credits = 3m,
            bool passed = true) =>
            new(Guid.NewGuid(), null, courseCode, credits, passed);
    }

    private sealed class AcademicProfileDouble
    {
        private const int MaximumPageSize = 100;
        private const int MaximumActiveHolds = 100;
        private readonly List<Attempt> _attempts;
        private readonly List<Provenance> _provenance;
        private readonly List<Hold> _holds;

        private AcademicProfileDouble(
            IEnumerable<Attempt> attempts,
            IEnumerable<Provenance> provenance,
            IEnumerable<Hold> holds)
        {
            _attempts = [.. attempts];
            _provenance = [.. provenance];
            _holds = [.. holds];
        }

        public decimal CurrentGpa { get; private set; } = 3.25m;

        public int StudentVersion { get; private set; } = 1;

        public int StudentTermVersion { get; private set; } = 1;

        public int SuccessfulAuditCount { get; private set; }

        public IReadOnlyList<Attempt> Attempts => _attempts;

        public static AcademicProfileDouble Create(
            IEnumerable<Attempt>? attempts = null,
            IEnumerable<Provenance>? provenance = null,
            IEnumerable<Hold>? holds = null) =>
            new(attempts ?? [], provenance ?? [], holds ?? []);

        public ProfileReadResult Read(
            int transcriptPage,
            int transcriptPageSize,
            int provenancePage,
            int provenancePageSize,
            DateTime nowUtc)
        {
            if (!ValidPage(transcriptPage, transcriptPageSize) ||
                !ValidPage(provenancePage, provenancePageSize))
            {
                return new(ProfileReadCode.PageSizeInvalid, null);
            }

            var activeHolds = _holds.Where(hold => hold.IsActiveAt(nowUtc)).ToArray();
            if (activeHolds.Length > MaximumActiveHolds)
            {
                return new(ProfileReadCode.ProfileNotReady, null);
            }

            return new(
                ProfileReadCode.Ready,
                new ProfileProjection(
                    Page(_attempts, transcriptPage, transcriptPageSize),
                    activeHolds,
                    Page(_provenance, provenancePage, provenancePageSize)));
        }

        public CorrectionCode SetGpa(
            decimal currentGpa,
            int expectedStudentVersion,
            int expectedStudentTermVersion)
        {
            if (!VersionsMatch(expectedStudentVersion, expectedStudentTermVersion))
            {
                return CorrectionCode.StaleVersion;
            }

            CurrentGpa = currentGpa;
            AdvanceVersionsAndAudit();
            return CorrectionCode.Applied;
        }

        public CorrectionCode SupersedeAttempt(
            Guid priorAttemptId,
            decimal credits,
            bool passed,
            int expectedStudentVersion,
            int expectedStudentTermVersion)
        {
            if (!VersionsMatch(expectedStudentVersion, expectedStudentTermVersion))
            {
                return CorrectionCode.StaleVersion;
            }

            var prior = _attempts.SingleOrDefault(attempt => attempt.Id == priorAttemptId);
            if (prior is null || _attempts.Any(
                attempt => attempt.SupersedesAttemptId == priorAttemptId))
            {
                return CorrectionCode.InvalidSupersession;
            }

            _attempts.Add(new Attempt(
                Guid.NewGuid(),
                prior.Id,
                prior.CourseCode,
                credits,
                passed));
            AdvanceVersionsAndAudit();
            return CorrectionCode.Applied;
        }

        public IReadOnlyList<Attempt> CurrentTranscriptLeaves()
        {
            var superseded = _attempts
                .Where(attempt => attempt.SupersedesAttemptId is not null)
                .Select(attempt => attempt.SupersedesAttemptId!.Value)
                .ToHashSet();
            return _attempts.Where(attempt => !superseded.Contains(attempt.Id)).ToArray();
        }

        private bool VersionsMatch(int student, int studentTerm) =>
            student == StudentVersion && studentTerm == StudentTermVersion;

        private void AdvanceVersionsAndAudit()
        {
            StudentVersion++;
            StudentTermVersion++;
            SuccessfulAuditCount++;
        }

        private static bool ValidPage(int page, int pageSize) =>
            page >= 1 && pageSize is >= 1 and <= MaximumPageSize;

        private static PageDouble<T> Page<T>(
            IReadOnlyList<T> source,
            int page,
            int pageSize) =>
            new(
                source.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
                page,
                pageSize,
                source.Count);
    }

    private sealed class StudentTermBoundaryDouble
    {
        private readonly Dictionary<(Guid StudentId, Guid TermId), BoundaryState> _states = [];

        public int ReadVersion(Guid studentId, Guid termId) =>
            State(studentId, termId).Version;

        public async Task AddBlockingHoldAsync(Guid studentId, Guid termId)
        {
            var state = State(studentId, termId);
            await state.Gate.WaitAsync();
            try
            {
                state.HasBlockingHold = true;
                state.Version++;
            }
            finally
            {
                state.Gate.Release();
            }
        }

        public async Task<RegistrationBoundaryCode> RevalidateAndConsumeAsync(
            Guid studentId,
            Guid termId,
            int observedVersion,
            Func<Task> commit)
        {
            var state = State(studentId, termId);
            await state.Gate.WaitAsync();
            try
            {
                if (state.HasBlockingHold)
                {
                    return RegistrationBoundaryCode.HoldBlocked;
                }

                if (state.Version != observedVersion)
                {
                    return RegistrationBoundaryCode.StaleVersion;
                }

                await commit();
                return RegistrationBoundaryCode.Committed;
            }
            finally
            {
                state.Gate.Release();
            }
        }

        private BoundaryState State(Guid studentId, Guid termId)
        {
            var key = (studentId, termId);
            if (!_states.TryGetValue(key, out var state))
            {
                state = new BoundaryState();
                _states.Add(key, state);
            }

            return state;
        }

        private sealed class BoundaryState
        {
            public SemaphoreSlim Gate { get; } = new(1, 1);

            public int Version { get; set; }

            public bool HasBlockingHold { get; set; }
        }
    }

    private sealed class CapturingProfileStore : IStudentAcademicProfileStore
    {
        public int CorrectCalls { get; private set; }

        public List<CorrectAcademicProfileStoreCommand> Commands { get; } = [];

        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.StorageUnavailable));

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.StorageUnavailable));

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            CorrectCalls++;
            Commands.Add(command);
            return Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.Succeeded));
        }

        public Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AcademicProfileStoreResult(
                AcademicProfileStoreOutcome.StorageUnavailable));
    }
}
