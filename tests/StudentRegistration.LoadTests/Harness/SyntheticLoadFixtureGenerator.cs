using System.Security.Cryptography;
using System.Text;

namespace StudentRegistration.LoadTesting.Spec018;

public sealed record SyntheticFixtureProfile(
    string Version,
    int AccountCount,
    int ConcurrentSessionCount)
{
    public static SyntheticFixtureProfile Required { get; } = new(
        Version: "SPEC018-LOAD-FIXTURE-1.0.0",
        AccountCount: 25_000,
        ConcurrentSessionCount: 5_000);
}

public sealed record SyntheticAccount(
    Guid AccountId,
    string LogicalAccountKey,
    string ProgramCode,
    decimal Gpa,
    int EarnedCredits);

public sealed record SyntheticSession(
    Guid SessionId,
    Guid AccountId);

public sealed record SyntheticLoadFixture(
    string Version,
    IReadOnlyList<SyntheticAccount> Accounts,
    IReadOnlyList<SyntheticSession> Sessions);

public static class SyntheticLoadFixtureGenerator
{
    private static readonly string[] ProgramCodes =
    {
        "AI-CORE",
        "AI-DATA",
        "AI-ROBOTICS"
    };

    public static SyntheticLoadFixture Build(SyntheticFixtureProfile? profile = null)
    {
        profile ??= SyntheticFixtureProfile.Required;
        Validate(profile);

        var accounts = new SyntheticAccount[profile.AccountCount];
        for (var index = 0; index < accounts.Length; index++)
        {
            accounts[index] = new SyntheticAccount(
                AccountId: DeterministicGuid(profile.Version, "account", index),
                LogicalAccountKey: $"logical-account-{index:00000}",
                ProgramCode: ProgramCodes[index % ProgramCodes.Length],
                Gpa: 2.00m + ((index % 201) / 100m),
                EarnedCredits: (index % 41) * 3);
        }

        var sessions = new SyntheticSession[profile.ConcurrentSessionCount];
        for (var index = 0; index < sessions.Length; index++)
        {
            sessions[index] = new SyntheticSession(
                SessionId: DeterministicGuid(profile.Version, "session", index),
                AccountId: accounts[index].AccountId);
        }

        return new SyntheticLoadFixture(profile.Version, accounts, sessions);
    }

    private static Guid DeterministicGuid(string version, string scope, int index)
    {
        var input = Encoding.UTF8.GetBytes($"{version}:{scope}:{index}");
        var hash = SHA256.HashData(input);
        return new Guid(hash.AsSpan(0, 16));
    }

    private static void Validate(SyntheticFixtureProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Version) || profile.AccountCount <= 0 ||
            profile.ConcurrentSessionCount <= 0 ||
            profile.ConcurrentSessionCount > profile.AccountCount)
        {
            throw new ArgumentException("Synthetic load fixture profile is invalid.", nameof(profile));
        }
    }
}
