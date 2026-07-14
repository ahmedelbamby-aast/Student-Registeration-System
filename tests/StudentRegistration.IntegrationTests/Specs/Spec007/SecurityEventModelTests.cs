using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class SecurityEventModelTests
{
    [Fact]
    public void Security_event_is_append_only_safe_and_bounded()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/SecurityEvent.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class SecurityEvent",
            "public Guid Id { get; }",
            "public string EventType { get; }",
            "public string ActorReference { get; }",
            "public string SubjectReference { get; }",
            "public string Reason { get; }",
            "public string? BeforeSummaryJson { get; }",
            "public string? AfterSummaryJson { get; }",
            "public string? MetadataJson { get; }",
            "public string CorrelationId { get; }",
            "public DateTime OccurredAtUtc { get; }");
        Assert.DoesNotContain(" set;", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Token", source, StringComparison.Ordinal);
    }
}
