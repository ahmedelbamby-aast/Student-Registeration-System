using StudentRegistration.TestSupport;

namespace StudentRegistration.AuthorizationTests;

public sealed class AuditExportScopeTests
{
    private const string ServicePath =
        "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";
    private const string StorePortPath =
        "src/StudentRegistration.StaffAdministration/Application/Ports/IAdminExportStore.cs";
    private const string ArtifactPortPath =
        "src/StudentRegistration.StaffAdministration/Application/Ports/IAdminExportArtifactStore.cs";
    private const string SqlStorePath =
        "src/StudentRegistration.Infrastructure.SqlServer/Admin/SqlAdminExportStore.cs";

    [Fact]
    public void Export_lane_exists_at_the_approved_module_boundaries()
    {
        var missing = new[] { ServicePath, StorePortPath, ArtifactPortPath, SqlStorePath }
            .Where(path => !RepositoryFiles.Exists(path))
            .ToArray();

        Assert.True(
            missing.Length == 0,
            "Expected-red for T055/T057: T056/T058 have not delivered "
            + string.Join(", ", missing));
    }

    [Fact]
    public void Service_binds_scope_filters_idempotency_audit_and_expiry_without_pii_or_a_broker()
    {
        var source = RepositoryFiles.Read(ServicePath);

        RepositoryFiles.ContainsAll(
            source,
            "MaximumPageSize = 100",
            "MaximumExportRows",
            "MaximumFilterRange",
            "ScopeHash",
            "ClientRequestId",
            "RequestHash",
            "IDEMPOTENCY_KEY_REUSED",
            "AdminExportRequested",
            "AdminExportDownloaded",
            "CanReadAll",
            "ExpireAsync",
            "DeleteAsync",
            "RedactedBeforeSummary",
            "RedactedAfterSummary");
        Assert.DoesNotContain("MetadataJson", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ConcurrentQueue", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Task.Run(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Store_contract_and_sql_adapter_use_conditional_renewable_leases()
    {
        var port = RepositoryFiles.Read(StorePortPath);
        var sql = RepositoryFiles.Read(SqlStorePath);

        RepositoryFiles.ContainsAll(
            port,
            "CreateOrReplayAsync",
            "ReadAuthorizedAsync",
            "TryClaimAsync",
            "RenewLeaseAsync",
            "PublishAsync",
            "RecordFailureAsync",
            "RecordDownloadAsync",
            "ExpireAsync");
        RepositoryFiles.ContainsAll(
            sql,
            "UPDLOCK",
            "HOLDLOCK",
            "[ScopeHash]",
            "RequestHash",
            "[ClientRequestId]",
            "[AttemptCount] < 3",
            "[LeaseExpiresAtUtc]",
            "[ArtifactId] IS NULL",
            "IAuditEventWriter");
        Assert.DoesNotContain("FromSqlRaw", sql, StringComparison.Ordinal);
    }
}
