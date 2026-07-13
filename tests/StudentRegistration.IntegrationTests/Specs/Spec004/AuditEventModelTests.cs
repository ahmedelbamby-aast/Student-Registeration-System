using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004;

public sealed class AuditEventModelTests
{
    private const string ModelPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs";

    [Fact]
    public void Audit_event_has_canonical_ownership_complete_fields_and_no_mutation_surface()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            ModelPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("004:AuditEvent")
                .GetString());

        var source = RepositoryFiles.Read(ModelPath);
        RepositoryFiles.ContainsAll(
            source,
            "public sealed class AuditEvent",
            "public AuditEvent(",
            "public Guid Id { get; }",
            "public string ActorReference { get; }",
            "public string SubjectReference { get; }",
            "public string Action { get; }",
            "public string EntityType { get; }",
            "public string EntityId { get; }",
            "public string Reason { get; }",
            "public string? BeforeSummaryJson { get; }",
            "public string? AfterSummaryJson { get; }",
            "public string CorrelationId { get; }",
            "public DateTime OccurredAtUtc { get; }");
        Assert.DoesNotContain(" set;", source, StringComparison.Ordinal);
        Assert.DoesNotContain(" init;", source, StringComparison.Ordinal);
    }
}
