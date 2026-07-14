using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Identity;

public sealed class StudentActivationConcurrencyTests
{
    [Fact]
    public async Task Ten_parallel_activation_attempts_complete_exactly_one_transition()
    {
        var fixture = new IdentityServiceTestFixture();
        var user = fixture.AddStudent(
            "202600002",
            "issued initial credential",
            activated: false);
        var originalStamp = user.SecurityStamp;
        var service = fixture.CreateStudentActivation();

        var attempts = Enumerable.Range(0, 10)
            .Select(_ => service.ActivateAsync(
                "202600002",
                "issued initial credential",
                "replacement credential value"))
            .ToArray();
        var results = await Task.WhenAll(attempts);

        Assert.Single(results, result => result.Succeeded);
        Assert.Equal(9, results.Count(result => !result.Succeeded));
        Assert.True(await fixture.Store.IsStudentActivatedAsync(user.Id, default));
        Assert.NotEqual(originalStamp, user.SecurityStamp);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            fixture.Hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                "replacement credential value"));
    }

    [Fact]
    public void Activation_is_one_conditional_hash_replacement_and_security_rotation()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs");

        RepositoryFiles.ContainsAll(
            source,
            "StudentActivationService",
            "NormalizeUniversityId",
            "VerifyHashedPassword",
            "HashPassword",
            "TryActivateAsync",
            "newSecurityStamp",
            "_dummyHash",
            "ActivationFailed",
            "CancellationToken");
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unknown_activation_executes_the_generic_verification_path_without_creating_an_identity()
    {
        var fixture = new IdentityServiceTestFixture();
        var service = fixture.CreateStudentActivation();

        var result = await service.ActivateAsync(
            "209999999",
            "unrecognized initial credential",
            "replacement credential value");

        Assert.False(result.Succeeded);
        Assert.Equal(0, fixture.Store.UserCount);
    }

    [Fact]
    public async Task Activation_rejects_a_new_password_containing_the_university_id()
    {
        var fixture = new IdentityServiceTestFixture();
        var user = fixture.AddStudent(
            "202600009",
            "issued initial credential",
            activated: false);
        var service = fixture.CreateStudentActivation();

        var result = await service.ActivateAsync(
            "202600009",
            "issued initial credential",
            "safe-202600009-credential");

        Assert.Equal(AuthenticationOutcome.PasswordRejected, result.Outcome);
        Assert.False(await fixture.Store.IsStudentActivatedAsync(user.Id, default));
    }
}
