using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Failed_or_expired_export_has_safe_status_and_only_an_authorized_new_key_retry()
    {
        const string servicePath =
            "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";

        Assert.True(
            RepositoryFiles.Exists(servicePath),
            "Expected red for T042/EC-2: AuditExportService is deferred until T056.");

        var source = RepositoryFiles.Read(servicePath);
        RepositoryFiles.ContainsAll(
            source,
            "Failed",
            "Expired",
            "ClientRequestId",
            "OwnerId",
            "ScopeHash",
            "Authorize",
            "EXPORT_EXPIRED");
        Assert.DoesNotContain("Reset", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Requeue", source, StringComparison.Ordinal);
    }
}
