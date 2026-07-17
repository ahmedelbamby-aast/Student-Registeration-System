using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class AuditSourceMergeContractTests
{
    private const string DeliveryPath =
        "src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs";

    [Fact]
    public void Merged_projection_is_scoped_redacted_correlated_and_chronological()
    {
        var contract = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "interface RedactedChangeSummaryDto",
            "beforeSummary: RedactedChangeSummaryDto",
            "afterSummary: RedactedChangeSummaryDto",
            "correlationId: string",
            "sourceStream: \"audit\" | \"identity-security\"",
            "merge shared SPEC-004",
            "AuditEvent and SPEC-007 SecurityEvent records",
            "never expose",
            "secret/unbounded",
            "metadata.");

        Assert.True(
            RepositoryFiles.Exists(DeliveryPath),
            $"Expected-red: the merged read projection is deferred to T070 at {DeliveryPath}.");

        var source = RepositoryFiles.Read(DeliveryPath)
            + RepositoryFiles.Read(
                "src/StudentRegistration.Infrastructure.SqlServer/Admin/SqlAdminAuditReader.cs");
        RepositoryFiles.ContainsAll(
            source,
            "AuditEvent",
            "SecurityEvent",
            "OccurredAtUtc",
            "CorrelationId",
            "BeforeSummary",
            "AfterSummary",
            "identity-security");
        Assert.Contains("scope", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OrderByDescending", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MetadataJson", source, StringComparison.Ordinal);
    }
}
