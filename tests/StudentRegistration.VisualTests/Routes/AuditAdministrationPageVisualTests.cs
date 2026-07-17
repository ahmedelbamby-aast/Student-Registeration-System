using StudentRegistration.TestSupport;

namespace StudentRegistration.VisualTests.Routes;

public sealed class AuditAdministrationPageVisualTests
{
    [Fact]
    public void Adm_09_visual_layout_keeps_scope_results_detail_and_export_status_together()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor.css");

        RepositoryFiles.ContainsAll(
            css,
            ".srs-audit-admin",
            "max-inline-size:",
            "grid-template-columns:",
            "min-block-size: 44px",
            "@media (max-width: 48rem)",
            "overflow-wrap: anywhere");
        Assert.DoesNotContain("position: absolute", css, StringComparison.OrdinalIgnoreCase);
    }
}
