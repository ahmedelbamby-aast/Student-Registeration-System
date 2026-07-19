using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;
using Testcontainers.MsSql;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class Spec005QueryPlanRehearsalTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-005-NFR-1-plans.json";
    private const string SqlImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Production_like_fixture_captures_reviewable_actual_plans()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SPEC005_RUN_QUERY_PLAN_REHEARSAL"),
                "1",
                StringComparison.Ordinal))
        {
            Assert.True(RepositoryFiles.Exists(EvidencePath));
            return;
        }

        var suffix = Guid.NewGuid().ToString("N");
        var password = $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        await using var container = new MsSqlBuilder(SqlImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await container.StartAsync(timeout.Token);
        var databaseName = $"StudentRegistration_Test_{suffix}";
        var master = container.GetConnectionString();
        await ExecuteAsync(
            master,
            $"CREATE DATABASE [{databaseName}]; ALTER DATABASE [{databaseName}] SET COMPATIBILITY_LEVEL = 160;",
            timeout.Token);
        var connectionString = new SqlConnectionStringBuilder(master)
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using (var context = new StudentRegistrationDbContext(options))
        {
            await context.Database.MigrateAsync(timeout.Token);
        }

        await SeedAsync(connectionString, timeout.Token);
        var definitions = Definitions();
        var plans = new List<QueryPlanEvidence>();
        foreach (var definition in definitions)
        {
            plans.Add(await MeasureAsync(connectionString, definition, timeout.Token));
        }

        var evidence = new QueryPlanEvidenceRoot(
            "spec005-query-plans/1.0",
            "SPEC-005",
            "SQL Server 2022 Developer",
            160,
            "synthetic-only",
            DateTimeOffset.UtcNow,
            new(25_000, 25_000, 8_000, 25_000, 75_000, 100_000),
            plans,
            plans.All(plan =>
                plan.ActualPlanCaptured &&
                plan.P95Milliseconds <= 300 &&
                (plan.ScanOperators.Count == 0 ||
                 plan.ExceptionId == "SPEC005-SCAN-20260719"))
                ? "pass"
                : "fail",
            "Ahmed ELbamby",
            false);
        await File.WriteAllTextAsync(
            Path.Combine(RepositoryFiles.Root, EvidencePath),
            JsonSerializer.Serialize(
                evidence,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    WriteIndented = true
                }),
            timeout.Token);

        Assert.Equal("pass", evidence.Status);
    }

    private static IReadOnlyList<QueryDefinition> Definitions() =>
    [
        new(
            "CQ-01",
            "SELECT s.[Id],s.[ProgramCode],s.[Standing] FROM [auth].[ApplicationUsers] u INNER JOIN [academics].[Students] s ON s.[ApplicationUserId]=u.[Id] WHERE u.[NormalizedUserName]=@value;",
            "@value",
            "SPEC005-STUDENT-000100",
            null),
        new(
            "CQ-02",
            "SELECT TOP (100) o.[Id],g.[Id],g.[GroupCode],g.[Capacity],g.[EnrolledCount] FROM [scheduling].[CourseOfferings] o INNER JOIN [scheduling].[SectionGroups] g ON g.[OfferingId]=o.[Id] WHERE o.[TermId]=@value ORDER BY o.[Id],g.[Id];",
            "@value",
            Guid.Parse("50000000-0000-0000-0000-000000000001"),
            null),
        new(
            "CQ-03",
            "SELECT p.[Id],p.[State],p.[TotalCredits],p.[Version] FROM [registration].[RegistrationPlans] p WHERE p.[StudentId]=@value AND p.[TermId]='50000000-0000-0000-0000-000000000001';",
            "@value",
            Id("student", 100),
            null),
        new(
            "CQ-04",
            "SELECT r.[ProcessingState],r.[ResultCode],r.[Reference],r.[ReceiptSnapshotJson],r.[DecisionSnapshotJson] FROM [registration].[RegistrationSubmissions] r WHERE r.[StudentId]=@value AND r.[TermId]='50000000-0000-0000-0000-000000000001' AND r.[ClientRequestId]=@request;",
            "@value",
            Id("student", 100),
            null,
            new SqlParameter("@request", Id("request", 100))),
        new(
            "CQ-05",
            "SELECT e.[StudentId],e.[RegisteredAtUtc] FROM [registration].[Enrollments] e WHERE e.[OfferingId]=@value AND e.[GroupId]=@group AND e.[State]='active' ORDER BY e.[StudentId] OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY;",
            "@value",
            Guid.Parse("50000000-0000-0000-0000-000000000101"),
            "SPEC005-SCAN-20260719",
            new SqlParameter("@group", Guid.Parse("50000000-0000-0000-0000-000000000201"))),
        new(
            "CQ-06",
            "SELECT TOP (100) a.[Id],a.[Action],a.[EntityType],a.[EntityId],a.[OccurredAtUtc] FROM [audit].[AuditEvents] a WHERE a.[OccurredAtUtc]>=@value ORDER BY a.[OccurredAtUtc],a.[Id];",
            "@value",
            new DateTime(2026, 7, 18, 23, 58, 0, DateTimeKind.Utc),
            null),
        new(
            "CQ-07",
            "SELECT COUNT_BIG(*) FROM [registration].[Enrollments] e WHERE e.[OfferingId]=@value AND e.[GroupId]=@group AND e.[State]='active';",
            "@value",
            Guid.Parse("50000000-0000-0000-0000-000000000101"),
            null,
            new SqlParameter("@group", Guid.Parse("50000000-0000-0000-0000-000000000201")))
    ];

    private static async Task<QueryPlanEvidence> MeasureAsync(
        string connectionString,
        QueryDefinition definition,
        CancellationToken cancellationToken)
    {
        var samples = new List<double>();
        for (var iteration = 0; iteration < 15; iteration++)
        {
            var stopwatch = Stopwatch.StartNew();
            await ExecuteQueryAsync(connectionString, definition, false, cancellationToken);
            stopwatch.Stop();
            if (iteration >= 3)
            {
                samples.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        var planXml = await ExecuteQueryAsync(
            connectionString,
            definition,
            true,
            cancellationToken)
            ?? throw new InvalidOperationException($"{definition.QueryId} returned no actual ShowPlan XML.");
        var document = XDocument.Parse(planXml);
        var relational = document.Descendants()
            .Where(element => element.Name.LocalName == "RelOp")
            .ToArray();
        var operators = relational
            .Select(element => element.Attribute("PhysicalOp")?.Value)
            .OfType<string>()
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var indexes = document.Descendants()
            .Where(element => element.Name.LocalName == "Object")
            .Select(element => element.Attribute("Index")?.Value ?? element.Attribute("Table")?.Value)
            .OfType<string>()
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var scans = relational
            .Select(element => element.Attribute("PhysicalOp")?.Value)
            .OfType<string>()
            .Where(value => value is "Table Scan" or "Index Scan")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var ordered = samples.Order().ToArray();
        var p95 = ordered[(int)Math.Ceiling(ordered.Length * 0.95) - 1];

        return new(
            definition.QueryId,
            true,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(planXml))),
            Math.Round(p95, 3),
            operators,
            indexes,
            scans,
            definition.ExceptionId,
            definition.ExceptionId is null ? null : "Ahmed ELbamby",
            definition.ExceptionId is null ? null : "2026-08-02");
    }

    private static async Task<string?> ExecuteQueryAsync(
        string connectionString,
        QueryDefinition definition,
        bool capturePlan,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 300;
        command.CommandText = capturePlan
            ? $"SET STATISTICS XML ON; {definition.Sql} SET STATISTICS XML OFF;"
            : definition.Sql;
        command.Parameters.AddWithValue(definition.ParameterName, definition.ParameterValue);
        foreach (var parameter in definition.AdditionalParameters)
        {
            command.Parameters.Add(new SqlParameter(parameter.ParameterName, parameter.Value));
        }

        string? plan = null;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        do
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.FieldCount == 1 && reader.GetValue(0) is string value &&
                    value.Contains("ShowPlanXML", StringComparison.Ordinal))
                {
                    plan = value;
                }
            }
        }
        while (await reader.NextResultAsync(cancellationToken));
        return plan;
    }

    private static async Task SeedAsync(
        string connectionString,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(
            connectionString,
            """
            SET NOCOUNT ON;
            CREATE TABLE #N ([N] int NOT NULL PRIMARY KEY);
            INSERT #N ([N])
            SELECT TOP (25000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
            FROM sys.all_objects a CROSS JOIN sys.all_objects b;

            INSERT [academics].[AcademicTerms]
              ([Id],[Code],[CreationClientRequestId],[CreationPayloadHash],[DisplayName],[TeachingStartsOn],[TeachingEndsOn],[TimeZoneId],[State])
            VALUES
              ('50000000-0000-0000-0000-000000000001','SPEC005-TERM','50000000-0000-0000-0000-000000000002','spec005-term','SPEC-005 Plan Fixture','2026-09-01','2026-12-31','Africa/Cairo','registrationOpen');

            INSERT [auth].[ApplicationUsers]
              ([Id],[UserName],[NormalizedUserName],[UniversityId],[PasswordHash],[SecurityStamp],[IsEnabled],[AccessFailedCount],[LockoutEndUtc])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-user-',N))),
                   CONCAT('spec005-student-',RIGHT(CONCAT('000000',N),6)),
                   CONCAT('SPEC005-STUDENT-',RIGHT(CONCAT('000000',N),6)),
                   CONCAT('S5',RIGHT(CONCAT('0000000',N),7)),
                   'AQAAAAIAAYagAAAAISpec005SyntheticHashOnly','spec005',1,0,NULL
            FROM #N;

            INSERT [academics].[Students]
              ([Id],[ApplicationUserId],[ProgramCode],[Cohort],[CurrentGpa],[EarnedCredits],[Standing],[IsActive],[Source],[SourceReference],[DataVersion],[DataAsOfUtc],[ImportedAtUtc])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-student-',N))),
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-user-',N))),
                   'AI-DS','2026',3.0,60,'good',1,'synthetic','SPEC-005-NFR-1','fixture/1','2026-07-19','2026-07-19'
            FROM #N;

            INSERT [academics].[CatalogueDrafts]
              ([Id],[ScopeCode],[BasedOnVersionId],[CanonicalContentHash],[ContentJson],[ValidationSummaryJson],[State])
            VALUES ('50000000-0000-0000-0000-000000000010','SPEC005',NULL,'spec005','{}','{}','published');
            INSERT [academics].[CatalogueVersions]
              ([Id],[SourceDraftId],[SupersedesId],[ScopeCode],[VersionCode],[SourceReference],[EffectiveFromUtc],[PublishedAtUtc],[PublishedBy],[State])
            VALUES ('50000000-0000-0000-0000-000000000011','50000000-0000-0000-0000-000000000010',NULL,'SPEC005','fixture/1','SPEC-005-NFR-1','2026-07-19','2026-07-19','Ahmed ELbamby','published');
            INSERT [academics].[Courses]
              ([Id],[CatalogueVersionId],[Code],[Title],[Credits],[IsActive],[ProvenanceSourceReference],[ProvenanceAccessedOn],[ProvenanceSourceKind],[ProvenanceSyntheticFieldsJson])
            VALUES
              ('50000000-0000-0000-0000-000000000021','50000000-0000-0000-0000-000000000011','S5-1','Fixture 1',3,1,'SPEC-005-NFR-1','2026-07-19','synthetic-demo','[]'),
              ('50000000-0000-0000-0000-000000000022','50000000-0000-0000-0000-000000000011','S5-2','Fixture 2',3,1,'SPEC-005-NFR-1','2026-07-19','synthetic-demo','[]'),
              ('50000000-0000-0000-0000-000000000023','50000000-0000-0000-0000-000000000011','S5-3','Fixture 3',3,1,'SPEC-005-NFR-1','2026-07-19','synthetic-demo','[]');
            INSERT [scheduling].[CourseOfferings] ([Id],[TermId],[CourseId],[State]) VALUES
              ('50000000-0000-0000-0000-000000000101','50000000-0000-0000-0000-000000000001','50000000-0000-0000-0000-000000000021','published'),
              ('50000000-0000-0000-0000-000000000102','50000000-0000-0000-0000-000000000001','50000000-0000-0000-0000-000000000022','published'),
              ('50000000-0000-0000-0000-000000000103','50000000-0000-0000-0000-000000000001','50000000-0000-0000-0000-000000000023','published');
            INSERT [scheduling].[SectionGroups]
              ([Id],[OfferingId],[GroupCode],[Capacity],[EnrolledCount],[State],[RegistrationPaused]) VALUES
              ('50000000-0000-0000-0000-000000000201','50000000-0000-0000-0000-000000000101','G1',25000,25000,'published',0),
              ('50000000-0000-0000-0000-000000000202','50000000-0000-0000-0000-000000000102','G2',25000,25000,'published',0),
              ('50000000-0000-0000-0000-000000000203','50000000-0000-0000-0000-000000000103','G3',25000,25000,'published',0);

            INSERT [registration].[RegistrationPlans]
              ([Id],[StudentId],[TermId],[TotalCredits],[State],[ConflictsJson],[ValidationSnapshotJson])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-plan-',N))),
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-student-',N))),
                   '50000000-0000-0000-0000-000000000001',0,'draft','[]',NULL
            FROM #N WHERE N<=8000;

            INSERT [registration].[RegistrationSubmissions]
              ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],[ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],[DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-submission-',N))),
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-student-',N))),
                   '50000000-0000-0000-0000-000000000001',
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-request-',N))),
                   CONCAT('payload-',N),'accepted','REGISTERED',CONCAT('SPEC005-',RIGHT(CONCAT('000000',N),6)),'{}','{}','2026-07-18','2026-07-18','2026-07-18'
            FROM #N;

            INSERT [registration].[Enrollments]
              ([Id],[StudentId],[OfferingId],[GroupId],[SubmissionId],[State],[RegisteredAtUtc])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-enrollment-',N,'-',o.Ordinal))),
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-student-',N))),
                   o.OfferingId,o.GroupId,
                   CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-submission-',N))),
                   'active','2026-07-18'
            FROM #N CROSS JOIN (VALUES
              (1,CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000101'),CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000201')),
              (2,CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000102'),CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000202')),
              (3,CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000103'),CONVERT(uniqueidentifier,'50000000-0000-0000-0000-000000000203'))
            ) o(Ordinal,OfferingId,GroupId);

            CREATE TABLE #M ([N] int NOT NULL PRIMARY KEY);
            INSERT #M ([N])
            SELECT TOP (100000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
            FROM sys.all_objects a CROSS JOIN sys.all_objects b;
            INSERT [audit].[AuditEvents]
              ([Id],[ActorReference],[SubjectReference],[Action],[EntityType],[EntityId],[Reason],[BeforeSummaryJson],[AfterSummaryJson],[CorrelationId],[OccurredAtUtc])
            SELECT CONVERT(uniqueidentifier,HASHBYTES('MD5',CONCAT('spec005-audit-',N))),
                   'spec005-worker','group:fixture','RegistrationObserved','SectionGroup',CONVERT(varchar(20),N),'POC_MEASUREMENT',NULL,NULL,CONCAT('spec005-',N),DATEADD(second,-N,'2026-07-19')
            FROM #M;

            UPDATE STATISTICS [auth].[ApplicationUsers] WITH FULLSCAN;
            UPDATE STATISTICS [academics].[Students] WITH FULLSCAN;
            UPDATE STATISTICS [registration].[RegistrationPlans] WITH FULLSCAN;
            UPDATE STATISTICS [registration].[RegistrationSubmissions] WITH FULLSCAN;
            UPDATE STATISTICS [registration].[Enrollments] WITH FULLSCAN;
            UPDATE STATISTICS [audit].[AuditEvents] WITH FULLSCAN;
            """,
            cancellationToken);

    private static Guid Id(string scope, int ordinal) =>
        new(MD5.HashData(Encoding.UTF8.GetBytes($"spec005-{scope}-{ordinal}")));

    private static async Task ExecuteAsync(
        string connectionString,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 600;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record QueryDefinition(
        string QueryId,
        string Sql,
        string ParameterName,
        object ParameterValue,
        string? ExceptionId,
        params SqlParameter[] AdditionalParameters);

    private sealed record QueryPlanEvidenceRoot(
        string SchemaVersion,
        string OwnerSpec,
        string SqlEdition,
        int CompatibilityLevel,
        string DataClassification,
        DateTimeOffset RecordedAtUtc,
        QueryPlanRowCounts RowCounts,
        IReadOnlyList<QueryPlanEvidence> Plans,
        string Status,
        string ApprovedBy,
        bool ProductionAuthorized);

    private sealed record QueryPlanRowCounts(
        int ApplicationUsers,
        int Students,
        int RegistrationPlans,
        int RegistrationSubmissions,
        int Enrollments,
        int AuditEvents);

    private sealed record QueryPlanEvidence(
        string QueryId,
        bool ActualPlanCaptured,
        string ActualPlanSha256,
        double P95Milliseconds,
        IReadOnlyList<string> PhysicalOperators,
        IReadOnlyList<string> IndexesUsed,
        IReadOnlyList<string> ScanOperators,
        string? ExceptionId,
        string? ExceptionOwner,
        string? ExceptionExpiresOn);
}
