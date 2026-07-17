namespace StudentRegistration.ApplicationTests.Specs.Spec017;

internal static class Spec017BehaviorFiles
{
    private static readonly string Root = FindRoot();

    public static string FutureSource(string relativePath, string expectedRedMessage)
    {
        var path = Path.Combine(Root, Normalize(relativePath));
        Assert.True(File.Exists(path), expectedRedMessage);
        return File.ReadAllText(path);
    }

    public static void ContainsAll(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.Contains(fragment, value, StringComparison.Ordinal);
        }
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StudentRegistration.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);
}
