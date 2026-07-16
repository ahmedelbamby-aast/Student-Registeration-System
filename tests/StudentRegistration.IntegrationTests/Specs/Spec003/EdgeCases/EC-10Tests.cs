using Microsoft.AspNetCore.Identity;
using StudentRegistration.IntegrationTests.Identity;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_10Tests
{
    [Fact]
    public async Task Rapid_activation_attempts_show_one_pending_command_and_server_commits_one_result()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor");
        RepositoryFiles.ContainsAll(
            page,
            "if (IsSubmitting || !Validate())",
            "IsSubmitting = true;",
            "disabled=\"@IsSubmitting\"",
            "Activating…",
            "IdentityFeedback",
            "ClearSecrets()");
        Assert.Equal(1, Count(page, "ActivateStudentAsync("));

        var fixture = new IdentityServiceTestFixture();
        var user = fixture.AddStudent(
            "202600042",
            "issued initial credential",
            activated: false);
        var service = fixture.CreateStudentActivation();
        var attempts = Enumerable.Range(0, 10)
            .Select(_ => service.ActivateAsync(
                "202600042",
                "issued initial credential",
                "replacement credential value"))
            .ToArray();

        var results = await Task.WhenAll(attempts);

        Assert.Single(results, result => result.Succeeded);
        Assert.Equal(9, results.Count(result => !result.Succeeded));
        Assert.True(await fixture.Store.IsStudentActivatedAsync(user.Id, default));
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            fixture.Hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                "replacement credential value"));
    }

    private static int Count(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
