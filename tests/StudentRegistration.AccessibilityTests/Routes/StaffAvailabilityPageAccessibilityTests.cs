using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffAvailabilityPageAccessibilityTests
{
    [Fact]
    public void Stf_04_has_labelled_text_editor_validation_and_table_alternative()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        RepositoryFiles.ContainsAll(page, "aria-describedby", "role=\"alert\"",
            "<label", "type=\"time\"", "<table", "<caption>", "scope=\"col\"");
    }
}
