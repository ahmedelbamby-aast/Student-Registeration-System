using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Api.Composition;
using StudentRegistration.Api.Development;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.IntegrationTests.Infrastructure;

/// <summary>
/// SPEC-007/SPEC-008 migration-first synthetic bootstrap for one isolated
/// per-run Testing database. Credentials remain process memory only.
/// </summary>
public sealed class Spec008SqlServerTestDatabaseBootstrapper
    : ISqlServerTestDatabaseBootstrapper
{
    private const string MigrationId =
        "20260713010000_IdentityAcademicFoundation";
    private const string DemoTermCode = "DEMO-2026-FALL";
    private static readonly TimeProvider SeedTimeProvider = new FixedTimeProvider(
        new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero));
    private static readonly IReadOnlyList<string> Owners =
        ["SPEC-007", "SPEC-008"];
    private readonly int _studentCount;
    private IReadOnlyList<DemoCredential> _credentials = [];

    public Spec008SqlServerTestDatabaseBootstrapper(int studentCount)
    {
        if (studentCount is < 1 or > 25_000)
        {
            throw new ArgumentOutOfRangeException(nameof(studentCount));
        }

        _studentCount = studentCount;
    }

    public string ProfileVersion => SqlServerTestDatabaseFixture.SeedProfileVersion;

    public string DataClassification => "synthetic-only";

    public IReadOnlyList<string> ContributorOwnerSpecs => Owners;

    public IReadOnlyList<DemoCredential> Credentials => _credentials;

    public Task EnsureAvailableAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public async Task ApplyMigrationsAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        EnsureTestingTarget(connectionString);
        await using var provider = BuildProvider(connectionString, out _);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<StudentRegistrationDbContext>();
        await context.Database.MigrateAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SyntheticSeedResult> SeedAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        EnsureTestingTarget(connectionString);
        await using var provider = BuildProvider(
            connectionString,
            out var credentialHandoff);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<StudentRegistrationDbContext>();
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)
                .ConfigureAwait(false)).Any())
        {
            throw new InvalidOperationException(
                "Testing seed requires the complete migration chain first.");
        }

        Guid? completedImportId = null;
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
                async () =>
                {
                    var importId = Guid.NewGuid();
                    var prepared = false;
                    await using var transaction = await context.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken)
                        .ConfigureAwait(false);
                    try
                    {
                        var identity = scope.ServiceProvider
                            .GetRequiredService<DemoIdentitySeedContributor>();
                        var credentials = await identity.SeedAsync(
                                SqlServerTestDatabaseFixture.TestingEnvironmentName,
                                _studentCount,
                                cancellationToken)
                            .ConfigureAwait(false);

                        var termId = await EnsureCanonicalAcademicContextAsync(
                                context,
                                cancellationToken)
                            .ConfigureAwait(false);
                        var academic = scope.ServiceProvider
                            .GetRequiredService<DemoStudentProfileSeedContributor>();
                        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
                        {
                            var universityId = $"AI26{ordinal:00000}";
                            await academic.ContributeAsync(
                                    SqlServerTestDatabaseFixture.TestingEnvironmentName,
                                    ProfileVersion,
                                    ordinal,
                                    StableGuid(universityId),
                                    universityId,
                                    termId,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }

                        if (credentials.Count > 0)
                        {
                            await credentialHandoff.PrepareAsync(
                                    importId,
                                    credentials,
                                    cancellationToken)
                                .ConfigureAwait(false);
                            prepared = true;
                        }

                        await transaction.CommitAsync(cancellationToken)
                            .ConfigureAwait(false);
                        if (prepared)
                        {
                            completedImportId = importId;
                        }
                    }
                    catch
                    {
                        try
                        {
                            await transaction.RollbackAsync(CancellationToken.None)
                                .ConfigureAwait(false);
                        }
                        catch
                        {
                            // Preserve the original seed failure.
                        }

                        if (prepared)
                        {
                            await credentialHandoff.AbortAsync(
                                    importId,
                                    CancellationToken.None)
                                .ConfigureAwait(false);
                        }

                        throw;
                    }
                })
            .ConfigureAwait(false);

        if (completedImportId is { } importId)
        {
            await credentialHandoff.CompleteAsync(
                    importId,
                    CancellationToken.None)
                .ConfigureAwait(false);
            if (!credentialHandoff.TryGetCompleted(importId, out var credentials))
            {
                throw new InvalidOperationException(
                    "The Testing credential batch was not completed in memory.");
            }

            _credentials = credentials.ToArray();
        }

        context.ChangeTracker.Clear();
        var fingerprint = await ComputeLogicalFingerprintAsync(
                context,
                cancellationToken)
            .ConfigureAwait(false);
        return new SyntheticSeedResult(
            ProfileVersion,
            fingerprint,
            IsSyntheticOnly: true);
    }

    public async Task<SqlServerTestDatabaseReadiness> VerifyReadinessAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        EnsureTestingTarget(connectionString);
        await using var provider = BuildProvider(connectionString, out _);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<StudentRegistrationDbContext>();
        var appliedMigrations = await context.Database
            .GetAppliedMigrationsAsync(cancellationToken)
            .ConfigureAwait(false);
        var migrationsApplied = appliedMigrations.Contains(
                MigrationId,
                StringComparer.Ordinal)
            && !(await context.Database.GetPendingMigrationsAsync(cancellationToken)
                .ConfigureAwait(false)).Any();
        if (!migrationsApplied)
        {
            return new SqlServerTestDatabaseReadiness(
                MigrationsApplied: false,
                SeedComplete: false,
                ProfileVersion,
                LogicalFingerprint: string.Empty);
        }

        var seedComplete = await HasCompleteSyntheticGraphAsync(
                context,
                cancellationToken)
            .ConfigureAwait(false);
        var fingerprint = await ComputeLogicalFingerprintAsync(
                context,
                cancellationToken)
            .ConfigureAwait(false);
        return new SqlServerTestDatabaseReadiness(
            MigrationsApplied: true,
            seedComplete,
            ProfileVersion,
            fingerprint);
    }

    private static ServiceProvider BuildProvider(
        string connectionString,
        out TestingProvisionedCredentialHandoff credentialHandoff)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:StudentRegistration"] = connectionString
            })
            .Build();
        var environment = new TestingHostEnvironment();
        credentialHandoff = new TestingProvisionedCredentialHandoff(environment);
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddSingleton<TimeProvider>(SeedTimeProvider);
        services.AddSingleton(credentialHandoff);
        services.AddSingleton<IProvisionedCredentialHandoff>(credentialHandoff);
        services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        services.AddScoped<IIdentitySeedStore, IdentitySeedStore>();
        services.AddScoped<DemoIdentitySeedContributor>();
        services.AddScoped<DemoStudentProfileSeedContributor>();
        services.AddScoped<IAuditEventWriter, AuditTransactionWriter>();
        services.AddStudentRegistrationAcademicModule(configuration);
        services.AddStudentRegistrationSqlServer(configuration);
        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true
            });
    }

    private static void EnsureTestingTarget(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var databaseName = new SqlConnectionStringBuilder(connectionString)
            .InitialCatalog;
        NonProductionDatabaseGuard.EnsureSeedAllowed(
            SqlServerTestDatabaseFixture.TestingEnvironmentName,
            databaseName);
    }

    private static async Task<Guid> EnsureCanonicalAcademicContextAsync(
        StudentRegistrationDbContext context,
        CancellationToken cancellationToken)
    {
        var termId = StableGuid($"term:{SqlServerTestDatabaseFixture.SeedProfileVersion}");
        var windowId = StableGuid($"window:{SqlServerTestDatabaseFixture.SeedProfileVersion}");
        var creationRequestId = StableGuid(
            $"term-request:{SqlServerTestDatabaseFixture.SeedProfileVersion}");
        var payloadHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(
                $"{SqlServerTestDatabaseFixture.SeedProfileVersion}|{DemoTermCode}")));
        var term = await context.Set<AcademicTerm>()
            .SingleOrDefaultAsync(item => item.Id == termId, cancellationToken)
            .ConfigureAwait(false);
        if (term is null)
        {
            context.Add(new AcademicTerm(
                termId,
                DemoTermCode,
                creationRequestId,
                payloadHash,
                "Synthetic AI Demo Fall 2026",
                new DateOnly(2026, 9, 20),
                new DateOnly(2027, 1, 15),
                "Africa/Cairo",
                TermState.RegistrationOpen));
            context.Add(new RegistrationWindow(
                windowId,
                termId,
                RegistrationWindowScopeType.AllStudents,
                scopeValue: null,
                new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                RegistrationWindowLifecycleState.Published));
            await context.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
            context.ChangeTracker.Clear();
            return termId;
        }

        var windowExists = await context.Set<RegistrationWindow>()
            .AsNoTracking()
            .AnyAsync(
                item => item.Id == windowId && item.TermId == termId,
                cancellationToken)
            .ConfigureAwait(false);
        if (!string.Equals(term.Code, DemoTermCode, StringComparison.Ordinal)
            || term.CreationClientRequestId != creationRequestId
            || !string.Equals(term.CreationPayloadHash, payloadHash, StringComparison.Ordinal)
            || !windowExists)
        {
            throw new InvalidOperationException(
                "The Testing academic seed context does not match the fixture profile version.");
        }

        context.ChangeTracker.Clear();
        return termId;
    }

    private async Task<bool> HasCompleteSyntheticGraphAsync(
        StudentRegistrationDbContext context,
        CancellationToken cancellationToken)
    {
        var termId = StableGuid($"term:{ProfileVersion}");
        var windowId = StableGuid($"window:{ProfileVersion}");
        var users = await context.Set<ApplicationUser>()
            .AsNoTracking()
            .Select(user => new { user.Id, user.UniversityId })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var students = await context.Set<Student>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var states = await context.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var attempts = await context.Set<TranscriptAttempt>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var holds = await context.Set<StudentHold>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var termReady = await context.Set<AcademicTerm>()
            .AsNoTracking()
            .AnyAsync(
                term => term.Id == termId
                    && term.Code == DemoTermCode
                    && term.State == TermState.RegistrationOpen,
                cancellationToken)
            .ConfigureAwait(false);
        var windowReady = await context.Set<RegistrationWindow>()
            .AsNoTracking()
            .AnyAsync(
                window => window.Id == windowId
                    && window.TermId == termId
                    && window.State == RegistrationWindowLifecycleState.Published,
                cancellationToken)
            .ConfigureAwait(false);
        if (!termReady
            || !windowReady
            || users.Length != _studentCount + 3
            || students.Length != _studentCount
            || states.Length != _studentCount
            || attempts.Length != _studentCount * 2
            || holds.Length != _studentCount * 2)
        {
            return false;
        }

        var usersById = users.ToDictionary(user => user.Id);
        var statesByStudent = states.ToLookup(state => state.StudentId);
        var attemptsByStudent = attempts.ToLookup(attempt => attempt.StudentId);
        var holdsByStudent = holds.ToLookup(hold => hold.StudentId);
        foreach (var student in students)
        {
            if (!usersById.TryGetValue(student.ApplicationUserId, out var user)
                || string.IsNullOrWhiteSpace(user.UniversityId)
                || !string.Equals(student.Source, "Synthetic", StringComparison.Ordinal)
                || !string.Equals(student.DataVersion, ProfileVersion, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(student.SourceReference))
            {
                return false;
            }

            var studentStates = statesByStudent[student.Id].ToArray();
            var studentAttempts = attemptsByStudent[student.Id].ToArray();
            var studentHolds = holdsByStudent[student.Id].ToArray();
            if (studentStates.Length != 1
                || studentStates[0].TermId != termId
                || !string.Equals(
                    studentStates[0].DataVersion,
                    ProfileVersion,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(studentStates[0].SourceReference)
                || studentAttempts.Length != 2
                || studentAttempts.Any(attempt => attempt.TermId != termId
                    || !string.Equals(attempt.Source, "Synthetic", StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(attempt.SourceReference))
                || studentHolds.Length != 2
                || studentHolds.Count(hold => hold.BlocksRegistration) != 1
                || studentHolds.Any(hold => hold.TermId != termId
                    || !string.Equals(hold.Source, "Synthetic", StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(hold.SourceReference))
                || studentHolds.Count(hold => hold.IsActiveAt(
                    SeedTimeProvider.GetUtcNow().UtcDateTime)) > 100)
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<string> ComputeLogicalFingerprintAsync(
        StudentRegistrationDbContext context,
        CancellationToken cancellationToken)
    {
        var values = new List<string>();
        var users = await context.Set<ApplicationUser>()
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(users.Select(user => FormattableString.Invariant(
            $"user|{user.Id:D}|{user.UserName}|{user.UniversityId}")));
        var roles = await context.Set<RoleAssignment>()
            .AsNoTracking()
            .OrderBy(role => role.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(roles.Select(role => FormattableString.Invariant(
            $"role|{role.Id:D}|{role.ApplicationUserId:D}|{role.RoleCode}")));

        var terms = await context.Set<AcademicTerm>()
            .AsNoTracking()
            .OrderBy(term => term.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(terms.Select(term => FormattableString.Invariant(
            $"term|{term.Id:D}|{term.Code}|{term.DisplayName}|{term.TeachingStartsOn:yyyy-MM-dd}|{term.TeachingEndsOn:yyyy-MM-dd}|{term.TimeZoneId}|{term.State}")));
        var windows = await context.Set<RegistrationWindow>()
            .AsNoTracking()
            .OrderBy(window => window.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(windows.Select(window => FormattableString.Invariant(
            $"window|{window.Id:D}|{window.TermId:D}|{window.ScopeType}|{window.ScopeValue}|{window.OpensAtUtc:O}|{window.ClosesAtUtc:O}|{window.State}")));
        var students = await context.Set<Student>()
            .AsNoTracking()
            .OrderBy(student => student.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(students.Select(student => FormattableString.Invariant(
            $"student|{student.Id:D}|{student.ApplicationUserId:D}|{student.ProgramCode}|{student.Cohort}|{student.CurrentGpa}|{student.EarnedCredits}|{student.Standing}|{student.IsActive}|{student.Source}|{student.SourceReference}|{student.DataVersion}|{student.DataAsOfUtc:O}|{student.ImportedAtUtc:O}")));
        var states = await context.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .OrderBy(state => state.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(states.Select(state => FormattableString.Invariant(
            $"state|{state.Id:D}|{state.StudentId:D}|{state.TermId:D}|{state.GpaAtStart}|{state.EarnedCreditsAtStart}|{state.StandingAtStart}|{state.Source}|{state.SourceReference}|{state.DataVersion}|{state.DataAsOfUtc:O}")));
        var attempts = await context.Set<TranscriptAttempt>()
            .AsNoTracking()
            .OrderBy(attempt => attempt.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(attempts.Select(attempt => FormattableString.Invariant(
            $"attempt|{attempt.Id:D}|{attempt.StudentId:D}|{attempt.TermId:D}|{attempt.SupersedesAttemptId}|{attempt.CourseCode}|{attempt.Credits}|{attempt.GradeCode}|{attempt.Status}|{attempt.Source}|{attempt.SourceReference}|{attempt.ImportedAtUtc:O}")));
        var holds = await context.Set<StudentHold>()
            .AsNoTracking()
            .OrderBy(hold => hold.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        values.AddRange(holds.Select(hold => FormattableString.Invariant(
            $"hold|{hold.Id:D}|{hold.StudentId:D}|{hold.TermId:D}|{hold.Code}|{hold.Message}|{hold.BlocksRegistration}|{hold.EffectiveFromUtc:O}|{hold.EffectiveToUtc:O}|{hold.Source}|{hold.SourceReference}|{hold.ImportedAtUtc:O}")));

        var canonical = string.Join('\n', values);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class TestingHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } =
            SqlServerTestDatabaseFixture.TestingEnvironmentName;

        public string ApplicationName { get; set; } = "StudentRegistration.IntegrationTests";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
