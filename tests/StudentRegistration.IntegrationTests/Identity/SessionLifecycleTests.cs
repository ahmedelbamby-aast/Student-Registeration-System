using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Identity;

public sealed class SessionLifecycleTests
{
    [Fact]
    public async Task Recovery_is_single_use_and_every_credential_change_rotates_security_state()
    {
        var fixture = new IdentityServiceTestFixture();
        var user = fixture.AddStudent(
            "202600003",
            "correct horse battery staple");
        var service = fixture.CreateSessionLifecycle();
        var initialStamp = user.SecurityStamp;

        var requested = await service.RequestRecoveryAsync("202600003");
        var proof = Assert.IsType<RecoveryProofDeliveryRequest>(fixture.Delivery.LastRequest).Proof;
        var completed = await service.CompleteRecoveryAsync(
            proof,
            "replacement credential value");
        var repeated = await service.CompleteRecoveryAsync(
            proof,
            "another replacement value");
        var recoveryStamp = user.SecurityStamp;
        var revoked = await service.RevokeAllSessionsAsync(user.Id);

        Assert.Equal(SessionLifecycleOutcome.RecoveryAccepted, requested.Outcome);
        Assert.Equal(SessionLifecycleOutcome.Completed, completed.Outcome);
        Assert.Equal(SessionLifecycleOutcome.ChallengeInvalid, repeated.Outcome);
        Assert.NotEqual(initialStamp, recoveryStamp);
        Assert.Equal(SessionLifecycleOutcome.Completed, revoked.Outcome);
        Assert.NotEqual(recoveryStamp, user.SecurityStamp);
    }

    [Fact]
    public void Session_lifecycle_delivers_no_proof_and_rotates_shared_security_state()
    {
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs");
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/Ports/IAccountRecoveryProofDelivery.cs");

        RepositoryFiles.ContainsAll(
            service,
            "RequestRecoveryAsync",
            "IAccountRecoveryProofDelivery",
            "DeliverAsync",
            "CompleteRecoveryAsync",
            "ChangePasswordAsync",
            "RevokeAllSessionsAsync",
            "RotateSecurityStampAsync",
            "RecoveryAccepted",
            "ChallengeInvalid",
            "TimeProvider");
        RepositoryFiles.ContainsAll(port, "RecoveryProofDeliveryRequest", "CancellationToken");
        Assert.DoesNotContain("return challengeToken", service, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DateTime.UtcNow", service, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Recovery_rejects_a_new_password_containing_the_username()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStaff(
            "lecturer.nadia",
            "current credential value",
            ["Lecturer"],
            displayName: "Dr Nadia Hassan");
        var service = fixture.CreateSessionLifecycle();

        await service.RequestRecoveryAsync("lecturer.nadia");
        var proof = Assert.IsType<RecoveryProofDeliveryRequest>(
            fixture.Delivery.LastRequest).Proof;
        var result = await service.CompleteRecoveryAsync(
            proof,
            "safe lecturer.nadia credential");

        Assert.Equal(SessionLifecycleOutcome.PasswordRejected, result.Outcome);
    }

    [Fact]
    public async Task Password_change_rejects_a_new_password_containing_the_display_name()
    {
        var fixture = new IdentityServiceTestFixture();
        var (user, _) = fixture.AddStaff(
            "ta.ibrahim",
            "current credential value",
            ["TeachingAssistant"],
            displayName: "Hassan Ibrahim");
        var service = fixture.CreateSessionLifecycle();

        var result = await service.ChangePasswordAsync(
            user.Id,
            "current credential value",
            "secure Hassan Ibrahim credential");

        Assert.Equal(SessionLifecycleOutcome.PasswordRejected, result.Outcome);
    }
}
