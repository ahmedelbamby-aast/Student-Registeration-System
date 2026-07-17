using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class TermAdministrationPageContributorTests
{
    [Fact]
    public void Adm_02_contribution_freezes_reasoned_versioned_owner_commands()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-02",
            "SPEC-008",
            "TermAdministrationPage.razor",
            "/admin/terms",
            "spec017-adm02/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "GET /api/admin/terms",
            "POST /api/admin/terms",
            "PUT /api/admin/terms/{termId}",
            "POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish",
            "AcademicTerms.Manage",
            "antiforgery",
            "expected term and child rowversions",
            "reason/source",
            "atomic audit",
            "VALIDATION_ERROR",
            "WINDOW_OVERLAP",
            "STALE_VERSION",
            "IDEMPOTENCY_KEY_REUSED");
        RepositoryFiles.ContainsAll(
            page,
            "AcademicApi.ListAdminTermsAsync",
            "AcademicApi.CreateAdminTermAsync",
            "AcademicApi.UpdateAdminTermAsync",
            "AcademicApi.PublishAdminRegistrationWindowAsync",
            "RefetchContestedTermAsync",
            "actor and timestamp remain in server audit");
    }

    [Fact]
    public void Adm_02_adds_no_generic_or_bypass_action()
    {
        var contract = ContributorContractAssertions.Contract("ADM-02");
        RepositoryFiles.ContainsAll(
            contract,
            "does not invent a generic `AdminConfirmationService`",
            "No browser-authored current term/time",
            "enrollment correction",
            "capacity override",
            "Success appears only after the SPEC-008 server accepts");
        Assert.DoesNotContain("Canonical page owner:** SPEC-017", contract);
    }
}

internal static class ContributorContractAssertions
{
    public static (string Contract, string Page) Load(
        string route,
        string implementationOwner,
        string pageName,
        string routeTemplate,
        string contractVersion)
    {
        var contract = Contract(route);
        var design = Normalize(RepositoryFiles.Read(
            $"specs/003-ux-storyboard-accessibility/design/pages/{route}.md"));
        var pagePath = $"src/StudentRegistration.Client/Pages/{pageName}";
        var page = RepositoryFiles.Read(pagePath);
        if (RepositoryFiles.Exists(pagePath + ".cs"))
        {
            page += RepositoryFiles.Read(pagePath + ".cs");
        }
        page = Normalize(page);

        RepositoryFiles.ContainsAll(
            contract,
            contractVersion,
            $"**Route:** `{routeTemplate}`",
            $"**Canonical page owner:** {implementationOwner}",
            $"**Canonical page:** `{pageName}`",
            "does not own or edit the canonical Razor page",
            "design-only");
        RepositoryFiles.ContainsAll(
            design,
            $"\"routeTemplate\": \"{routeTemplate}\"",
            $"\"pageName\": \"{pageName}\"",
            $"\"implementationOwnerSpec\": \"{implementationOwner}\"",
            "\"SPEC-017\"",
            "\"readinessState\": \"design-only\"");
        Assert.Contains($"@page \"{routeTemplate}\"", page);
        return (contract, page);
    }

    public static string Contract(string route) => Normalize(RepositoryFiles.Read(
        $"specs/017-admin-operations-audit-reporting/contracts/routes/{route}.md"));

    private static string Normalize(string value) => string.Join(
        " ",
        value.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
