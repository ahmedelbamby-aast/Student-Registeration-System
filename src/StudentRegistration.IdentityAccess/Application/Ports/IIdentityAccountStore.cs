using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application.Ports;

public interface IIdentityAccountStore
{
    Task<ApplicationUser?> FindStudentByUniversityIdAsync(
        string normalizedUniversityId,
        CancellationToken cancellationToken);

    Task<ApplicationUser?> FindByNormalizedUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken);

    Task<ApplicationUser?> FindByIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken);

    Task<Staff?> FindStaffAsync(Guid applicationUserId, CancellationToken cancellationToken);

    Task<bool> IsStudentActivatedAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetEffectiveRolesAsync(
        Guid applicationUserId,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task RecordAuthenticationFailureAsync(
        Guid? applicationUserId,
        string operation,
        CancellationToken cancellationToken);

    Task ResetAuthenticationFailuresAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken);

    Task RecordActivationFailureAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken);

    Task<bool> TryActivateAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime activatedAtUtc,
        CancellationToken cancellationToken);

    Task<bool> AddRecoveryChallengeAsync(
        AccountRecoveryChallenge challenge,
        CancellationToken cancellationToken);

    Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task RecordRecoveryFailureAsync(
        Guid challengeId,
        int maximumAttempts,
        CancellationToken cancellationToken);

    Task<bool> TryCompleteRecoveryAsync(
        Guid challengeId,
        byte[] expectedChallengeVersion,
        Guid applicationUserId,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime consumedAtUtc,
        int maximumAttempts,
        CancellationToken cancellationToken);

    Task<bool> ChangePasswordAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        CancellationToken cancellationToken);

    Task<bool> RotateSecurityStampAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newSecurityStamp,
        CancellationToken cancellationToken);
}
