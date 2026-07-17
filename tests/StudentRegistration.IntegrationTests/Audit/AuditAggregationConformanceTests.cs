using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Admin;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Audit;

public sealed class AuditAggregationConformanceTests
{
    private const string QueryPath =
        "src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs";
    private const string PortPath =
        "src/StudentRegistration.StaffAdministration/Application/Ports/IAdminAuditReader.cs";
    private const string ReaderPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Admin/SqlAdminAuditReader.cs";

    [Fact]
    public void Merged_read_only_query_delivery_is_required()
    {
        var missing = new[] { QueryPath, PortPath, ReaderPath }
            .Where(path => !RepositoryFiles.Exists(path))
            .ToArray();

        Assert.True(
            missing.Length == 0,
            "Expected-red for T049/T059/T070: merged audit query is missing: "
            + string.Join(", ", missing));

        Assert.Single(typeof(IAdminAuditReader).GetMethods());
        Assert.Equal("ReadAsync", typeof(IAdminAuditReader).GetMethods()[0].Name);

        var reader = RepositoryFiles.Read(ReaderPath);
        RepositoryFiles.ContainsAll(
            reader,
            "AuditEvents.AsNoTracking()",
            "Set<SecurityEvent>().AsNoTracking()",
            "OrderByDescending(row => row.OccurredAtUtc)",
            "ThenByDescending(row => row.Id)",
            "RedactionVersion",
            "identity-security");
        Assert.DoesNotContain("_dbContext.Add(", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("AuditEvents.Add(", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Update(", reader, StringComparison.Ordinal);
        Assert.DoesNotContain(".Remove(", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("IAuditEventWriter", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteSql", reader, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_merge_scopes_before_paging_redacts_and_orders_both_streams()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new StudentRegistrationDbContext(options);

        try
        {
            await context.Database.EnsureCreatedAsync();
            var occurredAtUtc = new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc);
            const string allowedSubject = "student:allowed";
            context.AuditEvents.AddRange(
                Audit(
                    "00000000-0000-0000-0000-000000000001",
                    allowedSubject,
                    occurredAtUtc,
                    "CapacityChanged",
                    "{\"capacity\":30,\"password\":\"never\"}",
                    "{\"capacity\":31,\"nested\":{\"secret\":true}}"),
                Audit(
                    "00000000-0000-0000-0000-000000000003",
                    allowedSubject,
                    occurredAtUtc.AddMinutes(-1),
                    "StatusChanged",
                    null,
                    "{\"status\":\"enabled\"}"),
                Audit(
                    "00000000-0000-0000-0000-000000000004",
                    "student:restricted",
                    occurredAtUtc.AddMinutes(1),
                    "RestrictedChanged",
                    null,
                    "{\"state\":\"restricted\"}"));
            context.Set<SecurityEvent>().Add(new SecurityEvent(
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                null,
                "RoleChanged",
                "admin:one",
                allowedSubject,
                "Replace staff roles",
                "{\"roles\":\"Lecturer\",\"token\":\"never\"}",
                "{\"roles\":\"Admin\",\"passwordHash\":\"never\"}",
                "{\"accessToken\":\"never\"}",
                "correlation-security",
                occurredAtUtc));
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var queries = new AuditEventQueries(new SqlAdminAuditReader(context));
            var page = await queries.SearchAsync(new AdminAuditQuery(
                AdminAuditScope.Restricted([allowedSubject], includeIdentitySecurityEvents: true),
                Page: 1,
                PageSize: 2));

            Assert.Equal(3, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(
                [
                    Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Guid.Parse("00000000-0000-0000-0000-000000000001")
                ],
                page.Items.Select(item => item.Id));
            Assert.Equal(
                ["identity-security", "audit"],
                page.Items.Select(item => item.SourceStream));
            Assert.DoesNotContain(page.Items, item => item.Action == "RestrictedChanged");

            var audit = Assert.Single(page.Items, item => item.SourceStream == "audit");
            Assert.Equal("spec017-v1", audit.BeforeSummary.RedactionVersion);
            Assert.Equal(["capacity"], audit.BeforeSummary.Fields.Select(field => field.Name));
            Assert.Equal(["capacity"], audit.AfterSummary.Fields.Select(field => field.Name));
            Assert.DoesNotContain(
                "password",
                string.Join('|', audit.BeforeSummary.Fields.Select(field => field.Name)),
                StringComparison.OrdinalIgnoreCase);

            var auditOnly = await queries.SearchAsync(new AdminAuditQuery(
                AdminAuditScope.Restricted([allowedSubject], includeIdentitySecurityEvents: false),
                PageSize: 10));
            Assert.Equal(2, auditOnly.TotalCount);
            Assert.All(auditOnly.Items, item => Assert.Equal("audit", item.SourceStream));

            var securityOnly = await queries.SearchAsync(new AdminAuditQuery(
                AdminAuditScope.Restricted([allowedSubject], includeIdentitySecurityEvents: true),
                PageSize: 10,
                SourceStream: AuditSourceStream.IdentitySecurity));
            var security = Assert.Single(securityOnly.Items);
            Assert.Equal("RoleChanged", security.Action);
            Assert.Equal("correlation-security", security.CorrelationId);
            Assert.Equal(["roles"], security.BeforeSummary.Fields.Select(field => field.Name));
            Assert.Equal(["roles"], security.AfterSummary.Fields.Select(field => field.Name));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public void Canonical_sources_and_atomic_writer_remain_upstream_owned()
    {
        var auditEvent = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs");
        var securityEvent = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/SecurityEvent.cs");
        var writer = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditTransactionWriter.cs");
        var atomicity = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs");

        RepositoryFiles.ContainsAll(
            auditEvent,
            "public sealed class AuditEvent",
            "BeforeSummaryJson",
            "AfterSummaryJson",
            "CorrelationId",
            "OccurredAtUtc");
        RepositoryFiles.ContainsAll(
            securityEvent,
            "public sealed class SecurityEvent",
            "BeforeSummaryJson",
            "AfterSummaryJson",
            "CorrelationId",
            "OccurredAtUtc");
        RepositoryFiles.ContainsAll(
            writer,
            "public sealed class AuditTransactionWriter : IAuditEventWriter",
            "AUDIT_CALLER_TRANSACTION_REQUIRED",
            "_dbContext.AuditEvents.Add");
        RepositoryFiles.ContainsAll(
            atomicity,
            "Fault_injected_audit_save_rolls_back_business_state",
            "transaction.RollbackAsync",
            "BusinessStateCountAsync",
            "AuditEvents.AsNoTracking().CountAsync");

        var staffSources = string.Join(
            "\n",
            Directory.EnumerateFiles(
                    RepositoryFiles.PathTo("src/StudentRegistration.StaffAdministration"),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(path => !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));
        Assert.DoesNotContain(
            "class AuditTransactionWriter",
            staffSources,
            StringComparison.Ordinal);
        Assert.DoesNotContain("IAuditEventWriter", staffSources, StringComparison.Ordinal);
    }

    private static AuditEvent Audit(
        string id,
        string subjectReference,
        DateTime occurredAtUtc,
        string action,
        string? beforeSummaryJson,
        string? afterSummaryJson) =>
        new(
            Guid.Parse(id),
            "admin:one",
            subjectReference,
            action,
            "SyntheticEntity",
            id,
            "Acceptance evidence",
            beforeSummaryJson,
            afterSummaryJson,
            $"correlation-{id[^1]}",
            occurredAtUtc);
}
