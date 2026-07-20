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
            "AccessibleValidationSummary",
            "ApplyFailure",
            "SERVICE_UNAVAILABLE");
        Assert.True(
            source.Contains("disabled=", StringComparison.OrdinalIgnoreCase),
            $"{pagePath} must expose a disabled/busy control state.");
        RepositoryFiles.ContainsAll(source, required);
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        return source;
    }
}
