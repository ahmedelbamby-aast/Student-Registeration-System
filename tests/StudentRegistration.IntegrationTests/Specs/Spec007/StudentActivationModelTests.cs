using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class StudentActivationModelTests
{
    [Fact]
    public void Activation_targets_identity_user_and_contains_only_single_use_state()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/StudentActivation.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class StudentActivation",
            "public Guid ApplicationUserId { get;",
            "public DateTime ProvisionedAtUtc { get;",
            "public DateTime? ActivatedAtUtc { get;",
            "public int FailedAttemptCount { get;",
            "public byte[] Version { get;",
            "TryConsume");
        Assert.DoesNotContain("StudentId", source, StringComparison.Ordinal);
        Assert.DoesNotContain("InitialPassword", source, StringComparison.Ordinal);
        Assert.DoesNotContain("InitialPin", source, StringComparison.Ordinal);
    }
}
