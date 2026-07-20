using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_9Tests
{
    [Fact]
    public void Accessibility_gates_cover_every_route_and_state()
    {
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var routeIds = manifest.RootElement.GetProperty("routes").EnumerateArray()
            .Select(route => route.GetProperty("id").GetString()!)
            .ToArray();
        var accessibilitySource = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(
                    RepositoryFiles.PathTo("tests/StudentRegistration.AccessibilityTests/Routes"),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.Equal(30, routeIds.Length);
        Assert.All(routeIds, routeId =>
            Assert.Contains(routeId, accessibilitySource, StringComparison.Ordinal));
        Assert.DoesNotContain("SkipException", accessibilitySource, StringComparison.Ordinal);
    }
}
