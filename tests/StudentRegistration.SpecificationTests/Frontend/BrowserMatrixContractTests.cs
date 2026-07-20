using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed class BrowserMatrixContractTests
{
    private const string MatrixPath =
        "tests/StudentRegistration.E2ETests/browser-matrix.json";

    [Fact]
    public void Matrix_pins_current_stable_browsers_and_playwright_webkit_with_exact_provenance()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(MatrixPath));
        var root = document.RootElement;
        var targets = root.GetProperty("targets").EnumerateArray().ToArray();

        Assert.Equal("browser-matrix/1.0.0", root.GetProperty("version").GetString());
        Assert.Equal("2026-07-19", root.GetProperty("verifiedOn").GetString());
        Assert.Equal(5, targets.Length);

        AssertTarget(targets, "Google Chrome", "150.0.7871.125", "required");
        AssertTarget(targets, "Microsoft Edge", "150.0.4078.83", "required");
        AssertTarget(targets, "Mozilla Firefox", "152.0.6", "required");
        AssertTarget(targets, "Playwright WebKit", "26.5", "required");
        AssertTarget(targets, "Apple Safari", "not-run", "deferred");

        Assert.All(targets, target =>
        {
            Assert.False(string.IsNullOrWhiteSpace(target.GetProperty("engine").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(target.GetProperty("osImage").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(target.GetProperty("source").GetString()));
        });
    }

    [Fact]
    public void Webkit_is_not_labelled_safari_and_actual_safari_is_not_claimed_passed()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(MatrixPath));
        var targets = document.RootElement.GetProperty("targets").EnumerateArray().ToArray();
        var webkit = Assert.Single(targets, target =>
            target.GetProperty("name").GetString() == "Playwright WebKit");
        var safari = Assert.Single(targets, target =>
            target.GetProperty("name").GetString() == "Apple Safari");

        Assert.Equal("Playwright WebKit (not Safari)", webkit.GetProperty("label").GetString());
        Assert.Equal("1.61.0", webkit.GetProperty("playwrightVersion").GetString());
        Assert.Equal("deferred", safari.GetProperty("status").GetString());
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());
    }

    private static void AssertTarget(
        JsonElement[] targets,
        string name,
        string browserBuild,
        string status)
    {
        var target = Assert.Single(targets, candidate =>
            candidate.GetProperty("name").GetString() == name);
        Assert.Equal(browserBuild, target.GetProperty("browserBuild").GetString());
        Assert.Equal(status, target.GetProperty("status").GetString());
    }
}
