using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed class BrowserGateFailClosedTests
{
    [Fact]
    public void Browser_fixtures_fail_instead_of_skipping_when_a_required_runtime_is_missing()
    {
        string[] fixturePaths =
        [
            "tests/StudentRegistration.E2ETests/Infrastructure/Spec008BrowserFixture.cs",
            "tests/StudentRegistration.E2ETests/Infrastructure/Spec003PublishedBrowserFixture.cs",
            "tests/StudentRegistration.AccessibilityTests/Infrastructure/AxeAccessibilityFixture.cs",
            "tests/StudentRegistration.VisualTests/Infrastructure/VisualRegressionFixture.cs"
        ];

        foreach (var path in fixturePaths)
        {
            var source = RepositoryFiles.Read(path);
            Assert.DoesNotContain("SkipException", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ForSkip", source, StringComparison.Ordinal);
            Assert.Contains("XunitException", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Ci_executes_contract_accessibility_and_visual_gates_and_rejects_zero_tests()
    {
        var workflow = RepositoryFiles.Read(".github/workflows/ci.yml");

        RepositoryFiles.ContainsAll(
            workflow,
            "StudentRegistration.Client.ContractTests.csproj",
            "StudentRegistration.VisualTests.csproj",
            "StudentRegistration.AccessibilityTests.csproj",
            "--fail-if-no-tests",
            "$env:SRS_BROWSER_TARGET = $target",
            "Playwright WebKit");
        Assert.DoesNotContain(
            "--filter \"Category=Accessibility\"",
            workflow,
            StringComparison.Ordinal);
    }
}
