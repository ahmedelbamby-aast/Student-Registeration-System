using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

internal static class Spec003RouteComponentAssertions
{
    public static void AssertRoute(
        string routeId,
        string taskId,
        string pageName,
        string[] stateTokens,
        params string[] interactionTokens)
    {
        var page = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{pageName}.razor");
        var design = RepositoryFiles.Read(
            $"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");

        Assert.Contains($"{routeId}-COMP-{taskId}", design, StringComparison.Ordinal);
        Assert.Contains("\"focusOrder\"", design, StringComparison.Ordinal);
        Assert.Contains("\"state\": \"loading\"", design, StringComparison.Ordinal);
        Assert.Contains("\"state\": \"success\"", design, StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(page, stateTokens);
        RepositoryFiles.ContainsAll(page, interactionTokens);
        RepositoryFiles.ContainsAll(page, "aria-live", "tabindex=\"-1\"");
        Assert.DoesNotContain("Task.Run", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
    }
}
