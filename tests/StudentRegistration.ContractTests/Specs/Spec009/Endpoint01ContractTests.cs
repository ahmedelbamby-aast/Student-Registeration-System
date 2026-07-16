using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Program_list_is_bounded_authorized_and_provenance_safe()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("ProgramAdminDto"),
            "Id", "Code", "DisplayName", "Active", "Provenance");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("CatalogueFieldProvenanceDto"),
            "SourceReference", "AccessedOn", "SourceKind", "SyntheticFields");

        var endpoint = Spec009ContractAssertions.Endpoint("GET", "/api/admin/programs");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.DeclaresPage(endpoint, StatusCodes.Status200OK, "ProgramAdminDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec009ContractAssertions.ContractContains(
            "Default sort `code,id`",
            "maximum size 100",
            "`400 PAGE_SIZE_INVALID`");
    }
}
