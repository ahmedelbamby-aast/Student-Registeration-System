using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application;

public enum SessionLifecycleOutcome
{
    Completed,
    RecoveryAccepted,
    ChallengeInvalid,
    CurrentPasswordInvalid,
    PasswordRejected,
    InvalidRoleConfiguration,
    AuthenticationFailed
}

public sealed record SessionLifecycleResult(
    SessionLifecycleOutcome Outcome,
    AuthenticationResult? Session = null)
{
    public static SessionLifecycleResult Completed(AuthenticationResult? session = null) =>
        new(SessionLifecycleOutcome.Completed, session);
}

public sealed class SessionLifecycleService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(60);
    private readonly IIdentityAccountStore _store;
    private readonly IAccountRecoveryProofDelivery _proofDelivery;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IIdentityPasswordValidator _passwordValidator;
    private readonly IdentitySecurityOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly IdentityAbuseControl? _abuseControl;

    public SessionLifecycleService(
        IIdentityAccountStore store,
        IAccountRecoveryProofDelivery proofDelivery,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IIdentityPasswordValidator passwordValidator,
        IdentitySecurityOptions options,
        TimeProvider timeProvider,
        IdentityAbuseControl? abuseControl = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _proofDelivery = proofDelivery ?? throw new ArgumentNullException(nameof(proofDelivery));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _abuseControl = abuseControl;
    }

    public Task<SessionLifecycleResult> RequestRecoveryAsync(
        string universityIdOrUserName,
        CancellationToken cancellationToken = default) =>
        RequestRecoveryAsync(
            universityIdOrUserName,
            networkScope: null,
            cancellationToken);

    public async Task<SessionLifecycleResult> RequestRecoveryAsync(
        string universityIdOrUserName,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var normalized = IdentityTextNormalizer.NormalizeUserName(universityIdOrUserName);
        if (_abuseControl is not null)
        {
            var decision = await _abuseControl.RecordFailureAsync(
                "recovery",
                normalized,
                networkScope,
                cancellationToken);
            if (decision.IsRateLimited)
            {
                return new SessionLifecycleResult(SessionLifecycleOutcome.RecoveryAccepted);
            }
        }

        var user = await _store.FindByNormalizedUserNameAsync(normalized, cancellationToken)
            ?? await _store.FindStudentByUniversityIdAsync(normalized, cancellationToken);

        if (user is null || !user.IsEnabled)
        {
            return new SessionLifecycleResult(SessionLifecycleOutcome.RecoveryAccepted);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var expiresAtUtc = utcNow.Add(_options.ProofLifetime);
        var proof = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var delivery = await _proofDelivery.DeliverAsync(
            new RecoveryProofDeliveryRequest(
                user.Id,
                user.UserName,
                proof,
                expiresAtUtc),
            cancellationToken);

        if (delivery.Accepted)
        {
            var challenge = new AccountRecoveryChallenge(
                Guid.NewGuid(),
                user.Id,
                HashOpaque(proof),
                HashOpaque(delivery.DeliveryReference),
                expiresAtUtc);
            await _store.AddRecoveryChallengeAsync(challenge, cancellationToken);
        }

        return new SessionLifecycleResult(SessionLifecycleOutcome.RecoveryAccepted);
    }

    public Task<SessionLifecycleResult> CompleteRecoveryAsync(
        string challengeToken,
        string newPassword,
        CancellationToken cancellationToken = default) =>
        CompleteRecoveryAsync(
            challengeToken,
            newPassword,
            networkScope: null,
            cancellationToken);

    public async Task<SessionLifecycleResult> CompleteRecoveryAsync(
        string challengeToken,
        string newPassword,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return new SessionLifecycleResult(SessionLifecycleOutcome.ChallengeInvalid);
        }

        var sharedBlocked = _abuseControl is not null &&
            await _abuseControl.IsBlockedAsync(
                "recovery",
                challengeToken,
                networkScope,
                cancellationToken);

        var challenge = await _store.FindRecoveryChallengeAsync(
            HashOpaque(challengeToken),
            cancellationToken);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        if (challenge is null ||
            challenge.ConsumedAtUtc is not null ||
            challenge.ExpiresAtUtc <= utcNow ||
            challenge.FailedAttemptCount >= _options.MaximumFailures ||
            sharedBlocked)
        {
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "recovery",
                    challengeToken,
                    networkScope,
                    cancellationToken);
            }

            return new SessionLifecycleResult(SessionLifecycleOutcome.ChallengeInvalid);
        }

        var user = await _store.FindByIdAsync(challenge.ApplicationUserId, cancellationToken);
        var passwordContext = user is null
            ? IdentityPasswordContext.Empty
            : await CreatePasswordContextAsync(user, cancellationToken);
        var validation = _passwordValidator.Validate(newPassword, passwordContext);
        if (user is null || !user.IsEnabled || !validation.IsValid)
        {
            await _store.RecordRecoveryFailureAsync(
                challenge.Id,
                _options.MaximumFailures,
                cancellationToken);
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "recovery",
                    challengeToken,
                    networkScope,
                    cancellationToken);
            }

            return new SessionLifecycleResult(
                validation.IsValid
                    ? SessionLifecycleOutcome.ChallengeInvalid
                    : SessionLifecycleOutcome.PasswordRejected);
        }

        var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);
        var newSecurityStamp = CreateSecurityStamp();
        var completed = await _store.TryCompleteRecoveryAsync(
            challenge.Id,
            challenge.Version,
            user.Id,
            newPasswordHash,
            newSecurityStamp,
            utcNow,
            _options.MaximumFailures,
            cancellationToken);

        if (!completed)
        {
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "recovery",
                    challengeToken,
                    networkScope,
                    cancellationToken);
            }

            return new SessionLifecycleResult(SessionLifecycleOutcome.ChallengeInvalid);
        }

        if (_abuseControl is not null)
        {
            await _abuseControl.ResetAsync(
                "recovery",
                challengeToken,
                networkScope,
                cancellationToken);
        }

        return SessionLifecycleResult.Completed();
    }

    public Task<SessionLifecycleResult> ChangePasswordAsync(
        Guid applicationUserId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default) =>
        ChangePasswordAsync(
            applicationUserId,
            currentPassword,
            newPassword,
            networkScope: null,
            cancellationToken);

    public async Task<SessionLifecycleResult> ChangePasswordAsync(
        Guid applicationUserId,
        string currentPassword,
        string newPassword,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var subjectScope = applicationUserId.ToString("N");
        var sharedBlocked = _abuseControl is not null &&
            await _abuseControl.IsBlockedAsync(
                "password-change",
                subjectScope,
                networkScope,
                cancellationToken);
        var user = await _store.FindByIdAsync(applicationUserId, cancellationToken);
        if (user is null ||
            sharedBlocked ||
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                currentPassword ?? string.Empty) == PasswordVerificationResult.Failed)
        {
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "password-change",
                    subjectScope,
                    networkScope,
                    cancellationToken);
            }

            return new SessionLifecycleResult(SessionLifecycleOutcome.CurrentPasswordInvalid);
        }

        var passwordContext = await CreatePasswordContextAsync(user, cancellationToken);
        var validation = _passwordValidator.Validate(newPassword, passwordContext);
        if (!validation.IsValid)
        {
            return new SessionLifecycleResult(SessionLifecycleOutcome.PasswordRejected);
        }

        var changed = await _store.ChangePasswordAsync(
            user.Id,
            user.Version,
            _passwordHasher.HashPassword(user, newPassword),
            CreateSecurityStamp(),
            cancellationToken);
        if (!changed)
        {
            return new SessionLifecycleResult(SessionLifecycleOutcome.AuthenticationFailed);
        }

        if (_abuseControl is not null)
        {
            await _abuseControl.ResetAsync(
                "password-change",
                subjectScope,
                networkScope,
                cancellationToken);
        }

        return SessionLifecycleResult.Completed();
    }

    public async Task<SessionLifecycleResult> RevokeAllSessionsAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _store.FindByIdAsync(applicationUserId, cancellationToken);
        if (user is null)
        {
            return new SessionLifecycleResult(SessionLifecycleOutcome.AuthenticationFailed);
        }

        var rotated = await _store.RotateSecurityStampAsync(
            user.Id,
            user.Version,
            CreateSecurityStamp(),
            cancellationToken);
        return new SessionLifecycleResult(
            rotated ? SessionLifecycleOutcome.Completed : SessionLifecycleOutcome.AuthenticationFailed);
    }

    public async Task<AuthenticationResult> GetSessionAsync(
        Guid applicationUserId,
        string? activeRole,
        CancellationToken cancellationToken = default)
    {
        var user = await _store.FindByIdAsync(applicationUserId, cancellationToken);
        if (user is null || !user.IsEnabled)
        {
            return AuthenticationResult.AuthenticationFailed();
        }

        var sessionSecurityStamp = user.SecurityStamp;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var roles = await _store.GetEffectiveRolesAsync(user.Id, utcNow, cancellationToken);
        if (roles.Count != 1 ||
            (activeRole is not null && !string.Equals(activeRole, roles[0], StringComparison.Ordinal)))
        {
            return AuthenticationResult.Failure(AuthenticationOutcome.InvalidRoleConfiguration);
        }

        var selectedRole = roles[0];
        return AuthenticationResult.Success(
            user.Id,
            user.UserName,
            sessionSecurityStamp,
            roles,
            selectedRole,
            utcNow.Add(SessionLifetime));
    }

    private static string HashOpaque(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private async Task<IdentityPasswordContext> CreatePasswordContextAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var staff = await _store.FindStaffAsync(user.Id, cancellationToken);
        return new IdentityPasswordContext(
            user.UniversityId,
            user.UserName,
            staff?.DisplayName);
    }

    private static string CreateSecurityStamp() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
