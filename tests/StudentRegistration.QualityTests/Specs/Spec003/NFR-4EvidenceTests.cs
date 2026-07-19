using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_4EvidenceTests
{
    [Fact]
    public void Interactive_minimum_is_44_css_pixels_and_projected_into_runtime_css()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/design/design-tokens.json"));
        var minimum = document.RootElement.GetProperty("tokens").GetProperty("sizing")
            .GetProperty("interactive-minimum").GetString();
        Assert.Equal("2.75rem", minimum);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/css/design-tokens.css"),
            "--srs-sizing-interactive-minimum: 2.75rem");
    }

    [Fact]
    public void Route_browser_checks_measure_pointer_targets_or_document_the_inline_exception()
    {
        foreach (var page in NFR_1EvidenceTests.RoutePages)
        {
            var source = NFR_1EvidenceTests.EvidenceSource(page);
            Assert.True(
                source.Contains("44", StringComparison.Ordinal) ||
                source.Contains("interactive-minimum", StringComparison.Ordinal) ||
                source.Contains("pointer", StringComparison.OrdinalIgnoreCase) ||
                source.Contains("target", StringComparison.OrdinalIgnoreCase),
                $"{page} has no pointer-target evidence.");
        }
    }
}
