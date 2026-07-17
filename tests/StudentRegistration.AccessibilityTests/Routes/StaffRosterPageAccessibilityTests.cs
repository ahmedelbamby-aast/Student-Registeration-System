using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffRosterPageAccessibilityTests
{
    [Fact]
    public void Stf_03_has_caption_headers_labelled_overflow_and_keyboard_paging()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffRosterPage.razor");
        RepositoryFiles.ContainsAll(page, "role=\"region\"", "tabindex=\"0\"",
            "<caption>", "scope=\"col\"", "Pagination", "aria-label=\"Roster compact view\"");
    }
}
