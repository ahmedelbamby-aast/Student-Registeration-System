using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec013;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Apply_request_contains_only_the_protected_option_and_concurrency_inputs()
    {
        var request = Spec013ContractAssertions.ContractType(
            "ApplyScheduleOptionRequest");
        Spec013ContractAssertions.HasExactProperties(
            request,
            "OptionToken", "ExpectedPlanRowVersion", "RequestCorrelationId");
        Spec013ContractAssertions.ExcludesProperties(
            request,
            "StudentId", "PlanId", "TermId", "GroupIds", "Groups",
            "CatalogueVersion", "PolicyVersion");
    }

    [Fact]
    public void Put_is_student_self_scoped_atomic_apply_with_safe_failure_matrix()
    {
        var endpoint = Spec013ContractAssertions.Endpoint(
            "PUT",
            "/api/student/terms/{termId}/registration-plan/recommended-option");
        Spec013ContractAssertions.RequiresStudent(endpoint);
        Spec013ContractAssertions.HasNoClientOwnedIdentifier(endpoint);
        Spec013ContractAssertions.HasRequestBody(
            endpoint,
            "ApplyScheduleOptionRequest");
        Spec013ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "RegistrationPlanDto");
        Spec013ContractAssertions.DeclaresApiErrors(
            endpoint,
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict,
            StatusCodes.Status429TooManyRequests,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);

        Spec013ContractAssertions.ContractContains(
            "purpose `Registration.ScheduleOption.v1`",
            "valid for 10 minutes",
            "option lookup, sticky sessions, and a durable option table are prohibited",
            "replace the complete existing selected group set",
            "`200 RegistrationPlanDto`",
            "`400 INVALID_OPTION_TOKEN`",
            "`404 REGISTRATION_CONTEXT_NOT_FOUND`",
            "`409 OPTION_EXPIRED`",
            "`409 PLAN_CHANGED`",
            "`409 STALE_INPUT`",
            "`429 RATE_LIMITED`",
            "`503 RECOMMENDATIONS_UNAVAILABLE`",
            "`500 INTERNAL_ERROR`");
    }
}
