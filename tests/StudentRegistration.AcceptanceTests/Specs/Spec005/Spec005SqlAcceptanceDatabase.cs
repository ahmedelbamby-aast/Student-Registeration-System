using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using Testcontainers.MsSql;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class Spec005SqlAcceptanceCollection :
    ICollectionFixture<Spec005SqlAcceptanceDatabase>
{
    public const string Name = "SPEC-005 acceptance SQL";
}

public sealed class Spec005SqlAcceptanceDatabase : IAsyncLifetime
{
    private const string SqlImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";
    private readonly MsSqlContainer _container = new MsSqlBuilder(SqlImage)
        .WithPassword($"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a")
        .WithEnvironment("MSSQL_PID", "Developer")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        await using (var connection = new SqlConnection(_container.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{databaseName}]; ALTER DATABASE [{databaseName}] SET COMPATIBILITY_LEVEL = 160;";
            await command.ExecuteNonQueryAsync();
        }

        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    public async Task<Spec005SqlGraph> SeedAsync(
        int studentCount,
        int activeEnrollmentCount,
        int capacity)
    {
        Assert.InRange(studentCount, 1, 100);
        Assert.InRange(activeEnrollmentCount, 0, studentCount);
        Assert.True(capacity >= activeEnrollmentCount);
        var token = Guid.NewGuid().ToString("N");
        var termId = Guid.NewGuid();
        var secondTermId = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        var catalogueVersionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var offeringId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var studentIds = Enumerable.Range(0, studentCount).Select(_ => Guid.NewGuid()).ToArray();
        var submissionIds = Enumerable.Range(0, studentCount).Select(_ => Guid.NewGuid()).ToArray();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        async Task ExecuteAsync(string sql, params SqlParameter[] parameters)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            await command.ExecuteNonQueryAsync();
        }

        await ExecuteAsync(
            """
            INSERT [academics].[AcademicTerms]
              ([Id],[Code],[CreationClientRequestId],[CreationPayloadHash],[DisplayName],[TeachingStartsOn],[TeachingEndsOn],[TimeZoneId],[State])
            VALUES
              (@term,@code,@request,@hash,@display,'2026-09-01','2026-12-31','Africa/Cairo','draft'),
              (@term2,@code2,@request2,@hash,@display2,'2027-02-01','2027-06-01','Africa/Cairo','draft');
            """,
            new("@term", termId), new("@term2", secondTermId),
            new("@code", $"T-{token}"), new("@code2", $"T2-{token}"),
            new("@request", Guid.NewGuid()), new("@request2", Guid.NewGuid()),
            new("@hash", token), new("@display", $"Term {token}"),
            new("@display2", $"Term 2 {token}"));
        await ExecuteAsync(
            """
            INSERT [academics].[CatalogueDrafts]
              ([Id],[ScopeCode],[BasedOnVersionId],[CanonicalContentHash],[ContentJson],[ValidationSummaryJson],[State])
            VALUES (@id,@scope,NULL,@hash,'{}','{}','published');
            INSERT [academics].[CatalogueVersions]
              ([Id],[SourceDraftId],[SupersedesId],[ScopeCode],[VersionCode],[SourceReference],[EffectiveFromUtc],[PublishedAtUtc],[PublishedBy],[State])
            VALUES (@version,@id,NULL,@scope,@versionCode,'SPEC-005','2026-07-19','2026-07-19','Ahmed ELbamby','published');
            INSERT [academics].[Courses]
              ([Id],[CatalogueVersionId],[Code],[Title],[Credits],[IsActive],[ProvenanceSourceReference],[ProvenanceAccessedOn],[ProvenanceSourceKind],[ProvenanceSyntheticFieldsJson])
            VALUES (@course,@version,@courseCode,'SPEC-005 SQL acceptance',3,1,'SPEC-005','2026-07-19','synthetic-demo','[]');
            INSERT [scheduling].[CourseOfferings] ([Id],[TermId],[CourseId],[State])
            VALUES (@offering,@term,@course,'published');
            INSERT [scheduling].[SectionGroups]
              ([Id],[OfferingId],[GroupCode],[Capacity],[EnrolledCount],[State],[RegistrationPaused])
            VALUES (@group,@offering,@groupCode,@capacity,@enrolled,'published',0);
            """,
            new("@id", draftId), new("@scope", $"S-{token}"), new("@hash", token),
            new("@version", catalogueVersionId), new("@versionCode", $"V-{token}"),
            new("@course", courseId), new("@courseCode", $"C-{token[..10]}"),
            new("@offering", offeringId), new("@term", termId),
            new("@group", groupId), new("@groupCode", $"G-{token[..8]}"),
            new("@capacity", capacity), new("@enrolled", activeEnrollmentCount));

        for (var index = 0; index < studentCount; index++)
        {
            var userId = Guid.NewGuid();
            await ExecuteAsync(
                """
                INSERT [auth].[ApplicationUsers]
                  ([Id],[UserName],[NormalizedUserName],[UniversityId],[PasswordHash],[SecurityStamp],[IsEnabled],[AccessFailedCount],[LockoutEndUtc])
                VALUES (@user,@name,@normalized,@university,'AQAAAAIAAYagAAAAISpec005HashOnly','spec005',1,0,NULL);
                INSERT [academics].[Students]
                  ([Id],[ApplicationUserId],[ProgramCode],[Cohort],[CurrentGpa],[EarnedCredits],[Standing],[IsActive],[Source],[SourceReference],[DataVersion],[DataAsOfUtc],[ImportedAtUtc])
                VALUES (@student,@user,'AI-DS','2026',3.0,60,'good',1,'synthetic','SPEC-005','fixture/1','2026-07-19','2026-07-19');
                INSERT [registration].[RegistrationSubmissions]
                  ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],[ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],[DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
                VALUES (@submission,@student,@term,@request,'payload',@submissionState,@resultCode,@reference,@receipt,'{"policyVersion":"2026.1"}','2026-07-19','2026-07-19','2026-07-19');
                """,
                new("@user", userId), new("@name", $"student-{token}-{index}"),
                new("@normalized", $"STUDENT-{token}-{index}"),
                new("@university", $"S{token[..8]}{index:D3}"),
                new("@student", studentIds[index]), new("@submission", submissionIds[index]),
                new("@term", termId), new("@request", Guid.NewGuid()),
                new("@submissionState", index < activeEnrollmentCount ? "accepted" : "rejected"),
                new("@resultCode", index < activeEnrollmentCount ? "REGISTERED" : "GROUP_FULL"),
                new("@reference", index < activeEnrollmentCount
                    ? $"REG-{token[..8]}-{index:D3}"
                    : DBNull.Value),
                new("@receipt", index < activeEnrollmentCount ? "{}" : DBNull.Value));
            if (index < activeEnrollmentCount)
            {
                await ExecuteAsync(
                    """
                    INSERT [registration].[Enrollments]
                      ([Id],[StudentId],[OfferingId],[GroupId],[SubmissionId],[State],[RegisteredAtUtc])
                    VALUES (@id,@student,@offering,@group,@submission,'active','2026-07-19');
                    """,
                    new("@id", Guid.NewGuid()), new("@student", studentIds[index]),
                    new("@offering", offeringId), new("@group", groupId),
                    new("@submission", submissionIds[index]));
            }
        }

        await transaction.CommitAsync();
        return new(
            termId,
            secondTermId,
            studentIds,
            submissionIds,
            offeringId,
            groupId,
            catalogueVersionId);
    }
}

public sealed record Spec005SqlGraph(
    Guid TermId,
    Guid SecondTermId,
    Guid[] StudentIds,
    Guid[] SubmissionIds,
    Guid OfferingId,
    Guid GroupId,
    Guid CatalogueVersionId);
