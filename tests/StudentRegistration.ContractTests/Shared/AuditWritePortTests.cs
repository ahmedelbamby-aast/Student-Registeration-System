using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class AuditWritePortTests
{
    private const string PortPath =
        "src/StudentRegistration.Contracts/Auditing/IAuditEventWriter.cs";

    [Fact]
    public void Audit_write_port_is_canonical_narrow_and_transaction_aware()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            PortPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("004:AuditWritePort")
                .GetString());

        var source = RepositoryFiles.Read(PortPath);
        RepositoryFiles.ContainsAll(
            source,
            "public sealed record AuditEventDraft(",
            "string ActorReference",
            "string SubjectReference",
            "string Action",
            "string EntityType",
            "string EntityId",
            "string Reason",
            "string? BeforeSummaryJson",
            "string? AfterSummaryJson",
            "string CorrelationId",
            "DateTime OccurredAtUtc",
            "public interface IAuditEventWriter",
            "Task AppendAsync(",
            "AuditEventDraft auditEvent",
            "CancellationToken cancellationToken");
        Assert.DoesNotContain("BeginTransaction", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CommitAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChangesAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("StaffAdministration", source, StringComparison.Ordinal);
    }
}
