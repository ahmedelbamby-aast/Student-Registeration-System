using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class ResourceAdministrationPageComponentTests
{
    [Fact]
    public void Adm_07_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("ResourceAdministrationPage", "ADM-07");

    [Fact]
    public void Adm_07_covers_state_forms_read_only_availability_and_pending_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-07", "T219", "ResourceAdministrationPage",
            ["LoadingState", "EmptyState", "SuccessState", "ValidationErrorState", "ServiceErrorState",
             "UnauthorizedState", "SessionExpiredState", "StaleState", "OfflineState"],
            "read-only-staff-availability", "Revalidate impact", "Disabled=\"@_pending\"", "<AppButton");

    [Fact]
    public void Adm_07_room_table_and_compact_list_expose_equivalent_fields_and_action()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor");
        RepositoryFiles.ContainsAll(source, "Room results", "Room results, compact view",
            "data-testid=\"room-result-list\"", "@room.Code", "@room.Location",
            "@room.Capacity", "@room.State", "@room.RowVersion");
        Assert.True(Occurrences(source, "SelectRoom(room)") >= 2);
        Assert.True(Occurrences(source, "@room.RowVersion") >= 2);
    }

    private static int Occurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
