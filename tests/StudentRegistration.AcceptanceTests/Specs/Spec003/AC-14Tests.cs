using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_14Tests
{
    [Fact]
    public void Public_gateway_covers_available_maintenance_and_safe_retry_states()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor");
        var journey = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec008/RoleGatewayPageFeatureTests.cs");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/\"",
            "GetPublicContextAsync",
            "Student login",
            "Student activation",
            "Staff login",
            "Retry public context");
        RepositoryFiles.ContainsAll(
            journey,
            "**/api/public/context",
            "Auth_01_e2e_primary_uses_server_context_and_named_destinations",
            "MAINTENANCE",
            "SERVICE_UNAVAILABLE",
            "Retry public context",
            "Africa/Cairo");
    }

    [Fact]
    public void Student_dashboard_covers_authoritative_window_profile_and_action_states()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor");
        var journey = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec008/StudentDashboardPageFeatureTests.cs");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student\"",
            "GetAppContextAsync",
            "GetStudentAcademicContextAsync",
            "Start registration",
            "Resume registration",
            "WINDOW_CLOSED",
            "REGISTRATION_HOLD",
            "PROFILE_NOT_READY");
        RepositoryFiles.ContainsAll(
            journey,
            "Stu_01_e2e_primary_open",
            "Stu_01_e2e_primary_upcoming",
            "Stu_01_e2e_failure_closed",
            "Stu_01_e2e_failure_no_term",
            "Stu_01_e2e_failure_hold",
            "Stu_01_e2e_failure_incomplete_profile",
            "AssertRegistrationNavigationUnavailableAsync");
    }
}
