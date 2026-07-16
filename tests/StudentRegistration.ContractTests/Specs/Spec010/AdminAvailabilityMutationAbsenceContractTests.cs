namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class AdminAvailabilityMutationAbsenceContractTests
{
    [Fact]
    public void Admin_availability_mutation_routes_and_contracts_are_absent()
    {
        var routes = Spec010ContractAssertions.Endpoints();
        Assert.Equal(14, routes.Count);
        var forbidden = new[]
        {
            "PUT",
            "POST",
            "PATCH",
            "DELETE",
        };

        Assert.DoesNotContain(
            routes,
            endpoint =>
                endpoint.RoutePattern.RawText ==
                    "/api/admin/staff/{staffId}/terms/{termId}/availability"
                && endpoint.Metadata
                    .GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()
                    ?.HttpMethods.Any(forbidden.Contains) == true);
        Assert.Null(Spec010ContractAssertions.OptionalContractType(
            "AdminAvailabilityMutationRequest"));
        Assert.Null(Spec010ContractAssertions.OptionalContractType(
            "OverrideStaffAvailabilityRequest"));
        Assert.NotNull(Spec010ContractAssertions.Endpoint(
            "GET",
            "/api/admin/staff-availability"));
        Spec010ContractAssertions.ContractContains(
            "There is no Admin route, permission, request DTO, command handler",
            "it does not copy, replace, or mutate the staff-owned range set",
            "Availability.ManageOwn");
    }
}
