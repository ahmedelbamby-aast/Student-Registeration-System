using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Endpoints;
using Testcontainers.MsSql;

namespace StudentRegistration.QualityTests.Specs.Spec015;

[CollectionDefinition(Name)]
public sealed class Spec015SqlEvidenceCollection
    : ICollectionFixture<Spec015SqlEvidenceFixture>
{
    public const string Name = "SPEC-015 SQL/API evidence";
}

[Collection(Spec015SqlEvidenceCollection.Name)]
public sealed class NFR_1EvidenceTests(Spec015SqlEvidenceFixture fixture)
{
    private const int WarmupRequests = 20;
    private const int MeasuredRequests = 120;

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Actual_bounded_receipt_api_over_representative_sql_is_at_most_300_ms_p95()
    {
        using var client = fixture.CreateClient(
            fixture.TargetApplicationUserId,
            RolePolicies.Student,
            RolePolicies.RegistrationRecordsReadOwn);
        var path = $"/api/student/registrations/{fixture.TargetSubmissionId:D}";

        for (var request = 0; request < WarmupRequests; request++)
        {
            using var warmup = await client.GetAsync(path);
            warmup.EnsureSuccessStatusCode();
        }

        var samples = new double[MeasuredRequests];
        for (var request = 0; request < samples.Length; request++)
        {
            var started = Stopwatch.GetTimestamp();
            using var response = await client.GetAsync(path);
            samples[request] = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            response.EnsureSuccessStatusCode();
            Assert.InRange(response.Content.Headers.ContentLength ?? 0, 1, 64 * 1024);
        }

        Array.Sort(samples);
        var p95 = PercentileFromSorted(samples, 0.95);
        var detail = await client.GetFromJsonAsync<RegistrationDetailDto>(path);

        Assert.Equal(Spec015SqlEvidenceFixture.RepresentativeAccountCount,
            await fixture.CountStudentsAsync());
        Assert.Equal(Spec015SqlEvidenceFixture.RepresentativeAccountCount +
            Spec015SqlEvidenceFixture.ArchivedTermCount,
            await fixture.CountSubmissionsAsync());
        Assert.Equal("accepted", detail!.Status);
        Assert.Equal(fixture.TargetSubmissionId, detail.Receipt!.SubmissionId);
        Assert.Single(detail.Receipt.Groups);
        Assert.InRange(p95, 0, 300);
        Console.WriteLine(FormattableString.Invariant(
            $"SPEC-015 NFR-1 warmup={WarmupRequests}; measured={MeasuredRequests}; sorted-p95-ms={p95:F3}"));
    }

    private static double PercentileFromSorted(
        IReadOnlyList<double> sortedSamples,
        double percentile)
    {
        Assert.NotEmpty(sortedSamples);
        Assert.Equal(
            sortedSamples.OrderBy(value => value).ToArray(),
            sortedSamples.ToArray());
        var rank = (int)Math.Ceiling(percentile * sortedSamples.Count);
        return sortedSamples[Math.Clamp(rank - 1, 0, sortedSamples.Count - 1)];
    }
}

public sealed class Spec015SqlEvidenceFixture : IAsyncLifetime
{
    public const int RepresentativeAccountCount = 25_000;
    public const int ArchivedTermCount = 8;
    private const string PreviousMigration = "20260713040000_DiscoveryPlanning";
    private const string SyntheticScheme = "SPEC015-Synthetic";
    private readonly MsSqlContainer _container = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56")
        .WithPassword($"Srs!1{Guid.NewGuid():N}a")
        .Build();
    private WebApplication? _application;

    public Guid TargetApplicationUserId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000001");
    public Guid TargetStudentId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000002");
    public Guid TargetSubmissionId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000003");
    public Guid OtherApplicationUserId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000011");
    public Guid OtherStudentId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000012");
    public Guid OtherSubmissionId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000013");
    public Guid PerformanceTermId { get; } =
        Guid.Parse("15000000-0000-0000-0000-000000000100");
    public IReadOnlyList<Guid> ArchivedTermIds { get; } = Enumerable.Range(1, ArchivedTermCount)
        .Select(value => Guid.Parse($"15000000-0000-0000-0000-{value + 200:D12}"))
        .ToArray();
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var created = await _container.ExecScriptAsync($"""
            CREATE DATABASE [{databaseName}];
            ALTER DATABASE [{databaseName}] SET COMPATIBILITY_LEVEL = 160;
            """);
        Assert.Equal(0L, created.ExitCode);
        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await ApplyPriorVersionAndSeedAccountsAsync();
        await ApplyRegistrationUpgradeAndSeedReceiptsAsync();
        await StartApiAsync();
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }
        await _container.DisposeAsync();
    }

    public HttpClient CreateClient(Guid subjectId, string role, string permission)
    {
        var client = (_application ?? throw new InvalidOperationException(
            "The SPEC-015 API fixture is not ready.")).GetTestClient();
        client.DefaultRequestHeaders.Add("X-Spec015-Subject", subjectId.ToString("D"));
        client.DefaultRequestHeaders.Add("X-Spec015-Role", role);
        if (!string.IsNullOrWhiteSpace(permission))
        {
            client.DefaultRequestHeaders.Add("X-Spec015-Permission", permission);
        }
        return client;
    }

    public async Task<int> CountStudentsAsync() => await ScalarAsync(
        "SELECT COUNT(*) FROM [academics].[Students];");

    public async Task<int> CountSubmissionsAsync() => await ScalarAsync(
        "SELECT COUNT(*) FROM [registration].[RegistrationSubmissions];");

    public async Task<int> CountInspectionAuditsAsync(
        Guid studentId,
        Guid termId,
        string endpoint) => await ScalarAsync(
        """
        SELECT COUNT(*)
        FROM [audit].[AuditEvents]
        WHERE [Action] = N'RegistrationRecordInspected'
          AND JSON_VALUE([AfterSummaryJson], '$.targetStudentId') = @studentId
          AND JSON_VALUE([AfterSummaryJson], '$.termId') = @termId
          AND JSON_VALUE([AfterSummaryJson], '$.endpoint') = @endpoint;
        """,
        new SqlParameter("@studentId", studentId.ToString("D")),
        new SqlParameter("@termId", termId.ToString("D")),
        new SqlParameter("@endpoint", endpoint));

    public async Task<IReadOnlyList<string>> AppliedMigrationsAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT [MigrationId] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];";
        await using var reader = await command.ExecuteReaderAsync();
        var migrations = new List<string>();
        while (await reader.ReadAsync()) migrations.Add(reader.GetString(0));
        return migrations;
    }

    public async Task RenameArchivedTermsAsync()
    {
        await ExecuteAsync(
            "UPDATE [academics].[AcademicTerms] SET [DisplayName] = CONCAT(N'MUTABLE-', [Code]) WHERE [State] = N'archived';");
    }

    private async Task ApplyPriorVersionAndSeedAccountsAsync()
    {
        await using (var context = CreateContext())
        {
            await context.Database.GetService<IMigrator>().MigrateAsync(PreviousMigration);
        }

        var termValues = string.Join(",\n", ArchivedTermIds.Select((id, index) =>
            $"('{id:D}', N'ARCH-{index + 1:D2}', N'Archived mutable term {index + 1:D2}', 'archived')"));
        var passwordHash = new PasswordHasher<string>().HashPassword(
            "SPEC-015 synthetic fixture",
            $"synthetic-{Guid.NewGuid():N}");
        await ExecuteAsync($"""
            INSERT INTO [academics].[AcademicTerms]
                ([Id],[Code],[CreationClientRequestId],[CreationPayloadHash],[DisplayName],
                 [TeachingStartsOn],[TeachingEndsOn],[TimeZoneId],[State])
            VALUES
                ('{PerformanceTermId:D}', N'PERF-TERM', NEWID(), N'SYNTHETIC-PERF',
                 N'Performance mutable term', '2040-09-01', '2041-01-15', N'Africa/Cairo', N'completed');

            INSERT INTO [academics].[AcademicTerms]
                ([Id],[Code],[CreationClientRequestId],[CreationPayloadHash],[DisplayName],
                 [TeachingStartsOn],[TeachingEndsOn],[TimeZoneId],[State])
            SELECT CAST([Id] AS uniqueidentifier), [Code], NEWID(), CONCAT(N'SYNTHETIC-', [Code]),
                   [DisplayName], '2030-09-01', '2031-01-15', N'Africa/Cairo', [State]
            FROM (VALUES
                {termValues}
            ) AS archived([Id],[Code],[DisplayName],[State]);

            CREATE TABLE #Accounts
            (
                [Ordinal] int NOT NULL,
                [ApplicationUserId] uniqueidentifier NOT NULL,
                [StudentId] uniqueidentifier NOT NULL
            );
            WITH numbers AS
            (
                SELECT TOP ({RepresentativeAccountCount})
                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS [Ordinal]
                FROM sys.all_objects AS firstSet
                CROSS JOIN sys.all_objects AS secondSet
            )
            INSERT INTO #Accounts ([Ordinal],[ApplicationUserId],[StudentId])
            SELECT [Ordinal],
                CASE [Ordinal]
                    WHEN 1 THEN '{TargetApplicationUserId:D}'
                    WHEN 2 THEN '{OtherApplicationUserId:D}'
                    ELSE NEWID()
                END,
                CASE [Ordinal]
                    WHEN 1 THEN '{TargetStudentId:D}'
                    WHEN 2 THEN '{OtherStudentId:D}'
                    ELSE NEWID()
                END
            FROM numbers;

            INSERT INTO [auth].[ApplicationUsers]
                ([Id],[UserName],[NormalizedUserName],[UniversityId],[PasswordHash],
                 [SecurityStamp],[IsEnabled],[AccessFailedCount],[LockoutEndUtc])
            SELECT [ApplicationUserId], CONCAT(N'synthetic-', [Ordinal]),
                   CONCAT(N'SYNTHETIC-', [Ordinal]), NULL,
                   @passwordHash, CONVERT(nvarchar(36), NEWID()),
                   1, 0, NULL
            FROM #Accounts;

            INSERT INTO [academics].[Students]
                ([Id],[ApplicationUserId],[ProgramCode],[Cohort],[CurrentGpa],
                 [EarnedCredits],[Standing],[IsActive],[Source],[SourceReference],
                 [DataVersion],[DataAsOfUtc],[ImportedAtUtc])
            SELECT [StudentId],[ApplicationUserId],N'AI',N'2040',3.00,60.00,
                   N'active',1,N'synthetic-quality',N'SPEC-015-NFR',N'1',
                   '2040-07-01T00:00:00Z','2040-07-01T00:00:00Z'
            FROM #Accounts;
            """,
            new SqlParameter("@passwordHash", passwordHash));
    }

    private async Task ApplyRegistrationUpgradeAndSeedReceiptsAsync()
    {
        await using (var context = CreateContext())
        {
            await context.Database.MigrateAsync();
        }

        var performanceSnapshot = ReceiptSnapshot(
            PerformanceTermId,
            "PERF-TERM",
            "Original performance term",
            "CS-PERF",
            "Original performance room");
        await ExecuteAsync(
            $"""
            WITH students AS
            (
                SELECT [Id], [ApplicationUserId],
                       ROW_NUMBER() OVER (ORDER BY [Id]) AS [Ordinal]
                FROM [academics].[Students]
            )
            INSERT INTO [registration].[RegistrationSubmissions]
                ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],
                 [ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],
                 [DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
            SELECT
                CASE [ApplicationUserId]
                    WHEN '{TargetApplicationUserId:D}' THEN '{TargetSubmissionId:D}'
                    WHEN '{OtherApplicationUserId:D}' THEN '{OtherSubmissionId:D}'
                    ELSE NEWID()
                END,
                [Id], '{PerformanceTermId:D}', NEWID(), N'SYNTHETIC-PAYLOAD',
                N'accepted', N'REGISTERED',
                CASE [ApplicationUserId]
                    WHEN '{TargetApplicationUserId:D}' THEN N'REG-TARGET'
                    WHEN '{OtherApplicationUserId:D}' THEN N'REG-OTHER'
                    ELSE CONCAT(N'REG-', RIGHT(REPLICATE(N'0', 8) + CONVERT(nvarchar(12), [Ordinal]), 8))
                END,
                @receipt, @decision,
                DATEADD(second, [Ordinal] % 3600, CAST('2040-07-17T08:00:00' AS datetime2)),
                DATEADD(second, ([Ordinal] % 3600) + 1, CAST('2040-07-17T08:00:00' AS datetime2)),
                DATEADD(second, ([Ordinal] % 3600) + 1, CAST('2040-07-17T08:00:00' AS datetime2))
            FROM students;
            """,
            new SqlParameter("@receipt", performanceSnapshot),
            new SqlParameter("@decision", "{\"policyVersion\":\"DEMO-POC-2026.1\"}"));

        for (var index = 0; index < ArchivedTermIds.Count; index++)
        {
            var termId = ArchivedTermIds[index];
            var submissionId = Guid.Parse($"15000000-0000-0000-0000-{index + 301:D12}");
            var snapshot = ReceiptSnapshot(
                termId,
                $"ARCH-{index + 1:D2}",
                $"Original archived term {index + 1:D2}",
                $"CS-{index + 1:D3}",
                $"Original archived room {index + 1:D2}");
            await ExecuteAsync(
                """
                INSERT INTO [registration].[RegistrationSubmissions]
                    ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],
                     [ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],
                     [DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
                VALUES
                    (@submissionId,@studentId,@termId,NEWID(),N'SYNTHETIC-ARCHIVE',
                     N'accepted',N'REGISTERED',@reference,@receipt,
                     N'{"policyVersion":"DEMO-POC-2026.1"}',
                     @receivedAt,@completedAt,@completedAt);
                """,
                new SqlParameter("@submissionId", submissionId),
                new SqlParameter("@studentId", TargetStudentId),
                new SqlParameter("@termId", termId),
                new SqlParameter("@reference", $"REG-ARCH-{index + 1:D2}"),
                new SqlParameter("@receipt", snapshot),
                new SqlParameter("@receivedAt", new DateTime(2030 + index, 7, 17, 8, 0, 0, DateTimeKind.Utc)),
                new SqlParameter("@completedAt", new DateTime(2030 + index, 7, 17, 8, 0, 1, DateTimeKind.Utc)));
        }
    }

    private async Task StartApiAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDbContext<StudentRegistrationDbContext>(options =>
            options.UseSqlServer(ConnectionString));
        builder.Services.AddScoped<IRegistrationRecordReader, SqlRegistrationRecordReader>();
        builder.Services.AddStudentRegistrationRegistrationModule();
        builder.Services.AddAuthentication(SyntheticScheme)
            .AddScheme<AuthenticationSchemeOptions, Spec015SyntheticAuthenticationHandler>(
                SyntheticScheme,
                _ => { });
        builder.Services.AddIdentityAuthorization();

        _application = builder.Build();
        _application.UseRouting();
        _application.UseAuthentication();
        _application.UseAuthorization();
        _application.MapSpec015Endpoints();
        await _application.StartAsync();
    }

    private StudentRegistrationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

    private async Task<int> ScalarAsync(string sql, params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task ExecuteAsync(string sql, params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();
    }

    private static string ReceiptSnapshot(
        Guid termId,
        string termCode,
        string termName,
        string courseCode,
        string location) => JsonSerializer.Serialize(new
        {
            term = new
            {
                id = termId,
                code = termCode,
                displayName = termName,
                timeZoneId = "Africa/Cairo"
            },
            groups = new[]
            {
                new
                {
                    offeringId = Guid.Parse("15000000-0000-0000-0000-000000000401"),
                    courseCode,
                    subjectTitle = "Synthetic registration evidence",
                    groupId = Guid.Parse("15000000-0000-0000-0000-000000000402"),
                    groupCode = "L1",
                    credits = 3m,
                    meetings = new[]
                    {
                        new
                        {
                            meetingId = Guid.Parse("15000000-0000-0000-0000-000000000403"),
                            activityType = "Lecture",
                            dayOfWeek = 1,
                            startLocal = "09:00",
                            endLocal = "10:30",
                            roomCode = "R-015",
                            location,
                            staff = new[]
                            {
                                new { role = "Lecturer", displayName = "Synthetic lecturer" }
                            }
                        }
                    }
                }
            },
            totalCredits = 3m,
            policySetId = Guid.Parse("15000000-0000-0000-0000-000000000404"),
            policyVersion = "DEMO-POC-2026.1",
            submittedAtUtc = new DateTime(2040, 7, 17, 8, 0, 0, DateTimeKind.Utc)
        });
}

internal sealed class Spec015SyntheticAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Guid.TryParse(Request.Headers["X-Spec015-Subject"], out var subjectId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        var role = Request.Headers["X-Spec015-Role"].ToString();
        var permission = Request.Headers["X-Spec015-Permission"].ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subjectId.ToString("D")),
            new(ClaimTypes.Name, "synthetic-spec015-caller"),
            new(ClaimTypes.Role, role)
        };
        if (!string.IsNullOrWhiteSpace(permission))
        {
            claims.Add(new Claim(RolePolicies.PermissionClaimType, permission));
        }
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            Scheme.Name,
            ClaimTypes.Name,
            ClaimTypes.Role));
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, Scheme.Name)));
    }
}
