using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_17Tests
{
    [Fact]
    public void Safe_system_status_covers_every_approved_outcome_without_diagnostics()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor");
        var journeys = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Routes/SystemStatusPageJourneyTests.cs");

        RepositoryFiles.ContainsAll(
            page,
            "Systems available",
            "Service degraded",
            "Service unavailable",
            "Access unavailable",
            "Page not found",
            "Session expired",
            "Maintenance in progress",
            "You are offline",
            "Unexpected error",
            "SafeReference",
            "Open public gateway",
            "Student login",
            "Staff login");
        RepositoryFiles.ContainsAll(
            journeys,
            "HealthSummaryStatus.Healthy",
            "HealthSummaryStatus.Degraded",
            "HealthSummaryStatus.Unhealthy",
            "[InlineData(\"403\"",
            "[InlineData(\"404\"",
            "[InlineData(\"expired\"",
            "[InlineData(\"maintenance\"",
            "[InlineData(\"offline\"",
            "[InlineData(\"unexpected\"");
        Assert.DoesNotContain("Exception.Message", page, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionString", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SqlException", page, StringComparison.Ordinal);
    }
}
