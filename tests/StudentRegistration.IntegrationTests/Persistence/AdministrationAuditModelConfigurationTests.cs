using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Data.SqlClient;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class AdministrationAuditModelConfigurationTests
{
    private const string ConnectionString =
        "Server=localhost;Database=Spec017AdministrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string ExportJobClrType =
        "StudentRegistration.StaffAdministration.Domain.ExportJob";

    [Fact]
    public void Export_job_mapping_is_bounded_versioned_and_lease_safe()
    {
        using var context = CreateContext();
        var runtimeModel = context.Model;
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var job = FindEntity(runtimeModel, ExportJobClrType);
        var designJob = FindEntity(designModel, ExportJobClrType);

        Assert.Equal("ExportJobs", job.GetTableName());
        Assert.Equal("administration", job.GetSchema());
        var key = Assert.Single(job.FindPrimaryKey()!.Properties);
        Assert.Equal("Id", key.Name);
        Assert.Equal("JobId", key.GetColumnName());
        Assert.Equal(ValueGenerated.Never, key.ValueGenerated);

        Required(job, "OwnerId");
        Required(job, "ScopeHash", maximumLength: 200);
        Required(job, "RequestHash", maximumLength: 200);
        Required(job, "ClientRequestId");
        Required(job, "State", maximumLength: 20);
        Required(job, "CreatedAtUtc");
        Required(job, "AttemptCount");
        Optional(job, "LeaseOwnerId", maximumLength: 200);
        Optional(job, "LeaseExpiresAtUtc");
        Optional(job, "ArtifactId");
        Optional(job, "CompletedAtUtc");
        Optional(job, "ExpiresAtUtc");
        Optional(job, "FailureCode", maximumLength: 100);

        var version = Property(job, "Version");
        Assert.False(version.IsNullable);
        Assert.True(version.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);

        AssertUniqueIndex(job, "OwnerId", "ScopeHash", "ClientRequestId");
        AssertUniqueIndex(job, "ArtifactId");
        AssertIndex(job, "State", "LeaseExpiresAtUtc", "AttemptCount", "Id");
        AssertIndex(job, "ExpiresAtUtc", "Id");

        var constraints = designJob.GetCheckConstraints()
            .ToDictionary(constraint => constraint.Name!, constraint => constraint.Sql!);
        Assert.Contains("CK_ExportJobs_State", constraints.Keys);
        Assert.Contains("pending", constraints["CK_ExportJobs_State"], StringComparison.Ordinal);
        Assert.Contains("running", constraints["CK_ExportJobs_State"], StringComparison.Ordinal);
        Assert.Contains("complete", constraints["CK_ExportJobs_State"], StringComparison.Ordinal);
        Assert.Contains("failed", constraints["CK_ExportJobs_State"], StringComparison.Ordinal);
        Assert.Contains("expired", constraints["CK_ExportJobs_State"], StringComparison.Ordinal);
        Assert.Contains("CK_ExportJobs_AttemptCount", constraints.Keys);
        Assert.Contains("[AttemptCount] <= 3", constraints["CK_ExportJobs_AttemptCount"], StringComparison.Ordinal);
        Assert.Contains("CK_ExportJobs_LeaseShape", constraints.Keys);
        Assert.Contains("[LeaseOwnerId]", constraints["CK_ExportJobs_LeaseShape"], StringComparison.Ordinal);
        Assert.Contains("[LeaseExpiresAtUtc]", constraints["CK_ExportJobs_LeaseShape"], StringComparison.Ordinal);
        Assert.Contains("CK_ExportJobs_ResultShape", constraints.Keys);
        Assert.Contains("[ArtifactId]", constraints["CK_ExportJobs_ResultShape"], StringComparison.Ordinal);
        Assert.Contains("[ExpiresAtUtc]", constraints["CK_ExportJobs_ResultShape"], StringComparison.Ordinal);
    }

    [Fact]
    public void Administration_mapping_contributes_only_export_job_and_preserves_upstream_owners()
    {
        using var context = CreateContext();
        var model = context.Model;

        Assert.Single(model.GetEntityTypes(), entity =>
            string.Equals(entity.ClrType.FullName, ExportJobClrType, StringComparison.Ordinal));
        Assert.Single(model.GetEntityTypes(), entity => entity.ClrType == typeof(AuditEvent));
        Assert.Single(model.GetEntityTypes(), entity => entity.ClrType == typeof(SecurityEvent));
        Assert.Single(model.GetEntityTypes(), entity => entity.ClrType == typeof(AdminSecurityGuard));

        Assert.Equal("AuditEvents", model.FindEntityType(typeof(AuditEvent))!.GetTableName());
        Assert.Equal("audit", model.FindEntityType(typeof(AuditEvent))!.GetSchema());
        Assert.Equal("SecurityEvents", model.FindEntityType(typeof(SecurityEvent))!.GetTableName());
        Assert.Equal("auth", model.FindEntityType(typeof(SecurityEvent))!.GetSchema());
        Assert.Equal("AdminSecurityGuards", model.FindEntityType(typeof(AdminSecurityGuard))!.GetTableName());
        Assert.Equal("auth", model.FindEntityType(typeof(AdminSecurityGuard))!.GetSchema());
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_persists_versioned_job_and_enforces_lease_and_idempotency_constraints()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec017_Mapping_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);
        try
        {
            await context.Database.MigrateAsync("20260717120000_Spec017ExportFilter");
            var ownerId = Guid.NewGuid();
            var clientRequestId = Guid.NewGuid();
            var createdAtUtc = new DateTime(2026, 7, 17, 12, 0, 0, DateTimeKind.Utc);
            var job = new ExportJob(
                Guid.NewGuid(),
                ownerId,
                clientRequestId,
                "SHA256:SCOPE",
                "SHA256:REQUEST",
                createdAtUtc);
            context.ExportJobs.Add(job);

            await context.SaveChangesAsync();

            Assert.NotEmpty(job.Version);
            context.ChangeTracker.Clear();
            var persisted = await context.ExportJobs.AsNoTracking().SingleAsync();
            Assert.Equal(ExportJobState.Pending, persisted.State);
            Assert.Equal(DateTimeKind.Utc, persisted.CreatedAtUtc.Kind);

            await Assert.ThrowsAsync<SqlException>(() => context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO [administration].[ExportJobs]
                    ([JobId], [OwnerId], [ClientRequestId], [ScopeHash], [RequestHash],
                     [State], [CreatedAtUtc], [AttemptCount])
                VALUES
                    ({0}, {1}, {2}, N'SHA256:INVALID-SCOPE', N'SHA256:INVALID-REQUEST',
                     N'running', {3}, 1);
                """,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                createdAtUtc));

            context.ExportJobs.Add(new ExportJob(
                Guid.NewGuid(),
                ownerId,
                clientRequestId,
                "SHA256:SCOPE",
                "SHA256:DIFFERENT-REQUEST",
                createdAtUtc));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

            Assert.Equal(1, await CanonicalTableCountAsync(connectionString, "audit", "AuditEvents"));
            Assert.Equal(1, await CanonicalTableCountAsync(connectionString, "auth", "SecurityEvents"));
            Assert.Equal(1, await CanonicalTableCountAsync(connectionString, "auth", "AdminSecurityGuards"));
            Assert.Equal(1, await CanonicalTableCountAsync(connectionString, "administration", "ExportJobs"));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext()
        => CreateContext(ConnectionString);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static async Task<int> CanonicalTableCountAsync(
        string connectionString,
        string schema,
        string table)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @table;
            """;
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", table);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static IEntityType FindEntity(IModel model, string clrTypeName) =>
        Assert.Single(model.GetEntityTypes(), entity =>
            string.Equals(entity.ClrType.FullName, clrTypeName, StringComparison.Ordinal));

    private static IProperty Property(IEntityType entity, string name) =>
        entity.FindProperty(name) ?? throw new Xunit.Sdk.XunitException(
            $"Expected {entity.ClrType.Name}.{name} to be mapped.");

    private static void Required(IEntityType entity, string name, int? maximumLength = null)
    {
        var property = Property(entity, name);
        Assert.False(property.IsNullable);
        if (maximumLength is not null)
        {
            Assert.Equal(maximumLength, property.GetMaxLength());
        }
    }

    private static void Optional(IEntityType entity, string name, int? maximumLength = null)
    {
        var property = Property(entity, name);
        Assert.True(property.IsNullable);
        if (maximumLength is not null)
        {
            Assert.Equal(maximumLength, property.GetMaxLength());
        }
    }

    private static void AssertUniqueIndex(IEntityType entity, params string[] properties)
    {
        var index = AssertIndex(entity, properties);
        Assert.True(index.IsUnique);
    }

    private static IIndex AssertIndex(IEntityType entity, params string[] properties) =>
        Assert.Single(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(properties));
}
