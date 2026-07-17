using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class AdminDashboardPageComponentTests
{
    [Fact]
    public void Adm_01_declares_all_reviewed_states_and_refresh_controls_without_zero_fill()
    {
        const string path = "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor";
        Assert.True(
            RepositoryFiles.Exists(path),
            "Expected-red for T073: ADM-01 component states are deferred to T074.");
        var page = RepositoryFiles.Read(path);

        RepositoryFiles.ContainsAll(
            page,
            "ADM-01-COMP-STATE-LOADING",
            "ADM-01-COMP-STATE-SUCCESS",
            "ADM-01-COMP-STATE-UNAUTHORIZED",
            "ADM-01-COMP-STATE-SESSION-EXPIRED",
            "ADM-01-COMP-STATE-STALE",
            "ADM-01-COMP-STATE-SERVICE-ERROR",
            "ADM-01-COMP-STATE-OFFLINE",
            "Pause refresh",
            "Resume refresh",
            "Refresh metrics",
            "Refresh paused",
            "Live metrics",
            "Stale metrics",
            "Degraded metrics",
            "Not reported by the server",
            "ObservedAtUtc");
        Assert.DoesNotContain("?? 0", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DefaultIfEmpty(0", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Value = 0", page, StringComparison.Ordinal);
    }
}
