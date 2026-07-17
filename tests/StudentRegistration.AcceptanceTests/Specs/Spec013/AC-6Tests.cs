using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_6Tests
{
    private const string EndpointPath =
        "src/StudentRegistration.Registration/Endpoints/Spec013Endpoints.cs";
    private const string ApplicationPath =
        "src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs";

    [Fact]
    public void Applying_an_option_for_an_old_plan_version_returns_plan_changed_without_a_second_mutation()
    {
        var endpoint = FutureSource(
            EndpointPath,
            "The recommended-option endpoint must map stale plans before AC-6 can pass.");
        var application = FutureSource(
            ApplicationPath,
            "FR-9 and FR-10 must be implemented before stale option application can pass.");

        RepositoryFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status409Conflict",
            "PLAN_CHANGED");
        RepositoryFiles.ContainsAll(
            application,
            "ExpectedPlanRowVersion",
            "PLAN_CHANGED",
            "ReplaceAsync");

        Assert.Equal(
            1,
            CountOccurrences(application, "ReplaceAsync"));
    }

    private static int CountOccurrences(
        string source,
        string value)
    {
        var count = 0;
        var index = 0;

        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(RepositoryFiles.Exists(path), message);
        return RepositoryFiles.Read(path);
    }
}
