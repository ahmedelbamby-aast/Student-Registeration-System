using System.Text.RegularExpressions;

namespace StudentRegistration.TestSupport;

public static partial class RepositoryFiles
{
    private static readonly Lazy<string> RootDirectory = new(FindRoot);

    public static string Root => RootDirectory.Value;

    public static string PathTo(string relativePath) =>
        Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    public static bool Exists(string relativePath) => File.Exists(PathTo(relativePath));

    public static string Read(string relativePath)
    {
        var path = PathTo(relativePath);
        Assert.True(File.Exists(path), $"Required repository artifact does not exist: {relativePath}");
        return File.ReadAllText(path);
    }

    public static string Section(string markdown, string heading)
    {
        var match = Regex.Match(
            markdown,
            $@"(?ms)^##\s+{Regex.Escape(heading)}\s*\r?\n(?<body>.*?)(?=^##\s+|\z)");

        Assert.True(match.Success, $"Markdown section was not found: {heading}");
        return match.Groups["body"].Value;
    }

    public static void ContainsAll(string text, params string[] expectedValues)
    {
        foreach (var expected in expectedValues)
        {
            Assert.Contains(expected, text, StringComparison.Ordinal);
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, ".specify")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from {AppContext.BaseDirectory}.");
    }
}
