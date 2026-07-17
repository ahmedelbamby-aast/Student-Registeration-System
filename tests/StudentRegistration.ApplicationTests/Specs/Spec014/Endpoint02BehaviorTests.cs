namespace StudentRegistration.ApplicationTests.Specs.Spec014;

public sealed class Endpoint02BehaviorTests
{
    private const string EndpointPath =
        "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs";

    [Fact]
    public void Get_handler_authorizes_before_owner_term_result_or_version_lookup()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "/api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
            "ClaimTypes.NameIdentifier",
            "Student",
            "Registration.SubmitOwn",
            "RequireAuthorization",
            "REQUEST_NOT_FOUND");
    }

    [Fact]
    public void Get_handler_maps_final_processing_and_privacy_safe_not_found_outcomes()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status200OK",
            "StatusCodes.Status202Accepted",
            "StatusCodes.Status404NotFound",
            "RegistrationInProgressResponse",
            "REQUEST_NOT_FOUND");
        Assert.DoesNotContain("OWNER_MISMATCH", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("STUDENT_NOT_FOUND", endpoint, StringComparison.Ordinal);
    }

    [Fact]
    public void Get_handler_uses_the_authenticated_student_term_and_request_scope_only()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "termId",
            "clientRequestId",
            "ClaimTypes.NameIdentifier",
            "CancellationToken");
        Assert.DoesNotContain("studentId,", endpoint, StringComparison.OrdinalIgnoreCase);
    }

    private static string FutureEndpointSource()
    {
        Assert.True(
            Spec014BehaviorFiles.Exists(EndpointPath),
            "Expected red for T029: Spec014Endpoints/GET handler is intentionally absent until T110.");
        return Spec014BehaviorFiles.Read(EndpointPath);
    }
}
