using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class RoadmapEndpointContractTests
{
    [Fact]
    public void Student_roadmap_is_an_authoritative_read_projection()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("StudentRoadmapDto"),
            "ProgramCode", "Cohort", "CatalogueVersion", "Terms");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("RoadmapTermDto"),
            "RecommendedTerm", "Level", "Subjects");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("RoadmapSubjectDto"),
            "CourseId", "Code", "Title", "Credits", "Required", "CohortScope",
            "Status", "AutomaticRegistration", "Prerequisites", "MissingPrerequisites");

        var endpoint = Spec009ContractAssertions.Endpoint("GET", "/api/students/me/roadmap");
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == "AcademicProfile.ReadOwn");
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.StudentRoadmapDto");
        Spec009ContractAssertions.DeclaresResponse(endpoint, StatusCodes.Status404NotFound);
    }

    [Fact]
    public void Admin_can_create_a_version_based_editable_draft()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("CreateCatalogueDraftRequest"),
            "BasedOnVersionId", "Reason", "ClientRequestId");
        var endpoint = Spec009ContractAssertions.Endpoint("POST", "/api/admin/catalogue/drafts");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, "CreateCatalogueDraftRequest");
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.CatalogueDraftDto");
    }
}
