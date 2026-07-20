using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationApprovalPageComponentTests
{
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
            "CapacityBreakdown", "AppButtonVariant.Danger", "COMP-STATE-");
    }
}
