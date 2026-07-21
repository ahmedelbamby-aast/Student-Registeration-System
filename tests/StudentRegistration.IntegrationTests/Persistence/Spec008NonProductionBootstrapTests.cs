using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Api.Development;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.TestSupport;
using Testcontainers.MsSql;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class Spec008NonProductionBootstrapTests
{
    private const string InitializerPath =
        "src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs";
    private static readonly DateTimeOffset SeedNowUtc =
        new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Development_initializer_is_an_explicit_development_only_operation()
    {
        var type = typeof(DemoDatabaseInitializer);
        var source = RepositoryFiles.Read(InitializerPath);

        Assert.True(type.IsClass && type.IsSealed && type.IsPublic);
        var method = type.GetMethod(
            "InitializeAsync",
            [typeof(CancellationToken)]);
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
        RepositoryFiles.ContainsAll(
            source,
            "IsDevelopment()",
            NonProductionDatabaseGuard.DevelopmentDatabaseName,
            "InitializeAsync",
            "MigrateAsync");
        Assert.DoesNotContain("EnsureCreated", source, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Production", "StudentRegistration_Development")]
    [InlineData("Staging", "StudentRegistration_Development")]
    [InlineData("Development", "StudentRegistration_Production")]
    [InlineData("Development", "StudentRegistration_Test_deadbeefdeadbeefdeadbeefdeadbeef")]
    public async Task Real_initializer_rejects_the_environment_or_initial_catalog_before_sql(
        string environmentName,
        string databaseName)
    {
        var events = new BootstrapEventRecorder();
        var connectionString = InvalidConnectionString(databaseName);
        await using var context = CreateDbContext(connectionString, events);
        var initializer = CreateInitializer(
            environmentName,
            connectionString,
            context,
            events);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            initializer.InitializeAsync(CancellationToken.None));

        Assert.IsNotType<SqlException>(exception);
        Assert.Empty(events.Events);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Development_initializer_executes_migration_identity_academics_and_reveal_once_transaction_protocol()
    {
        await using var container = new MsSqlBuilder(
                SqlServerTestDatabaseFixture.SqlServerImage)
            .WithPassword($"Srs!1{Guid.NewGuid():N}a")
            .Build();
        await container.StartAsync();
        await CreateDevelopmentDatabaseAsync(container);
        var connectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = NonProductionDatabaseGuard.DevelopmentDatabaseName
        }.ConnectionString;

        var success = new BootstrapEventRecorder();
        await using (var context = CreateDbContext(connectionString, success))
        {
            var initializer = CreateInitializer(
                "Development",
                connectionString,
                context,
                success);

            await initializer.InitializeAsync(CancellationToken.None);

            Assert.Equal(
                [
                    "migration",
                    "identity",
                    "academic:1",
                    "academic:2",
                    "prepare",
                    "commit",
                    "complete"
                ],
                success.Events);
            Assert.Equal(5, success.PreparedCredentialCount);
            Assert.Equal(success.PreparedImportId, success.CompletedImportId);
            Assert.Null(success.AbortedImportId);
        }

        var commitFailure = new BootstrapEventRecorder(failPreparedCommit: true);
        await using (var context = CreateDbContext(connectionString, commitFailure))
        {
            var initializer = CreateInitializer(
                "Development",
                connectionString,
                context,
                commitFailure);

            await Assert.ThrowsAsync<SyntheticSeedCommitException>(() =>
                initializer.InitializeAsync(CancellationToken.None));

            Assert.Equal(
                [
                    "migration",
                    "identity",
                    "academic:1",
                    "academic:2",
                    "prepare",
                    "rollback",
                    "abort"
                ],
                commitFailure.Events);
            Assert.Equal(
                commitFailure.PreparedImportId,
                commitFailure.AbortedImportId);
            Assert.Null(commitFailure.CompletedImportId);
        }
    }

    [Fact]
    public void Development_persists_until_an_explicit_guarded_reset()
    {
        var databaseName = NonProductionDatabaseGuard.DevelopmentDatabaseName;

        NonProductionDatabaseGuard.EnsureSeedAllowed("Development", databaseName);
        NonProductionDatabaseGuard.EnsureExplicitResetAllowed(
            "Development",
            databaseName);

        Assert.False(NonProductionDatabaseGuard.ShouldDisposeAfterRun(
            "Development",
            databaseName));
    }

    private static DemoDatabaseInitializer CreateInitializer(
        string environmentName,
        string connectionString,
        StudentRegistrationDbContext context,
        BootstrapEventRecorder events)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:StudentRegistration"] = connectionString,
                ["DemoDatabase:StudentCount"] = "2",
                ["DemoDatabase:SeedProfileVersion"] =
                    SqlServerTestDatabaseFixture.SeedProfileVersion
            })
            .Build();
        var timeProvider = new FixedTimeProvider(SeedNowUtc);
        var identity = new DemoIdentitySeedContributor(
            new RecordingIdentitySeedStore(context, events),
            new PasswordHasher<ApplicationUser>(),
            timeProvider);
        var academics = new DemoStudentProfileSeedContributor(
            new RecordingAcademicSeedStore(events));

        return new DemoDatabaseInitializer(
            new TestWebHostEnvironment(environmentName),
            configuration,
            context,
            identity,
            academics,
            events,
            timeProvider);
    }

    private static StudentRegistrationDbContext CreateDbContext(
        string connectionString,
        BootstrapEventRecorder events)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .AddInterceptors(events)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static string InvalidConnectionString(string databaseName) =>
        new SqlConnectionStringBuilder
        {
            DataSource = "invalid.example",
            InitialCatalog = databaseName,
            UserID = "unused",
            Password = "unused",
            TrustServerCertificate = true,
            ConnectTimeout = 1
        }.ConnectionString;

    private static async Task CreateDevelopmentDatabaseAsync(MsSqlContainer container)
    {
        var result = await container.ExecScriptAsync($"""
            IF DB_ID(N'{NonProductionDatabaseGuard.DevelopmentDatabaseName}') IS NOT NULL
                THROW 51020, 'Development database already exists.', 1;
            CREATE DATABASE [{NonProductionDatabaseGuard.DevelopmentDatabaseName}];
            """);
        Assert.Equal<long?>(0L, result.ExitCode);
    }

    private sealed class RecordingIdentitySeedStore(
        StudentRegistrationDbContext context,
        BootstrapEventRecorder events)
        : IIdentitySeedStore
    {
        public async Task<IReadOnlySet<Guid>> ReconcileAsync(
            IReadOnlyList<DemoSeedIdentity> identities,
            IReadOnlySet<Guid> retiredUserIds,
            DateTime retiredAtUtc,
            string clientRequestId,
            CancellationToken cancellationToken)
        {
            Assert.NotEmpty(identities);
            Assert.Single(retiredUserIds);
            Assert.Equal(DateTimeKind.Utc, retiredAtUtc.Kind);
            Assert.False(string.IsNullOrWhiteSpace(clientRequestId));
            Assert.Empty(await context.Database.GetPendingMigrationsAsync(cancellationToken));
            events.Record("migration");
            events.Record("identity");
            return identities.Select(identity => identity.UserId).ToHashSet();
        }
    }

    private sealed class RecordingAcademicSeedStore(BootstrapEventRecorder events)
        : IDemoStudentProfileSeedStore
    {
        public Task<DemoStudentProfileSeedResult> ReconcileAsync(
            DemoStudentProfileSeedCommand command,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(
                SqlServerTestDatabaseFixture.SeedProfileVersion,
                command.SeedProfileVersion);
            Assert.Equal($"AI26{command.FixtureOrdinal:00000}", command.UniversityId);
            Assert.Equal(2, command.TranscriptAttempts.Count);
            Assert.Equal(2, command.Holds.Count);
            Assert.Equal(command.Student.Id, command.StudentTermAcademicState.StudentId);
            events.Record($"academic:{command.FixtureOrdinal}");
            return Task.FromResult(new DemoStudentProfileSeedResult(
                command.Student.Id,
                Created: true));
        }
    }

    private sealed class BootstrapEventRecorder(bool failPreparedCommit = false)
        : DbTransactionInterceptor, IProvisionedCredentialHandoff
    {
        private readonly List<string> _events = [];

        public IReadOnlyList<string> Events => _events;

        public Guid? PreparedImportId { get; private set; }

        public Guid? CompletedImportId { get; private set; }

        public Guid? AbortedImportId { get; private set; }

        public int PreparedCredentialCount { get; private set; }

        public void Record(string value) => _events.Add(value);

        public Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Null(PreparedImportId);
            Assert.NotEmpty(credentials);
            PreparedImportId = importId;
            PreparedCredentialCount = credentials.Count;
            Record("prepare");
            return Task.CompletedTask;
        }

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(PreparedImportId, importId);
            CompletedImportId = importId;
            Record("complete");
            return Task.CompletedTask;
        }

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(PreparedImportId, importId);
            AbortedImportId = importId;
            Record("abort");
            return Task.CompletedTask;
        }

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (PreparedImportId is null)
            {
                return ValueTask.FromResult(result);
            }

            if (failPreparedCommit)
            {
                throw new SyntheticSeedCommitException();
            }

            return ValueTask.FromResult(result);
        }

        public override Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (PreparedImportId is not null && CompletedImportId is null)
            {
                Record("commit");
            }

            return Task.CompletedTask;
        }

        public override Task TransactionRolledBackAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (PreparedImportId is not null)
            {
                Record("rollback");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class SyntheticSeedCommitException : Exception;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class TestWebHostEnvironment(string environmentName)
        : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "StudentRegistration.Api";

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = environmentName;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
