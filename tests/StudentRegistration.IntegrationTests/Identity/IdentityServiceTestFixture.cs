using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IntegrationTests.Identity;

internal sealed class IdentityServiceTestFixture
{
    public IdentityServiceTestFixture()
    {
        Time = new FixedTimeProvider(new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero));
        Options = new IdentitySecurityOptions();
        Hasher = new PasswordHasher<ApplicationUser>(
            Microsoft.Extensions.Options.Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = Options.PasswordHashIterations
            }));
        Store = new InMemoryIdentityAccountStore();
        Delivery = new CapturingRecoveryProofDelivery();
    }

    public FixedTimeProvider Time { get; }
    public IdentitySecurityOptions Options { get; }
    public IPasswordHasher<ApplicationUser> Hasher { get; }
    public InMemoryIdentityAccountStore Store { get; }
    public CapturingRecoveryProofDelivery Delivery { get; }

    public ApplicationUser AddStudent(
        string universityId,
        string password,
        bool activated = true,
        bool enabled = true)
    {
        var user = CreateUser(
            $"student.{universityId}",
            universityId,
            password,
            enabled);
        Store.AddUser(user, activated, ["Student"]);
        return user;
    }

    public (ApplicationUser User, Staff Staff) AddStaff(
        string userName,
        string password,
        IReadOnlyCollection<string> roles,
        bool enabled = true,
        bool active = true)
    {
        var user = CreateUser(userName, null, password, enabled);
        var staff = new Staff(
            Guid.NewGuid(),
            user.Id,
            $"STAFF-{userName}",
            $"Display {userName}",
            active);
        Store.AddUser(user, activated: true, roles);
        Store.AddStaff(staff);
        return (user, staff);
    }

    public StudentAuthenticationService CreateStudentAuthentication() =>
        new(Store, Hasher, Time);

    public StudentActivationService CreateStudentActivation() =>
        new(Store, Hasher, new IdentityPasswordValidator(Options), Time);

    public StaffAuthenticationService CreateStaffAuthentication() =>
        new(Store, Hasher, Time);

    public SessionLifecycleService CreateSessionLifecycle() =>
        new(
            Store,
            Delivery,
            Hasher,
            new IdentityPasswordValidator(Options),
            Options,
            Time);

    private ApplicationUser CreateUser(
        string userName,
        string? universityId,
        string password,
        bool enabled)
    {
        var user = new ApplicationUser(
            Guid.NewGuid(),
            userName,
            userName.ToUpperInvariant(),
            universityId,
            "TRANSIENT",
            Guid.NewGuid().ToString("N"),
            enabled);
        user.ReplacePasswordHash(Hasher.HashPassword(user, password), user.SecurityStamp);
        return user;
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public DateTimeOffset UtcNow { get; private set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => UtcNow;

    public void Advance(TimeSpan duration) => UtcNow = UtcNow.Add(duration);
}

internal sealed class CapturingRecoveryProofDelivery : IAccountRecoveryProofDelivery
{
    public RecoveryProofDeliveryRequest? LastRequest { get; private set; }

    public Task<RecoveryProofDeliveryResult> DeliverAsync(
        RecoveryProofDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastRequest = request;
        return Task.FromResult(
            new RecoveryProofDeliveryResult(true, $"test:{Guid.NewGuid():N}"));
    }
}

internal sealed class InMemoryIdentityAccountStore : IIdentityAccountStore
{
    private readonly ConcurrentDictionary<Guid, ApplicationUser> _users = [];
    private readonly ConcurrentDictionary<string, Guid> _userNames =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Guid> _universityIds =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Guid, Staff> _staff = [];
    private readonly ConcurrentDictionary<Guid, bool> _activated = [];
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<string>> _roles = [];
    private readonly ConcurrentDictionary<string, AccountRecoveryChallenge> _challenges =
        new(StringComparer.Ordinal);
    private readonly object _transitionLock = new();

    public void AddUser(
        ApplicationUser user,
        bool activated,
        IReadOnlyCollection<string> roles)
    {
        _users[user.Id] = user;
        _userNames[user.NormalizedUserName] = user.Id;
        if (user.UniversityId is not null)
        {
            _universityIds[user.UniversityId] = user.Id;
        }

        _activated[user.Id] = activated;
        _roles[user.Id] = roles.ToArray();
    }

    public void AddStaff(Staff staff) => _staff[staff.ApplicationUserId] = staff;

    public Task<ApplicationUser?> FindStudentByUniversityIdAsync(
        string normalizedUniversityId,
        CancellationToken cancellationToken) =>
        Task.FromResult(
            _universityIds.TryGetValue(normalizedUniversityId, out var id)
                ? _users[id]
                : null);

    public Task<ApplicationUser?> FindByNormalizedUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken) =>
        Task.FromResult(
            _userNames.TryGetValue(normalizedUserName, out var id)
                ? _users[id]
                : null);

    public Task<ApplicationUser?> FindByIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(applicationUserId));

    public Task<Staff?> FindStaffAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_staff.GetValueOrDefault(applicationUserId));

    public Task<bool> IsStudentActivatedAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_activated.GetValueOrDefault(applicationUserId));

    public Task<IReadOnlyList<string>> GetEffectiveRolesAsync(
        Guid applicationUserId,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        Task.FromResult(_roles.GetValueOrDefault(applicationUserId) ?? []);

    public Task RecordAuthenticationFailureAsync(
        Guid? applicationUserId,
        string operation,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ResetAuthenticationFailuresAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RecordActivationFailureAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<bool> TryActivateAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime activatedAtUtc,
        CancellationToken cancellationToken)
    {
        lock (_transitionLock)
        {
            if (_activated.GetValueOrDefault(applicationUserId))
            {
                return Task.FromResult(false);
            }

            _users[applicationUserId].ReplacePasswordHash(newPasswordHash, newSecurityStamp);
            _activated[applicationUserId] = true;
            return Task.FromResult(true);
        }
    }

    public Task<bool> AddRecoveryChallengeAsync(
        AccountRecoveryChallenge challenge,
        CancellationToken cancellationToken) =>
        Task.FromResult(_challenges.TryAdd(challenge.TokenHash, challenge));

    public Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        Task.FromResult(_challenges.GetValueOrDefault(tokenHash));

    public Task RecordRecoveryFailureAsync(
        Guid challengeId,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        lock (_transitionLock)
        {
            _challenges.Values.Single(challenge => challenge.Id == challengeId)
                .RecordFailedAttempt(maximumAttempts);
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryCompleteRecoveryAsync(
        Guid challengeId,
        byte[] expectedChallengeVersion,
        Guid applicationUserId,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime consumedAtUtc,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        lock (_transitionLock)
        {
            var challenge = _challenges.Values.Single(candidate => candidate.Id == challengeId);
            if (!challenge.TryConsume(consumedAtUtc, maximumAttempts))
            {
                return Task.FromResult(false);
            }

            _users[applicationUserId].ReplacePasswordHash(newPasswordHash, newSecurityStamp);
            return Task.FromResult(true);
        }
    }

    public Task<bool> ChangePasswordAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        CancellationToken cancellationToken)
    {
        lock (_transitionLock)
        {
            if (!_users.TryGetValue(applicationUserId, out var user))
            {
                return Task.FromResult(false);
            }

            user.ReplacePasswordHash(newPasswordHash, newSecurityStamp);
            return Task.FromResult(true);
        }
    }

    public Task<bool> RotateSecurityStampAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newSecurityStamp,
        CancellationToken cancellationToken)
    {
        lock (_transitionLock)
        {
            if (!_users.TryGetValue(applicationUserId, out var user))
            {
                return Task.FromResult(false);
            }

            user.RotateSecurityStamp(newSecurityStamp);
            return Task.FromResult(true);
        }
    }
}
