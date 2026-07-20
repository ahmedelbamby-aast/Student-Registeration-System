using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class AuditAdministrationPageContractTests
{
    [Fact]
    public void Adm_09_is_the_scoped_audit_and_export_lifecycle_contract()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-09.md");
        var api = RepositoryFiles.Read(
            "specs/017-admin-operations-audit-reporting/contracts/api.md");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/audit\"",
            "ADM-09-pagination-v1",
            "ADM-09-ready-v1",
            "ADM-09-expired-v1");
        RepositoryFiles.ContainsAll(
            api,
            "GET /api/admin/audit",
            "POST /api/admin/exports",
            "GET /api/admin/exports/{jobId}/download");
        RepositoryFiles.ContainsAll(
            page,
            "@page \"/admin/audit\"",
            "data-route-id=\"ADM-09\"",
            "OperationsApi.SearchAuditAsync",
            "OperationsApi.CreateExportAsync",
            "OperationsApi.GetExportStatusAsync",
            "Scoped audit search",
            "Request export",
            "Download ready export");
    }
}
