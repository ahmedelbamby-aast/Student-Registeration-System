using Microsoft.Data.SqlClient;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Infrastructure;

public sealed class Spec008SqlServerTestDatabaseBootstrapperTests
{
    [Fact]
    public void Bootstrapper_freezes_the_versioned_synthetic_contributor_contract()
    {
        ISqlServerTestDatabaseBootstrapper bootstrapper =
            new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 6);

        Assert.Equal(SqlServerTestDatabaseFixture.SeedProfileVersion, bootstrapper.ProfileVersion);
        Assert.Equal("synthetic-only", bootstrapper.DataClassification);
        Assert.Equal(["SPEC-007", "SPEC-008"], bootstrapper.ContributorOwnerSpecs);
        Assert.IsAssignableFrom<ISqlServerTestDatabaseBootstrapper>(bootstrapper);
    }

    [Theory]
    [InlineData("StudentRegistration_Development")]
    [InlineData("StudentRegistration_Production")]
    [InlineData("StudentRegistration_Test_shared")]
    public async Task Testing_bootstrapper_rejects_every_non_unique_testing_target_before_sql_mutation(
        string databaseName)
    {
        var bootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 1);
        var connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "invalid.example",
            InitialCatalog = databaseName,
            UserID = "unused",
            Password = "unused",
            TrustServerCertificate = true
        }.ConnectionString;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bootstrapper.ApplyMigrationsAsync(connectionString, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bootstrapper.SeedAsync(connectionString, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bootstrapper.VerifyReadinessAsync(connectionString, CancellationToken.None));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Testing_database_migrates_then_seeds_is_idempotent_ready_and_disposable()
    {
        var bootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 6);
        var fixture = new SqlServerTestDatabaseFixture(bootstrapper);

        await fixture.InitializeAsync();

        Assert.True(fixture.IsReady);
        Assert.NotNull(fixture.ConnectionString);
        Assert.False(string.IsNullOrWhiteSpace(fixture.LogicalFingerprint));
        Assert.Equal(10, bootstrapper.Credentials.Count);
        Assert.All(bootstrapper.Credentials, AssertTransientCredential);

        await using (var connection = new SqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync();
            Assert.Equal(10, await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [auth].[ApplicationUsers];"));
            Assert.Equal(6, await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[Students];"));
            Assert.Equal(6, await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[StudentTermAcademicStates];"));
            Assert.Equal(12, await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[TranscriptAttempts];"));
            Assert.Equal(12, await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[StudentHolds];"));
            Assert.True(await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[AcademicTerms];") > 0);
            Assert.True(await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [academics].[RegistrationWindows];") > 0);
            Assert.Equal(6, await ScalarIntAsync(
                connection,
                CompleteAcademicGraphSql));
            Assert.True(await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM [__EFMigrationsHistory];") > 0);

            var passwordHashes = await ReadPasswordHashesAsync(connection);
            Assert.Equal(10, passwordHashes.Count);
            Assert.All(passwordHashes, hash => Assert.False(string.IsNullOrWhiteSpace(hash)));
            Assert.DoesNotContain(
                bootstrapper.Credentials,
                credential => passwordHashes.Contains(credential.Secret, StringComparer.Ordinal));
        }

        var originalFingerprint = fixture.LogicalFingerprint;
        var originalCredentials = bootstrapper.Credentials.ToArray();
        IReadOnlyList<string> originalPasswordHashes;
        await using (var connection = new SqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync();
            originalPasswordHashes = await ReadPasswordHashesAsync(connection);
        }

        var replayFingerprint = await fixture.ReseedAsync();

        Assert.Equal(originalFingerprint, replayFingerprint);
        Assert.Equal(originalCredentials, bootstrapper.Credentials.ToArray());
        await using (var connection = new SqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync();
            Assert.Equal(
                originalPasswordHashes.ToArray(),
                (await ReadPasswordHashesAsync(connection)).ToArray());
        }

        await fixture.DisposeAsync();

        Assert.False(fixture.IsReady);
        Assert.Null(fixture.ConnectionString);
        Assert.Null(fixture.LogicalFingerprint);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Testing_credentials_stay_in_memory_and_rebuilds_keep_logical_data_stable()
    {
        var firstBootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 3);
        var secondBootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 3);
        await using var first = new SqlServerTestDatabaseFixture(firstBootstrapper);
        await using var second = new SqlServerTestDatabaseFixture(secondBootstrapper);

        await first.InitializeAsync();
        await second.InitializeAsync();

        Assert.NotEqual(first.DatabaseName, second.DatabaseName);
        Assert.Equal(first.LogicalFingerprint, second.LogicalFingerprint);
        Assert.Equal(7, firstBootstrapper.Credentials.Count);
        Assert.Equal(7, secondBootstrapper.Credentials.Count);
        Assert.Equal(
            firstBootstrapper.Credentials.Select(credential => credential.LoginIdentifier),
            secondBootstrapper.Credentials.Select(credential => credential.LoginIdentifier));
        Assert.All(firstBootstrapper.Credentials, AssertTransientCredential);
        Assert.All(secondBootstrapper.Credentials, AssertTransientCredential);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Testing_readiness_fails_closed_when_one_required_academic_edge_is_missing()
    {
        var bootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(studentCount: 3);
        await using var fixture = new SqlServerTestDatabaseFixture(bootstrapper);
        await fixture.InitializeAsync();
        Assert.NotNull(fixture.ConnectionString);
        var readyFingerprint = fixture.LogicalFingerprint;

        await using (var connection = new SqlConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "DELETE TOP (1) FROM [academics].[StudentHolds];",
                connection);
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        var readiness = await bootstrapper.VerifyReadinessAsync(
            fixture.ConnectionString,
            CancellationToken.None);

        Assert.False(readiness.SeedComplete);
        Assert.NotEqual(readyFingerprint, readiness.LogicalFingerprint);
    }

    private static void AssertTransientCredential(DemoCredential credential)
    {
        Assert.False(string.IsNullOrWhiteSpace(credential.LoginIdentifier));
        Assert.InRange(credential.Secret.Length, 15, 128);
        Assert.Equal(DateTimeKind.Utc, credential.GeneratedAtUtc.Kind);
    }

    private static async Task<int> ScalarIntAsync(
        SqlConnection connection,
        string commandText)
    {
        await using var command = new SqlCommand(commandText, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<IReadOnlyList<string>> ReadPasswordHashesAsync(
        SqlConnection connection)
    {
        await using var command = new SqlCommand(
            "SELECT [PasswordHash] FROM [auth].[ApplicationUsers] ORDER BY [NormalizedUserName];",
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        var values = new List<string>();
        while (await reader.ReadAsync())
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private const string CompleteAcademicGraphSql = """
        SELECT COUNT(*)
        FROM [academics].[Students] AS s
        INNER JOIN [auth].[ApplicationUsers] AS u
            ON u.[Id] = s.[ApplicationUserId]
        WHERE s.[Source] = N'Synthetic'
          AND NULLIF(LTRIM(RTRIM(s.[SourceReference])), N'') IS NOT NULL
          AND NULLIF(LTRIM(RTRIM(s.[DataVersion])), N'') IS NOT NULL
          AND (
              SELECT COUNT(*)
              FROM [academics].[StudentTermAcademicStates] AS sts
              WHERE sts.[StudentId] = s.[Id]
                AND sts.[Source] = N'Synthetic'
                AND NULLIF(LTRIM(RTRIM(sts.[SourceReference])), N'') IS NOT NULL
          ) = 1
          AND (
              SELECT COUNT(*)
              FROM [academics].[TranscriptAttempts] AS ta
              WHERE ta.[StudentId] = s.[Id]
                AND ta.[Source] = N'Synthetic'
                AND NULLIF(LTRIM(RTRIM(ta.[SourceReference])), N'') IS NOT NULL
          ) = 2
          AND (
              SELECT COUNT(*)
              FROM [academics].[StudentHolds] AS h
              WHERE h.[StudentId] = s.[Id]
                AND h.[Source] = N'Synthetic'
                AND NULLIF(LTRIM(RTRIM(h.[SourceReference])), N'') IS NOT NULL
          ) = 2;
        """;
}
