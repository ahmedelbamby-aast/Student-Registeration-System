using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_4Tests
{
    [Fact]
    public void Distinct_identity_pages_use_server_authoritative_roles_and_complete_test_evidence()
    {
        var studentLogin = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentLoginPage.razor");
        var activation = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor");
        var staffLogin = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor");
        var recovery = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor");

        RepositoryFiles.ContainsAll(
            studentLogin,
            "@page \"/student/login\"",
            "StudentLoginRequest",
            "data-testid=\"student-login-form\"");
        RepositoryFiles.ContainsAll(
            activation,
            "@page \"/student/activate\"",
            "ActivateStudentRequest",
            "data-testid=\"student-activation-form\"");
        RepositoryFiles.ContainsAll(
            recovery,
            "@page \"/account/recovery\"",
            "RecoveryRequest",
            "RecoveryCompleteRequest");
        RepositoryFiles.ContainsAll(
            staffLogin,
            "@page \"/staff/login\"",
            "StaffLoginRequest",
            "role-selection-required",
            "_availableRoles",
            "SelectRoleContextAsync");
        Assert.DoesNotContain("<select", staffLogin, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"role\"", staffLogin, StringComparison.OrdinalIgnoreCase);

        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpoints,
            "MapPost(\"/api/auth/student/login\"",
            "MapPost(\"/api/auth/staff/login\"",
            "MapPut(\"/api/auth/session/context\"",
            ".RequireAuthorization(RolePolicies.StaffContext)");

        foreach (var routeId in new[] { "AUTH-02", "AUTH-03", "AUTH-04", "AUTH-05" })
        {
            Assert.True(RepositoryFiles.Exists(
                $"tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/{routeId}/route-contract.json"));
            using var visual = JsonDocument.Parse(RepositoryFiles.Read(
                $"tests/StudentRegistration.VisualTests/Baselines/Spec007/{routeId}/baseline-targets.json"));
            Assert.Equal("approved", visual.RootElement.GetProperty("status").GetString());
            Assert.Equal(16, visual.RootElement.GetProperty("targets").GetArrayLength());
            Assert.All(
                visual.RootElement.GetProperty("targets").EnumerateArray(),
                target => Assert.NotEqual("PENDING", target.GetProperty("sha256").GetString()));
        }

        var journeys = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec007/ExecutableIdentityJourneys.cs");
        RepositoryFiles.ContainsAll(
            journeys,
            "context.Render<StudentLoginPage>()",
            "context.Render<StudentActivationPage>()",
            "context.Render<StaffLoginPage>()",
            "context.Render<AccountRecoveryPage>()",
            "Dual_role_staff_chooses_only_a_server_returned_context",
            "Assert.DoesNotContain(options",
            "Student");
    }
}
