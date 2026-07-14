using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class ApplicationUserModelTests
{
    [Fact]
    public void Application_user_is_the_plain_domain_security_and_concurrency_root()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/ApplicationUser.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class ApplicationUser",
            "public Guid Id { get;",
            "public string UserName { get;",
            "public string NormalizedUserName { get;",
            "public string? UniversityId { get;",
            "public string PasswordHash { get;",
            "public string SecurityStamp { get;",
            "public bool IsEnabled { get;",
            "public int AccessFailedCount { get;",
            "public DateTime? LockoutEndUtc { get;",
            "public byte[] Version { get;");
        Assert.DoesNotContain("IdentityUser", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", source, StringComparison.Ordinal);
    }
}
