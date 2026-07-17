using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_5Tests
{
    private const string ApplicationPath =
        "src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs";

    [Fact]
    public void Applying_a_recommendation_revalidates_the_captured_group_state_without_reserving_a_seat()
    {
        var application = FutureSource(
            ApplicationPath,
            "FR-8 must be implemented before recommendation revalidation can pass.");

        RepositoryFiles.ContainsAll(
            application,
            "GroupVersions",
            "STALE_INPUT",
            "ReplaceAsync");
        Assert.DoesNotContain("Reserve", application, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Reservation", application, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EnrolledCount++", application, StringComparison.Ordinal);
        Assert.DoesNotContain("RemainingSeats--", application, StringComparison.Ordinal);
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(RepositoryFiles.Exists(path), message);
        return RepositoryFiles.Read(path);
    }
}
