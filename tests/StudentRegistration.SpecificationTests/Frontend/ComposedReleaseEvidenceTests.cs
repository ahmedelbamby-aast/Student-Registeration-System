using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed class ComposedReleaseEvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-003-composed-no-interception.json";

    [Fact]
    public void Release_evidence_covers_the_nine_composed_registration_scenarios_without_interception()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(EvidencePath));
        var root = document.RootElement;
        var browser = root.GetProperty("browserEvidence");

        Assert.False(browser.GetProperty("requestInterceptionAllowed").GetBoolean());
        Assert.True(browser.GetProperty("releaseRequiresStudent").GetBoolean());

        var runner = browser.GetProperty("runner").GetString()!;
        var browserSourcePath = browser.GetProperty("source").GetString()!;
        var runnerSource = RepositoryFiles.Read(runner);
        var browserSource = RepositoryFiles.Read(browserSourcePath);
        RepositoryFiles.ContainsAll(runnerSource, "RequireStudent", "SRS_LIVE_REQUIRE_STUDENT", "notExecuted");
        Assert.DoesNotContain("RouteAsync", browserSource, StringComparison.Ordinal);
        Assert.DoesNotContain("FulfillAsync", browserSource, StringComparison.Ordinal);

        var scenarios = root.GetProperty("scenarios").EnumerateArray().ToArray();
        Assert.Equal(9, scenarios.Length);
        Assert.Equal(
            [
                "first-term-auto-enrollment", "normal-plan", "gpa-credit-boundaries",
                "held-capacity", "admin-decision", "lecturer-decision",
                "teaching-assistant-decision", "student-terminal-states", "cross-role-denial"
            ],
            scenarios.Select(item => item.GetProperty("id").GetString()!).ToArray());

        foreach (var scenario in scenarios)
        {
            var routes = scenario.GetProperty("uiRoutes").EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray();
            Assert.NotEmpty(routes);
            foreach (var route in routes)
            {
                Assert.Contains(route, browserSource, StringComparison.Ordinal);
            }

            var assertions = scenario.GetProperty("evidence").EnumerateArray().ToArray();
            Assert.NotEmpty(assertions);
            foreach (var evidence in assertions)
            {
                var source = RepositoryFiles.Read(evidence.GetProperty("path").GetString()!);
                Assert.Contains(evidence.GetProperty("assertion").GetString()!, source, StringComparison.Ordinal);
            }
        }
    }
}
