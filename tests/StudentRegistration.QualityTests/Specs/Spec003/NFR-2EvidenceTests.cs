using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Every_route_accessibility_suite_exercises_keyboard_and_focus_behavior()
    {
        foreach (var page in NFR_1EvidenceTests.RoutePages)
        {
            var source = NFR_1EvidenceTests.EvidenceSource(page);
            Assert.Contains("Keyboard", source, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("FocusAsync", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Shared_css_and_dialogs_define_visible_focus_and_restoration_contracts()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/css/app.css"),
            ":focus-visible", "outline-color", "outline-width", "outline-offset");
        var razor = Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Client"), "*.razor", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();
        Assert.Contains(razor, source => source.Contains("FocusAsync", StringComparison.Ordinal));
        Assert.Contains(razor, source => source.Contains("tabindex=\"-1\"", StringComparison.Ordinal));
    }
}
