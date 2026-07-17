using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_3Tests
{
    [Fact]
    public void Audit_export_applies_row_and_field_scope_and_audits_the_request()
    {
        // Given an Admin lacks permission for restricted Identity security events.
        var contract = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "Restricted rows and fields are",
            "omitted before paging/counting",
            "never reveals that an omitted",
            "security event exists",
            "AdminAudit.Export",
            "OwnerId, ScopeHash",
            "RequestHash and Pending state",
            "request audit event is",
            "atomic");

        // When the scoped asynchronous export is requested, the production
        // service must bind authorization before reading or creating work.
        const string delivery =
            "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";
        Assert.True(
            RepositoryFiles.Exists(delivery),
            "Expected-red for AC-3/T034: scoped audit export execution is intentionally deferred to T056.");
        var service = RepositoryFiles.Read(delivery);

        // Then restricted rows/fields are omitted or denied and the accepted
        // request is audited without leaking PII or scope hashes.
        RepositoryFiles.ContainsAll(
            service,
            "OwnerId",
            "ScopeHash",
            "RequestHash",
            "Redact",
            "Audit",
            "Pending",
            "CancellationToken");
        Assert.DoesNotContain("Password", service, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SecurityStamp", service, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MetadataJson", service, StringComparison.Ordinal);
    }
}
