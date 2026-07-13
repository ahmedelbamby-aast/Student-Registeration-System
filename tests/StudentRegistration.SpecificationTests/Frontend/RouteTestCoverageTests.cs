using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed partial class RouteTestCoverageTests
{
    private const string FixturePath =
        "tests/StudentRegistration.E2ETests/Infrastructure/FrontendTestFixture.cs";

    [Fact]
    public void Fixture_plans_all_27_routes_and_required_test_families()
    {
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var source = RepositoryFiles.Read(FixturePath);
        var plannedIds = RoutePlanPattern().Matches(source)
            .Select(match => match.Groups["id"].Value)
            .ToArray();
        var manifestIds = manifest.RootElement.GetProperty("routes")
            .EnumerateArray()
            .Select(route => route.GetProperty("id").GetString()!)
            .ToArray();

        Assert.Equal(27, plannedIds.Length);
        Assert.Equal(manifestIds.Order(), plannedIds.Order());
        RepositoryFiles.ContainsAll(
            source,
            "frontend-fixture/1.0",
            "component",
            "contract",
            "e2e",
            "accessibility",
            "visual",
            "axe",
            "keyboard",
            "approved visual baseline",
            "does not represent executed evidence");
    }

    [GeneratedRegex("new\\(\\\"(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\\\"")]
    private static partial Regex RoutePlanPattern();
}
