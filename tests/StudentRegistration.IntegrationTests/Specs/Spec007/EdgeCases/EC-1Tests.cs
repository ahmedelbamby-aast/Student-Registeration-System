using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Already_activated_identity_receives_the_same_safe_activation_result()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs");
        RepositoryFiles.ContainsAll(source, "TryActivateAsync", "ActivationFailed");
        Assert.DoesNotContain("AlreadyActivated", source, StringComparison.Ordinal);
    }
}
