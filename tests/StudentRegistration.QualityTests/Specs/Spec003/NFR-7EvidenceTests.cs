using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_7EvidenceTests
{
    [Fact]
    public void Required_poc_browser_matrix_is_versioned_executed_and_webkit_is_not_safari()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/browser-matrix.json"));
        Assert.Equal("2026-07-19", document.RootElement.GetProperty("verifiedOn").GetString());
        var targets = document.RootElement.GetProperty("targets").EnumerateArray().ToArray();
        var required = targets.Where(target => target.GetProperty("status").GetString() == "required").ToArray();
        Assert.Equal(4, required.Length);
        Assert.Equal(
            ["Google Chrome", "Microsoft Edge", "Mozilla Firefox", "Playwright WebKit"],
            required.Select(target => target.GetProperty("name").GetString()!).ToArray());
        Assert.All(required, target =>
        {
            Assert.Equal("passed", target.GetProperty("result").GetString());
            Assert.Equal("executed", target.GetProperty("preflightStatus").GetString());
            Assert.DoesNotContain("latest", target.GetProperty("browserBuild").GetString()!, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal("150.0.7871.125", required[0].GetProperty("observedLocalVersion").GetString());
        Assert.Equal("150.0.4078.83", required[1].GetProperty("observedLocalVersion").GetString());
        var firefox = required[2];
        Assert.Equal("152.0.6", firefox.GetProperty("browserBuild").GetString());
        Assert.Equal("STU-02", firefox.GetProperty("smokeRouteId").GetString());
        Assert.Equal("service-error", firefox.GetProperty("smokeState").GetString());
        Assert.Equal("Available subjects", firefox.GetProperty("smokeHeading").GetString());
        Assert.Equal(64, firefox.GetProperty("smokeScreenshotSha256").GetString()!.Length);
        Assert.Contains("Firefox/152.0", firefox.GetProperty("smokeUserAgent").GetString());
        Assert.Contains("Playwright Firefox 151.0", firefox.GetProperty("visualCaptureBuild").GetString());
        var webkit = required[^1];
        Assert.Contains("not Safari", webkit.GetProperty("label").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("26.5", webkit.GetProperty("browserBuild").GetString());
        Assert.Equal("1.61.0", webkit.GetProperty("playwrightVersion").GetString());
        var safari = Assert.Single(targets, target => target.GetProperty("name").GetString() == "Apple Safari");
        Assert.Equal("deferred", safari.GetProperty("status").GetString());
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());
    }
}
