using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class RegistrationLockOrderTests
{
    [Fact]
    public async Task Registration_enters_spec008_then_locks_context_scopes_and_sorted_groups_before_final_validation_and_commit()
    {
        var events = new List<string>();
        var academicStore = new AcademicBoundaryStore(events);
        var localStore = new LocalTransactionStore(events, academicStore);
        var coordinator = Coordinator(academicStore, localStore);
        var firstGroup = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var secondGroup = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var plan = Plan([secondGroup, firstGroup]);

        var result = await coordinator.ExecuteRegistrationAsync(
            plan,
            _ =>
            {
                Assert.True(academicStore.InsideBoundary);
                events.Add("atomic-commit");
                return Task.CompletedTask;
            });

        Assert.Equal(RegistrationBoundaryOutcome.Committed, result.Outcome);
        Assert.Equal(
            [
                "academic-boundary",
                "registration-context",
                "scope:Catalogue:CATALOGUE-2026",
                "scope:Policy:DEMO-POC-2026.1",
                $"groups:{firstGroup:D},{secondGroup:D}",
                "final-re-read",
                "atomic-commit"
            ],
            events);
        Assert.Equal(RegistrationTransactionCoordinator.RequiredMutableInputs, localStore.Validation!.MutableInputs);
        Assert.Equal([firstGroup, secondGroup], localStore.Validation.GroupIds);
        Assert.Equal(plan.PlanId, localStore.Validation.PlanId);
        Assert.Equal("plan-row-version-1", localStore.Validation.ExpectedPlanRowVersion);
        Assert.Equal(
            "registration-context-version-1",
            localStore.Validation.ExpectedRegistrationContextVersion);
        Assert.Equal(plan.ReceivedAtUtc, localStore.Validation.ReceivedAtUtc);
    }

    [Fact]
    public async Task A_blocked_spec008_boundary_prevents_every_registration_lock_and_commit()
    {
        var events = new List<string>();
        var academicStore = new AcademicBoundaryStore(events, AcademicProfileStoreOutcome.HoldBlocked);
        var localStore = new LocalTransactionStore(events, academicStore);
        var coordinator = Coordinator(academicStore, localStore);
        var committed = false;

        var result = await coordinator.ExecuteRegistrationAsync(
            Plan([Guid.NewGuid()]),
            _ =>
            {
                committed = true;
                return Task.CompletedTask;
            });

        Assert.Equal(RegistrationBoundaryOutcome.HoldBlocked, result.Outcome);
        Assert.False(committed);
        Assert.Equal(["academic-boundary"], events);
    }

    [Fact]
    public async Task Public_lock_operations_normalize_scope_codes_sort_groups_and_reject_invalid_boundaries()
    {
        var events = new List<string>();
        var academicStore = new AcademicBoundaryStore(events);
        var localStore = new LocalTransactionStore(events, academicStore, requireAcademicBoundary: false);
        var coordinator = Coordinator(academicStore, localStore);
        var high = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var low = Guid.Parse("00000000-0000-0000-0000-000000000001");

        await coordinator.LockRegistrationContextAsync(Guid.NewGuid(), Guid.NewGuid());
        await coordinator.LockPolicyPublicationScopeAsync("  catalogue-2026 ", " demo-poc-2026.1 ");
        await coordinator.LockGroupVersionsAsync([high, low, high]);

        Assert.Equal("registration-context", events[0]);
        Assert.Equal("scope:Catalogue:CATALOGUE-2026", events[1]);
        Assert.Equal("scope:Policy:DEMO-POC-2026.1", events[2]);
        Assert.Equal($"groups:{low:D},{high:D}", events[3]);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            coordinator.LockPolicyPublicationScopeAsync(" ", "POLICY"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            coordinator.LockGroupVersionsAsync([Guid.Empty]));
    }

    [Fact]
    public async Task Complete_retry_restarts_the_canonical_boundary_and_full_lock_sequence()
    {
        var events = new List<string>();
        var academicStore = new AcademicBoundaryStore(events, failuresBeforeSuccess: 1);
        var localStore = new LocalTransactionStore(events, academicStore);
        var coordinator = Coordinator(academicStore, localStore);

        var result = await coordinator.ExecuteWithCompleteRetryAsync(
            Plan([Guid.NewGuid()]),
            _ =>
            {
                events.Add("atomic-commit");
                return Task.CompletedTask;
            },
            exception => exception is TimeoutException,
            maximumAttempts: 2);

        Assert.Equal(RegistrationBoundaryOutcome.Committed, result.Outcome);
        Assert.Equal(2, events.Count(item => item == "academic-boundary"));
        Assert.Equal(1, events.Count(item => item == "registration-context"));
        Assert.Equal(1, events.Count(item => item == "atomic-commit"));
    }

    [Fact]
    public async Task Retry_is_bounded_and_never_retries_a_non_transient_failure()
    {
        var coordinator = Coordinator(
            new AcademicBoundaryStore([]),
            new LocalTransactionStore([], null, requireAcademicBoundary: false));
        var attempts = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            coordinator.ExecuteWithRetryAsync<int>(
                _ =>
                {
                    attempts++;
                    throw new InvalidOperationException("deterministic");
                },
                exception => exception is TimeoutException,
                maximumAttempts: 3));

        Assert.Equal(1, attempts);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            coordinator.ExecuteWithRetryAsync(
                _ => Task.FromResult(0),
                _ => true,
                maximumAttempts: RegistrationTransactionCoordinator.MaximumRetryAttempts + 1));
    }

    [Fact]
    public void Coordinator_has_only_local_database_collaborators_and_no_process_lock()
    {
        var fields = typeof(RegistrationTransactionCoordinator)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.DoesNotContain(fields, field => field.FieldType == typeof(HttpClient));
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(SemaphoreSlim));
        Assert.DoesNotContain(fields, field => field.FieldType == typeof(Mutex));
        Assert.DoesNotContain(
            typeof(RegistrationTransactionCoordinator).Assembly.GetTypes(),
            type => type.Name.Contains("StudentTermRegistrationGuard", StringComparison.Ordinal));
    }

    private static RegistrationTransactionCoordinator Coordinator(
        AcademicBoundaryStore academicStore,
        IRegistrationLocalTransactionStore localStore) =>
        new(
            new StudentAcademicProfileService(academicStore, TimeProvider.System),
            localStore);

    private static RegistrationTransactionPlan Plan(IReadOnlyList<Guid> groups) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Convert.ToBase64String([1, 2, 3]),
            Guid.NewGuid(),
            "plan-row-version-1",
            "registration-context-version-1",
            DateTime.SpecifyKind(new DateTime(2026, 7, 17, 8, 0, 0), DateTimeKind.Utc),
            " catalogue-2026 ",
            " demo-poc-2026.1 ",
            groups);

    private sealed class AcademicBoundaryStore : IStudentAcademicProfileStore
    {
        private readonly List<string> _events;
        private readonly AcademicProfileStoreOutcome _outcome;
        private int _failuresBeforeSuccess;

        public AcademicBoundaryStore(
            List<string> events,
            AcademicProfileStoreOutcome outcome = AcademicProfileStoreOutcome.Succeeded,
            int failuresBeforeSuccess = 0)
        {
            _events = events;
            _outcome = outcome;
            _failuresBeforeSuccess = failuresBeforeSuccess;
        }

        public bool InsideBoundary { get; private set; }

        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public async Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default)
        {
            _events.Add("academic-boundary");
            if (_failuresBeforeSuccess-- > 0)
            {
                throw new TimeoutException("simulated transient SQL failure");
            }

            if (_outcome == AcademicProfileStoreOutcome.Succeeded)
            {
                InsideBoundary = true;
                try
                {
                    await commitCallback(cancellationToken);
                }
                finally
                {
                    InsideBoundary = false;
                }
            }

            return new(_outcome);
        }
    }

    private sealed class LocalTransactionStore : IRegistrationLocalTransactionStore
    {
        private readonly List<string> _events;
        private readonly AcademicBoundaryStore? _academicStore;
        private readonly bool _requireAcademicBoundary;

        public LocalTransactionStore(
            List<string> events,
            AcademicBoundaryStore? academicStore,
            bool requireAcademicBoundary = true)
        {
            _events = events;
            _academicStore = academicStore;
            _requireAcademicBoundary = requireAcademicBoundary;
        }

        public RegistrationFinalValidation? Validation { get; private set; }

        public Task LockRegistrationContextAsync(
            Guid studentId,
            Guid termId,
            CancellationToken cancellationToken)
        {
            RequireBoundary();
            _events.Add("registration-context");
            return Task.CompletedTask;
        }

        public Task LockSerializablePublicationScopeRangeAsync(
            RegistrationPublicationScope scope,
            string normalizedScopeCode,
            CancellationToken cancellationToken)
        {
            RequireBoundary();
            _events.Add($"scope:{scope}:{normalizedScopeCode}");
            return Task.CompletedTask;
        }

        public Task LockSectionGroupVersionsAsync(
            IReadOnlyList<Guid> sortedGroupIds,
            CancellationToken cancellationToken)
        {
            RequireBoundary();
            _events.Add($"groups:{string.Join(',', sortedGroupIds.Select(item => item.ToString("D")))}");
            return Task.CompletedTask;
        }

        public Task ReReadAndValidateAsync(
            RegistrationFinalValidation validation,
            CancellationToken cancellationToken)
        {
            RequireBoundary();
            Validation = validation;
            _events.Add("final-re-read");
            return Task.CompletedTask;
        }

        private void RequireBoundary()
        {
            if (_requireAcademicBoundary && _academicStore?.InsideBoundary != true)
            {
                throw new InvalidOperationException("Registration work escaped the SPEC-008 transaction boundary.");
            }
        }
    }
}
