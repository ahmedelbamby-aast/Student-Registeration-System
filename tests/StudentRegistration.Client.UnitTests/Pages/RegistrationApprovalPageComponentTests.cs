using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationApprovalPageComponentTests
{
    [Fact]
    public void Adm_10_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("ApprovalAdministrationPage", "ADM-10");

    [Theory]
    [InlineData("ApprovalAdministrationPage.razor", "/admin/approvals", "true")]
    [InlineData("StaffApprovalInboxPage.razor", "/staff/approvals", "false")]
    public void Approval_routes_are_thin_wrappers_over_one_shared_workspace(string file, string route, string admin)
    {
        var wrapper = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{file}");
        RepositoryFiles.ContainsAll(wrapper, $"@page \"{route}\"", $"<ApprovalWorkspace IsAdmin=\"{admin}\" />");
    }

    [Fact]
    public void Adm_10_and_stf_05_share_all_queue_states_and_compact_visual_components()
    {
        var component = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Approvals/ApprovalWorkspace.razor");
        RepositoryFiles.ContainsAll(component,
            "ADM-10", "STF-05", "AuthenticatedPage", "UiDensity.Compact",
            "RouteStatePanel", "SurfaceCard", "EntityCard", "ApprovalStatusBadge",
            "CapacityBreakdown", "CapacityState.Full", "ApprovalStatusBadge",
            "CurrentCredits=\"@((int)row.RequestedCredits)\"", "CurrentCgpa=\"@row.CurrentCgpa\"",
            "AppButtonVariant.Danger", "COMP-STATE-");
    }

    [Fact]
    public void Shared_workspace_guards_double_action_stale_versions_and_server_derived_scope()
    {
        var component = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Approvals/ApprovalWorkspace.razor");
        RepositoryFiles.ContainsAll(component,
            "Disabled=\"@_deciding\"", "_deciding = true", "finally", "_deciding = false;",
            "row.SubmissionVersion", "row.Line.RowVersion", "Guid.NewGuid()",
            "ListAsync(IsAdmin)", "HasExpectedRole", "Lecturer", "TeachingAssistant");
        Assert.DoesNotContain("role selector", component, StringComparison.OrdinalIgnoreCase);
    }
}
