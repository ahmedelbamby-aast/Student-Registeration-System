using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint09ContractTests
{
    [Fact]
    public void Policy_list_is_bounded_and_keeps_rule_source_classification()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("PolicySetAdminDto"),
            "Id", "Scope", "Version", "State", "RowVersion", "Rules");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("PolicyRuleAdminDto"),
            "Id", "Code", "ValueType", "Value", "EffectiveFromUtc",
            "EffectiveToUtc", "SourceReference", "SourceKind");
        var endpoint = Spec009ContractAssertions.Endpoint("GET", "/api/admin/policies");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "PolicySetAdminDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec009ContractAssertions.ContractContains(
            "effective-date filters",
            "effectiveFromUtc-desc,id",
            "each rule includes source reference and source kind");
    }
}
