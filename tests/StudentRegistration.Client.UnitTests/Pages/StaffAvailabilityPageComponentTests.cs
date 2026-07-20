using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffAvailabilityPageComponentTests
{
    [Fact]
    public void Stf_04_declares_keyboard_editor_validation_and_concurrency_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        RepositoryFiles.ContainsAll(source,
            "AuthenticatedPage", "WorkspaceKind.Staff", "UiDensity.Compact", "STF-04-COMP-STATE-EMPTY",
            "STF-04-COMP-STATE-SUCCESS", "STF-04-COMP-STATE-VALIDATION-ERROR",
            "STF-04-COMP-STATE-UNAUTHORIZED", "STF-04-COMP-STATE-STALE",
            "STF-04-COMP-STATE-SERVICE-ERROR", "Validation summary",
            "Add availability range", "Remove range", "Save availability");
        Assert.DoesNotContain("preferred", source, StringComparison.OrdinalIgnoreCase);
    }
}
