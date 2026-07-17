namespace StudentRegistration.ApplicationTests.Specs.Spec013;

public sealed class Endpoint02BehaviorTests
{
    private const string EndpointPath =
        "src/StudentRegistration.Registration/Endpoints/Spec013Endpoints.cs";
    private const string ApplicationPath =
        "src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs";

    [Fact]
    public void Apply_endpoint_preserves_private_owner_semantics_and_stable_codes()
    {
        var endpoint = FutureSource(
            EndpointPath,
            "T056 must deliver the future recommended-option handler.");

        Spec013BehaviorFiles.ContainsAll(
            endpoint,
            "/api/student/terms/{termId}/registration-plan/recommended-option",
            "ApplyScheduleOptionRequest",
            "RecommendationApplicationService",
            "ClaimTypes.NameIdentifier",
            "RequireAuthorization",
            "Student",
            "INVALID_OPTION_TOKEN",
            "REGISTRATION_CONTEXT_NOT_FOUND",
            "OPTION_EXPIRED",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "RATE_LIMITED",
            "RECOMMENDATIONS_UNAVAILABLE",
            "INTERNAL_ERROR");
        Assert.DoesNotContain("TOKEN_OWNER_MISMATCH", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("STUDENT_NOT_FOUND", endpoint, StringComparison.Ordinal);
    }

    [Fact]
    public void Option_protection_is_purpose_bound_expiring_and_replica_safe()
    {
        var application = FutureSource(
            ApplicationPath,
            "T049/T051/T053 must deliver the future option application service.");

        Spec013BehaviorFiles.ContainsAll(
            application,
            "IDataProtectionProvider",
            "Registration.ScheduleOption.v1",
            "TimeProvider",
            "TimeSpan.FromMinutes(10)",
            "OptionToken",
            "RequestCorrelationId",
            "ExpectedPlanRowVersion");
        Assert.DoesNotContain("MemoryCache", application, StringComparison.Ordinal);
        Assert.DoesNotContain("Dictionary<string, ScheduleOption", application, StringComparison.Ordinal);
        Assert.DoesNotContain("ScheduleOptionEntity", application, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_validates_owner_expiry_plan_and_dependencies_before_one_atomic_mutation()
    {
        var application = FutureSource(
            ApplicationPath,
            "T049/T051/T053 must deliver atomic recommendation application.");

        Spec013BehaviorFiles.ContainsAll(
            application,
            "INVALID_OPTION_TOKEN",
            "OPTION_EXPIRED",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "AcademicContextVersion",
            "CatalogueVersion",
            "PolicySetId",
            "PolicyVersion",
            "OfferingVersions",
            "GroupVersions",
            "OptimizerConfigurationVersion",
            "ReplaceAsync");
        Assert.DoesNotContain("Reserve", application, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EnrolledCount++", application, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChangesAsync", application, StringComparison.Ordinal);
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(Spec013BehaviorFiles.Exists(path), message);
        return Spec013BehaviorFiles.Read(path);
    }
}
