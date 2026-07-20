using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr1EvidenceTests
{
    [Fact]
    public void Versioned_generator_creates_25_000_accounts_and_5_000_logical_sessions()
    {
        var fixture = SyntheticLoadFixtureGenerator.Build();

        Assert.Equal("SPEC018-LOAD-FIXTURE-1.0.0", fixture.Version);
        Assert.Equal(25_000, fixture.Accounts.Count);
        Assert.Equal(5_000, fixture.Sessions.Count);
        Assert.Equal(25_000, fixture.Accounts.Select(account => account.AccountId).Distinct().Count());
        Assert.Equal(
            25_000,
            fixture.Accounts.Select(account => account.LogicalAccountKey).Distinct().Count());
        Assert.Equal(5_000, fixture.Sessions.Select(session => session.SessionId).Distinct().Count());
        var accountIds = fixture.Accounts.Select(account => account.AccountId).ToHashSet();
        Assert.All(
            fixture.Sessions,
            session => Assert.Contains(session.AccountId, accountIds));
    }

    [Fact]
    public void Rebuilds_reproduce_logical_identifiers_and_profiles_exactly()
    {
        var first = SyntheticLoadFixtureGenerator.Build();
        var second = SyntheticLoadFixtureGenerator.Build();

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(first.Accounts, second.Accounts);
        Assert.Equal(first.Sessions, second.Sessions);
    }

    [Fact]
    public void Logical_fixture_contract_contains_no_credential_or_full_profile_shape()
    {
        var propertyNames = typeof(SyntheticAccount)
            .GetProperties()
            .Concat(typeof(SyntheticSession).GetProperties())
            .Select(property => property.Name)
            .ToArray();
        var forbiddenFragments = new[]
        {
            "Password", "Pin", "Credential", "Hash", "Email", "Address",
            "Phone", "NationalId", "DateOfBirth", "FullName"
        };

        Assert.All(forbiddenFragments, fragment =>
            Assert.DoesNotContain(
                propertyNames,
                name => name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Evidence_records_the_executed_logical_sql_and_authenticated_scale_proofs()
    {
        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-1");

        Assert.Contains("Logical fixture contract result: PASS", evidence, StringComparison.Ordinal);
        Assert.Contains("25,000", evidence, StringComparison.Ordinal);
        Assert.Contains("5,000", evidence, StringComparison.Ordinal);
        Assert.Contains("salted password", evidence, StringComparison.Ordinal);
        Assert.Contains("hashes are never byte-compared", evidence, StringComparison.Ordinal);
        Assert.Contains("180,000", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Canonical_identity_hasher_verifies_generated_credentials_without_hash_byte_comparison()
    {
        var logicalFixture = SyntheticLoadFixtureGenerator.Build();
        Assert.Equal(5_000, logicalFixture.Sessions.Count);

        var identityOptions = new IdentitySecurityOptions();
        var hasher = new PasswordHasher<ApplicationUser>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = identityOptions.PasswordHashIterations
            }));
        var store = new CapturingIdentitySeedStore();
        var contributor = new DemoIdentitySeedContributor(
            store,
            hasher,
            TimeProvider.System);

        var credentials = await contributor.SeedAsync(
            "Testing",
            studentCount: 1);

        Assert.Equal(4, credentials.Count);
        Assert.Equal(credentials.Count, store.Identities.Count);
        Assert.DoesNotContain(
            store.Identities,
            identity => string.Equals(identity.UserName, "DUAL-0001", StringComparison.Ordinal));
        Assert.All(
            store.Identities.Where(identity => identity.StaffNumber is not null),
            identity => Assert.Single(identity.Roles));
        foreach (var credential in credentials)
        {
            var identity = Assert.Single(
                store.Identities,
                candidate => string.Equals(
                    candidate.UserName,
                    credential.LoginIdentifier,
                    StringComparison.Ordinal));
            var user = new ApplicationUser(
                identity.UserId,
                identity.UserName,
                identity.NormalizedUserName,
                identity.UniversityId,
                identity.PasswordHash,
                identity.SecurityStamp);

            Assert.NotEqual(credential.Secret, identity.PasswordHash);
            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(
                    user,
                    identity.PasswordHash,
                    credential.Secret));
        }
    }

    [Fact]
    public void Recorded_sql_profile_proves_the_required_account_and_session_scale()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        Assert.Equal(25_000, recorded.SyntheticAccountCount);
        Assert.Equal(5_000, recorded.LogicalSessionCount);
        Assert.Equal(2, recorded.LogicalApplicationReplicaCount);
        Assert.Equal(0, recorded.PrivacyViolations);
    }

    private sealed class CapturingIdentitySeedStore : IIdentitySeedStore
    {
        public IReadOnlyList<DemoSeedIdentity> Identities { get; private set; } = [];

        public Task<IReadOnlySet<Guid>> ReconcileAsync(
            IReadOnlyList<DemoSeedIdentity> identities,
            IReadOnlySet<Guid> retiredUserIds,
            DateTime retiredAtUtc,
            string clientRequestId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Single(retiredUserIds);
            Assert.Equal(DateTimeKind.Utc, retiredAtUtc.Kind);
            Identities = identities.ToArray();
            return Task.FromResult<IReadOnlySet<Guid>>(
                identities.Select(identity => identity.UserId).ToHashSet());
        }
    }
}
