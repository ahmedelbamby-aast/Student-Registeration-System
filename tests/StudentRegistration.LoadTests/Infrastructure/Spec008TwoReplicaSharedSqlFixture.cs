using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StudentRegistration.Academics.Endpoints;
using StudentRegistration.Api.Composition;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.IdentityAccess.Endpoints;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Endpoints;
using Testcontainers.MsSql;

namespace StudentRegistration.LoadTests.Infrastructure;

/// <summary>
/// One isolated SQL Server database shared by two separately hosted stateless
/// API replicas. This is test infrastructure, not a deployment topology.
/// </summary>
public sealed class Spec008TwoReplicaSharedSqlFixture : IAsyncDisposable
{
    public const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";
    public const int DefaultStudentCount = 25_000;
    public const string SeedProfileVersion = "synthetic-fixture/1.0";
    private const string TestingEnvironment = "Testing";
    private const string DemoTermCode = "DEMO-2026-FALL";
    private const int BulkBatchSize = 1_000;
    private static readonly DateTime SeedEpochUtc =
        new(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IReadOnlyList<StaffFixture> StaffFixtures =
    [
        StaffFixture.Create(
            "ADM-0001",
            "ADMIN.DEMO",
            "ADM-0001",
            "Demo Administrator",
            RolePolicies.Admin),
        StaffFixture.Create(
            "LEC-0001",
            "LECTURER.DEMO",
            "LEC-0001",
            "Demo Lecturer",
            RolePolicies.Lecturer),
        StaffFixture.Create(
            "TA-0001",
            "TA.DEMO",
            "TA-0001",
            "Demo Teaching Assistant",
            RolePolicies.TeachingAssistant),
        StaffFixture.Create(
            "DUAL-0001",
            "DUAL.DEMO",
            "DUAL-0001",
            "Demo Lecturer and TA",
            RolePolicies.Lecturer,
            RolePolicies.TeachingAssistant)
    ];
    private readonly MsSqlContainer _container;
    private readonly int _studentCount;
    private readonly bool _enableReadCommittedSnapshot;
    private readonly string _artifactRoot;
    private WebApplication? _firstReplica;
    private WebApplication? _secondReplica;
    private HttpClient? _firstClient;
    private HttpClient? _secondClient;
    private string? _certificatePassword;
    private string? _certificatePath;
    private bool _containerStarted;
    private bool _databaseCreated;
    private bool _initializationAttempted;
    private bool _disposed;

    public Spec008TwoReplicaSharedSqlFixture(
        int studentCount = DefaultStudentCount,
        bool enableReadCommittedSnapshot = false)
    {
        if (studentCount is < 1 or > DefaultStudentCount)
        {
            throw new ArgumentOutOfRangeException(nameof(studentCount));
        }

        _studentCount = studentCount;
        _enableReadCommittedSnapshot = enableReadCommittedSnapshot;
        RunId = Guid.NewGuid().ToString("N");
        DatabaseName = $"StudentRegistration_Test_{RunId}";
        _artifactRoot = Path.Combine(
            Path.GetTempPath(),
            "StudentRegistration-SPEC008-load",
            RunId);
        var password =
            $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        _container = new MsSqlBuilder(SqlServerImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithLogger(NullLogger.Instance)
            .Build();
    }

    public string RunId { get; }

    public string DatabaseName { get; }

    public string? ConnectionString { get; private set; }

    public bool IsReady { get; private set; }

    public IReadOnlyList<Spec008AuthenticatedStudentSession> StudentSessions
    {
        get;
        private set;
    } = [];

    public bool CrossReplicaTicketVerified { get; private set; }

    public Uri FirstReplicaAddress { get; } =
        new("https://spec008-replica-1.test/");

    public Uri SecondReplicaAddress { get; } =
        new("https://spec008-replica-2.test/");

    public HttpClient FirstReplicaClient => _firstClient
        ?? throw new InvalidOperationException("The first API replica is not ready.");

    public HttpClient SecondReplicaClient => _secondClient
        ?? throw new InvalidOperationException("The second API replica is not ready.");

    public IReadOnlyList<HttpClient> ReplicaClients =>
        [FirstReplicaClient, SecondReplicaClient];

    public async Task StopFirstReplicaAsync()
    {
        if (_firstReplica is null || _firstClient is null)
        {
            throw new InvalidOperationException("The first API replica is not running.");
        }

        _firstClient.Dispose();
        _firstClient = null;
        var replica = _firstReplica;
        _firstReplica = null;
        await StopReplicaAsync(replica).ConfigureAwait(false);
    }

    public async Task RestartFirstReplicaAsync(
        CancellationToken cancellationToken = default)
    {
        if (_firstReplica is not null || _firstClient is not null ||
            ConnectionString is null)
        {
            throw new InvalidOperationException(
                "Only a stopped, initialized first API replica can be restarted.");
        }

        _firstReplica = await StartReplicaAsync(
                FirstReplicaAddress,
                ConnectionString,
                cancellationToken)
            .ConfigureAwait(false);
        _firstClient = _firstReplica.GetTestClient();
        _firstClient.BaseAddress = FirstReplicaAddress;
        await VerifyAuthenticatedContextAsync(
                _firstClient,
                StudentSessions[0],
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initializationAttempted)
        {
            throw new InvalidOperationException(
                "The two-replica fixture can be initialized only once.");
        }

        _initializationAttempted = true;
        try
        {
            await _container.StartAsync(cancellationToken).ConfigureAwait(false);
            _containerStarted = true;
            await CreateDatabaseAsync(cancellationToken).ConfigureAwait(false);
            _databaseCreated = true;
            ConnectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                _container.GetConnectionString())
            {
                InitialCatalog = DatabaseName
            }.ConnectionString;

            await MigrateAndSeedAsync(ConnectionString, cancellationToken)
                .ConfigureAwait(false);
            await CreateCertificateAsync(cancellationToken).ConfigureAwait(false);
            _firstReplica = await StartReplicaAsync(
                    FirstReplicaAddress,
                    ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);
            _firstClient = _firstReplica.GetTestClient();
            _firstClient.BaseAddress = FirstReplicaAddress;
            StudentSessions = await CreateAuthenticatedStudentSessionsAsync(
                    _firstReplica,
                    cancellationToken)
                .ConfigureAwait(false);
            _secondReplica = await StartReplicaAsync(
                    SecondReplicaAddress,
                    ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);
            _secondClient = _secondReplica.GetTestClient();
            _secondClient.BaseAddress = SecondReplicaAddress;
            await VerifyCrossReplicaTicketAsync(cancellationToken)
                .ConfigureAwait(false);
            IsReady = true;
        }
        catch
        {
            try
            {
                await CleanupAsync().ConfigureAwait(false);
            }
            catch
            {
                // Preserve the original initialization failure.
            }

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async Task MigrateAndSeedAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new StudentRegistrationDbContext(options);
        await context.Database.MigrateAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            await BulkSeedMigratedSchemaAsync(
                    connection,
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
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
                // Preserve the original migration or seed failure.
            }

            throw;
        }
    }

    private async Task BulkSeedMigratedSchemaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var termId = StableGuid($"term:{SeedProfileVersion}");
        var windowId = StableGuid($"window:{SeedProfileVersion}");
        var requestId = StableGuid($"term-request:{SeedProfileVersion}");
        var payloadHash = HashText($"{SeedProfileVersion}|{DemoTermCode}");
        var passwordHash = CreateNonExportedPasswordHash();

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[AcademicTerms]",
            [
                Column("Id", typeof(Guid)),
                Column("Code", typeof(string)),
                Column("CreationClientRequestId", typeof(Guid)),
                Column("CreationPayloadHash", typeof(string)),
                Column("DisplayName", typeof(string)),
                Column("TeachingStartsOn", typeof(DateTime)),
                Column("TeachingEndsOn", typeof(DateTime)),
                Column("TimeZoneId", typeof(string)),
                Column("State", typeof(string))
            ],
            [
                [
                    termId,
                    DemoTermCode,
                    requestId,
                    payloadHash,
                    "Synthetic AI Demo Fall 2026",
                    new DateTime(2026, 9, 20),
                    new DateTime(2027, 1, 15),
                    "Africa/Cairo",
                    "registrationOpen"
                ]
            ],
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[auth].[ApplicationUsers]",
            [
                Column("Id", typeof(Guid)),
                Column("UserName", typeof(string)),
                Column("NormalizedUserName", typeof(string)),
                Column("UniversityId", typeof(string), nullable: true),
                Column("PasswordHash", typeof(string)),
                Column("SecurityStamp", typeof(string)),
                Column("IsEnabled", typeof(bool)),
                Column("AccessFailedCount", typeof(int)),
                Column("LockoutEndUtc", typeof(DateTime), nullable: true)
            ],
            IdentityRows(passwordHash),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[RegistrationWindows]",
            [
                Column("Id", typeof(Guid)),
                Column("TermId", typeof(Guid)),
                Column("ScopeType", typeof(string)),
                Column("ScopeValue", typeof(string), nullable: true),
                Column("OpensAtUtc", typeof(DateTime)),
                Column("ClosesAtUtc", typeof(DateTime)),
                Column("State", typeof(string))
            ],
            [
                [
                    windowId,
                    termId,
                    "all-students",
                    null,
                    new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                    "published"
                ]
            ],
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[auth].[RoleAssignments]",
            [
                Column("Id", typeof(Guid)),
                Column("ApplicationUserId", typeof(Guid)),
                Column("RoleCode", typeof(string)),
                Column("EffectiveFromUtc", typeof(DateTime)),
                Column("EffectiveToUtc", typeof(DateTime), nullable: true),
                Column("AssignedByReference", typeof(string))
            ],
            RoleRows(),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[auth].[StudentActivations]",
            [
                Column("Id", typeof(Guid)),
                Column("ApplicationUserId", typeof(Guid)),
                Column("ProvisionedAtUtc", typeof(DateTime)),
                Column("ActivatedAtUtc", typeof(DateTime), nullable: true),
                Column("FailedAttemptCount", typeof(int))
            ],
            StudentActivationRows(),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[auth].[Staff]",
            [
                Column("Id", typeof(Guid)),
                Column("ApplicationUserId", typeof(Guid)),
                Column("StaffNumber", typeof(string)),
                Column("DisplayName", typeof(string)),
                Column("IsActive", typeof(bool))
            ],
            StaffRows(),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[Students]",
            [
                Column("Id", typeof(Guid)),
                Column("ApplicationUserId", typeof(Guid)),
                Column("ProgramCode", typeof(string)),
                Column("Cohort", typeof(string)),
                Column("CurrentGpa", typeof(decimal)),
                Column("EarnedCredits", typeof(decimal)),
                Column("Standing", typeof(string)),
                Column("IsActive", typeof(bool)),
                Column("Source", typeof(string)),
                Column("SourceReference", typeof(string)),
                Column("DataVersion", typeof(string)),
                Column("DataAsOfUtc", typeof(DateTime)),
                Column("ImportedAtUtc", typeof(DateTime))
            ],
            StudentRows(),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[StudentTermAcademicStates]",
            [
                Column("Id", typeof(Guid)),
                Column("StudentId", typeof(Guid)),
                Column("TermId", typeof(Guid)),
                Column("GpaAtStart", typeof(decimal)),
                Column("EarnedCreditsAtStart", typeof(decimal)),
                Column("StandingAtStart", typeof(string)),
                Column("Source", typeof(string)),
                Column("SourceReference", typeof(string)),
                Column("DataVersion", typeof(string)),
                Column("DataAsOfUtc", typeof(DateTime))
            ],
            StudentTermRows(termId),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[TranscriptAttempts]",
            [
                Column("Id", typeof(Guid)),
                Column("StudentId", typeof(Guid)),
                Column("TermId", typeof(Guid)),
                Column("SupersedesAttemptId", typeof(Guid), nullable: true),
                Column("CourseCode", typeof(string)),
                Column("Credits", typeof(decimal)),
                Column("GradeCode", typeof(string), nullable: true),
                Column("Status", typeof(string)),
                Column("Source", typeof(string)),
                Column("SourceReference", typeof(string)),
                Column("ImportedAtUtc", typeof(DateTime))
            ],
            TranscriptRows(termId),
            cancellationToken).ConfigureAwait(false);

        await BulkCopyAsync(
            connection,
            transaction,
            "[academics].[StudentHolds]",
            [
                Column("Id", typeof(Guid)),
                Column("StudentId", typeof(Guid)),
                Column("TermId", typeof(Guid)),
                Column("Code", typeof(string)),
                Column("Message", typeof(string)),
                Column("BlocksRegistration", typeof(bool)),
                Column("EffectiveFromUtc", typeof(DateTime)),
                Column("EffectiveToUtc", typeof(DateTime), nullable: true),
                Column("Source", typeof(string)),
                Column("SourceReference", typeof(string)),
                Column("ImportedAtUtc", typeof(DateTime))
            ],
            HoldRows(termId),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task BulkCopyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string destinationTable,
        IReadOnlyList<BulkColumn> columns,
        IEnumerable<object?[]> rows,
        CancellationToken cancellationToken)
    {
        using var table = new DataTable
        {
            Locale = CultureInfo.InvariantCulture
        };
        foreach (var column in columns)
        {
            table.Columns.Add(new DataColumn(column.Name, column.Type)
            {
                AllowDBNull = column.Nullable
            });
        }

        using var bulkCopy = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.CheckConstraints |
            SqlBulkCopyOptions.KeepNulls |
            SqlBulkCopyOptions.TableLock,
            transaction)
        {
            DestinationTableName = destinationTable,
            BatchSize = BulkBatchSize,
            BulkCopyTimeout = 0
        };
        foreach (var column in columns)
        {
            bulkCopy.ColumnMappings.Add(column.Name, column.Name);
        }

        foreach (var row in rows)
        {
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());
            if (table.Rows.Count < BulkBatchSize)
            {
                continue;
            }

            await bulkCopy.WriteToServerAsync(table, cancellationToken)
                .ConfigureAwait(false);
            table.Clear();
        }

        if (table.Rows.Count > 0)
        {
            await bulkCopy.WriteToServerAsync(table, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private IEnumerable<object?[]> IdentityRows(string passwordHash)
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var universityId = UniversityId(ordinal);
            yield return
            [
                StableGuid(universityId),
                universityId,
                universityId,
                universityId,
                passwordHash,
                NewSecurityStamp(),
                true,
                0,
                null
            ];
        }

        foreach (var staff in StaffFixtures)
        {
            yield return
            [
                staff.UserId,
                staff.UserName,
                staff.NormalizedUserName,
                null,
                passwordHash,
                NewSecurityStamp(),
                true,
                0,
                null
            ];
        }
    }

    private IEnumerable<object?[]> RoleRows()
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var applicationUserId = StableGuid(UniversityId(ordinal));
            yield return
            [
                StableGuid(applicationUserId, "role:Student"),
                applicationUserId,
                RolePolicies.Student,
                SeedEpochUtc,
                null,
                "demo-seed:spec007-demo-seed-v1"
            ];
        }

        foreach (var staff in StaffFixtures)
        {
            foreach (var role in staff.Roles)
            {
                yield return
                [
                    StableGuid(staff.UserId, $"role:{role}"),
                    staff.UserId,
                    role,
                    SeedEpochUtc,
                    null,
                    "demo-seed:spec007-demo-seed-v1"
                ];
            }
        }
    }

    private IEnumerable<object?[]> StudentActivationRows()
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var applicationUserId = StableGuid(UniversityId(ordinal));
            yield return
            [
                StableGuid(applicationUserId, "student-activation"),
                applicationUserId,
                SeedEpochUtc,
                null,
                0
            ];
        }
    }

    private static IEnumerable<object?[]> StaffRows() =>
        StaffFixtures.Select(staff => new object?[]
        {
            StableGuid(staff.UserId, "staff"),
            staff.UserId,
            staff.StaffNumber,
            staff.DisplayName,
            true
        });

    private IEnumerable<object?[]> StudentRows()
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var identity = SeedIdentity(ordinal);
            yield return
            [
                identity.StudentId,
                identity.ApplicationUserId,
                "AI",
                "2026",
                identity.Gpa,
                identity.EarnedCredits,
                "Active",
                true,
                "Synthetic",
                identity.SourceReference,
                SeedProfileVersion,
                identity.DataAsOfUtc,
                identity.ImportedAtUtc
            ];
        }
    }

    private IEnumerable<object?[]> StudentTermRows(Guid termId)
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var identity = SeedIdentity(ordinal);
            yield return
            [
                StableGuid($"student-term:{identity.Identity}:{termId:D}"),
                identity.StudentId,
                termId,
                identity.Gpa,
                identity.EarnedCredits,
                "Active",
                "Synthetic",
                $"{identity.SourceReference};TermId={termId:D}",
                SeedProfileVersion,
                identity.DataAsOfUtc
            ];
        }
    }

    private IEnumerable<object?[]> TranscriptRows(Guid termId)
    {
        foreach (var ordinal in Enumerable.Range(1, _studentCount))
        {
            var identity = SeedIdentity(ordinal);
            yield return TranscriptRow(identity, termId, "CC214", "B+");
            yield return TranscriptRow(identity, termId, "AI201", "A");
        }
    }

    private static object?[] TranscriptRow(
        SeedStudent identity,
        Guid termId,
        string courseCode,
        string gradeCode) =>
        [
            StableGuid($"attempt:{identity.Identity}:{courseCode}"),
            identity.StudentId,
            termId,
            null,
            courseCode,
            3m,
            gradeCode,
            "passed",
            "Synthetic",
            $"{identity.SourceReference};CourseCode={courseCode}",
            identity.ImportedAtUtc
        ];

    private IEnumerable<object?[]> HoldRows(Guid termId)
    {
        for (var ordinal = 1; ordinal <= _studentCount; ordinal++)
        {
            var identity = SeedIdentity(ordinal);
            yield return
            [
                StableGuid($"hold:{identity.Identity}:blocking"),
                identity.StudentId,
                termId,
                "REGISTRATION-HOLD",
                "Resolve this synthetic demo hold before registration.",
                true,
                identity.DataAsOfUtc.AddDays(-1),
                null,
                "Synthetic",
                $"{identity.SourceReference};Hold=blocking",
                identity.ImportedAtUtc
            ];
            yield return
            [
                StableGuid($"hold:{identity.Identity}:advisory"),
                identity.StudentId,
                termId,
                "ADVISORY",
                "Synthetic advising reminder.",
                false,
                identity.DataAsOfUtc.AddDays(-1),
                null,
                "Synthetic",
                $"{identity.SourceReference};Hold=advisory",
                identity.ImportedAtUtc
            ];
        }
    }

    private static SeedStudent SeedIdentity(int ordinal)
    {
        var universityId = UniversityId(ordinal);
        var identity = $"{SeedProfileVersion}:{ordinal}";
        var dataAsOfUtc = SeedEpochUtc.AddMinutes(ordinal);
        return new SeedStudent(
            identity,
            StableGuid(universityId),
            StableGuid($"student:{identity}"),
            2.50m + (ordinal % 15) / 10m,
            30m + ordinal % 90,
            $"SeedProfileVersion={SeedProfileVersion};FixtureOrdinal={ordinal}",
            dataAsOfUtc,
            dataAsOfUtc.AddHours(1));
    }

    private static string CreateNonExportedPasswordHash()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        try
        {
            var transient = new ApplicationUser(
                StableGuid("spec008-load-password"),
                "load.fixture",
                "LOAD.FIXTURE",
                universityId: null,
                "TRANSIENT",
                HashText("load-fixture-security-stamp"));
            return new PasswordHasher<ApplicationUser>().HashPassword(
                transient,
                Convert.ToBase64String(secretBytes));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretBytes);
        }
    }

    private static BulkColumn Column(
        string name,
        Type type,
        bool nullable = false) =>
        new(name, type, nullable);

    private static string UniversityId(int ordinal) => $"AI26{ordinal:00000}";

    private async Task<IReadOnlyList<Spec008AuthenticatedStudentSession>>
        CreateAuthenticatedStudentSessionsAsync(
            WebApplication replica,
            CancellationToken cancellationToken)
    {
        await using var scope = replica.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<StudentRegistrationDbContext>();
        var students = await context.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(user => user.UniversityId != null)
            .OrderBy(user => user.UniversityId)
            .Select(user => new
            {
                user.Id,
                user.UserName,
                UniversityId = user.UniversityId!,
                user.SecurityStamp
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (students.Count != _studentCount)
        {
            throw new InvalidOperationException(
                $"Expected {_studentCount} load students but found {students.Count}.");
        }

        var cookieOptions = replica.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityAuthenticationDefaults.AuthenticationScheme);
        var issuedAt = DateTimeOffset.UtcNow;
        var sessions = new List<Spec008AuthenticatedStudentSession>(students.Count);
        for (var index = 0; index < students.Count; index++)
        {
            var student = students[index];
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, student.Id.ToString("D")),
                new(ClaimTypes.Name, student.UserName),
                new(
                    IdentityAuthenticationDefaults.SecurityStampClaim,
                    student.SecurityStamp),
                new(RolePolicies.AvailableRoleClaimType, RolePolicies.Student),
                new(ClaimTypes.Role, RolePolicies.Student),
                new(
                    IdentityAuthenticationDefaults.ActiveRoleClaim,
                    RolePolicies.Student)
            };
            claims.AddRange(RolePolicies.PermissionsForRole(RolePolicies.Student)
                .Select(permission => new Claim(
                    RolePolicies.PermissionClaimType,
                    permission)));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                IdentityAuthenticationDefaults.AuthenticationScheme));
            var ticket = new AuthenticationTicket(
                principal,
                new AuthenticationProperties
                {
                    AllowRefresh = false,
                    IsPersistent = false,
                    IssuedUtc = issuedAt,
                    ExpiresUtc = issuedAt.AddMinutes(60)
                },
                IdentityAuthenticationDefaults.AuthenticationScheme);
            var protectedTicket = cookieOptions.TicketDataFormat.Protect(ticket);
            sessions.Add(new Spec008AuthenticatedStudentSession(
                index + 1,
                protectedTicket));
        }

        return sessions;
    }

    private async Task VerifyCrossReplicaTicketAsync(
        CancellationToken cancellationToken)
    {
        await VerifyAuthenticatedContextAsync(
                FirstReplicaClient,
                StudentSessions[0],
                cancellationToken)
            .ConfigureAwait(false);
        await VerifyAuthenticatedContextAsync(
                SecondReplicaClient,
                StudentSessions[0],
                cancellationToken)
            .ConfigureAwait(false);

        CrossReplicaTicketVerified = true;
    }

    private static async Task VerifyAuthenticatedContextAsync(
        HttpClient client,
        Spec008AuthenticatedStudentSession session,
        CancellationToken cancellationToken)
    {
        using var request = session.CreateRequest(HttpMethod.Get, "/api/context");
        using var response = await client
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                "A protected student session was not accepted by an API replica.");
        }

        var context = await response.Content
            .ReadFromJsonAsync<AppContextDto>(cancellationToken)
            .ConfigureAwait(false);
        if (context is null ||
            context.ActiveRole != RolePolicies.Student ||
            !context.AuthorizedRoles.Contains(
                RolePolicies.Student,
                StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "An API replica returned an invalid authenticated student context.");
        }
    }

    private async Task<WebApplication> StartReplicaAsync(
        Uri address,
        string connectionString,
        CancellationToken cancellationToken)
    {
        var configuration = Configuration(
            connectionString,
            _certificatePath,
            _certificatePassword);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(StudentRegistration.Api.Composition.Program)
                .Assembly.FullName,
            EnvironmentName = TestingEnvironment,
            ContentRootPath = _artifactRoot
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.AddStudentRegistrationSqlServer(builder.Configuration);
        builder.Services.AddStudentRegistrationSecurity(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddStudentRegistrationIdentitySecurity(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddStudentRegistrationModules();
        builder.Services.AddProblemDetails();
        builder.Services.AddStudentRegistrationAcademicModule(builder.Configuration);
        builder.Services.AddStudentRegistrationRegistrationModule();

        var application = builder.Build();
        application.Urls.Add(address.ToString());
        application.UseSafeApiErrors();
        application.UseStudentRegistrationObservability();
        application.UseRouting();
        application.UseStudentRegistrationIdentitySecurity();
        application.MapSpec007Endpoints();
        application.MapSpec008Endpoints();
        application.MapSpec011Endpoints();
        application.MapSpec012Endpoints();
        application.MapSpec014Endpoints();
        application.MapSpec015Endpoints();
        await application.StartAsync(cancellationToken).ConfigureAwait(false);
        return application;
    }

    private static IConfigurationRoot Configuration(
        string connectionString,
        string? certificatePath = null,
        string? certificatePassword = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:StudentRegistration"] = connectionString,
                ["DataProtection:Repository"] = "SqlServer",
                ["DataProtection:Encryption"] = "ExternalCertificate",
                ["DataProtection:ApplicationName"] = "StudentRegistration.Spec008.Load",
                ["DataProtection:CertificatePath"] = certificatePath,
                ["DataProtection:CertificatePassword"] = certificatePassword
            })
            .Build();

    private async Task CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        var readCommittedSnapshotSql = _enableReadCommittedSnapshot
            ? $"ALTER DATABASE [{DatabaseName}] SET READ_COMMITTED_SNAPSHOT ON;"
            : string.Empty;
        var result = await _container.ExecScriptAsync($"""
            IF DB_ID(N'{DatabaseName}') IS NOT NULL
                THROW 51030, 'Per-run load database already exists.', 1;
            CREATE DATABASE [{DatabaseName}];
            ALTER DATABASE [{DatabaseName}] SET COMPATIBILITY_LEVEL = 160;
            {readCommittedSnapshotSql}
            """, cancellationToken).ConfigureAwait(false);
        if (result.ExitCode is not 0)
        {
            throw new InvalidOperationException(
                $"The per-run load database could not be created (exit {result.ExitCode?.ToString() ?? "unknown"}).");
        }
    }

    private async Task CreateCertificateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_artifactRoot);
        _certificatePath = Path.Combine(_artifactRoot, "data-protection.pfx");
        _certificatePassword = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(24));
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=StudentRegistration-SPEC008-Load",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(
            certificateAuthority: false,
            hasPathLengthConstraint: false,
            pathLengthConstraint: 0,
            critical: true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature,
            critical: true));
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30));
        await File.WriteAllBytesAsync(
                _certificatePath,
                certificate.Export(X509ContentType.Pfx, _certificatePassword),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task CleanupAsync()
    {
        if (_disposed)
        {
            return;
        }

        IsReady = false;
        _firstClient?.Dispose();
        _firstClient = null;
        _secondClient?.Dispose();
        _secondClient = null;
        await StopReplicaSafelyAsync(_secondReplica).ConfigureAwait(false);
        _secondReplica = null;
        await StopReplicaSafelyAsync(_firstReplica).ConfigureAwait(false);
        _firstReplica = null;

        try
        {
            if (_databaseCreated && _containerStarted)
            {
                try
                {
                    await _container.ExecScriptAsync($"""
                        IF DB_ID(N'{DatabaseName}') IS NOT NULL
                        BEGIN
                            ALTER DATABASE [{DatabaseName}]
                                SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{DatabaseName}];
                        END;
                        """, CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // Container disposal remains the final isolation boundary.
                }
            }

            _databaseCreated = false;
            try
            {
                await _container.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                DeleteArtifactRoot();
            }
        }
        finally
        {
            _containerStarted = false;
            StudentSessions = [];
            CrossReplicaTicketVerified = false;
            ConnectionString = null;
            _certificatePassword = null;
            _certificatePath = null;
            _disposed = true;
        }
    }

    private static async Task StopReplicaAsync(WebApplication? application)
    {
        if (application is null)
        {
            return;
        }

        try
        {
            await application.StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        finally
        {
            await application.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task StopReplicaSafelyAsync(WebApplication? application)
    {
        try
        {
            await StopReplicaAsync(application).ConfigureAwait(false);
        }
        catch
        {
            // Continue releasing SQL Server and temporary certificate state.
        }
    }

    private void DeleteArtifactRoot()
    {
        var expectedRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "StudentRegistration-SPEC008-load"));
        var resolved = Path.GetFullPath(_artifactRoot);
        if (!resolved.StartsWith(
                expectedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The load fixture artifact path escaped its temporary root.");
        }

        if (Directory.Exists(resolved))
        {
            Directory.Delete(resolved, recursive: true);
        }
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static Guid StableGuid(Guid userId, string discriminator) =>
        StableGuid($"{userId:N}|{discriminator}");

    private static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string NewSecurityStamp() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private sealed record BulkColumn(string Name, Type Type, bool Nullable);

    private sealed record SeedStudent(
        string Identity,
        Guid ApplicationUserId,
        Guid StudentId,
        decimal Gpa,
        decimal EarnedCredits,
        string SourceReference,
        DateTime DataAsOfUtc,
        DateTime ImportedAtUtc);

    private sealed record StaffFixture(
        Guid UserId,
        string UserName,
        string NormalizedUserName,
        string StaffNumber,
        string DisplayName,
        IReadOnlyList<string> Roles)
    {
        public static StaffFixture Create(
            string userName,
            string normalizedUserName,
            string staffNumber,
            string displayName,
            params string[] roles) =>
            new(
                StableGuid(normalizedUserName),
                userName,
                normalizedUserName,
                staffNumber,
                displayName,
                roles);
    }
}

/// <summary>
/// Opaque authenticated session handle held only in memory. Diagnostics never
/// reveal the cookie, user identifiers, or security stamp.
/// </summary>
public sealed class Spec008AuthenticatedStudentSession
{
    private readonly string _protectedCookie;

    internal Spec008AuthenticatedStudentSession(
        int ordinal,
        string protectedCookie)
    {
        Ordinal = ordinal;
        _protectedCookie = protectedCookie;
    }

    public int Ordinal { get; }

    public HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        var request = new HttpRequestMessage(method, requestUri);
        ApplyTo(request);
        return request;
    }

    public void ApplyTo(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.TryAddWithoutValidation(
            "Cookie",
            $"{IdentitySecurityRegistration.AuthenticationCookieName}={_protectedCookie}");
    }

    public override string ToString() =>
        $"StudentSession #{Ordinal}";
}
