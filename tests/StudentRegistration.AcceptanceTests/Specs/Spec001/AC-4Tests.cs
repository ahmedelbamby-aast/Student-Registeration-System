using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class AC_4Tests
{
    [Fact]
    public void Gate_c_proves_atomic_student_journey_and_all_staff_workspaces()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-traceability.md");
        RepositoryFiles.ContainsAll(
            evidence,
            "| AC-4 |",
            "atomic student journey",
            "Admin, Lecturer, and TeachingAssistant",
            "| PASS |");

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs"),
            "Shared_workspace_uses_only_the_selected_teaching_context",
            "Lecturer",
            "TeachingAssistant",
            "Non_teaching_roles_cannot_reach_workspace_reads_or_admin_mutations");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.E2ETests/Specs/Spec017/AdminDashboardPageFeatureTests.cs"),
            "Adm_01_live_and_paused_refresh_journeys_preserve_server_timestamp_and_values",
            "Adm_01_unauthorized_journey_hides_all_metric_content",
            "Admin");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.E2ETests/Specs/Spec016/StaffDashboardPageFeatureTests.cs"),
            "Stf_01_lecturer_v1_shows_only_server_authorized_assignments",
            "stale_assignment");
    }
}
