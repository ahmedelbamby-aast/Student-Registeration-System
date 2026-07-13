using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class TimeProviderUsageTests
{
    private static readonly string[] GovernedProjects =
    [
        "StudentRegistration.Client",
        "StudentRegistration.IdentityAccess",
        "StudentRegistration.Academics",
        "StudentRegistration.Scheduling",
        "StudentRegistration.Registration",
        "StudentRegistration.StaffAdministration"
    ];

    [Fact]
    public void API_composition_registers_the_single_authoritative_TimeProvider()
    {
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/TimeProviderRegistration.cs");
        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/ModuleRegistration.cs");

        RepositoryFiles.ContainsAll(
            registration,
            "AddAuthoritativeTime",
            "TimeProvider.System",
            "TryAddSingleton");
        Assert.Contains("AddAuthoritativeTime", composition, StringComparison.Ordinal);
    }

    [Fact]
    public void Client_and_business_modules_do_not_read_the_system_clock_directly()
    {
        string[] forbiddenReads =
        [
            "DateTime.Now",
            "DateTime.UtcNow",
            "DateTime.Today",
            "DateTimeOffset.Now",
            "DateTimeOffset.UtcNow"
        ];

        foreach (var project in GovernedProjects)
        {
            var root = RepositoryFiles.PathTo($"src/{project}");
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                         .Where(path => !IsGenerated(path)))
            {
                var source = File.ReadAllText(path);
                Assert.All(
                    forbiddenReads,
                    forbidden => Assert.DoesNotContain(
                        forbidden,
                        source,
                        StringComparison.Ordinal));
            }
        }
    }

    private static bool IsGenerated(string path) =>
        Path.GetRelativePath(RepositoryFiles.Root, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");
}
