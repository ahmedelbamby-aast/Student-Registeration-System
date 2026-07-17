using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec015;

public sealed class ScopeReviewEvidenceTests
{
    [Fact]
    public void Delivered_surface_is_read_only_private_and_projection_only()
    {
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/endpoint-manifest.json"));
        var endpoints = manifest.RootElement.GetProperty("endpoints")
            .EnumerateArray()
            .Where(endpoint => endpoint.GetProperty("owner").GetString() == "015")
            .ToArray();

        Assert.Equal(5, endpoints.Length);
        Assert.All(endpoints, endpoint =>
            Assert.Equal("GET", endpoint.GetProperty("method").GetString()));

        var endpointSource = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Endpoints/Spec015Endpoints.cs");
        Assert.DoesNotContain("MapPost", endpointSource, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPut", endpointSource, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPatch", endpointSource, StringComparison.Ordinal);
        Assert.DoesNotContain("MapDelete", endpointSource, StringComparison.Ordinal);

        var projection = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationReceiptModelConfiguration.cs");
        RepositoryFiles.ContainsAll(projection, "IQueryable<RegistrationReceipt>", "RegistrationSubmission");
        Assert.DoesNotContain("IEntityTypeConfiguration", projection, StringComparison.Ordinal);
        Assert.DoesNotContain("ToTable", projection, StringComparison.Ordinal);
    }

    [Fact]
    public void Evidence_binds_all_four_exclusions_to_the_inspected_surface()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-015-scope-review.md");
        RepositoryFiles.ContainsAll(
            evidence,
            "T059 / OS-1",
            "T060 / OS-2",
            "T061 / OS-3",
            "T062 / OS-4",
            "Drop, withdrawal, or correction workflow",
            "Email/SMS receipt",
            "Public/shareable receipt link",
            "Transcript replacement",
            "projection-on-014",
            "SPEC-015 adds no migration",
            "ScopeReviewEvidenceTests");
    }
}
