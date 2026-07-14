using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class NFR_4EvidenceTests
{
    private static readonly string[] EndpointIds =
        Enumerable.Range(1, 16).Select(number => $"Endpoint{number:00}").ToArray();

    [Fact]
    public void Authorization_matrix_covers_every_identity_endpoint()
    {
        var evidence = RepositoryFiles.Read("docs/release-evidence/SPEC-007-NFR-4.md");
        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-007 NFR-4",
            "Anonymous",
            "Student",
            "Admin",
            "Lecturer",
            "TeachingAssistant",
            "positive",
            "negative",
            "16",
            "**Result: PASS.**");

        Assert.All(EndpointIds, endpointId =>
            Assert.Contains($"| {endpointId} |", evidence, StringComparison.Ordinal));
        Assert.Equal(
            16,
            evidence.Split('\n').Count(line =>
                EndpointIds.Any(id => line.StartsWith($"| {id} |", StringComparison.Ordinal))));
    }
}
