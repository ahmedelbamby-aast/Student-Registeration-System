using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class AccountRecoveryChallengeModelTests
{
    [Fact]
    public void Recovery_challenge_is_hashed_expiring_attempt_bounded_and_single_use()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/AccountRecoveryChallenge.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class AccountRecoveryChallenge",
            "public string TokenHash { get;",
            "public string DeliveryReferenceHash { get;",
            "public DateTime ExpiresAtUtc { get;",
            "public int FailedAttemptCount { get;",
            "public DateTime? ConsumedAtUtc { get;",
            "public byte[] Version { get;",
            "TryConsume");
        Assert.DoesNotContain("public string Token {", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RecoveryProof", source, StringComparison.Ordinal);
    }
}
