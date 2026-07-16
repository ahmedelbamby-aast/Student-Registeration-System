using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

internal static class IdentityPageComponentAssertions
{
    internal static string AssertCommon(
        string pagePath,
        string route,
        string formTestId,
        string busyGuard,
        params string[] required)
    {
        var source = RepositoryFiles.Read(pagePath);
        RepositoryFiles.ContainsAll(
            source,
            $"@page \"{route}\"",
            $"data-testid=\"{formTestId}\"",
            "@onsubmit:preventDefault",
            busyGuard,
            "disabled=",
            "AccessibleValidationSummary",
            "ApplyFailure",
            "SERVICE_UNAVAILABLE");
        RepositoryFiles.ContainsAll(source, required);
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        return source;
    }
}
