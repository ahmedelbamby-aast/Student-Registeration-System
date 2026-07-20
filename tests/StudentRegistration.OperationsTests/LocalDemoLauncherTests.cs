using StudentRegistration.TestSupport;

namespace StudentRegistration.OperationsTests;

public sealed class LocalDemoLauncherTests
{
    [Fact]
    public void Launcher_reuses_the_completed_solution_build_for_migrations_and_seed()
    {
        var launcher = RepositoryFiles.Read("ops/scripts/Start-LocalDemo.ps1");

        Assert.Contains(
            "'ef', 'database', 'update',\n        '--no-build'",
            launcher.Replace("\r\n", "\n", StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Contains(
            "'--configuration', 'Release',\n        '--no-build'",
            launcher.Replace("\r\n", "\n", StringComparison.Ordinal),
            StringComparison.Ordinal);
    }
}
