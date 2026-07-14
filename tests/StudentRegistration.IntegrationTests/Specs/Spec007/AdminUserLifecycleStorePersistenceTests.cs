using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class AdminUserLifecycleStorePersistenceTests
{
    private static readonly DateTime ProvisionedAtUtc =
        new(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CommandAtUtc =
        new(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Concurrent_real_sql_publishers_handoff_only_the_committed_password()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await using var setup = CreateContext(connectionString);
        try
        {
            Assert.True(await setup.Database.EnsureCreatedAsync());
            var hasher = CreateHasher();
            var seed = new DemoIdentitySeedContributor(
                new IdentitySeedStore(setup),
                hasher,
                new FixedTimeProvider(new DateTimeOffset(ProvisionedAtUtc)));
            await seed.SeedAsync("Testing", 1);
            var admin = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.NormalizedUserName == "ADMIN.DEMO");
            var handoff = new DelayedStrictCredentialHandoff();
            var creationStore = CreateAdminStore(setup, hasher, handoff);
            var created = await creationStore.CreateImportAsync(
                new CreateIdentityImport(
                    admin.Id,
                    "upload-publish-race",
                    "SHA256:REQUEST-PUBLISH-RACE",
                    "publish-race.csv",
                    $"SHA256:{new string('D', 64)}",
                    [new IdentityImportCandidate(
                        "publish-race-staff",
                        "staff",
                        null,
                        "lecturer.publish.race",
                        "IMP-RACE-001",
                        "Publish Race Lecturer",
                        ["Lecturer"])],
                    $"user:{admin.Id:N}",
                    "trace-publish-race-create",
                    CommandAtUtc),
                default);
            Assert.Equal(AdminStoreOutcome.Succeeded, created.Outcome);

            var command = new PublishIdentityImport(
                admin.Id,
                created.Value!.Id,
                "publish-race",
                "SHA256:PUBLISH-RACE",
                created.Value.Version,
                $"user:{admin.Id:N}",
                "trace-publish-race",
                CommandAtUtc.AddMinutes(1));
            await using var replicaOne = CreateContext(connectionString);
            await using var replicaTwo = CreateContext(connectionString);
            var first = CreateAdminStore(replicaOne, hasher, handoff)
                .PublishImportAsync(command, default);
            var second = CreateAdminStore(replicaTwo, hasher, handoff)
                .PublishImportAsync(command, default);
            var results = await Task.WhenAll(first, second);

            Assert.All(
                results,
                result => Assert.Equal(AdminStoreOutcome.Succeeded, result.Outcome));
            Assert.Equal(1, handoff.PrepareCount);
            var credential = Assert.Single(handoff.CompletedCredentials);
            setup.ChangeTracker.Clear();
            var importedUser = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.NormalizedUserName == "LECTURER.PUBLISH.RACE");
            Assert.Equal(credential.LoginIdentifier, importedUser.UserName);
            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(
                    importedUser,
                    importedUser.PasswordHash,
                    credential.Secret));
            Assert.Equal(1, await setup.Set<SecurityEvent>()
                .CountAsync(row => row.CorrelationId == "trace-publish-race"));
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Status_and_role_commands_recover_from_an_ambiguous_real_sql_commit_once()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await using var setup = CreateContext(connectionString);
        try
        {
            Assert.True(await setup.Database.EnsureCreatedAsync());
            var hasher = CreateHasher();
            var seed = new DemoIdentitySeedContributor(
                new IdentitySeedStore(setup),
                hasher,
                new FixedTimeProvider(new DateTimeOffset(ProvisionedAtUtc)));
            await seed.SeedAsync("Testing", 1);
            var users = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(user => user.NormalizedUserName == "ADMIN.DEMO"
                    || user.NormalizedUserName == "LECTURER.DEMO"
                    || user.NormalizedUserName == "TA.DEMO")
                .ToDictionaryAsync(user => user.NormalizedUserName);
            var admin = users["ADMIN.DEMO"];
            var lecturer = users["LECTURER.DEMO"];
            var teachingAssistant = users["TA.DEMO"];

            var statusFault = new PostCommitTransientFaultInterceptor();
            await using (var statusContext = CreateFaultRetryContext(
                             connectionString,
                             statusFault))
            {
                var status = await CreateAdminStore(statusContext, hasher, new NoOpCredentialHandoff())
                    .SetUserStatusAsync(
                        new SetUserStatus(
                            admin.Id,
                            lecturer.Id,
                            false,
                            lecturer.Version,
                            "Ambiguous status commit retry",
                            "STAMP-AMBIGUOUS-STATUS",
                            $"user:{admin.Id:N}",
                            $"user:{lecturer.Id:N}",
                            "trace-ambiguous-status",
                            CommandAtUtc.AddHours(2)),
                        default);
                Assert.Equal(AdminStoreOutcome.Succeeded, status.Outcome);
                Assert.False(status.Value!.Enabled);
            }

            Assert.Equal(1, statusFault.FaultCount);
            var roleFault = new PostCommitTransientFaultInterceptor();
            await using (var roleContext = CreateFaultRetryContext(connectionString, roleFault))
            {
                var roles = await CreateAdminStore(roleContext, hasher, new NoOpCredentialHandoff())
                    .ReplaceUserRolesAsync(
                        new ReplaceUserRoles(
                            admin.Id,
                            teachingAssistant.Id,
                            ["Lecturer", "TeachingAssistant"],
                            teachingAssistant.Version,
                            "Ambiguous role commit retry",
                            "STAMP-AMBIGUOUS-ROLES",
                            $"user:{admin.Id:N}",
                            $"user:{teachingAssistant.Id:N}",
                            "trace-ambiguous-roles",
                            CommandAtUtc.AddHours(2).AddMinutes(1)),
                        default);
                Assert.Equal(AdminStoreOutcome.Succeeded, roles.Outcome);
                Assert.Equal(
                    ["Lecturer", "TeachingAssistant"],
                    roles.Value!.Roles);
            }

            Assert.Equal(1, roleFault.FaultCount);
            setup.ChangeTracker.Clear();
            Assert.False((await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.Id == lecturer.Id)).IsEnabled);
            Assert.Equal(1, await setup.Set<SecurityEvent>()
                .CountAsync(row => row.CorrelationId == "trace-ambiguous-status"));
            Assert.Equal(1, await setup.AuditEvents
                .CountAsync(row => row.CorrelationId == "trace-ambiguous-status"));
            Assert.Equal(1, await setup.Set<SecurityEvent>()
                .CountAsync(row => row.CorrelationId == "trace-ambiguous-roles"));
            Assert.Equal(1, await setup.AuditEvents
                .CountAsync(row => row.CorrelationId == "trace-ambiguous-roles"));
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_admin_lifecycle_is_idempotent_guarded_and_audit_atomic()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await using var setup = CreateContext(connectionString);
        try
        {
            Assert.True(await setup.Database.EnsureCreatedAsync());
            var hasher = CreateHasher();
            var seed = new DemoIdentitySeedContributor(
                new IdentitySeedStore(setup),
                hasher,
                new FixedTimeProvider(new DateTimeOffset(ProvisionedAtUtc)));
            Assert.Equal(5, (await seed.SeedAsync("Testing", 1)).Count);
            var firstAdmin = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.NormalizedUserName == "ADMIN.DEMO");
            var secondAdmin = await AddSecondAdminAsync(setup);

            var handoff = new FaultableCredentialHandoff { FailNextCompletion = true };
            var store = CreateAdminStore(setup, hasher, handoff);
            var import = new CreateIdentityImport(
                firstAdmin.Id,
                "upload-1",
                "SHA256:REQUEST-ONE",
                "staff.csv",
                $"SHA256:{new string('A', 64)}",
                [new IdentityImportCandidate(
                    "staff-1",
                    "staff",
                    null,
                    "lecturer.imported",
                    "IMP-001",
                    "Imported Lecturer",
                    ["Lecturer"])],
                $"user:{firstAdmin.Id:N}",
                "trace-import-create",
                CommandAtUtc);

            var created = await store.CreateImportAsync(import, default);
            var createReplay = await store.CreateImportAsync(import, default);
            Assert.Equal(AdminStoreOutcome.Succeeded, created.Outcome);
            Assert.Equal(IdentityImportStates.Validated, created.Value!.State);
            Assert.Equal(created.Value.Id, createReplay.Value!.Id);
            var creationSecurityEvent = Assert.Single(await setup.Set<SecurityEvent>()
                .AsNoTracking()
                .Where(row => row.EventType == "IdentityImportValidated")
                .ToArrayAsync());
            var creationAuditEvent = Assert.Single(await setup.AuditEvents
                .AsNoTracking()
                .Where(row => row.Action == "IdentityImportValidated")
                .ToArrayAsync());
            Assert.Equal("trace-import-create", creationSecurityEvent.CorrelationId);
            Assert.Equal("trace-import-create", creationAuditEvent.CorrelationId);
            Assert.Equal(
                $"identity-import:{created.Value.Id:N}",
                creationSecurityEvent.SubjectReference);
            Assert.DoesNotContain(
                "lecturer.imported",
                string.Join(
                    '|',
                    creationSecurityEvent.SubjectReference,
                    creationSecurityEvent.Reason,
                    creationSecurityEvent.BeforeSummaryJson,
                    creationSecurityEvent.AfterSummaryJson,
                    creationSecurityEvent.MetadataJson),
                StringComparison.OrdinalIgnoreCase);
            var persistedProvenance = await setup.Set<IdentityImportBatch>()
                .AsNoTracking()
                .SingleAsync(batch => batch.Id == created.Value.Id);
            Assert.Equal(import.SourceName, persistedProvenance.SourceName);
            Assert.Equal(import.SourceHash, persistedProvenance.SourceHash);

            var rejectedSourceHash = $"SHA256:{new string('F', 64)}";
            await using (var createFaultContext = CreateContext(connectionString))
            {
                var createFaultStore = new AdminUserLifecycleStore(
                    createFaultContext,
                    new ThrowingAuditWriter(),
                    hasher,
                    handoff,
                    TimeProvider.System);
                var failedCreate = await createFaultStore.CreateImportAsync(
                    import with
                    {
                        ClientRequestId = "upload-audit-failure",
                        RequestHash = "SHA256:REQUEST-AUDIT-FAILURE",
                        SourceName = "audit-failure.csv",
                        SourceHash = rejectedSourceHash,
                        Users = [new IdentityImportCandidate(
                            "audit-failure-row",
                            "staff",
                            null,
                            "audit.failure",
                            "IMP-099",
                            "Audit Failure",
                            ["Lecturer"])],
                        CorrelationId = "trace-import-create-failure"
                    },
                    default);
                Assert.Equal(AdminStoreOutcome.StorageFailure, failedCreate.Outcome);
            }

            Assert.False(await setup.Set<IdentityImportBatch>()
                .AsNoTracking()
                .AnyAsync(batch => batch.SourceHash == rejectedSourceHash));
            Assert.False(await setup.Set<IdentityImportCandidateRow>()
                .AsNoTracking()
                .AnyAsync(row => row.ExternalReference == "audit-failure-row"));

            var duplicateContent = await store.CreateImportAsync(
                import with { ClientRequestId = "upload-2" },
                default);
            Assert.Equal(AdminStoreOutcome.ImportContentExists, duplicateContent.Outcome);

            var publish = new PublishIdentityImport(
                firstAdmin.Id,
                created.Value.Id,
                "publish-1",
                "SHA256:PUBLISH-ONE",
                created.Value.Version,
                $"user:{firstAdmin.Id:N}",
                "trace-import-publish",
                CommandAtUtc);
            var firstPublish = await store.PublishImportAsync(publish, default);
            Assert.Equal(AdminStoreOutcome.StorageFailure, firstPublish.Outcome);
            Assert.True(handoff.HasPending(created.Value.Id));
            Assert.False(handoff.HasCompleted(created.Value.Id));

            var publishReplay = await store.PublishImportAsync(publish, default);
            Assert.Equal(AdminStoreOutcome.Succeeded, publishReplay.Outcome);
            Assert.Equal(IdentityImportStates.Published, publishReplay.Value!.State);
            Assert.False(handoff.HasPending(created.Value.Id));
            Assert.True(handoff.HasCompleted(created.Value.Id));
            Assert.Equal(1, handoff.PrepareCount);
            Assert.Equal(2, handoff.CompleteCount);
            Assert.Equal(0, handoff.AbortCount);

            var importedUser = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.NormalizedUserName == "LECTURER.IMPORTED");
            var handedOff = Assert.Single(handoff.Completed(created.Value.Id));
            Assert.Equal("lecturer.imported", handedOff.LoginIdentifier);
            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(
                    importedUser,
                    importedUser.PasswordHash,
                    handedOff.Secret));
            Assert.Equal(1, await setup.Set<SecurityEvent>()
                .CountAsync(securityEvent => securityEvent.EventType == "IdentityImportPublished"));
            Assert.Equal(1, await setup.AuditEvents
                .CountAsync(auditEvent => auditEvent.Action == "IdentityImportPublished"));

            var retryHandoff = new RetryOnceCredentialHandoff();
            await using (var retryContext = CreateContext(connectionString, enableRetries: true))
            {
                var retryStore = CreateAdminStore(retryContext, hasher, retryHandoff);
                var retryImport = import with
                {
                    ClientRequestId = "upload-retry",
                    RequestHash = "SHA256:REQUEST-RETRY",
                    SourceName = "staff-retry.csv",
                    SourceHash = $"SHA256:{new string('C', 64)}",
                    Users = [new IdentityImportCandidate(
                        "staff-retry",
                        "staff",
                        null,
                        "lecturer.retry",
                        "IMP-003",
                        "Retry Lecturer",
                        ["Lecturer"])],
                    CorrelationId = "trace-import-retry-create"
                };
                var retryCreated = await retryStore.CreateImportAsync(retryImport, default);
                Assert.Equal(AdminStoreOutcome.Succeeded, retryCreated.Outcome);
                var retryPublished = await retryStore.PublishImportAsync(
                    new PublishIdentityImport(
                        firstAdmin.Id,
                        retryCreated.Value!.Id,
                        "publish-retry",
                        "SHA256:PUBLISH-RETRY",
                        retryCreated.Value.Version,
                        $"user:{firstAdmin.Id:N}",
                        "trace-import-retry-publish",
                        CommandAtUtc),
                    default);
                Assert.Equal(AdminStoreOutcome.Succeeded, retryPublished.Outcome);
            }

            Assert.Equal(2, retryHandoff.PrepareCount);
            Assert.True(retryHandoff.RetryUsedIdenticalCredentials);
            Assert.Equal(1, retryHandoff.CompleteCount);
            Assert.Equal(0, retryHandoff.AbortCount);
            var retryCredential = Assert.Single(retryHandoff.CompletedCredentials!);
            var retryUser = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.NormalizedUserName == "LECTURER.RETRY");
            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(
                    retryUser,
                    retryUser.PasswordHash,
                    retryCredential.Secret));

            var abortingImport = import with
            {
                ClientRequestId = "upload-abort",
                RequestHash = "SHA256:REQUEST-ABORT",
                SourceName = "staff-abort.csv",
                SourceHash = $"SHA256:{new string('B', 64)}",
                Users = [new IdentityImportCandidate(
                    "staff-abort",
                    "staff",
                    null,
                    "ta.imported",
                    "IMP-002",
                    "Imported Teaching Assistant",
                    ["TeachingAssistant"])],
                CorrelationId = "trace-import-abort-create"
            };
            var abortingCreated = await store.CreateImportAsync(abortingImport, default);
            Assert.Equal(AdminStoreOutcome.Succeeded, abortingCreated.Outcome);
            var abortingPublish = new PublishIdentityImport(
                firstAdmin.Id,
                abortingCreated.Value!.Id,
                "publish-abort",
                "SHA256:PUBLISH-ABORT",
                abortingCreated.Value.Version,
                $"user:{firstAdmin.Id:N}",
                "trace-import-abort-publish",
                CommandAtUtc.AddMinutes(1));
            await using (var abortContext = CreateContext(connectionString))
            {
                var abortStore = new AdminUserLifecycleStore(
                    abortContext,
                    new ThrowingAuditWriter(),
                    hasher,
                    handoff,
                    TimeProvider.System);
                var aborted = await abortStore.PublishImportAsync(abortingPublish, default);
                Assert.Equal(AdminStoreOutcome.StorageFailure, aborted.Outcome);
            }

            Assert.Equal(1, handoff.AbortCount);
            Assert.False(handoff.HasPending(abortingCreated.Value.Id));
            Assert.False(handoff.HasCompleted(abortingCreated.Value.Id));
            Assert.Null(await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.NormalizedUserName == "TA.IMPORTED"));
            var batchAfterAbort = await setup.Set<IdentityImportBatch>()
                .AsNoTracking()
                .SingleAsync(batch => batch.Id == abortingCreated.Value.Id);
            Assert.Equal(IdentityImportStates.Validated, batchAfterAbort.State);

            var recoveredPublish = await store.PublishImportAsync(abortingPublish, default);
            Assert.Equal(AdminStoreOutcome.Succeeded, recoveredPublish.Outcome);
            Assert.True(handoff.HasCompleted(abortingCreated.Value.Id));
            Assert.NotNull(await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.NormalizedUserName == "TA.IMPORTED"));

            var beforeRaceSecurityEvents = await setup.Set<SecurityEvent>().CountAsync();
            var beforeRaceAuditEvents = await setup.AuditEvents.CountAsync();
            var raceAtUtc = CommandAtUtc.AddHours(1);
            await using var replicaOne = CreateContext(connectionString);
            await using var replicaTwo = CreateContext(connectionString);
            var replicaOneStore = CreateAdminStore(replicaOne, hasher, handoff);
            var replicaTwoStore = CreateAdminStore(replicaTwo, hasher, handoff);
            var disable = replicaOneStore.SetUserStatusAsync(
                new SetUserStatus(
                    firstAdmin.Id,
                    firstAdmin.Id,
                    false,
                    firstAdmin.Version,
                    "Demo Admin rotation",
                    "STAMP-DISABLED",
                    $"user:{firstAdmin.Id:N}",
                    $"user:{firstAdmin.Id:N}",
                    "trace-admin-disable",
                    raceAtUtc),
                default);
            var removeRole = replicaTwoStore.ReplaceUserRolesAsync(
                new ReplaceUserRoles(
                    firstAdmin.Id,
                    secondAdmin.Id,
                    ["Lecturer"],
                    secondAdmin.Version,
                    "Demo Admin rotation",
                    "STAMP-ROLE-REPLACED",
                    $"user:{firstAdmin.Id:N}",
                    $"user:{secondAdmin.Id:N}",
                    "trace-admin-role",
                    raceAtUtc),
                default);
            var raceResults = await Task.WhenAll(disable, removeRole);

            Assert.Single(raceResults, result => result.Outcome == AdminStoreOutcome.Succeeded);
            Assert.Single(
                raceResults,
                result => result.Outcome == AdminStoreOutcome.FinalAdminRequired);
            Assert.Equal(
                1,
                await CountEnabledAdminsAsync(setup, raceAtUtc));
            Assert.Single(await setup.Set<AdminSecurityGuard>().AsNoTracking().ToArrayAsync());
            Assert.Equal(
                beforeRaceSecurityEvents + 1,
                await setup.Set<SecurityEvent>().CountAsync());
            Assert.Equal(beforeRaceAuditEvents + 1, await setup.AuditEvents.CountAsync());

            var successfulRaceIndex = Array.FindIndex(
                raceResults,
                result => result.Outcome == AdminStoreOutcome.Succeeded);
            var staleTarget = successfulRaceIndex == 0 ? firstAdmin : secondAdmin;
            await using var staleContext = CreateContext(connectionString);
            var staleResult = await CreateAdminStore(staleContext, hasher, handoff)
                .SetUserStatusAsync(
                    new SetUserStatus(
                        firstAdmin.Id,
                        staleTarget.Id,
                        true,
                        staleTarget.Version,
                        "Stale retry",
                        "STAMP-STALE",
                        $"user:{firstAdmin.Id:N}",
                        $"user:{staleTarget.Id:N}",
                        "trace-stale",
                        raceAtUtc.AddMinutes(1)),
                    default);
            Assert.Equal(AdminStoreOutcome.StaleVersion, staleResult.Outcome);
            Assert.NotNull(staleResult.CurrentVersion);

            var importedBeforeFault = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.Id == importedUser.Id);
            var factsBeforeFault = (
                Security: await setup.Set<SecurityEvent>().CountAsync(),
                Audit: await setup.AuditEvents.CountAsync());
            await using var faultContext = CreateContext(connectionString);
            var faultStore = new AdminUserLifecycleStore(
                faultContext,
                new ThrowingAuditWriter(),
                hasher,
                handoff,
                TimeProvider.System);
            var faulted = await faultStore.SetUserStatusAsync(
                new SetUserStatus(
                    firstAdmin.Id,
                    importedUser.Id,
                    false,
                    importedBeforeFault.Version,
                    "Injected audit failure",
                    "STAMP-FAULT",
                    $"user:{firstAdmin.Id:N}",
                    $"user:{importedUser.Id:N}",
                    "trace-audit-fault",
                    raceAtUtc.AddMinutes(2)),
                default);
            Assert.Equal(AdminStoreOutcome.StorageFailure, faulted.Outcome);

            var importedAfterFault = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.Id == importedUser.Id);
            Assert.True(importedAfterFault.IsEnabled);
            Assert.True(importedBeforeFault.Version.SequenceEqual(importedAfterFault.Version));
            Assert.Equal(factsBeforeFault.Security, await setup.Set<SecurityEvent>().CountAsync());
            Assert.Equal(factsBeforeFault.Audit, await setup.AuditEvents.CountAsync());
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static async Task<ApplicationUser> AddSecondAdminAsync(
        StudentRegistrationDbContext context)
    {
        var user = new ApplicationUser(
            Guid.NewGuid(),
            "admin.two",
            "ADMIN.TWO",
            null,
            "$IDENTITYV3$SYNTHETIC-ADMIN-TWO",
            "STAMP-ADMIN-TWO");
        context.Add(user);
        context.Add(new Staff(
            Guid.NewGuid(),
            user.Id,
            "ADM-0002",
            "Second Administrator"));
        context.Add(new RoleAssignment(
            Guid.NewGuid(),
            user.Id,
            "Admin",
            ProvisionedAtUtc,
            null,
            "test-provisioner"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return await context.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == user.Id);
    }

    private static Task<int> CountEnabledAdminsAsync(
        StudentRegistrationDbContext context,
        DateTime utcNow) =>
        context.Set<ApplicationUser>()
            .AsNoTracking()
            .CountAsync(user => user.IsEnabled
                && context.Set<RoleAssignment>().Any(role =>
                    role.ApplicationUserId == user.Id
                    && role.RoleCode == "Admin"
                    && role.EffectiveFromUtc <= utcNow
                    && (role.EffectiveToUtc == null || utcNow < role.EffectiveToUtc)));

    private static AdminUserLifecycleStore CreateAdminStore(
        StudentRegistrationDbContext context,
        IPasswordHasher<ApplicationUser> hasher,
        IProvisionedCredentialHandoff handoff) =>
        new(
            context,
            new AuditTransactionWriter(context),
            hasher,
            handoff,
            TimeProvider.System);

    private static PasswordHasher<ApplicationUser> CreateHasher() =>
        new(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 100_000
        }));

    private static StudentRegistrationDbContext CreateContext(
        string connectionString,
        bool enableRetries = false)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServer =>
                {
                    if (enableRetries)
                    {
                        sqlServer.EnableRetryOnFailure(
                            maxRetryCount: 2,
                            maxRetryDelay: TimeSpan.Zero,
                            errorNumbersToAdd: null);
                    }
                })
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static StudentRegistrationDbContext CreateFaultRetryContext(
        string connectionString,
        PostCommitTransientFaultInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServer => sqlServer.ExecutionStrategy(dependencies =>
                    new PostCommitRetryExecutionStrategy(dependencies)))
            .AddInterceptors(interceptor)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class DelayedStrictCredentialHandoff : IProvisionedCredentialHandoff
    {
        private readonly object _gate = new();
        private IReadOnlyList<DemoCredential>? _pending;
        private IReadOnlyList<DemoCredential>? _completed;
        private int _prepareCount;

        public int PrepareCount => Volatile.Read(ref _prepareCount);

        public IReadOnlyList<DemoCredential> CompletedCredentials
        {
            get
            {
                lock (_gate)
                {
                    return _completed?.ToArray() ?? [];
                }
            }
        }

        public async Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = credentials.ToArray();
            var delayFirstPublisher = false;
            lock (_gate)
            {
                Interlocked.Increment(ref _prepareCount);
                if (_completed is not null)
                {
                    return;
                }

                if (_pending is null)
                {
                    _pending = snapshot;
                    delayFirstPublisher = true;
                }
                else if (!_pending.SequenceEqual(snapshot))
                {
                    throw new InvalidOperationException(
                        "IDENTITY_HANDOFF_PREPARE_MISMATCH: Concurrent publishers generated different credentials.");
                }
            }

            if (delayFirstPublisher)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(750), cancellationToken);
            }
        }

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                if (_completed is not null)
                {
                    _pending = null;
                    return Task.CompletedTask;
                }

                _completed = _pending
                    ?? throw new InvalidOperationException(
                        "IDENTITY_HANDOFF_NOT_PREPARED: The race-test handoff is unavailable.");
                _pending = null;
                return Task.CompletedTask;
            }
        }

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                _pending = null;
                return Task.CompletedTask;
            }
        }
    }

    private sealed class PostCommitTransientFaultInterceptor : DbTransactionInterceptor
    {
        private int _faultCount;

        public int FaultCount => Volatile.Read(ref _faultCount);

        public override Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref _faultCount, 1, 0) == 0)
            {
                throw new PostCommitTransientException();
            }

            return Task.CompletedTask;
        }
    }

    private sealed class PostCommitRetryExecutionStrategy(
        ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, maxRetryCount: 1, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) =>
            exception is PostCommitTransientException;
    }

    private sealed class PostCommitTransientException : Exception;

    private sealed class NoOpCredentialHandoff : IProvisionedCredentialHandoff
    {
        public Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FaultableCredentialHandoff : IProvisionedCredentialHandoff
    {
        private readonly Dictionary<Guid, IReadOnlyList<DemoCredential>> _pending = [];
        private readonly Dictionary<Guid, IReadOnlyList<DemoCredential>> _completed = [];

        public bool FailNextCompletion { get; set; }
        public int PrepareCount { get; private set; }
        public int CompleteCount { get; private set; }
        public int AbortCount { get; private set; }

        public Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_pending.ContainsKey(importId) && !_completed.ContainsKey(importId))
            {
                _pending.Add(importId, credentials.ToArray());
                PrepareCount++;
            }

            return Task.CompletedTask;
        }

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CompleteCount++;
            if (FailNextCompletion)
            {
                FailNextCompletion = false;
                throw new InvalidOperationException("Injected completion failure.");
            }

            if (_pending.Remove(importId, out var credentials))
            {
                _completed[importId] = credentials;
            }

            return Task.CompletedTask;
        }

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AbortCount++;
            _pending.Remove(importId);
            return Task.CompletedTask;
        }

        public bool HasPending(Guid importId) => _pending.ContainsKey(importId);
        public bool HasCompleted(Guid importId) => _completed.ContainsKey(importId);
        public IReadOnlyList<DemoCredential> Completed(Guid importId) => _completed[importId];
    }

    private sealed class RetryOnceCredentialHandoff : IProvisionedCredentialHandoff
    {
        private IReadOnlyList<DemoCredential>? _firstAttempt;

        public int PrepareCount { get; private set; }
        public int CompleteCount { get; private set; }
        public int AbortCount { get; private set; }
        public bool RetryUsedIdenticalCredentials { get; private set; }
        public IReadOnlyList<DemoCredential>? CompletedCredentials { get; private set; }

        public Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PrepareCount++;
            var snapshot = credentials.ToArray();
            if (_firstAttempt is null)
            {
                _firstAttempt = snapshot;
                throw new TimeoutException("Injected transient handoff timeout.");
            }

            RetryUsedIdenticalCredentials = _firstAttempt.SequenceEqual(snapshot);
            return Task.CompletedTask;
        }

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CompleteCount++;
            CompletedCredentials = _firstAttempt;
            return Task.CompletedTask;
        }

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AbortCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingAuditWriter : IAuditEventWriter
    {
        public Task AppendAsync(
            AuditEventDraft auditEvent,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Injected audit failure.");
    }
}
