using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentRoadmapPageComponentBehaviorTests
{
    [Fact]
    public void Stu_09_renders_loading_success_and_authoritative_failure_behaviors()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StudentRoadmapPage.razor");
        RepositoryFiles.ContainsAll(page, "STU-09", "RouteStatePanel", "SurfaceCard",
            "ROADMAP_LOADING", "Roadmap service unavailable", "FORBIDDEN",
            "GetStudentRoadmapAsync", "Automatic registration", "Missing prerequisites");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
    }
}

public sealed class AdminApprovalPageComponentBehaviorTests
{
    [Fact]
    public void Adm_10_selects_admin_scope_and_requires_a_reason_before_deciding()
    {
        var wrapper = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/ApprovalAdministrationPage.razor");
        var workspace = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Approvals/ApprovalWorkspace.razor");
        RepositoryFiles.ContainsAll(wrapper, "@page \"/admin/approvals\"", "IsAdmin=\"true\"");
        RepositoryFiles.ContainsAll(workspace, "ADM-10", "ListAsync(IsAdmin)",
            "Enter a reason before making this decision.", "Approve subject", "Reject plan");
    }
}

public sealed class StaffApprovalPageComponentBehaviorTests
{
    [Fact]
    public void Stf_05_selects_staff_scope_and_accepts_only_single_staff_roles()
    {
        var wrapper = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffApprovalInboxPage.razor");
        var workspace = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Approvals/ApprovalWorkspace.razor");
        RepositoryFiles.ContainsAll(wrapper, "@page \"/staff/approvals\"", "IsAdmin=\"false\"");
        RepositoryFiles.ContainsAll(workspace, "STF-05", "role is \"Lecturer\" or \"TeachingAssistant\"",
            "WorkspaceKind.Staff", "DecideAsync(IsAdmin");
        Assert.DoesNotContain("LecturerTeachingAssistant", workspace, StringComparison.Ordinal);
    }
}
