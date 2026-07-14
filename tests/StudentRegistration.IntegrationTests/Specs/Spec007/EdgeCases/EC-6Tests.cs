using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_6Tests
{
    [Fact]
    public void Lost_activation_response_retry_cannot_repeat_the_transition()
    {
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs");
        var activation = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/StudentActivation.cs");
        RepositoryFiles.ContainsAll(service, "TryActivateAsync", "ActivationFailed");
        RepositoryFiles.ContainsAll(activation, "ActivatedAtUtc", "TryConsume");
        Assert.Equal(
            1,
            service.Split("TryActivateAsync", StringSplitOptions.None).Length - 1);
    }
}
