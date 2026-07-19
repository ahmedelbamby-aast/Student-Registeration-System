using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_5EvidenceTests
{
    [Fact]
    public void Every_page_design_and_executable_route_suite_covers_reflow_and_required_widths()
    {
        var designDirectory = RepositoryFiles.PathTo(
            "specs/003-ux-storyboard-accessibility/design/pages");
        var records = Directory.EnumerateFiles(designDirectory, "*.md")
            .Where(path => !path.EndsWith("identity-boundary.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.Equal(27, records.Length);
        foreach (var record in records)
        {
            RepositoryFiles.ContainsAll(File.ReadAllText(record),
                "\"responsiveWidths\"", "320,", "375,", "768,", "1024,", "1280,", "1920");
        }

        foreach (var page in NFR_1EvidenceTests.RoutePages)
        {
            var source = NFR_1EvidenceTests.EvidenceSource(page);
            Assert.True(
                source.Contains("400", StringComparison.Ordinal) ||
                source.Contains("deviceScaleFactor: 4", StringComparison.Ordinal));
            Assert.True(
                source.Contains("responsive", StringComparison.OrdinalIgnoreCase) ||
                source.Contains("scrollWidth", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Layout_uses_logical_dimensions_and_tables_have_non_scrolling_semantic_alternatives()
    {
        var css = string.Join('\n', Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Client"), "*.css", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        RepositoryFiles.ContainsAll(css, "inline-size", "block-size", "margin-inline", "padding-block");

        var pages = string.Join('\n', Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Client/Pages"), "*.razor")
            .Select(File.ReadAllText));
        Assert.Contains("table", pages, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            pages.Contains("chronological", StringComparison.OrdinalIgnoreCase) ||
            pages.Contains("data-mobile", StringComparison.OrdinalIgnoreCase) ||
            pages.Contains("aria-describedby", StringComparison.OrdinalIgnoreCase));
    }
}
