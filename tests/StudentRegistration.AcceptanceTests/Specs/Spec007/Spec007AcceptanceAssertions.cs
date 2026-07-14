global using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

internal static class Spec007AcceptanceAssertions
{
    public static string Source(string relativePath, params string[] expected)
    {
        var source = RepositoryFiles.Read(relativePath);
        RepositoryFiles.ContainsAll(source, expected);
        return source;
    }
}
