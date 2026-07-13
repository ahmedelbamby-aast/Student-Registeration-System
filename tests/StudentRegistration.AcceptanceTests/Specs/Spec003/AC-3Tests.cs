using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_3Tests
{
    private static readonly string[] RequiredStates =
    [
        "loading",
        "empty",
        "success",
        "validation-error",
        "service-error",
        "unauthorized",
        "session-expired",
        "stale",
        "offline"
    ];

    [Fact]
    public void Functional_state_plan_is_deterministic_multilayered_and_not_executed_evidence()
    {
        // Given a future data route declares its complete applicable UI-state set.
        using var schema = JsonDocument.Parse(
            RepositoryFiles.Read(
                "specs/003-ux-storyboard-accessibility/schemas/page-design-record.schema.json"));
        var definitions = schema.RootElement.GetProperty("$defs");
        var stateCase = definitions.GetProperty("uiStateCase");
        var stateNames = stateCase.GetProperty("properties").GetProperty("state")
            .GetProperty("enum").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Equal(RequiredStates, stateNames);
        Assert.Equal(RequiredStates.Length, stateNames.Distinct(StringComparer.Ordinal).Count());

        // When its component and browser test-plan contracts are inspected.
        var stateRequired = stateCase.GetProperty("required").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            new[]
            {
                "state",
                "applicability",
                "fixture",
                "fixtureVersion",
                "expectedContent",
                "expectedFocusTarget",
                "liveRegion",
                "nextActions",
                "testIds"
            },
            field => Assert.Contains(field, stateRequired));
        Assert.True(
            definitions.GetProperty("nonemptyStringArray")
                .GetProperty("uniqueItems")
                .GetBoolean());

        var recordContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/page-design-record-contract.md");
        RepositoryFiles.ContainsAll(
            recordContract,
            "exactly once",
            "deterministic `fixture`",
            "`fixtureVersion`",
            "`expectedContent`",
            "`expectedFocusTarget`",
            "`nextActions`",
            "state-specific `testIds`");

        var fixtureSource = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Infrastructure/FrontendTestFixture.cs");
        var plannedIds = Regex.Matches(
                fixtureSource,
                "new\\(\\\"(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\\\"")
            .Select(match => match.Groups["id"].Value)
            .ToArray();
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var routeIds = manifest.RootElement.GetProperty("routes").EnumerateArray()
            .Select(route => route.GetProperty("id").GetString()!)
            .ToArray();

        Assert.Equal(27, plannedIds.Length);
        Assert.Equal(routeIds.Order(), plannedIds.Order());

        // Then each route plan is multi-layered; a visual plan alone cannot satisfy AC-3.
        RepositoryFiles.ContainsAll(
            fixtureSource,
            "frontend-fixture/1.0",
            "component",
            "contract",
            "e2e",
            "accessibility",
            "visual",
            "axe plus keyboard evidence",
            "approved visual baseline",
            "does not represent executed evidence");
    }
}
