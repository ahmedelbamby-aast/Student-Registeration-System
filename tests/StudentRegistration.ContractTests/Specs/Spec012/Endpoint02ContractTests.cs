using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec012;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Put_is_complete_versioned_atomic_replacement()
    {
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("RegistrationPlanMutationRequest"),
            "ExpectedPlanRowVersion", "SelectedGroupIds");
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("StaleRegistrationPlanResponse"),
            "Error", "CurrentPlan");

        var endpoint = Spec012ContractAssertions.Endpoint(
            "PUT",
            "/api/student/terms/{termId}/registration-plan");
        Spec012ContractAssertions.RequiresStudent(endpoint);
        Spec012ContractAssertions.HasNoClientOwnedIdentifier(endpoint);
        Spec012ContractAssertions.HasRequestBody(
            endpoint,
            "RegistrationPlanMutationRequest");
        Spec012ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "RegistrationPlanDto");
        Spec012ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status409Conflict,
            "StaleRegistrationPlanResponse");
        Spec012ContractAssertions.DeclaresApiErrors(
            endpoint,
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);

        Spec012ContractAssertions.ContractContains(
            "`selectedGroupIds` is the complete desired selection",
            "`expectedPlanRowVersion` is required and opaque",
            "more than one group for an offering",
            "one atomic operation",
            "`409 DUPLICATE_OFFERING_SELECTION`",
            "Authorized stale plan rowversion",
            "only the owner's current plan",
            "fixed 18/18 credit fields, and sourced load reasons",
            "no partial replacement is observable",
            "the update allocates no seat");
    }
}
