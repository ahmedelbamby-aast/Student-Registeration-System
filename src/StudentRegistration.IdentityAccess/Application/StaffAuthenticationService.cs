using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class StaffAuthenticationService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(60);
    private static readonly HashSet<string> AllowedStaffRoles = new(
        ["Admin", "Lecturer", "TeachingAssistant"],
        StringComparer.Ordinal);
    private readonly IIdentityAccountStore _store;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IdentityAbuseControl? _abuseControl;
    private readonly ApplicationUser _dummyUser;
    private readonly string _dummyHash;

    public StaffAuthenticationService(
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
            "unknown.staff",
            "UNKNOWN.STAFF",
            null,
            "TRANSIENT",
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            false);
        _dummyHash = _passwordHasher.HashPassword(
            _dummyUser,
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)));
    }

    public Task<AuthenticationResult> AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default) =>
        AuthenticateAsync(userName, password, networkScope: null, cancellationToken);

    public async Task<AuthenticationResult> AuthenticateAsync(
        string userName,
        string password,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserName = IdentityTextNormalizer.NormalizeUserName(userName);
        var sharedBlocked = _abuseControl is not null &&
            await _abuseControl.IsBlockedAsync(
                "login",
                normalizedUserName,
                networkScope,
                cancellationToken);
        var user = await _store.FindByNormalizedUserNameAsync(
            normalizedUserName,
            cancellationToken);
        var verificationUser = user ?? _dummyUser;
        var verificationHash = user?.PasswordHash ?? _dummyHash;
        var verifiedSecurityStamp = user?.SecurityStamp;
        var verification = _passwordHasher.VerifyHashedPassword(
            verificationUser,
            verificationHash,
            password ?? string.Empty);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var locked = user?.LockoutEndUtc is { } lockoutEnd && lockoutEnd > utcNow;
        var staff = user is null
            ? null
            : await _store.FindStaffAsync(user.Id, cancellationToken);
        var effectiveRoles = user is null
            ? []
            : (await _store.GetEffectiveRolesAsync(user.Id, utcNow, cancellationToken))
                .Where(AllowedStaffRoles.Contains)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

        if (user is null ||
            verification == PasswordVerificationResult.Failed ||
            !user.IsEnabled ||
            locked ||
            staff is null ||
            !staff.IsActive ||
            effectiveRoles.Length != 1 ||
            sharedBlocked)
        {
            await _store.RecordAuthenticationFailureAsync(
                user?.Id,
                "Staff",
                cancellationToken);
            if (_abuseControl is not null)
            {
                await _abuseControl.RecordFailureAsync(
                    "login",
                    normalizedUserName,
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
                normalizedUserName,
                networkScope,
                cancellationToken);
        }

        return AuthenticationResult.Success(
            user.Id,
            staff.DisplayName,
            verifiedSecurityStamp!,
            effectiveRoles,
            effectiveRoles[0],
            utcNow.Add(SessionLifetime));
    }
}
