using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Explicit Development-only migration and synthetic-data command. Application
/// startup does not invoke this initializer.
/// </summary>
public sealed class DemoDatabaseInitializer
{
    private const string DevelopmentDatabaseName = "StudentRegistration_Development";
    private const string DefaultSeedProfileVersion = "synthetic-fixture/2.0";
    private const string StableSeedIdentityVersion = "synthetic-fixture/1.0";
    private const int DefaultStudentCount = 25;
    private const string ConnectionStringName = "StudentRegistration";
    private const string DemoTermCode = "DEMO-2026-FALL";
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly StudentRegistrationDbContext _dbContext;
    private readonly DemoIdentitySeedContributor _identityContributor;
    private readonly DemoStudentProfileSeedContributor _academicContributor;
    private readonly IProvisionedCredentialHandoff _credentialHandoff;
    private readonly TimeProvider _timeProvider;
    private readonly DevelopmentManualTestDataSeeder? _manualTestDataSeeder;

    public DemoDatabaseInitializer(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        StudentRegistrationDbContext dbContext,
        DemoIdentitySeedContributor identityContributor,
        DemoStudentProfileSeedContributor academicContributor,
        IProvisionedCredentialHandoff credentialHandoff,
        TimeProvider timeProvider,
        DevelopmentManualTestDataSeeder? manualTestDataSeeder = null)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _identityContributor = identityContributor
            ?? throw new ArgumentNullException(nameof(identityContributor));
        _academicContributor = academicContributor
            ?? throw new ArgumentNullException(nameof(academicContributor));
        _credentialHandoff = credentialHandoff
            ?? throw new ArgumentNullException(nameof(credentialHandoff));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _manualTestDataSeeder = manualTestDataSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var settings = ValidateTargetAndSettings();

        // Migration is deliberately explicit and always precedes synthetic data.
        await _dbContext.Database.MigrateAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid? completedImportId = null;
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
                async () =>
                {
                    var importId = Guid.NewGuid();
                    var prepared = false;
                    await using var transaction = await _dbContext.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken)
                        .ConfigureAwait(false);
                    try
                    {
                        var credentials = await _identityContributor.SeedAsync(
                                "Development",
                                settings.StudentCount,
                                cancellationToken)
                            .ConfigureAwait(false);

                        var termId = await EnsureCanonicalAcademicContextAsync(
                                settings.SeedProfileVersion,
                                cancellationToken)
                            .ConfigureAwait(false);
                        for (var ordinal = 1; ordinal <= settings.StudentCount; ordinal++)
                        {
                            var universityId = $"AI26{ordinal:00000}";
                            await _academicContributor.ContributeAsync(
                                    "Development",
                                    settings.SeedProfileVersion,
                                    ordinal,
                                    StableGuid(universityId),
                                    universityId,
                                    termId,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }

                        if (_manualTestDataSeeder is not null)
                        {
                            await _manualTestDataSeeder.SeedAsync(
                                    termId,
                                    settings.SeedProfileVersion,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }

                        if (credentials.Count > 0)
                        {
                            await _credentialHandoff.PrepareAsync(
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
                            // Preserve the original bootstrap failure.
                        }

                        if (prepared)
                        {
                            await _credentialHandoff.AbortAsync(
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
            await _credentialHandoff.CompleteAsync(
                    importId,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
    }

    private BootstrapSettings ValidateTargetAndSettings()
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Synthetic database initialization is available only in Development.");
        }

        EnsureDevelopmentConnection(_dbContext.Database.GetConnectionString());
        var configuredConnection = _configuration.GetConnectionString(ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(configuredConnection))
        {
            EnsureDevelopmentConnection(configuredConnection);
        }

        var studentCount = _configuration.GetValue<int?>("DemoDatabase:StudentCount")
            ?? DefaultStudentCount;
        if (studentCount is < 1 or > 25_000)
        {
            throw new InvalidOperationException(
                "DemoDatabase:StudentCount must be between 1 and 25000.");
        }

        var seedProfileVersion = _configuration[
            "DemoDatabase:SeedProfileVersion"]?.Trim()
            ?? DefaultSeedProfileVersion;
        if (string.IsNullOrWhiteSpace(seedProfileVersion)
            || seedProfileVersion.Length > 100)
        {
            throw new InvalidOperationException(
                "DemoDatabase:SeedProfileVersion must contain 1 to 100 characters.");
        }

        return new BootstrapSettings(studentCount, seedProfileVersion);
    }

    private static void EnsureDevelopmentConnection(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The Development database connection is required.");
        }

        var databaseName = new SqlConnectionStringBuilder(connectionString)
            .InitialCatalog;
        if (!string.Equals(
                databaseName,
                DevelopmentDatabaseName,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Synthetic initialization requires the exact Development database target.");
        }
    }

    private async Task<Guid> EnsureCanonicalAcademicContextAsync(
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        var termId = StableGuid($"term:{StableSeedIdentityVersion}");
        var windowId = StableGuid($"window:{StableSeedIdentityVersion}");
        var creationRequestId = StableGuid($"term-request:{StableSeedIdentityVersion}");
        var payloadHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{StableSeedIdentityVersion}|{DemoTermCode}")));
        var term = await _dbContext.Set<AcademicTerm>()
            .SingleOrDefaultAsync(item => item.Id == termId, cancellationToken)
            .ConfigureAwait(false);
        if (term is null)
        {
            _dbContext.Add(new AcademicTerm(
                termId,
                DemoTermCode,
                creationRequestId,
                payloadHash,
                "Synthetic AI Demo Fall 2026",
                new DateOnly(2026, 9, 20),
                new DateOnly(2027, 1, 15),
                "Africa/Cairo",
                StudentRegistration.Contracts.TermState.RegistrationOpen));
            _dbContext.Add(new RegistrationWindow(
                windowId,
                termId,
                RegistrationWindowScopeType.AllStudents,
                scopeValue: null,
                new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                RegistrationWindowLifecycleState.Published));
            await _dbContext.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
            _dbContext.ChangeTracker.Clear();
            return termId;
        }

        var windowExists = await _dbContext.Set<RegistrationWindow>()
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
                "The existing Development academic seed context does not match the selected profile version.");
        }

        _dbContext.ChangeTracker.Clear();
        return termId;
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record BootstrapSettings(
        int StudentCount,
        string SeedProfileVersion);
}
