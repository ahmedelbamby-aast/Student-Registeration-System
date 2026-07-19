using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Expired_session_keeps_only_safe_plan_context_and_requires_authoritative_revalidation()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-04.md");
        var schedule = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor");
        var review = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor");
        var identity = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs");

        RepositoryFiles.ContainsAll(
            design,
            "SESSION_EXPIRED",
            "Only safe plan/reference ID retained",
            "Sign in again",
            "Refetch and revalidate before editing");
        RepositoryFiles.ContainsAll(
            schedule,
            "SESSION_EXPIRED",
            "SessionExpiredState",
            "GetRegistrationPlanAsync",
            "ReplaceRegistrationPlanAsync");
        RepositoryFiles.ContainsAll(
            review,
            "SESSION_EXPIRED",
            "GetRegistrationPlanAsync",
            "LookupRegistrationAsync");
        RepositoryFiles.ContainsAll(identity, "session-expired", "reauthenticate");

        Assert.DoesNotContain("localStorage", schedule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", schedule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cached plan success", schedule, StringComparison.OrdinalIgnoreCase);
    }
}
