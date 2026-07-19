using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class ImplementationReadinessEvidenceTests
{
    private const string ManifestPath =
        "specs/003-ux-storyboard-accessibility/design/implementation-readiness-2026-07-19.json";

    [Fact]
    public void Approved_readiness_manifest_pins_every_remaining_route_and_contract_by_hash()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = document.RootElement;

        Assert.Equal("frontend-route-readiness/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("SPEC-003-ROUTE-READINESS-2026-07-19", root.GetProperty("approvalVersion").GetString());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvedBy").GetString());
        Assert.Equal("implementation-ready", root.GetProperty("decision").GetString());

        var expectedRoutes = new[]
        {
            "STU-02", "STU-03", "STU-04", "STU-05", "STU-06", "STU-07",
            "ADM-01", "ADM-05", "ADM-06", "ADM-07", "ADM-08", "ADM-09",
            "STF-01", "STF-02", "STF-03", "STF-04",
        };
        Assert.Equal(
            expectedRoutes,
            root.GetProperty("routes").EnumerateArray()
                .Select(route => route.GetProperty("routeId").GetString()!)
                .ToArray());

        var pins = root.GetProperty("contractPins");
        foreach (var route in root.GetProperty("routes").EnumerateArray())
        {
            var owner = route.GetProperty("implementationOwner").GetString()!;
            Assert.True(pins.TryGetProperty(owner, out _), $"Missing owner pin for {owner}.");
            foreach (var contributor in route.GetProperty("contributors").EnumerateArray())
            {
                var spec = contributor.GetString()!;
                Assert.True(pins.TryGetProperty(spec, out _), $"Missing contributor pin for {spec}.");
            }
        }

        foreach (var pin in pins.EnumerateObject())
        {
            var path = pin.Value.GetProperty("path").GetString()!;
            var expected = pin.Value.GetProperty("sha256").GetString()!;
            var actual = Convert.ToHexString(
                SHA256.HashData(File.ReadAllBytes(RepositoryFiles.PathTo(path))))
                .ToLowerInvariant();
            Assert.Equal(expected, actual);

            var specPath = path.Replace("contracts/api.md", "spec.md", StringComparison.Ordinal);
            Assert.Contains(
                "Approved",
                RepositoryFiles.Read(specPath),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
