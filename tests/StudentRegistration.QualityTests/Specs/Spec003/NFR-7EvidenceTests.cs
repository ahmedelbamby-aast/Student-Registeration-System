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
        var webkit = required[^1];
        Assert.Contains("not Safari", webkit.GetProperty("label").GetString(), StringComparison.OrdinalIgnoreCase);
        var safari = Assert.Single(targets, target => target.GetProperty("name").GetString() == "Apple Safari");
        Assert.Equal("deferred", safari.GetProperty("status").GetString());
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());
    }
}
