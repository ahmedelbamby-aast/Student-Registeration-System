using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec012;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Validate_returns_complete_non_mutating_blocker_evidence()
    {
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("ResolutionActionDto"),
            "Action", "TargetGroupId", "Label", "Route");
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("ScheduleConflictParticipantDto"),
            "GroupId", "GroupCode", "CourseCode", "SubjectTitle",
            "StartLocal", "EndLocal");
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("ScheduleConflictDto"),
            "Code", "First", "Second", "DayOfWeek", "OverlapStartLocal",
            "OverlapEndLocal", "Message", "Actions");
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("PlanSelectionIssueDto"),
            "Code", "OfferingId", "GroupId", "GroupCode", "Message",
            "Blocking", "Actions");
        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("ValidationSnapshotDto"),
            "EvaluatedAtUtc", "AcademicContextVersion", "PolicyVersion",
            "CatalogueVersion", "OfferingVersions", "GroupVersions");

        var endpoint = Spec012ContractAssertions.Endpoint(
            "POST",
            "/api/student/terms/{termId}/registration-plan/validate");
        Spec012ContractAssertions.RequiresStudent(endpoint);
        Spec012ContractAssertions.HasNoClientOwnedIdentifier(endpoint);
        Spec012ContractAssertions.HasNoRequestBody(endpoint);
        Spec012ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "RegistrationPlanDto");
        Spec012ContractAssertions.DeclaresApiErrors(
            endpoint,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);

        Spec012ContractAssertions.ContractContains(
            "The request has no body",
            "fresh deterministic conflicts",
            "fixed 18/18 credit fields, sourced load reasons",
            "Validation is side-effect-free",
            "does not replace selections",
            "advance the plan rowversion",
            "reserve capacity, or allocate a seat",
            "changed, full, unpublished, closed, cancelled, paused",
            "blocking issue with change/remove actions",
            "no blocker is silently removed or overridden");
    }
}
