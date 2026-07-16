using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Offering_detail_exposes_state_groups_and_version_without_EF_shape()
    {
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("CourseOfferingDto"),
            "Id", "TermId", "CourseId", "CourseCode", "CourseTitle", "State",
            "Groups", "RowVersion");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "GET",
            "/api/offerings/{offeringId}");
        Spec010ContractAssertions.RequiresOfferingDetailsRead(endpoint);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.CourseOfferingDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, false);
        Spec010ContractAssertions.ContractContains(
            "Student callers receive only Published data",
            "Catalogue.ReadAvailable",
            "Offerings.Manage",
            "OFFERING_NOT_FOUND",
            "They do not evaluate SPEC-011",
            "Every non-success response body is the shared `ApiError`");
    }
}
