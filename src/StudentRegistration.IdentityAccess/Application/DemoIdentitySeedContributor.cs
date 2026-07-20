using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class DemoIdentitySeedContributor
{
    private const string ClientRequestId = "spec007-demo-seed-v2";
    private const string RetiredDualUserName = "DUAL-0001";
    private const string UniversityIdSeedPrefix = "AI26";
    private readonly IIdentitySeedStore _store;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public DemoIdentitySeedContributor(
        IIdentitySeedStore store,
        IPasswordHasher<ApplicationUser> passwordHasher,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<IReadOnlyList<DemoCredential>> SeedAsync(
        string environmentName,
        int studentCount,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(environmentName, "Development", StringComparison.Ordinal) &&
            !string.Equals(environmentName, "Testing", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Synthetic identity bootstrap is limited to Development and Testing.");
        }

        if (studentCount < 1 || studentCount > 25_000)
        {
            throw new ArgumentOutOfRangeException(nameof(studentCount));
        }

        var provisionedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var identities = new List<DemoSeedIdentity>(studentCount + 3);
        var credentials = new List<GeneratedDemoCredential>(studentCount + 3);

        for (var index = 1; index <= studentCount; index++)
        {
            var universityId = $"{UniversityIdSeedPrefix}{index:00000}";
            AddIdentity(
                universityId,
                universityId,
                $"Synthetic Student {index:00000}",
                null,
                ["Student"],
                provisionedAtUtc,
                identities,
                credentials);
        }

        AddIdentity("ADM-0001", null, "Demo Administrator", "ADM-0001", ["Admin"], provisionedAtUtc, identities, credentials);
        AddIdentity("LEC-0001", null, "Demo Lecturer", "LEC-0001", ["Lecturer"], provisionedAtUtc, identities, credentials);
        AddIdentity("TA-0001", null, "Demo Teaching Assistant", "TA-0001", ["TeachingAssistant"], provisionedAtUtc, identities, credentials);

        // The store performs an idempotent reconciliation by stable synthetic keys
        // and disables the retired dual-role demo identity if it exists from an
        // earlier local database.
        var provisionedUserIds = await _store.ReconcileAsync(
            identities,
            new HashSet<Guid> { StableGuid(IdentityTextNormalizer.NormalizeUserName(RetiredDualUserName)) },
            provisionedAtUtc,
            ClientRequestId,
            cancellationToken);
        return credentials
            .Where(generated => provisionedUserIds.Contains(generated.UserId))
            .Select(generated => generated.Credential)
            .ToArray();
    }

    private void AddIdentity(
        string userName,
        string? universityId,
        string displayName,
        string? staffNumber,
        IReadOnlyList<string> roles,
        DateTime provisionedAtUtc,
        ICollection<DemoSeedIdentity> identities,
        ICollection<GeneratedDemoCredential> credentials)
    {
        var normalizedUserName = IdentityTextNormalizer.NormalizeUserName(userName);
        var secret = $"Demo@2026-{userName}";
        var userId = StableGuid(normalizedUserName);
        var stamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var transient = new ApplicationUser(
            userId,
            userName,
            normalizedUserName,
            universityId,
            "TRANSIENT",
            stamp);
        var passwordHash = _passwordHasher.HashPassword(transient, secret);

        identities.Add(new DemoSeedIdentity(
            userId,
            userName,
            normalizedUserName,
            universityId,
            passwordHash,
            stamp,
            displayName,
            staffNumber,
            roles,
            provisionedAtUtc));
        credentials.Add(new GeneratedDemoCredential(
            userId,
            new DemoCredential(userName, secret, provisionedAtUtc)));
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record GeneratedDemoCredential(
        Guid UserId,
        DemoCredential Credential);
}

public sealed record DemoCredential(
    string LoginIdentifier,
    string Secret,
    DateTime GeneratedAtUtc);
