using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class OfferingAdministrationPageComponentTests
{
    [Fact]
    public void Adm_06_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("OfferingAdministrationPage", "ADM-06");

    [Fact]
    public void Adm_06_covers_state_editor_validation_dialog_and_duplicate_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-06", "T214", "OfferingAdministrationPage",
            ["LoadingState", "EmptyState", "SuccessState", "ValidationErrorState", "ServiceErrorState",
             "UnauthorizedState", "SessionExpiredState", "StaleState", "OfflineState"],
            "offering-validation-summary", "aria-modal=\"true\"", "Disabled=\"@_pending\"", "<AppButton");

    [Fact]
    public void Adm_06_tables_and_compact_lists_preserve_offering_and_activity_information()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor");
        RepositoryFiles.ContainsAll(source, "Offering results, compact view", "data-testid=\"offering-result-list\"",
            "Group activities, compact view", "data-testid=\"offering-activity-list\"",
            "@offering.CourseCode", "@offering.State", "@offering.GroupCount", "@offering.RowVersion",
            "@meeting.RoomCode", "@meeting.Location", "@meeting.DayOfWeek", "StaffFor(meeting.Id)");
        Assert.True(Occurrences(source, "SelectOfferingAsync(offering.Id)") >= 2);
        Assert.True(Occurrences(source, "StaffFor(meeting.Id)") >= 2);
    }

    private static int Occurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
