using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint06ContractTests
{
    [Fact]
    public void Import_status_bounds_safe_errors_and_excludes_raw_content()
    {
        var dto = Spec009ContractAssertions.ContractType("ImportBatchDto");
        Spec009ContractAssertions.HasExactProperties(
            dto,
            "Id", "DraftId", "State", "Source", "AccessedOn", "ContentHash",
            "SyntheticFieldCount", "RowVersion", "Errors", "TotalErrorCount",
            "HasMoreErrors", "PublishedVersionId");
        Spec009ContractAssertions.Excludes(dto, "RawRows", "RawContent", "FileBytes");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "GET",
            "/api/admin/catalogue/imports/{importId}");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.ImportBatchDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, false);
        Spec009ContractAssertions.ContractContains(
            "At most 100 errors",
            "continuation state",
            "Raw imported rows/content are never returned");
    }
}
