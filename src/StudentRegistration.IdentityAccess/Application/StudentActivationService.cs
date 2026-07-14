using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class StudentActivationService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(60);
    private readonly IIdentityAccountStore _store;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IIdentityPasswordValidator _passwordValidator;
    private readonly TimeProvider _timeProvider;
    private readonly IdentityAbuseControl? _abuseControl;
    private readonly ApplicationUser _dummyUser;
    private readonly string _dummyHash;

    public StudentActivationService(
        IIdentityAccountStore store,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IIdentityPasswordValidator passwordValidator,
        TimeProvider timeProvider,
        IdentityAbuseControl? abuseControl = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _abuseControl = abuseControl;
        _dummyUser = new ApplicationUser(
            Guid.NewGuid(),
            "unknown.activation",
            "UNKNOWN.ACTIVATION",
            null,
            "TRANSIENT",
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            false);
        _dummyHash = _passwordHasher.HashPassword(
            _dummyUser,
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)));
    }

    public Task<AuthenticationResult> ActivateAsync(
        string universityId,
        string initialPassword,
        string newPassword,
        CancellationToken cancellationToken = default) =>
        ActivateAsync(
            universityId,
            initialPassword,
            newPassword,
            networkScope: null,
            cancellationToken);

    public async Task<AuthenticationResult> ActivateAsync(
        string universityId,
        string initialPassword,
        string newPassword,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var normalizedUniversityId = IdentityTextNormalizer.NormalizeUniversityId(universityId);
        var sharedBlocked = _abuseControl is not null &&
            await _abuseControl.IsBlockedAsync(
                "activation",
                normalizedUniversityId,
                networkScope,
                cancellationToken);
        var user = await _store.FindStudentByUniversityIdAsync(
            normalizedUniversityId,
            cancellationToken);
        var verification = _passwordHasher.VerifyHashedPassword(
            user ?? _dummyUser,
            user?.PasswordHash ?? _dummyHash,
            initialPassword ?? string.Empty);
        var alreadyActivated = await _store.IsStudentActivatedAsync(
            user?.Id ?? Guid.Empty,
            cancellationToken);
        if (user is null ||
            alreadyActivated ||
            !user.IsEnabled ||
            sharedBlocked ||
            verification == PasswordVerificationResult.Failed)
        {
            if (user is not null &&
                !alreadyActivated &&
                user.IsEnabled &&
                !sharedBlocked &&
                verification == PasswordVerificationResult.Failed)
            {
                await _store.RecordActivationFailureAsync(user.Id, cancellationToken);
            }

            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "activation",
                    normalizedUniversityId,
                    networkScope,
                    cancellationToken);
            }

            return AuthenticationResult.ActivationFailed();
        }

        var validation = _passwordValidator.Validate(
            newPassword,
            new IdentityPasswordContext(
                user.UniversityId,
                user.UserName,
                DisplayName: null));
        if (!validation.IsValid)
        {
            return AuthenticationResult.Failure(AuthenticationOutcome.PasswordRejected);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);
        var newSecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var activated = await _store.TryActivateAsync(
            user.Id,
            user.Version,
            newPasswordHash,
            newSecurityStamp,
            utcNow,
            cancellationToken);
        if (!activated)
        {
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "activation",
                    normalizedUniversityId,
                    networkScope,
                    cancellationToken);
            }

            return AuthenticationResult.ActivationFailed();
        }

        if (_abuseControl is not null)
        {
            await _abuseControl.ResetAsync(
                "activation",
                normalizedUniversityId,
                networkScope,
                cancellationToken);
        }

        return AuthenticationResult.Success(
            user.Id,
            user.UserName,
            newSecurityStamp,
            ["Student"],
            "Student",
            utcNow.Add(SessionLifetime));
    }
}
