using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class AdminSecurityGuardModelTests
{
    [Fact]
    public void Admin_guard_is_the_singleton_rowversion_serialization_point()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/AdminSecurityGuard.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class AdminSecurityGuard",
            "public const int SingletonId = 1",
            "public int Id { get;",
            "public byte[] Version { get;");
        Assert.DoesNotContain("static byte[]", source, StringComparison.Ordinal);
        Assert.DoesNotContain("lock (", source, StringComparison.Ordinal);
    }
}
