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
    public void Evidence_distinguishes_logical_fixture_pass_from_runtime_pending()
    {
        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-1");

        Assert.Contains("Logical fixture contract result: PASS", evidence, StringComparison.Ordinal);
        Assert.Contains("25,000", evidence, StringComparison.Ordinal);
        Assert.Contains("5,000", evidence, StringComparison.Ordinal);
        Assert.Contains("salted password", evidence, StringComparison.Ordinal);
        Assert.Contains("hashes are never byte-compared", evidence, StringComparison.Ordinal);
    }

    [Fact(Skip =
        "Activation condition: SPEC-007 must deliver the canonical Development/Testing bootstrap, ApplicationUser persistence, and ASP.NET Identity hasher before 5,000 authenticated sessions and hash verification can be measured.")]
    public void Canonical_identity_hasher_verifies_generated_credentials_without_hash_byte_comparison()
    {
    }
}
