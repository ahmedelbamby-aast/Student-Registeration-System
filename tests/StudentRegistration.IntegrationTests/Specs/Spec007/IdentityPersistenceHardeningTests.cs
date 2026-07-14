using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class IdentityPersistenceHardeningTests
{
    private static readonly DateTime FirstWindowUtc =
        new(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_failure_state_is_time_correct_audited_and_savepoint_safe()
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
            var activationUser = new ApplicationUser(
                Guid.NewGuid(),
                "student.activation",
                "STUDENT.ACTIVATION",
                "AI2600101",
                "HASH-ACTIVATION",
                "STAMP-ACTIVATION");
            var lockoutUser = new ApplicationUser(
                Guid.NewGuid(),
                "student.lockout",
                "STUDENT.LOCKOUT",
                "AI2600102",
                "HASH-LOCKOUT",
                "STAMP-LOCKOUT");
            setup.AddRange(activationUser, lockoutUser);
            setup.Add(new StudentActivation(
                Guid.NewGuid(),
                activationUser.Id,
                FirstWindowUtc.AddMinutes(-1)));
            await setup.SaveChangesAsync();
            setup.ChangeTracker.Clear();

            var persistedActivationUser = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(user => user.Id == activationUser.Id);
            var impossibleVersion = persistedActivationUser.Version.ToArray();
            impossibleVersion[0] ^= 0xFF;

            await using (var outerTransaction = await setup.Database.BeginTransactionAsync())
            {
                var activated = await CreateAccountStore(
                        setup,
                        new MutableTimeProvider(new DateTimeOffset(FirstWindowUtc)))
                    .TryActivateAsync(
                        activationUser.Id,
                        impossibleVersion,
                        "HASH-REPLACEMENT",
                        "STAMP-REPLACEMENT",
                        FirstWindowUtc,
                        default);

                Assert.False(activated);
                await outerTransaction.CommitAsync();
            }

            setup.ChangeTracker.Clear();
            var activationAfterFailedNestedTransition = await setup
                .Set<StudentActivation>()
                .AsNoTracking()
                .SingleAsync(row => row.ApplicationUserId == activationUser.Id);
            Assert.Null(activationAfterFailedNestedTransition.ActivatedAtUtc);

            var time = new MutableTimeProvider(new DateTimeOffset(FirstWindowUtc));
            var accountStore = CreateAccountStore(setup, time);
            for (var attempt = 0; attempt < 5; attempt++)
            {
                await accountStore.RecordAuthenticationFailureAsync(
                    lockoutUser.Id,
                    "Student",
                    default);
            }

            var firstLockout = await ReadUserAsync(setup, lockoutUser.Id);
            Assert.Equal(5, firstLockout.AccessFailedCount);
            Assert.Equal(
                FirstWindowUtc.AddMinutes(5),
                firstLockout.LockoutEndUtc);

            time.Advance(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(1)));
            await accountStore.RecordAuthenticationFailureAsync(
                lockoutUser.Id,
                "Student",
                default);
            var freshWindow = await ReadUserAsync(setup, lockoutUser.Id);
            Assert.Equal(1, freshWindow.AccessFailedCount);
            Assert.Null(freshWindow.LockoutEndUtc);

            for (var attempt = 0; attempt < 4; attempt++)
            {
                await accountStore.RecordAuthenticationFailureAsync(
                    lockoutUser.Id,
                    "Student",
                    default);
            }

            var secondLockout = await ReadUserAsync(setup, lockoutUser.Id);
            Assert.Equal(5, secondLockout.AccessFailedCount);
            Assert.Equal(
                time.GetUtcNow().UtcDateTime.AddMinutes(5),
                secondLockout.LockoutEndUtc);

            var hmacKey = Enumerable.Range(1, 32)
                .Select(value => (byte)value)
                .ToArray();
            const string knownSubject = "AI2600102";
            const string unknownSubject = "UNKNOWN-IDENTITY-991";
            const string network = "192.0.2.40";
            var knownLoginKey = IdentityRateLimitPolicies.CreateSubjectKeyHash(
                "login",
                knownSubject,
                network,
                hmacKey);
            var unknownActivationKey = IdentityRateLimitPolicies.CreateSubjectKeyHash(
                "activation",
                unknownSubject,
                network,
                hmacKey);
            var unknownRecoveryKey = IdentityRateLimitPolicies.CreateSubjectKeyHash(
                "recovery",
                unknownSubject,
                network,
                hmacKey);
            var blockDuration = TimeSpan.FromMinutes(5);
            var firstObservedAt = new DateTimeOffset(FirstWindowUtc);
            var abuseStore = new IdentityAbuseStateStore(setup, time);

            for (var attempt = 0; attempt < 5; attempt++)
            {
                await abuseStore.RecordFailureAsync(
                    Failure(knownLoginKey, "login", firstObservedAt, blockDuration),
                    default);
            }

            var locked = await abuseStore.FindAsync(knownLoginKey, "login", default);
            Assert.NotNull(locked);
            var originalBlockedUntil = locked.BlockedUntilUtc;
            var activeAttempt = await abuseStore.RecordFailureAsync(
                Failure(
                    knownLoginKey,
                    "login",
                    firstObservedAt.AddMinutes(1),
                    blockDuration),
                default);
            Assert.Equal(5, activeAttempt.FailureCount);
            Assert.Equal(originalBlockedUntil, activeAttempt.BlockedUntilUtc);

            var afterExpiry = originalBlockedUntil!.Value.AddSeconds(1);
            var restarted = await abuseStore.RecordFailureAsync(
                Failure(knownLoginKey, "login", afterExpiry, blockDuration),
                default);
            Assert.Equal(1, restarted.FailureCount);
            Assert.Equal(afterExpiry, restarted.WindowStartedAtUtc);
            Assert.Null(restarted.BlockedUntilUtc);

            for (var attempt = 0; attempt < 4; attempt++)
            {
                restarted = await abuseStore.RecordFailureAsync(
                    Failure(knownLoginKey, "login", afterExpiry, blockDuration),
                    default);
            }

            Assert.Equal(5, restarted.FailureCount);
            Assert.Equal(afterExpiry.Add(blockDuration), restarted.BlockedUntilUtc);

            await abuseStore.RecordFailureAsync(
                Failure(
                    unknownActivationKey,
                    "activation",
                    firstObservedAt,
                    blockDuration),
                default);
            await abuseStore.RecordFailureAsync(
                Failure(
                    unknownRecoveryKey,
                    "recovery",
                    firstObservedAt,
                    blockDuration),
                default);

            var failureEvents = await setup.Set<SecurityEvent>()
                .AsNoTracking()
                .OrderBy(securityEvent => securityEvent.OccurredAtUtc)
                .ToArrayAsync();
            Assert.Contains(failureEvents, row => row.EventType == "IdentityLoginFailure");
            Assert.Contains(failureEvents, row => row.EventType == "IdentityActivationFailure");
            Assert.Contains(failureEvents, row => row.EventType == "IdentityRecoveryFailure");
            Assert.All(failureEvents, row =>
            {
                Assert.Null(row.ApplicationUserId);
                Assert.Equal("identity-gateway", row.ActorReference);
                Assert.StartsWith("hmac-sha256:", row.SubjectReference, StringComparison.Ordinal);
            });
            var persistedFacts = string.Join(
                '|',
                failureEvents.SelectMany(row => new[]
                {
                    row.ActorReference,
                    row.SubjectReference,
                    row.Reason,
                    row.BeforeSummaryJson,
                    row.AfterSummaryJson,
                    row.MetadataJson,
                    row.CorrelationId
                }).Where(value => value is not null));
            Assert.DoesNotContain(knownSubject, persistedFacts, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(unknownSubject, persistedFacts, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(network, persistedFacts, StringComparison.OrdinalIgnoreCase);

            await setup.Database.ExecuteSqlRawAsync(
                """
                CREATE TRIGGER [auth].[TR_SecurityEvents_RejectRecoveryForAtomicityTest]
                ON [auth].[SecurityEvents]
                AFTER INSERT
                AS
                BEGIN
                    IF EXISTS
                    (
                        SELECT 1 FROM inserted
                        WHERE [EventType] = N'IdentityRecoveryFailure'
                    )
                        THROW 51070, 'Injected security-event write failure.', 1;
                END;
                """);
            var rollbackKey = IdentityRateLimitPolicies.CreateSubjectKeyHash(
                "recovery",
                "ROLLBACK-SUBJECT",
                network,
                hmacKey);
            try
            {
                var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                    await abuseStore.RecordFailureAsync(
                        Failure(
                            rollbackKey,
                            "recovery",
                            firstObservedAt,
                            blockDuration),
                        default));
                Assert.True(
                    exception is DbUpdateException or SqlException,
                    $"Unexpected persistence exception: {exception.GetType().FullName}");
            }
            finally
            {
                await setup.Database.ExecuteSqlRawAsync(
                    "DROP TRIGGER [auth].[TR_SecurityEvents_RejectRecoveryForAtomicityTest];");
            }

            Assert.Null(await abuseStore.FindAsync(rollbackKey, "recovery", default));
            Assert.DoesNotContain(
                await setup.Set<SecurityEvent>().AsNoTracking().ToArrayAsync(),
                row => row.SubjectReference == $"hmac-sha256:{rollbackKey.Value}");
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static IdentityAbuseFailure Failure(
        SubjectKeyHash subjectKey,
        string operation,
        DateTimeOffset observedAtUtc,
        TimeSpan blockDuration) =>
        new(subjectKey, operation, observedAtUtc, 5, blockDuration);

    private static async Task<ApplicationUser> ReadUserAsync(
        StudentRegistrationDbContext context,
        Guid userId)
    {
        context.ChangeTracker.Clear();
        return await context.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleAsync(user => user.Id == userId);
    }

    private static IdentityAccountStore CreateAccountStore(
        StudentRegistrationDbContext context,
        TimeProvider timeProvider) =>
        new(context, new IdentitySecurityOptions(), timeProvider);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
