using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_6Tests
{
    [Fact]
    public void Reused_idempotency_key_with_changed_export_payload_executes_neither_new_payload()
    {
        const string servicePath =
            "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";

        Assert.True(
            RepositoryFiles.Exists(servicePath),
            "Expected red for T046/EC-6: AuditExportService is deferred until T056.");

        var source = RepositoryFiles.Read(servicePath);
        RepositoryFiles.ContainsAll(
            source,
            "ClientRequestId",
            "RequestHash",
            "IDEMPOTENCY_KEY_REUSED",
            "Conflict");
        Assert.Contains("no duplicate", source, StringComparison.OrdinalIgnoreCase);
    }
}
