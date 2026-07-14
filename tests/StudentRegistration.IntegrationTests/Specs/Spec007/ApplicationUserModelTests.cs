using StudentRegistration.IdentityAccess.Domain;
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

    [Fact]
    public void Expired_lockout_starts_a_new_five_attempt_window()
    {
        var user = new ApplicationUser(
            Guid.NewGuid(),
            "student.demo",
            "STUDENT.DEMO",
            "AI2600001",
            "HASH",
            "STAMP");
        var firstWindow = new DateTime(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromMinutes(5);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            Assert.Equal(
                attempt == 5,
                user.RecordFailedAccess(firstWindow, 5, duration));
        }

        var originalLockoutEnd = user.LockoutEndUtc;
        Assert.True(user.RecordFailedAccess(firstWindow.AddMinutes(1), 5, duration));
        Assert.Equal(5, user.AccessFailedCount);
        Assert.Equal(originalLockoutEnd, user.LockoutEndUtc);

        var secondWindow = originalLockoutEnd!.Value.AddSeconds(1);
        Assert.False(user.RecordFailedAccess(secondWindow, 5, duration));
        Assert.Equal(1, user.AccessFailedCount);
        Assert.Null(user.LockoutEndUtc);

        for (var attempt = 2; attempt <= 5; attempt++)
        {
            Assert.Equal(
                attempt == 5,
                user.RecordFailedAccess(secondWindow, 5, duration));
        }

        Assert.Equal(secondWindow.Add(duration), user.LockoutEndUtc);
    }
}
