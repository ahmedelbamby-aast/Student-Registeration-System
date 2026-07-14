namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_7Tests
{
    [Fact]
    public void Recovery_password_change_and_revoke_all_rotate_shared_state()
    {
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs",
            "CompleteRecoveryAsync",
            "ChangePasswordAsync",
            "RevokeAllSessionsAsync",
            "RotateSecurityStampAsync");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs",
            "SecurityStamp",
            "IsRowVersion");
    }
}
