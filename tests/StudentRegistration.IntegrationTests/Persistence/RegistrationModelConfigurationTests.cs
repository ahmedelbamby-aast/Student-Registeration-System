using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class RegistrationModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec014RegistrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Submission_mapping_owns_scope_result_snapshot_and_state_invariants()
    {
        using var context = CreateContext();
        var submission = Entity<RegistrationSubmission>(context);

        Assert.Equal("RegistrationSubmissions", submission.GetTableName());
        Assert.Equal("registration", submission.GetSchema());
        AssertUnique(
            submission,
            nameof(RegistrationSubmission.StudentId),
            nameof(RegistrationSubmission.TermId),
            nameof(RegistrationSubmission.ClientRequestId));

        var referenceIndex = Assert.Single(submission.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(RegistrationSubmission.Reference)]));
        Assert.Equal("[Reference] IS NOT NULL", referenceIndex.GetFilter());

        AssertColumn(
            submission,
            nameof(RegistrationSubmission.ReceiptSnapshotJson),
            "nvarchar(max)");
        AssertColumn(
            submission,
            nameof(RegistrationSubmission.DecisionSnapshotJson),
            "nvarchar(max)");
        Assert.Equal(
            typeof(string),
            submission.FindProperty(nameof(RegistrationSubmission.ProcessingState))!
                .GetTypeMapping().Converter?.ProviderClrType);

        AssertForeignKey<RegistrationSubmission, Student>(
            submission,
            nameof(RegistrationSubmission.StudentId));
        AssertForeignKey<RegistrationSubmission, AcademicTerm>(
            submission,
            nameof(RegistrationSubmission.TermId));

        var script = context.Database.GenerateCreateScript();
        Assert.Contains("CK_RegistrationSubmissions_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_RegistrationSubmissions_ResultShape", script, StringComparison.Ordinal);
        Assert.Contains("CK_RegistrationSubmissions_ReceiptSnapshotJson", script, StringComparison.Ordinal);
        Assert.Contains("CK_RegistrationSubmissions_DecisionSnapshotJson", script, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE TABLE [registration].[DecisionSnapshots]", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Enrollment_mapping_enforces_one_active_offering_and_group_ownership()
    {
        using var context = CreateContext();
        var enrollment = Entity<Enrollment>(context);

        Assert.Equal("Enrollments", enrollment.GetTableName());
        Assert.Equal("registration", enrollment.GetSchema());
        AssertUnique(
            enrollment,
            nameof(Enrollment.StudentId),
            nameof(Enrollment.OfferingId));
        AssertRowVersion(enrollment, nameof(Enrollment.Version));
        AssertForeignKey<Enrollment, Student>(
            enrollment,
            nameof(Enrollment.StudentId));
        AssertForeignKey<Enrollment, RegistrationSubmission>(
            enrollment,
            nameof(Enrollment.SubmissionId));
        AssertForeignKey<Enrollment, SectionGroup>(
            enrollment,
            nameof(Enrollment.OfferingId),
            nameof(Enrollment.GroupId));

        var group = Entity<SectionGroup>(context);
        Assert.Contains(group.GetKeys(), key =>
            key.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(SectionGroup.OfferingId), nameof(SectionGroup.Id)]));

        var script = context.Database.GenerateCreateScript();
        Assert.Contains("CK_Enrollments_State", script, StringComparison.Ordinal);
        Assert.Contains(
            "FK_Enrollments_SectionGroups_OfferingId_GroupId",
            script,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Registration_contribution_consumes_the_spec008_student_term_boundary()
    {
        using var context = CreateContext();

        var academicState = Entity<StudentTermAcademicState>(context);
        Assert.Equal("StudentTermAcademicStates", academicState.GetTableName());
        Assert.Equal("academics", academicState.GetSchema());
        AssertUnique(
            academicState,
            nameof(StudentTermAcademicState.StudentId),
            nameof(StudentTermAcademicState.TermId));
        AssertRowVersion(academicState, nameof(StudentTermAcademicState.Version));
        Assert.Single(
            context.Model.GetEntityTypes(),
            entity => entity.ClrType == typeof(StudentTermAcademicState));
        Assert.Null(context.Model.FindEntityType(typeof(DecisionSnapshot)));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_enforces_scope_reference_snapshot_state_and_enrollment_guards()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec014_Model_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);

        try
        {
            await context.Database.MigrateAsync();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await SeedDependenciesAsync(connection);

            await ExecuteAsync(connection, AcceptedSubmissionSql(
                "84000000-0000-0000-0000-000000000001",
                "85000000-0000-0000-0000-000000000001",
                "REG-2026-000001"));

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                AcceptedSubmissionSql(
                    "84000000-0000-0000-0000-000000000002",
                    "85000000-0000-0000-0000-000000000001",
                    "REG-2026-000002")));
            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                AcceptedSubmissionSql(
                    "84000000-0000-0000-0000-000000000003",
                    "85000000-0000-0000-0000-000000000003",
                    "REG-2026-000001")));
            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                AcceptedSubmissionSql(
                    "84000000-0000-0000-0000-000000000004",
                    "85000000-0000-0000-0000-000000000004",
                    "REG-2026-000004",
                    receiptSnapshotJson: "not-json")));
            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                AcceptedSubmissionSql(
                    "84000000-0000-0000-0000-000000000005",
                    "85000000-0000-0000-0000-000000000005",
                    "REG-2026-000005",
                    receiptSnapshotJson: null)));

            await ExecuteAsync(
                connection,
                """
                INSERT INTO [registration].[RegistrationSubmissions]
                    ([Id], [StudentId], [TermId], [ClientRequestId], [PayloadHash],
                     [ProcessingState], [ResultCode], [Reference], [ReceiptSnapshotJson],
                     [DecisionSnapshotJson], [ReceivedAtUtc], [UpdatedAtUtc], [CompletedAtUtc])
                VALUES
                    ('84000000-0000-0000-0000-000000000006',
                     '82000000-0000-0000-0000-000000000001',
                     '20000000-0000-0000-0000-000000000001',
                     '85000000-0000-0000-0000-000000000006', N'sha256:rejected',
                     N'rejected', N'CAPACITY_FULL', NULL, NULL, N'{"groupVersion":"1"}',
                     '2026-07-17T09:00:00Z', '2026-07-17T09:00:01Z',
                     '2026-07-17T09:00:01Z');
                """);

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                EnrollmentSql(
                    "86000000-0000-0000-0000-000000000001",
                    "50000000-0000-0000-0000-000000000099",
                    "84000000-0000-0000-0000-000000000001")));
            await ExecuteAsync(
                connection,
                EnrollmentSql(
                    "86000000-0000-0000-0000-000000000002",
                    "50000000-0000-0000-0000-000000000001",
                    "84000000-0000-0000-0000-000000000001"));
            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(
                connection,
                EnrollmentSql(
                    "86000000-0000-0000-0000-000000000003",
                    "50000000-0000-0000-0000-000000000001",
                    "84000000-0000-0000-0000-000000000006")));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext() =>
        CreateContext(ModelConnectionString);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static string AcceptedSubmissionSql(
        string id,
        string clientRequestId,
        string reference,
        string? receiptSnapshotJson = "{\"term\":\"2026-FALL\"}")
    {
        var receipt = receiptSnapshotJson is null
            ? "NULL"
            : $"N'{receiptSnapshotJson.Replace("'", "''", StringComparison.Ordinal)}'";
        const string decision = "N'{\"policyVersion\":\"2026.1\"}'";
        return $"""
            INSERT INTO [registration].[RegistrationSubmissions]
                ([Id], [StudentId], [TermId], [ClientRequestId], [PayloadHash],
                 [ProcessingState], [ResultCode], [Reference], [ReceiptSnapshotJson],
                 [DecisionSnapshotJson], [ReceivedAtUtc], [UpdatedAtUtc], [CompletedAtUtc])
            VALUES
                ('{id}', '82000000-0000-0000-0000-000000000001',
                 '20000000-0000-0000-0000-000000000001', '{clientRequestId}',
                 N'sha256:canonical', N'accepted', N'ACCEPTED', N'{reference}',
                 {receipt}, {decision},
                 '2026-07-17T09:00:00Z', '2026-07-17T09:00:01Z',
                 '2026-07-17T09:00:01Z');
            """;
    }

    private static string EnrollmentSql(
        string id,
        string groupId,
        string submissionId) =>
        $"""
        INSERT INTO [registration].[Enrollments]
            ([Id], [StudentId], [OfferingId], [GroupId], [SubmissionId],
             [State], [RegisteredAtUtc])
        VALUES
            ('{id}', '82000000-0000-0000-0000-000000000001',
             '10000000-0000-0000-0000-000000000001', '{groupId}',
             '{submissionId}', N'active', '2026-07-17T09:00:01Z');
        """;

    private static async Task SeedDependenciesAsync(SqlConnection connection) =>
        await ExecuteAsync(
            connection,
            """
            INSERT INTO [auth].[ApplicationUsers]
                ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                 [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
            VALUES
                ('81000000-0000-0000-0000-000000000001', N'student@example.test',
                 N'STUDENT@EXAMPLE.TEST', N'STU-001', N'not-a-real-hash', N'stamp', 1, 0, NULL);
            INSERT INTO [academics].[Students]
                ([Id], [ApplicationUserId], [ProgramCode], [Cohort], [CurrentGpa],
                 [EarnedCredits], [Standing], [IsActive], [Source], [SourceReference],
                 [DataVersion], [DataAsOfUtc], [ImportedAtUtc])
            VALUES
                ('82000000-0000-0000-0000-000000000001',
                 '81000000-0000-0000-0000-000000000001', N'AI', N'2025', 3.20,
                 30, N'good', 1, N'synthetic', N'student-1', N'1',
                 '2026-07-17T08:00:00Z', '2026-07-17T08:00:00Z');
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash],
                 [DisplayName], [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('20000000-0000-0000-0000-000000000001', N'2026-FALL',
                 '22000000-0000-0000-0000-000000000001', N'hash', N'Fall 2026',
                 '2026-09-01', '2026-12-31', N'Africa/Cairo', N'draft');
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash],
                 [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('31000000-0000-0000-0000-000000000001', N'AI', NULL, N'hash',
                 N'{}', N'{}', N'validated');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode],
                 [SourceReference], [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('32000000-0000-0000-0000-000000000001',
                 '31000000-0000-0000-0000-000000000001', NULL, N'AI', N'2026.1',
                 N'synthetic', '2026-07-01', '2026-07-01', N'admin', N'published');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn],
                 [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('30000000-0000-0000-0000-000000000001',
                 '32000000-0000-0000-0000-000000000001', N'CS101', N'Introduction',
                 3, 1, N'synthetic', '2026-07-01', N'synthetic-demo', N'[]');
            INSERT INTO [scheduling].[CourseOfferings]
                ([Id], [TermId], [CourseId], [State])
            VALUES
                ('10000000-0000-0000-0000-000000000001',
                 '20000000-0000-0000-0000-000000000001',
                 '30000000-0000-0000-0000-000000000001', N'published');
            INSERT INTO [scheduling].[SectionGroups]
                ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount],
                 [State], [RegistrationPaused])
            VALUES
                ('50000000-0000-0000-0000-000000000001',
                 '10000000-0000-0000-0000-000000000001', N'G01', 30, 0,
                 N'published', 0);
            """);

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static IEntityType Entity<TEntity>(DbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new Xunit.Sdk.XunitException(
            $"{typeof(TEntity).Name} is absent from the EF model.");

    private static void AssertUnique(IEntityType entity, params string[] properties) =>
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static void AssertColumn(
        IEntityType entity,
        string propertyName,
        string columnType)
    {
        var property = entity.FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.Equal(columnType, property.GetColumnType());
    }

    private static void AssertRowVersion(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    private static void AssertForeignKey<TEntity, TPrincipal>(
        IEntityType entity,
        params string[] properties)
    {
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(TPrincipal) &&
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(properties));
    }
}
