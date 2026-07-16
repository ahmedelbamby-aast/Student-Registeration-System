using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-005-NFR-4.md";

    [Fact]
    public void Evidence_is_bound_to_executable_sql_and_identity_tests()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-005 NFR-4 Sensitive Data Evidence",
            "**Result: PASS.**",
            "Spec008SqlServerTestDatabaseBootstrapperTests",
            "Testing_database_migrates_then_seeds_is_idempotent_ready_and_disposable",
            "password hashes",
            "generated plaintext credentials",
            "PasswordHasherCompatibilityMode.IdentityV3",
            "LocalArtifactLifecycle.MaximumAgeDays",
            "7",
            "4 passed",
            "0 failed");
    }

    [Fact]
    public void Migrations_and_model_snapshot_do_not_embed_plaintext_credentials()
    {
        var persistenceRoot = RepositoryFiles.PathTo(
            "src/StudentRegistration.Infrastructure.SqlServer/Migrations");
        var generatedFiles = Directory.EnumerateFiles(
                persistenceRoot,
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path =>
                path.EndsWith(
                    "StudentRegistrationDbContextModelSnapshot.cs",
                    StringComparison.Ordinal)
                || path.EndsWith(".Designer.cs", StringComparison.Ordinal)
                || char.IsDigit(Path.GetFileName(path)[0]))
            .ToArray();

        Assert.NotEmpty(generatedFiles);
        foreach (var path in generatedFiles)
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotContain("correct horse battery staple", source, StringComparison.Ordinal);
            Assert.DoesNotContain("issued initial credential", source, StringComparison.Ordinal);
            Assert.DoesNotContain("replacement credential value", source, StringComparison.Ordinal);
            Assert.DoesNotContain("safe-", source, StringComparison.Ordinal);
            Assert.DoesNotMatch(@"AI26\d{5}", source);
        }
    }

    [Fact]
    public void Identity_hashing_is_verified_without_deterministic_hash_bytes()
    {
        var evidenceTest = RepositoryFiles.Read(
            "tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-1EvidenceTests.cs");
        var sqlTest = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/Spec008SqlServerTestDatabaseBootstrapperTests.cs");

        RepositoryFiles.ContainsAll(
            evidenceTest,
            "PasswordHasher<ApplicationUser>",
            "PasswordHasherCompatibilityMode.IdentityV3",
            "VerifyHashedPassword",
            "Assert.NotEqual(credential.Secret, identity.PasswordHash)");
        RepositoryFiles.ContainsAll(
            sqlTest,
            "SELECT [PasswordHash] FROM [auth].[ApplicationUsers]",
            "Assert.DoesNotContain",
            "credential.Secret");
        Assert.DoesNotContain(
            "Assert.Equal(expectedHash",
            evidenceTest,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Local_sensitive_artifacts_are_ignored_and_expire_within_seven_days()
    {
        var ignore = RepositoryFiles.Read(".gitignore");
        var lifecycle = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerTestDatabaseFixture.cs");

        RepositoryFiles.ContainsAll(
            ignore,
            ".local/",
            "credentials/",
            "logs/",
            "exports/",
            "*.log");
        RepositoryFiles.ContainsAll(
            lifecycle,
            "public const int MaximumAgeDays = 7;",
            "[\".local\", \"credentials\", \"logs\", \"exports\"]",
            "createdAtUtc <= nowUtc.AddDays(-MaximumAgeDays)");
    }
}
