using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class IdentityAccountStorePersistenceTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_enforces_unique_identity_and_single_use_lifecycle_transitions()
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

            var securityOptions = new IdentitySecurityOptions();
            var hasher = new PasswordHasher<ApplicationUser>(
                Options.Create(new PasswordHasherOptions
                {
                    CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                    IterationCount = securityOptions.PasswordHashIterations
                }));
            var seedContributor = new DemoIdentitySeedContributor(
                new IdentitySeedStore(setup),
                hasher,
                new FixedSeedTimeProvider(
                    new DateTimeOffset(2026, 7, 14, 9, 0, 0, TimeSpan.Zero)));

            var firstCredentialHandoff = await seedContributor.SeedAsync(
                "Testing",
                studentCount: 1);
            Assert.Equal(4, firstCredentialHandoff.Count);

            var firstUsers = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .ToDictionaryAsync(user => user.Id);
            Assert.All(
                firstCredentialHandoff,
                credential =>
                {
                    var persisted = firstUsers.Values.Single(
                        user => user.UserName == credential.LoginIdentifier);
                    Assert.NotEqual(
                        PasswordVerificationResult.Failed,
                        hasher.VerifyHashedPassword(
                            persisted,
                            persisted.PasswordHash,
                            credential.Secret));
                });
            var firstHashes = firstUsers.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.PasswordHash);

            var secondCredentialHandoff = await seedContributor.SeedAsync(
                "Testing",
                studentCount: 1);
            Assert.Empty(secondCredentialHandoff);
            var secondHashes = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .ToDictionaryAsync(user => user.Id, user => user.PasswordHash);
            Assert.Equal(firstHashes.Count, secondHashes.Count);
            Assert.All(
                firstHashes,
                pair => Assert.Equal(pair.Value, secondHashes[pair.Key]));
            Assert.Equal(3, await setup.Set<Staff>().CountAsync());
            Assert.Single(await setup.Set<StudentActivation>().ToArrayAsync());
            Assert.Equal(4, await setup.Set<RoleAssignment>().CountAsync());

            var user = firstUsers.Values.Single(candidate => candidate.UniversityId == "AI2600001");
            var userId = user.Id;
            var initialUserVersion = user.Version.ToArray();

            Assert.False(await CreateStore(setup).TryActivateAsync(
                userId,
                initialUserVersion,
                "$IDENTITYV3$SYNTHETIC-EXPIRED",
                "STAMP-EXPIRED",
                new DateTime(2026, 7, 14, 9, 16, 0, DateTimeKind.Utc),
                default));

            var activationAttempts = Enumerable.Range(0, 10)
                .Select(index => ActivateAsync(
                    connectionString,
                    userId,
                    initialUserVersion,
                    index))
                .ToArray();
            var activationResults = await Task.WhenAll(activationAttempts);

            Assert.Single(activationResults, result => result);
            var activatedUser = await setup.Set<ApplicationUser>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == userId);
            var activatedState = await setup.Set<StudentActivation>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.ApplicationUserId == userId);
            Assert.NotNull(activatedState.ActivatedAtUtc);
            Assert.False(initialUserVersion.SequenceEqual(activatedUser.Version));

            await Assert.ThrowsAsync<DbUpdateException>(
                () => InsertDuplicateUniversityIdAsync(connectionString));

            var challenge = new AccountRecoveryChallenge(
                Guid.NewGuid(),
                userId,
                "RECOVERY-TOKEN-HASH",
                "DELIVERY-REFERENCE-HASH",
                new DateTime(2026, 7, 14, 10, 30, 0, DateTimeKind.Utc));
            var challengeStore = CreateStore(setup);
            Assert.True(await challengeStore.AddRecoveryChallengeAsync(challenge, default));
            setup.ChangeTracker.Clear();
            var storedChallenge = await setup.Set<AccountRecoveryChallenge>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == challenge.Id);

            var recoveryAttempts = Enumerable.Range(0, 2)
                .Select(index => CompleteRecoveryAsync(
                    connectionString,
                    storedChallenge,
                    index))
                .ToArray();
            var recoveryResults = await Task.WhenAll(recoveryAttempts);

            Assert.Single(recoveryResults, result => result);
            var consumedChallenge = await setup.Set<AccountRecoveryChallenge>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == challenge.Id);
            Assert.NotNull(consumedChallenge.ConsumedAtUtc);
            Assert.False(storedChallenge.Version.SequenceEqual(consumedChallenge.Version));

            var abuseKey = IdentityRateLimitPolicies.CreateSubjectKeyHash(
                "login",
                "AI2600001",
                "192.0.2.25",
                Enumerable.Range(1, 32).Select(value => (byte)value).ToArray());
            var observedAt = new DateTimeOffset(
                2026,
                7,
                14,
                10,
                10,
                0,
                TimeSpan.Zero);
            var abuseAttempts = Enumerable.Range(0, 10)
                .Select(_ => RecordAbuseFailureAsync(
                    connectionString,
                    new IdentityAbuseFailure(
                        abuseKey,
                        "login",
                        observedAt,
                        5,
                        TimeSpan.FromMinutes(5))))
                .ToArray();
            await Task.WhenAll(abuseAttempts);

            await using var abuseContext = CreateContext(connectionString);
            var abuseStore = new IdentityAbuseStateStore(abuseContext, TimeProvider.System);
            var sharedAbuseState = await abuseStore.FindAsync(abuseKey, "login", default);
            Assert.NotNull(sharedAbuseState);
            Assert.Equal(5, sharedAbuseState.FailureCount);
            Assert.True(sharedAbuseState.BlockedUntilUtc > observedAt);
            var durableAbuseRow = await abuseContext.Set<AuthenticationAbuseState>()
                .AsNoTracking()
                .SingleAsync();
            Assert.Equal(abuseKey.Value, durableAbuseRow.SubjectKeyHash);
            Assert.DoesNotContain("AI2600001", durableAbuseRow.SubjectKeyHash, StringComparison.Ordinal);
            Assert.DoesNotContain("192.0.2.25", durableAbuseRow.SubjectKeyHash, StringComparison.Ordinal);
            Assert.Equal(10, await abuseContext.Set<SecurityEvent>()
                .CountAsync(row => row.EventType == "IdentityLoginFailure"));

            await abuseStore.ResetAsync(abuseKey, "login", default);
            var resetAbuseState = await abuseStore.FindAsync(abuseKey, "login", default);
            Assert.NotNull(resetAbuseState);
            Assert.Equal(0, resetAbuseState.FailureCount);
            Assert.Null(resetAbuseState.BlockedUntilUtc);
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static async Task<bool> ActivateAsync(
        string connectionString,
        Guid userId,
        byte[] expectedVersion,
        int attempt)
    {
        await using var context = CreateContext(connectionString);
        return await CreateStore(context).TryActivateAsync(
            userId,
            expectedVersion,
            $"$IDENTITYV3$SYNTHETIC-ACTIVATED-{attempt}",
            $"STAMP-ACTIVATED-{attempt}",
            new DateTime(2026, 7, 14, 9, 14, 0, DateTimeKind.Utc),
            default);
    }

    private static async Task InsertDuplicateUniversityIdAsync(string connectionString)
    {
        await using var context = CreateContext(connectionString);
        context.Add(new ApplicationUser(
            Guid.NewGuid(),
            "student.duplicate",
            "STUDENT.DUPLICATE",
            "AI2600001",
            "$IDENTITYV3$SYNTHETIC-DUPLICATE",
            "STAMP-DUPLICATE"));
        await context.SaveChangesAsync();
    }

    private static async Task<bool> CompleteRecoveryAsync(
        string connectionString,
        AccountRecoveryChallenge challenge,
        int attempt)
    {
        await using var context = CreateContext(connectionString);
        return await CreateStore(context).TryCompleteRecoveryAsync(
            challenge.Id,
            challenge.Version,
            challenge.ApplicationUserId,
            $"$IDENTITYV3$SYNTHETIC-RECOVERED-{attempt}",
            $"STAMP-RECOVERED-{attempt}",
            new DateTime(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc),
            maximumAttempts: 5,
            default);
    }

    private static async Task<IdentityAbuseStateSnapshot> RecordAbuseFailureAsync(
        string connectionString,
        IdentityAbuseFailure failure)
    {
        await using var context = CreateContext(connectionString);
        var store = new IdentityAbuseStateStore(context, TimeProvider.System);
        return await store.RecordFailureAsync(failure, default);
    }

    private static IdentityAccountStore CreateStore(StudentRegistrationDbContext context) =>
        new(context, new IdentitySecurityOptions(), TimeProvider.System);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private sealed class FixedSeedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
