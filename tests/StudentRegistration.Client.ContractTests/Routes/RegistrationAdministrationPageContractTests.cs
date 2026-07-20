using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationAdministrationPageContractTests
{
    [Fact]
    public void Adm_08_is_the_read_only_registration_monitor_contract()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-08.md");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor");
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/registrations\"",
            "GET /api/admin/operations/metrics",
            "ADM-08-no-repair-correction-action-v1");
        RepositoryFiles.ContainsAll(
            page,
            "@page \"/admin/registrations\"",
            "data-route-id=\"ADM-08\"",
            "Read-only registration monitoring",
            "No repair or correction actions are available");
        RepositoryFiles.ContainsAll(
            client,
            "GetAdminRegistrationHistoryAsync",
            "GetAdminRegistrationDetailAsync",
            "/api/admin/students/{RequiredId(studentId, nameof(studentId)):D}");
        Assert.DoesNotContain("MapPost", page, StringComparison.Ordinal);
        Assert.DoesNotContain("RepairAsync", page, StringComparison.Ordinal);
    }
}
