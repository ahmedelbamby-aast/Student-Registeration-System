using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class StudentAuthenticationService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(60);
    private readonly IIdentityAccountStore _store;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IdentityAbuseControl? _abuseControl;
    private readonly ApplicationUser _dummyUser;
    private readonly string _dummyHash;

    public StudentAuthenticationService(
        IIdentityAccountStore store,
        IPasswordHasher<ApplicationUser> passwordHasher,
        TimeProvider timeProvider,
        IdentityAbuseControl? abuseControl = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _abuseControl = abuseControl;
        _dummyUser = new ApplicationUser(
            Guid.NewGuid(),
            "unknown.student",
            "UNKNOWN.STUDENT",
            null,
            "TRANSIENT",
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            false);
        _dummyHash = _passwordHasher.HashPassword(
            _dummyUser,
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)));
    }

    public Task<AuthenticationResult> AuthenticateAsync(
        string universityId,
        string password,
        CancellationToken cancellationToken = default) =>
        AuthenticateAsync(universityId, password, networkScope: null, cancellationToken);

    public async Task<AuthenticationResult> AuthenticateAsync(
        string universityId,
        string password,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var normalizedUniversityId = IdentityTextNormalizer.NormalizeUniversityId(universityId);
        var sharedBlocked = _abuseControl is not null &&
            await _abuseControl.IsBlockedAsync(
                "login",
                normalizedUniversityId,
                networkScope,
                cancellationToken);
        var user = await _store.FindStudentByUniversityIdAsync(
            normalizedUniversityId,
            cancellationToken);
        var verificationUser = user ?? _dummyUser;
        var verificationHash = user?.PasswordHash ?? _dummyHash;
        var verifiedSecurityStamp = user?.SecurityStamp;
        var verification = _passwordHasher.VerifyHashedPassword(
            verificationUser,
            verificationHash,
            password ?? string.Empty);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var activated = user is not null &&
            await _store.IsStudentActivatedAsync(user.Id, cancellationToken);
        var locked = user?.LockoutEndUtc is { } lockoutEnd && lockoutEnd > utcNow;

        if (user is null ||
            verification == PasswordVerificationResult.Failed ||
            !user.IsEnabled ||
            locked ||
            !activated ||
            sharedBlocked)
        {
            await _store.RecordAuthenticationFailureAsync(
                user?.Id,
                "Student",
                cancellationToken);
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "login",
                    normalizedUniversityId,
                    networkScope,
                    cancellationToken);
            }

            return AuthenticationResult.AuthenticationFailed();
        }

        await _store.ResetAuthenticationFailuresAsync(user.Id, cancellationToken);
        if (_abuseControl is not null)
        {
            await _abuseControl.ResetAsync(
                "login",
                normalizedUniversityId,
                networkScope,
                cancellationToken);
        }

        // Success maps to AuthenticationSucceeded without exposing an internal cause on failure.
        return AuthenticationResult.Success(
            user.Id,
            user.UserName,
            verifiedSecurityStamp!,
            ["Student"],
            "Student",
            utcNow.Add(SessionLifetime));
    }
}
