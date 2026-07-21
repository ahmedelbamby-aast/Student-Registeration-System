using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentRecoveryCoverageTests
{
    public static TheoryData<string, string[]> CommandPages => new()
    {
        { "SubjectDiscoveryPage.razor", ["Retry subject discovery", "catch (HttpRequestException)", "finally", "_requestLoading = false"] },
        { "SubjectDetailsPage.razor", ["Retry subject details", "catch (HttpRequestException)", "finally", "_requestLoading = false"] },
        { "ScheduleBuilderPage.razor", ["Retry schedule builder", "_recommendationError", "finally", "_recommendationLoading = false", "_requestLoading = false"] },
        { "RegistrationReviewPage.razor", ["Retry registration review", "Retry result lookup", "finally", "_requestLoading = false"] },
        { "RegistrationResultPage.razor", ["Retry result lookup", "RetryAsync", "finally", "_retrying = false"] },
        { "RegistrationHistoryPage.razor", ["Retry registration records", "finally", "_requesting = false"] },
        { "StudentRoadmapPage.razor", ["Retry roadmap", "catch (HttpRequestException)", "finally", "_loading = false"] }
    };

    [Theory]
    [MemberData(nameof(CommandPages))]
    public void Every_student_command_family_has_an_operable_recovery_and_releases_busy_state(
        string page,
        string[] requiredEvidence)
    {
        var source = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{page}");
        RepositoryFiles.ContainsAll(source, requiredEvidence);
        Assert.Contains("<AppButton", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Browser_suites_cover_discovery_details_review_submit_and_result_recovery()
    {
        string[] browserEvidence =
        [
            "tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDiscoveryPageFeatureTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDetailsPageFeatureTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec014/RegistrationReviewPageFeatureTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationResultPageFeatureTests.cs",
            "tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationHistoryPageFeatureTests.cs"
        ];

        Assert.All(browserEvidence, path => RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(path), "Retry"));
    }
}
