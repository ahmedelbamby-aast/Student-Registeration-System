using StudentRegistration.Contracts.Identity;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Identity;

public sealed class AdminUserLifecycleTests
{
    private static readonly Guid ActorId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task Import_keys_bind_payload_and_publication_replays_only_the_same_payload()
    {
        var store = new GovernedStoreDouble();
        var service = new AdminUserLifecycleService(store, TimeProvider.System);
        var row = StaffRow("staff-1", "lecturer.one", "S-001", "Lecturer One");
        var hash = AdminUserLifecycleService.ComputeCanonicalContentHash([row]);
        var request = new IdentityImportRequest(
            "staff.csv",
            hash,
            "upload-1",
            [row]);

        var created = await service.CreateImportAsync(ActorId, request, "trace-1");
        var replayed = await service.CreateImportAsync(ActorId, request, "trace-2");

        Assert.Equal(AdminUserLifecycleOutcome.Succeeded, created.Outcome);
        Assert.Equal(created.Value!.Id, replayed.Value!.Id);
        Assert.Equal(hash, created.Value.ContentHash);
        Assert.Empty(created.Value.Errors);

        var reusedKeyRow = StaffRow("staff-2", "ta.one", "S-002", "TA One");
        var reusedKeyHash = AdminUserLifecycleService.ComputeCanonicalContentHash([reusedKeyRow]);
        var reusedKey = await service.CreateImportAsync(
            ActorId,
            new IdentityImportRequest(
                "staff-2.csv",
                reusedKeyHash,
                "upload-1",
                [reusedKeyRow]),
            "trace-3");
        Assert.Equal(AdminUserLifecycleOutcome.IdempotencyKeyReused, reusedKey.Outcome);

        var duplicateContent = await service.CreateImportAsync(
            ActorId,
            request with { ClientRequestId = "upload-2" },
            "trace-4");
        Assert.Equal(AdminUserLifecycleOutcome.ImportContentExists, duplicateContent.Outcome);

        var publishRequest = new IdentityImportPublishRequest(
            created.Value.RowVersion,
            "publish-1");
        var published = await service.PublishImportAsync(
            ActorId,
            created.Value.Id,
            publishRequest,
            "trace-5");
        var publishReplay = await service.PublishImportAsync(
            ActorId,
            created.Value.Id,
            publishRequest,
            "trace-6");

        Assert.Equal(AdminUserLifecycleOutcome.Succeeded, published.Outcome);
        Assert.Equal(IdentityImportStates.Published, published.Value!.State);
        Assert.Equal(published.Value, publishReplay.Value);
        Assert.Equal(1, store.PublicationCommitCount);
    }

    [Fact]
    public async Task Invalid_hash_or_more_than_500_rows_never_reaches_staging()
    {
        var store = new GovernedStoreDouble();
        var service = new AdminUserLifecycleService(store, TimeProvider.System);
        var row = StaffRow("staff-1", "lecturer.one", "S-001", "Lecturer One");

        var wrongHash = await service.CreateImportAsync(
            ActorId,
            new IdentityImportRequest("staff.csv", "not-the-hash", "upload-1", [row]),
            "trace-1");
        var tooManyRows = Enumerable.Range(0, 501)
            .Select(index => StaffRow(
                $"staff-{index}",
                $"staff.{index}",
                $"S-{index:000}",
                $"Staff {index}"))
            .ToArray();
        var tooLarge = await service.CreateImportAsync(
            ActorId,
            new IdentityImportRequest(
                "staff.csv",
                AdminUserLifecycleService.ComputeCanonicalContentHash(tooManyRows),
                "upload-2",
                tooManyRows),
            "trace-2");

        Assert.Equal(AdminUserLifecycleOutcome.ImportInvalid, wrongHash.Outcome);
        Assert.Equal(AdminUserLifecycleOutcome.ImportInvalid, tooLarge.Outcome);
        Assert.Equal(0, store.ImportStageCallCount);
    }

    [Fact]
    public async Task User_list_is_server_paged_and_minimized_with_deterministic_defaults()
    {
        var store = new GovernedStoreDouble(
            User(Guid.Parse("15000000-0000-0000-0000-000000000003"), "Zed", true, ["Lecturer"]),
            User(Guid.Parse("15000000-0000-0000-0000-000000000001"), "Admin B", true, ["Admin"]),
            User(Guid.Parse("15000000-0000-0000-0000-000000000002"), "Admin A", true, ["Admin"]));
        var service = new AdminUserLifecycleService(store, TimeProvider.System);

        var page = await service.ListUsersAsync("Admin", page: 1, pageSize: 1);
        var invalid = await service.ListUsersAsync(pageSize: 101);

        Assert.Equal(AdminUserLifecycleOutcome.Succeeded, page.Outcome);
        Assert.Equal(2, page.Value!.TotalCount);
        Assert.Single(page.Value.Items);
        Assert.Equal("Admin A", page.Value.Items[0].DisplayName);
        Assert.Equal("displayName,id", page.Value.Sort);
        Assert.Equal(AdminUserLifecycleOutcome.PageSizeInvalid, invalid.Outcome);
        Assert.Equal(1, store.ListCallCount);
    }

    [Fact]
    public async Task Successful_status_and_role_changes_rotate_security_and_advance_one_version()
    {
        var disabledUserId = Guid.Parse("16000000-0000-0000-0000-000000000001");
        var staffUserId = Guid.Parse("16000000-0000-0000-0000-000000000002");
        var adminId = Guid.Parse("16000000-0000-0000-0000-000000000003");
        var store = new GovernedStoreDouble(
            User(disabledUserId, "Disabled Lecturer", false, ["Lecturer"]),
            User(staffUserId, "Teaching Staff", true, ["Lecturer"]),
            User(adminId, "Admin", true, ["Admin"]));
        var service = new AdminUserLifecycleService(store, TimeProvider.System);
        var version = Convert.ToBase64String([1]);

        var enabled = await service.ChangeStatusAsync(
            ActorId,
            disabledUserId,
            new UserStatusRequest(true, version, "Return from leave"),
            "trace-enable");
        var roles = await service.ReplaceRolesAsync(
            ActorId,
            staffUserId,
            new UserRolesRequest(
                ["TeachingAssistant", "Lecturer", "TeachingAssistant"],
                version,
                "Updated teaching duties"),
            "trace-role");

        Assert.Equal(AdminUserLifecycleOutcome.Succeeded, enabled.Outcome);
        Assert.Equal(AdminUserLifecycleOutcome.Succeeded, roles.Outcome);
        Assert.True(enabled.Value!.Enabled);
        Assert.Equal(Convert.ToBase64String([2]), enabled.Value.RowVersion);
        Assert.Equal(["Lecturer", "TeachingAssistant"], roles.Value!.Roles);
        Assert.Equal(Convert.ToBase64String([2]), roles.Value.RowVersion);
        Assert.NotEqual("initial", store.User(disabledUserId).SecurityStamp);
        Assert.NotEqual("initial", store.User(staffUserId).SecurityStamp);
        Assert.Equal(2, store.SecurityEventCount);
        Assert.Equal(2, store.AuditEventCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Two_replicas_serialize_final_admin_disable_and_role_removal(
        bool secondCommandRemovesRole)
    {
        var firstAdminId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var secondAdminId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var store = new GovernedStoreDouble(
            User(firstAdminId, "Admin One", true, ["Admin"]),
            User(secondAdminId, "Admin Two", true, ["Admin"]));
        var replicaOne = new AdminUserLifecycleService(store, TimeProvider.System);
        var replicaTwo = new AdminUserLifecycleService(store, TimeProvider.System);
        var version = Convert.ToBase64String([1]);

        var first = replicaOne.ChangeStatusAsync(
            ActorId,
            firstAdminId,
            new UserStatusRequest(false, version, "Demo rotation"),
            "replica-1");
        var second = secondCommandRemovesRole
            ? replicaTwo.ReplaceRolesAsync(
                ActorId,
                secondAdminId,
                new UserRolesRequest(["Lecturer"], version, "Demo rotation"),
                "replica-2")
            : replicaTwo.ChangeStatusAsync(
                ActorId,
                secondAdminId,
                new UserStatusRequest(false, version, "Demo rotation"),
                "replica-2");

        var outcomes = await Task.WhenAll(first, second);

        Assert.Single(outcomes, result =>
            result.Outcome == AdminUserLifecycleOutcome.Succeeded);
        Assert.Single(outcomes, result =>
            result.Outcome == AdminUserLifecycleOutcome.FinalAdminRequired);
        Assert.Equal(1, store.EnabledAdminCount);
        Assert.Equal(1, store.SecurityEventCount);
        Assert.Equal(1, store.AuditEventCount);
    }

    [Fact]
    public async Task Stale_or_faulted_atomic_mutation_changes_no_user_or_audit_fact()
    {
        var targetId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var otherAdminId = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var store = new GovernedStoreDouble(
            User(targetId, "Admin One", true, ["Admin"]),
            User(otherAdminId, "Admin Two", true, ["Admin"]));
        var service = new AdminUserLifecycleService(store, TimeProvider.System);

        var stale = await service.ChangeStatusAsync(
            ActorId,
            targetId,
            new UserStatusRequest(false, Convert.ToBase64String([9]), "Stale command"),
            "trace-stale");
        store.FailBeforeAuditCommit = true;
        var faulted = await service.ReplaceRolesAsync(
            ActorId,
            targetId,
            new UserRolesRequest(
                ["Lecturer"],
                Convert.ToBase64String([1]),
                "Injected failure"),
            "trace-fault");

        Assert.Equal(AdminUserLifecycleOutcome.StaleVersion, stale.Outcome);
        Assert.Equal(AdminUserLifecycleOutcome.StorageFailure, faulted.Outcome);
        Assert.True(store.User(targetId).Enabled);
        Assert.Contains("Admin", store.User(targetId).Roles);
        Assert.Equal(0, store.SecurityEventCount);
        Assert.Equal(0, store.AuditEventCount);
    }

    [Fact]
    public void Spec017_has_no_competing_role_or_guard_writer()
    {
        var delegation = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/data-model.md");
        RepositoryFiles.ContainsAll(
            delegation,
            "Every Admin-role mutation is delegated to SPEC-007",
            "AdminSecurityGuard");

        var staffAdministrationRoot = RepositoryFiles.PathTo(
            "src/StudentRegistration.StaffAdministration");
        var source = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(staffAdministrationRoot, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
        Assert.DoesNotContain("new RoleAssignment", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<RoleAssignment>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new AdminSecurityGuard", source, StringComparison.Ordinal);
    }

    private static IdentityImportUserRequest StaffRow(
        string externalReference,
        string userName,
        string staffNumber,
        string displayName) =>
        new(
            externalReference,
            "staff",
            null,
            userName,
            staffNumber,
            displayName,
            ["Lecturer"]);

    private static TestUser User(
        Guid id,
        string displayName,
        bool enabled,
        IReadOnlyList<string> roles) =>
        new(id, displayName, displayName.Replace(" ", ".").ToLowerInvariant(), enabled, roles);

    private sealed class GovernedStoreDouble : IAdminUserLifecycleStore
    {
        private readonly SemaphoreSlim _adminSecurityGuard = new(1, 1);
        private readonly Dictionary<Guid, TestUser> _users;
        private readonly Dictionary<(Guid Actor, string Key), StoredImport> _importsByKey = [];
        private readonly Dictionary<Guid, StoredImport> _importsById = [];
        private readonly Dictionary<(Guid Actor, Guid Import, string Key), StoredPublication>
            _publications = [];

        public GovernedStoreDouble(params TestUser[] users)
        {
            _users = users.ToDictionary(user => user.Id);
        }

        public int ImportStageCallCount { get; private set; }
        public int ListCallCount { get; private set; }
        public int PublicationCommitCount { get; private set; }
        public int SecurityEventCount { get; private set; }
        public int AuditEventCount { get; private set; }
        public bool FailBeforeAuditCommit { get; set; }
        public int EnabledAdminCount => _users.Values.Count(
            user => user.Enabled && user.Roles.Contains("Admin", StringComparer.Ordinal));

        public TestUser User(Guid id) => _users[id];

        public Task<AdminUserPageSnapshot> ListUsersAsync(
            AdminUserSearchCriteria criteria,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListCallCount++;
            var filtered = _users.Values
                .Where(user => criteria.Search is null ||
                    user.DisplayName.Contains(criteria.Search, StringComparison.OrdinalIgnoreCase) ||
                    user.LoginIdentifier.Contains(criteria.Search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.DisplayName, StringComparer.Ordinal)
                .ThenBy(user => user.Id)
                .ToArray();
            var page = filtered
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(ToSnapshot)
                .ToArray();
            return Task.FromResult(new AdminUserPageSnapshot(page, filtered.Length));
        }

        public async Task<AdminStoreResult<IdentityImportSnapshot>> CreateImportAsync(
            CreateIdentityImport command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _adminSecurityGuard.WaitAsync(cancellationToken);
            try
            {
                ImportStageCallCount++;
                var key = (command.RequestedByUserId, command.ClientRequestId);
                if (_importsByKey.TryGetValue(key, out var keyed))
                {
                    return keyed.RequestHash == command.RequestHash
                        ? Success(keyed.Snapshot)
                        : Failure<IdentityImportSnapshot>(AdminStoreOutcome.IdempotencyKeyReused);
                }

                if (_importsById.Values.Any(existing =>
                    existing.Snapshot.SourceHash == command.SourceHash))
                {
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.ImportContentExists);
                }

                var snapshot = new IdentityImportSnapshot(
                    Guid.NewGuid(),
                    command.SourceName,
                    command.SourceHash,
                    IdentityImportStates.Validated,
                    [],
                    [1]);
                var stored = new StoredImport(
                    command.RequestedByUserId,
                    command.RequestHash,
                    snapshot);
                _importsByKey.Add(key, stored);
                _importsById.Add(snapshot.Id, stored);
                return Success(snapshot);
            }
            finally
            {
                _adminSecurityGuard.Release();
            }
        }

        public Task<IdentityImportSnapshot?> GetImportAsync(
            Guid requestedByUserId,
            Guid importId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                _importsById.TryGetValue(importId, out var import) &&
                import.RequestedByUserId == requestedByUserId
                    ? import.Snapshot
                    : null);
        }

        public async Task<AdminStoreResult<IdentityImportSnapshot>> PublishImportAsync(
            PublishIdentityImport command,
            CancellationToken cancellationToken)
        {
            await _adminSecurityGuard.WaitAsync(cancellationToken);
            try
            {
                if (!_importsById.TryGetValue(command.ImportId, out var import) ||
                    import.RequestedByUserId != command.RequestedByUserId)
                {
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.NotFound);
                }

                var key = (command.RequestedByUserId, command.ImportId, command.ClientRequestId);
                if (_publications.TryGetValue(key, out var publication))
                {
                    return publication.RequestHash == command.RequestHash
                        ? Success(publication.Snapshot)
                        : Failure<IdentityImportSnapshot>(AdminStoreOutcome.IdempotencyKeyReused);
                }

                if (!import.Snapshot.Version.SequenceEqual(command.ExpectedVersion))
                {
                    return Failure<IdentityImportSnapshot>(
                        AdminStoreOutcome.StaleVersion,
                        import.Snapshot.Version);
                }

                if (import.Snapshot.State != IdentityImportStates.Validated)
                {
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.ImportNotValidated);
                }

                var published = import.Snapshot with
                {
                    State = IdentityImportStates.Published,
                    Version = [2]
                };
                import.Snapshot = published;
                _publications.Add(key, new StoredPublication(command.RequestHash, published));
                PublicationCommitCount++;
                return Success(published);
            }
            finally
            {
                _adminSecurityGuard.Release();
            }
        }

        public Task<AdminStoreResult<AdminUserSnapshot>> SetUserStatusAsync(
            SetUserStatus command,
            CancellationToken cancellationToken) =>
            MutateAsync(command.UserId, command.ExpectedVersion, user =>
            {
                user.Enabled = command.Enabled;
                user.SecurityStamp = command.NewSecurityStamp;
            }, cancellationToken);

        public Task<AdminStoreResult<AdminUserSnapshot>> ReplaceUserRolesAsync(
            ReplaceUserRoles command,
            CancellationToken cancellationToken) =>
            MutateAsync(command.UserId, command.ExpectedVersion, user =>
            {
                user.Roles = command.Roles.ToArray();
                user.SecurityStamp = command.NewSecurityStamp;
            }, cancellationToken);

        private async Task<AdminStoreResult<AdminUserSnapshot>> MutateAsync(
            Guid userId,
            byte[] expectedVersion,
            Action<TestUser> mutation,
            CancellationToken cancellationToken)
        {
            await _adminSecurityGuard.WaitAsync(cancellationToken);
            try
            {
                if (!_users.TryGetValue(userId, out var user))
                {
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.NotFound);
                }

                if (!user.Version.SequenceEqual(expectedVersion))
                {
                    return Failure<AdminUserSnapshot>(
                        AdminStoreOutcome.StaleVersion,
                        user.Version);
                }

                var wasEnabledAdmin = user.Enabled &&
                    user.Roles.Contains("Admin", StringComparer.Ordinal);
                var staged = user.Copy();
                mutation(staged);
                var remainsEnabledAdmin = staged.Enabled &&
                    staged.Roles.Contains("Admin", StringComparer.Ordinal);
                if (wasEnabledAdmin && !remainsEnabledAdmin && EnabledAdminCount <= 1)
                {
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.FinalAdminRequired);
                }

                if (FailBeforeAuditCommit)
                {
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.StorageFailure);
                }

                staged.Version = [checked((byte)(user.Version[0] + 1))];
                _users[userId] = staged;
                SecurityEventCount++;
                AuditEventCount++;
                return Success(ToSnapshot(staged));
            }
            finally
            {
                _adminSecurityGuard.Release();
            }
        }

        private static AdminUserSnapshot ToSnapshot(TestUser user) =>
            new(
                user.Id,
                user.DisplayName,
                user.LoginIdentifier,
                user.Enabled,
                user.Roles,
                user.Version);

        private static AdminStoreResult<T> Success<T>(T value) where T : class =>
            new(AdminStoreOutcome.Succeeded, value);

        private static AdminStoreResult<T> Failure<T>(
            AdminStoreOutcome outcome,
            byte[]? currentVersion = null)
            where T : class =>
            new(outcome, null, currentVersion);
    }

    private sealed class TestUser
    {
        public TestUser(
            Guid id,
            string displayName,
            string loginIdentifier,
            bool enabled,
            IReadOnlyList<string> roles)
        {
            Id = id;
            DisplayName = displayName;
            LoginIdentifier = loginIdentifier;
            Enabled = enabled;
            Roles = roles.ToArray();
        }

        public Guid Id { get; }
        public string DisplayName { get; }
        public string LoginIdentifier { get; }
        public bool Enabled { get; set; }
        public IReadOnlyList<string> Roles { get; set; }
        public string SecurityStamp { get; set; } = "initial";
        public byte[] Version { get; set; } = [1];

        public TestUser Copy() =>
            new(Id, DisplayName, LoginIdentifier, Enabled, Roles)
            {
                SecurityStamp = SecurityStamp,
                Version = Version.ToArray()
            };
    }

    private sealed record StoredPublication(
        string RequestHash,
        IdentityImportSnapshot Snapshot);

    private sealed class StoredImport
    {
        public StoredImport(
            Guid requestedByUserId,
            string requestHash,
            IdentityImportSnapshot snapshot)
        {
            RequestedByUserId = requestedByUserId;
            RequestHash = requestHash;
            Snapshot = snapshot;
        }

        public Guid RequestedByUserId { get; }
        public string RequestHash { get; }
        public IdentityImportSnapshot Snapshot { get; set; }
    }
}
