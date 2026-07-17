using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.IntegrationTests.Specs.Spec015;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class RegistrationReceiptModelConfigurationTests
{
    [Fact]
    public void Receipt_projection_registers_no_second_ef_entity_table_or_write_mapping()
    {
        using var context = ModelContext.Create("Spec015ReceiptProjectionModelOnly");

        Assert.Null(context.Model.FindEntityType(typeof(RegistrationReceipt)));
        var submission = context.Model.FindEntityType(typeof(RegistrationSubmission));
        Assert.NotNull(submission);
        Assert.Equal("RegistrationSubmissions", submission.GetTableName());
        Assert.Equal("registration", submission.GetSchema());
        Assert.DoesNotContain(
            context.Model.GetEntityTypes(),
            entity => entity.GetTableName() is "RegistrationReceipts");

        var script = context.Database.GenerateCreateScript();
        Assert.Contains(
            "CREATE TABLE [registration].[RegistrationSubmissions]",
            script,
            StringComparison.Ordinal);
        Assert.DoesNotContain("RegistrationReceipts", script, StringComparison.Ordinal);

        Assert.DoesNotContain(
            typeof(RegistrationReceiptModelConfiguration).GetInterfaces(),
            type => type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
    }

    [Fact]
    public void Projection_reads_only_accepted_canonical_submissions_and_preserves_snapshots()
    {
        var receivedAtUtc = new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc);
        var accepted = NewSubmission(receivedAtUtc);
        accepted.CompleteAccepted(
            "ACCEPTED",
            "REG-2026-000001",
            "{\"term\":{\"code\":\"2026-FALL\"}}",
            "{\"policyVersion\":\"DEMO-POC-2026.1\"}",
            receivedAtUtc.AddSeconds(1));
        var rejected = NewSubmission(receivedAtUtc);
        rejected.CompleteRejected(
            "GROUP_FULL",
            "{\"policyVersion\":\"DEMO-POC-2026.1\"}",
            receivedAtUtc.AddSeconds(1));

        var receipt = Assert.Single(
            RegistrationReceiptModelConfiguration.Project(
                new[] { accepted, rejected }.AsQueryable()));

        Assert.Equal(accepted.Id, receipt.SubmissionId);
        Assert.Equal(accepted.Reference, receipt.Reference);
        Assert.Equal(accepted.ReceiptSnapshotJson, receipt.ReceiptSnapshotJson);
        Assert.Equal(accepted.DecisionSnapshotJson, receipt.DecisionSnapshotJson);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_projects_the_canonical_row_without_a_receipt_table()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec015_Receipt_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);

        try
        {
            await context.Database.MigrateAsync();
            await context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO [auth].[ApplicationUsers]
                    ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                     [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
                VALUES
                    ('91000000-0000-0000-0000-000000000001', N'spec015@example.test',
                     N'SPEC015@EXAMPLE.TEST', N'STU-015', N'not-a-real-hash', N'stamp', 1, 0, NULL);
                INSERT INTO [academics].[Students]
                    ([Id], [ApplicationUserId], [ProgramCode], [Cohort], [CurrentGpa],
                     [EarnedCredits], [Standing], [IsActive], [Source], [SourceReference],
                     [DataVersion], [DataAsOfUtc], [ImportedAtUtc])
                VALUES
                    ('92000000-0000-0000-0000-000000000001',
                     '91000000-0000-0000-0000-000000000001', N'AI', N'2025', 3.20,
                     30, N'good', 1, N'synthetic', N'spec015-student', N'1',
                     '2026-07-17T08:00:00Z', '2026-07-17T08:00:00Z');
                INSERT INTO [academics].[AcademicTerms]
                    ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash],
                     [DisplayName], [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
                VALUES
                    ('93000000-0000-0000-0000-000000000001', N'2026-FALL',
                     '94000000-0000-0000-0000-000000000001', N'hash', N'Fall 2026',
                     '2026-09-01', '2026-12-31', N'Africa/Cairo', N'draft');
                INSERT INTO [registration].[RegistrationSubmissions]
                    ([Id], [StudentId], [TermId], [ClientRequestId], [PayloadHash],
                     [ProcessingState], [ResultCode], [Reference], [ReceiptSnapshotJson],
                     [DecisionSnapshotJson], [ReceivedAtUtc], [UpdatedAtUtc], [CompletedAtUtc])
                VALUES
                    ('95000000-0000-0000-0000-000000000001',
                     '92000000-0000-0000-0000-000000000001',
                     '93000000-0000-0000-0000-000000000001',
                     '96000000-0000-0000-0000-000000000001', N'sha256:canonical',
                     N'accepted', N'ACCEPTED', N'REG-2026-000015',
                     N'{{"term":{{"code":"2026-FALL"}}}}',
                     N'{{"policyVersion":"DEMO-POC-2026.1"}}',
                     '2026-07-17T09:00:00Z', '2026-07-17T09:00:01Z',
                     '2026-07-17T09:00:01Z');
                """);

            var receipt = Assert.Single(await RegistrationReceiptModelConfiguration.Project(
                    context.Set<RegistrationSubmission>().AsNoTracking())
                .ToListAsync());
            Assert.Equal("REG-2026-000015", receipt.Reference);
            Assert.Equal(
                "{\"term\":{\"code\":\"2026-FALL\"}}",
                receipt.ReceiptSnapshotJson);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT COUNT(*)
                FROM sys.tables AS t
                INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE s.name = N'registration' AND t.name = N'RegistrationReceipts';
                """;
            Assert.Equal(0, Convert.ToInt32(await command.ExecuteScalarAsync()));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static RegistrationSubmission NewSubmission(DateTime receivedAtUtc) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "sha256:canonical",
            receivedAtUtc);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
