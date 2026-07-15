using System.Reflection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class RegistrationWindowConcurrencyTests
{
    [Fact]
    public async Task Any_scope_same_term_overlap_locks_stably_and_changes_nothing()
    {
        var termId = Id(100);
        var candidateId = Id(1);
        var existingId = Id(2);
        var store = Store(
            new WindowRow(
                candidateId,
                termId,
                "all-students",
                Utc(10),
                Utc(14),
                WindowLifecycle.Draft,
                Version: 3),
            new WindowRow(
                existingId,
                termId,
                "program:AI",
                Utc(8),
                Utc(12),
                WindowLifecycle.Published,
                Version: 7));
        var service = new RegistrationWindowServiceDouble(store);

        var result = await service.PublishAsync(
            new(termId, candidateId, ExpectedTermVersion: 5, ExpectedWindowVersion: 3));

        Assert.Equal(PublicationOutcome.WindowOverlap, result);
        Assert.Equal([termId, candidateId, existingId], Assert.Single(store.LockOrders));
        Assert.Equal("all-students", store.Window(candidateId).Scope);
        Assert.Equal("program:AI", store.Window(existingId).Scope);
        Assert.Equal(WindowLifecycle.Draft, store.Window(candidateId).Lifecycle);
        Assert.Equal(3, store.Window(candidateId).Version);
        Assert.Equal(WindowLifecycle.Published, store.Window(existingId).Lifecycle);
        Assert.Equal(7, store.Window(existingId).Version);
        Assert.Equal(5, store.TermVersion);
        Assert.Empty(store.AuditEntries);
        Assert.Equal(0, store.CommitCount);

        RequirePlannedService();
    }

    [Theory]
    [InlineData(4, 3)]
    [InlineData(5, 2)]
    public async Task Stale_term_or_window_version_rejects_before_mutation_and_audit(
        int expectedTermVersion,
        int expectedWindowVersion)
    {
        var termId = Id(100);
        var windowId = Id(1);
        var store = Store(new WindowRow(
            windowId,
            termId,
            "cohort:2026",
            Utc(8),
            Utc(12),
            WindowLifecycle.Draft,
            Version: 3));
        var service = new RegistrationWindowServiceDouble(store);

        var result = await service.PublishAsync(
            new(termId, windowId, expectedTermVersion, expectedWindowVersion));

        Assert.Equal(PublicationOutcome.StaleVersion, result);
        Assert.Equal(WindowLifecycle.Draft, store.Window(windowId).Lifecycle);
        Assert.Equal(3, store.Window(windowId).Version);
        Assert.Equal(5, store.TermVersion);
        Assert.Empty(store.AuditEntries);
        Assert.Equal(0, store.CommitCount);

        RequirePlannedService();
    }

    [Fact]
    public async Task Publish_accepts_only_a_current_draft_window_lifecycle()
    {
        var termId = Id(100);
        var windowId = Id(1);
        var store = Store(new WindowRow(
            windowId,
            termId,
            "all-students",
            Utc(8),
            Utc(12),
            WindowLifecycle.EmergencyClosed,
            Version: 3));
        var service = new RegistrationWindowServiceDouble(store);

        var result = await service.PublishAsync(new(termId, windowId, 5, 3));

        Assert.Equal(PublicationOutcome.InvalidLifecycle, result);
        Assert.Equal(WindowLifecycle.EmergencyClosed, store.Window(windowId).Lifecycle);
        Assert.Equal(3, store.Window(windowId).Version);
        Assert.Empty(store.AuditEntries);
        Assert.Equal(0, store.CommitCount);

        RequirePlannedService();
    }

    [Fact]
    public async Task Two_admins_publishing_overlapping_drafts_have_exactly_one_atomic_winner()
    {
        var termId = Id(100);
        var firstId = Id(1);
        var secondId = Id(2);
        var store = Store(
            new WindowRow(
                firstId,
                termId,
                "program:AI",
                Utc(8),
                Utc(12),
                WindowLifecycle.Draft,
                Version: 1),
            new WindowRow(
                secondId,
                termId,
                "cohort:2026",
                Utc(10),
                Utc(14),
                WindowLifecycle.Draft,
                Version: 1));
        var service = new RegistrationWindowServiceDouble(store);

        var results = await Task.WhenAll(
            service.PublishAsync(new(termId, firstId, 5, 1)),
            service.PublishAsync(new(termId, secondId, 5, 1)));

        Assert.Single(results, outcome => outcome == PublicationOutcome.Succeeded);
        Assert.Single(
            results,
            outcome => outcome is PublicationOutcome.StaleVersion or
                PublicationOutcome.WindowOverlap);
        Assert.Equal(
            1,
            store.Windows.Count(window => window.Lifecycle == WindowLifecycle.Published));
        Assert.Equal(
            1,
            store.Windows.Count(window => window.Lifecycle == WindowLifecycle.Draft));
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
        Assert.Equal(6, store.TermVersion);
        Assert.All(
            store.LockOrders,
            order => Assert.Equal([termId, firstId, secondId], order));

        RequirePlannedService();
    }

    [Fact]
    public async Task Store_fault_before_commit_discards_window_term_and_audit_staging()
    {
        var termId = Id(100);
        var windowId = Id(1);
        var store = Store(new WindowRow(
            windowId,
            termId,
            "all-students",
            Utc(8),
            Utc(12),
            WindowLifecycle.Draft,
            Version: 1));
        store.FailBeforeCommit = true;
        var service = new RegistrationWindowServiceDouble(store);

        var result = await service.PublishAsync(new(termId, windowId, 5, 1));

        Assert.Equal(PublicationOutcome.StorageFailure, result);
        Assert.Equal(WindowLifecycle.Draft, store.Window(windowId).Lifecycle);
        Assert.Equal(1, store.Window(windowId).Version);
        Assert.Equal(5, store.TermVersion);
        Assert.Empty(store.AuditEntries);
        Assert.Equal(0, store.CommitCount);

        RequirePlannedStorePort();
    }

    [Fact]
    public async Task Term_create_replay_is_payload_bound_without_a_second_receipt_model()
    {
        var store = Store();
        var service = new RegistrationWindowServiceDouble(store);
        var requestId = "term-create-2026-fall";
        var payloadHash = "SHA256:CANONICAL-A";

        var created = await service.CreateTermAsync(requestId, payloadHash);
        var replayed = await service.CreateTermAsync(requestId, payloadHash);
        var reused = await service.CreateTermAsync(requestId, "SHA256:DIFFERENT-B");

        Assert.Equal(TermCreationOutcome.Created, created.Outcome);
        Assert.Equal(TermCreationOutcome.Replayed, replayed.Outcome);
        Assert.Equal(created.TermId, replayed.TermId);
        Assert.Equal(TermCreationOutcome.IdempotencyKeyReused, reused.Outcome);
        Assert.Null(reused.TermId);
        var term = Assert.Single(store.Terms);
        Assert.Equal(requestId, term.CreationClientRequestId);
        Assert.Equal(payloadHash, term.CreationPayloadHash);
        Assert.Equal(1, store.TermCreateCommitCount);
        Assert.Single(store.TermCreateAuditEntries);

        RequirePlannedService();
    }

    [Fact]
    public void Planned_service_and_store_port_expose_the_frozen_transaction_boundary()
    {
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/RegistrationWindowService.cs");
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/Ports/IRegistrationWindowStore.cs");

        RepositoryFiles.ContainsAll(
            service,
            "RegistrationWindowService",
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersion",
            "CreationClientRequestId",
            "CreationPayloadHash");
        RepositoryFiles.ContainsAll(
            port,
            "IRegistrationWindowStore",
            "PublishRegistrationWindowAsync",
            "CancellationToken");
    }

    private static InMemoryRegistrationWindowStoreDouble Store(
        params WindowRow[] windows) =>
        new(Id(100), TermVersion: 5, windows);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private static DateTime Utc(int hour) =>
        new(2026, 7, 20, hour, 0, 0, DateTimeKind.Utc);

    private static void RequirePlannedService() => RequireAcademicsType(
        "StudentRegistration.Academics.Application.RegistrationWindowService");

    private static void RequirePlannedStorePort() => RequireAcademicsType(
        "StudentRegistration.Academics.Application.Ports.IRegistrationWindowStore");

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics type {fullName} is missing.");
    }

    private enum WindowLifecycle
    {
        Draft,
        Published,
        EmergencyClosed,
        Superseded
    }

    private enum PublicationOutcome
    {
        Succeeded,
        StaleVersion,
        WindowOverlap,
        InvalidLifecycle,
        StorageFailure
    }

    private enum TermCreationOutcome
    {
        Created,
        Replayed,
        IdempotencyKeyReused
    }

    private sealed record PublishCommand(
        Guid TermId,
        Guid WindowId,
        int ExpectedTermVersion,
        int ExpectedWindowVersion);

    private sealed record PublicationAudit(Guid TermId, Guid WindowId);

    private sealed record TermCreationResult(
        TermCreationOutcome Outcome,
        Guid? TermId);

    private sealed record TermRow(
        Guid Id,
        string CreationClientRequestId,
        string CreationPayloadHash);

    private sealed record WindowRow(
        Guid Id,
        Guid TermId,
        string Scope,
        DateTime OpensAtUtc,
        DateTime ClosesAtUtc,
        WindowLifecycle Lifecycle,
        int Version)
    {
        public WindowRow Copy() => this with { };
    }

    private sealed class PublicationTransaction(
        int termVersion,
        IReadOnlyList<WindowRow> windows)
    {
        public int TermVersion { get; set; } = termVersion;
        public List<WindowRow> Windows { get; } = windows.Select(window => window.Copy()).ToList();
        public List<PublicationAudit> AuditEntries { get; } = [];
    }

    private interface IRegistrationWindowStoreDouble
    {
        Task<PublicationOutcome> ExecutePublicationAsync(
            PublishCommand command,
            Func<PublicationTransaction, PublicationOutcome> operation,
            CancellationToken cancellationToken);

        Task<TermCreationResult> CreateTermAsync(
            string clientRequestId,
            string payloadHash,
            CancellationToken cancellationToken);
    }

    private sealed class RegistrationWindowServiceDouble(IRegistrationWindowStoreDouble store)
    {
        public Task<PublicationOutcome> PublishAsync(
            PublishCommand command,
            CancellationToken cancellationToken = default) =>
            store.ExecutePublicationAsync(
                command,
                transaction =>
                {
                    var candidate = Assert.Single(
                        transaction.Windows,
                        window => window.Id == command.WindowId);
                    if (transaction.TermVersion != command.ExpectedTermVersion ||
                        candidate.Version != command.ExpectedWindowVersion)
                    {
                        return PublicationOutcome.StaleVersion;
                    }

                    if (candidate.Lifecycle != WindowLifecycle.Draft)
                    {
                        return PublicationOutcome.InvalidLifecycle;
                    }

                    var overlap = transaction.Windows.Any(window =>
                        window.Id != candidate.Id &&
                        window.TermId == candidate.TermId &&
                        window.Lifecycle == WindowLifecycle.Published &&
                        candidate.OpensAtUtc < window.ClosesAtUtc &&
                        window.OpensAtUtc < candidate.ClosesAtUtc);
                    if (overlap)
                    {
                        return PublicationOutcome.WindowOverlap;
                    }

                    var index = transaction.Windows.FindIndex(window => window.Id == candidate.Id);
                    transaction.Windows[index] = candidate with
                    {
                        Lifecycle = WindowLifecycle.Published,
                        Version = candidate.Version + 1
                    };
                    transaction.TermVersion++;
                    transaction.AuditEntries.Add(new(command.TermId, command.WindowId));
                    return PublicationOutcome.Succeeded;
                },
                cancellationToken);

        public Task<TermCreationResult> CreateTermAsync(
            string clientRequestId,
            string payloadHash,
            CancellationToken cancellationToken = default) =>
            store.CreateTermAsync(clientRequestId, payloadHash, cancellationToken);
    }

    private sealed class InMemoryRegistrationWindowStoreDouble(
        Guid termId,
        int TermVersion,
        IReadOnlyList<WindowRow> initialWindows)
        : IRegistrationWindowStoreDouble
    {
        private readonly SemaphoreSlim _transactionLock = new(1, 1);
        private List<WindowRow> _windows = initialWindows.Select(window => window.Copy()).ToList();

        public int TermVersion { get; private set; } = TermVersion;
        public IReadOnlyList<WindowRow> Windows => _windows;
        public List<PublicationAudit> AuditEntries { get; } = [];
        public List<IReadOnlyList<Guid>> LockOrders { get; } = [];
        public int CommitCount { get; private set; }
        public bool FailBeforeCommit { get; set; }
        public List<TermRow> Terms { get; } = [];
        public List<string> TermCreateAuditEntries { get; } = [];
        public int TermCreateCommitCount { get; private set; }

        public WindowRow Window(Guid id) => Assert.Single(_windows, window => window.Id == id);

        public async Task<PublicationOutcome> ExecutePublicationAsync(
            PublishCommand command,
            Func<PublicationTransaction, PublicationOutcome> operation,
            CancellationToken cancellationToken)
        {
            await _transactionLock.WaitAsync(cancellationToken);
            try
            {
                LockOrders.Add(
                    [
                        termId,
                        .. _windows
                            .Where(window => window.TermId == command.TermId)
                            .OrderBy(window => window.Id)
                            .Select(window => window.Id)
                    ]);
                var transaction = new PublicationTransaction(TermVersion, _windows);
                var outcome = operation(transaction);
                if (outcome != PublicationOutcome.Succeeded)
                {
                    return outcome;
                }

                if (FailBeforeCommit)
                {
                    return PublicationOutcome.StorageFailure;
                }

                _windows = transaction.Windows.Select(window => window.Copy()).ToList();
                TermVersion = transaction.TermVersion;
                AuditEntries.AddRange(transaction.AuditEntries);
                CommitCount++;
                return PublicationOutcome.Succeeded;
            }
            finally
            {
                _transactionLock.Release();
            }
        }

        public async Task<TermCreationResult> CreateTermAsync(
            string clientRequestId,
            string payloadHash,
            CancellationToken cancellationToken)
        {
            await _transactionLock.WaitAsync(cancellationToken);
            try
            {
                var existing = Terms.SingleOrDefault(term =>
                    term.CreationClientRequestId == clientRequestId);
                if (existing is not null)
                {
                    return existing.CreationPayloadHash == payloadHash
                        ? new(TermCreationOutcome.Replayed, existing.Id)
                        : new(TermCreationOutcome.IdempotencyKeyReused, null);
                }

                var term = new TermRow(Id(200), clientRequestId, payloadHash);
                Terms.Add(term);
                TermCreateAuditEntries.Add("academic-term-created");
                TermCreateCommitCount++;
                return new(TermCreationOutcome.Created, term.Id);
            }
            finally
            {
                _transactionLock.Release();
            }
        }
    }
}
