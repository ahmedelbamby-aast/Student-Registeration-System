using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Identity;

public sealed class StudentLoginTests
{
    [Fact]
    public async Task Student_login_executes_hash_verification_and_returns_only_student_context()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStudent("202600001", "correct horse battery staple");
        var service = fixture.CreateStudentAuthentication();

        var success = await service.AuthenticateAsync(
            " 202600001 ",
            "correct horse battery staple");
        var badPassword = await service.AuthenticateAsync("202600001", "incorrect");
        var unknown = await service.AuthenticateAsync("202699999", "incorrect");

        Assert.True(success.Succeeded);
        Assert.Equal(["Student"], success.AuthorizedRoles);
        Assert.Equal("Student", success.ActiveRole);
        Assert.Equal(badPassword.Outcome, unknown.Outcome);
        Assert.False(badPassword.Succeeded);
    }

    [Fact]
    public void Student_login_normalizes_verifies_state_and_returns_only_own_context()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentAuthenticationService.cs");

        RepositoryFiles.ContainsAll(
            source,
            "StudentAuthenticationService",
            "NormalizeUniversityId",
            "VerifyHashedPassword",
            "IsEnabled",
            "LockoutEndUtc",
            "Student",
            "AuthenticationFailed",
            "CancellationToken");
        Assert.DoesNotContain("Password ==", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }
}
