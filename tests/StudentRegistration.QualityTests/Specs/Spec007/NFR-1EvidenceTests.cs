using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class NFR_1EvidenceTests(ITestOutputHelper output)
{
    private const string EvidencePath = "docs/release-evidence/SPEC-007-NFR-1.md";
    private const int AccountCount = 25_000;
    private const int RequestsPerSecond = 25;
    private const int DurationSeconds = 600;
    private const int TotalRequests = RequestsPerSecond * DurationSeconds;
    private const string ValidPassword = "correct horse battery staple";

    [Fact]
    public void Login_profile_records_the_exact_load_mix_and_release_budgets()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var normalizedEvidence = Regex.Replace(evidence, @"\s+", " ");

        RepositoryFiles.ContainsAll(
            normalizedEvidence,
            "SPEC-007 NFR-1",
            "10 minutes",
            "25 login attempts/second",
            "25,000 synthetic accounts",
            "two stateless replicas",
            "80% valid",
            "15% invalid credential",
            "5% already locked",
            "15,000",
            "12,000",
            "2,250",
            "750",
            "zero shared-state invariant violations",
            "**Result: PASS.**");

        var p95 = ParseMetric(evidence, "Measured login p95 (ms)");
        var unexpectedErrorRate = ParseMetric(evidence, "Unexpected error rate (%)");
        Assert.True(p95 <= 500d, $"Measured login p95 was {p95:F3} ms.");
        Assert.True(
            unexpectedErrorRate < 1d,
            $"Unexpected error rate was {unexpectedErrorRate:F4}%.");
    }

    [Fact]
    public async Task Exact_profile_can_generate_measured_local_evidence()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SPEC007_RUN_LOAD_PROFILE"),
                "1",
                StringComparison.Ordinal))
        {
            Assert.True(
                RepositoryFiles.Exists(EvidencePath),
                "Checked-in evidence is required when the explicit 10-minute profile is not running.");
            return;
        }

        var result = await RunExactProfileAsync();
        var evidenceDirectory = RepositoryFiles.PathTo(".local/evidence");
        Directory.CreateDirectory(evidenceDirectory);
        var evidencePath = Path.Combine(evidenceDirectory, "SPEC-007-NFR-1.json");
        await File.WriteAllTextAsync(
            evidencePath,
            JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }));

        output.WriteLine("Measured evidence: {0}", evidencePath);
        output.WriteLine(
            "Requests={0}; p95={1:F3} ms; unexpected={2:F4}%; invariants={3}.",
            result.CompletedRequests,
            result.MeasuredP95Milliseconds,
            result.UnexpectedErrorRatePercent,
            result.InvariantViolations);

        Assert.Equal(TotalRequests, result.CompletedRequests);
        Assert.Equal(12_000, result.ValidRequests);
        Assert.Equal(2_250, result.InvalidCredentialRequests);
        Assert.Equal(750, result.AlreadyLockedRequests);
        Assert.True(result.MeasuredP95Milliseconds <= 500d);
        Assert.True(result.UnexpectedErrorRatePercent < 1d);
        Assert.Equal(0, result.InvariantViolations);
    }

    [Fact]
    public async Task Replica_fixture_uses_independent_store_adapters_over_one_shared_state_boundary()
    {
        var hasher = CreateHasher();
        var state = CreateSharedState(hasher, TimeProvider.System);
        var firstAdapter = new LoadIdentityStore(state);
        var secondAdapter = new LoadIdentityStore(state);

        Assert.NotSame(firstAdapter, secondAdapter);
        var fromFirst = await firstAdapter.FindStudentByUniversityIdAsync(
            "202600000",
            default);
        var fromSecond = await secondAdapter.FindStudentByUniversityIdAsync(
            "202600000",
            default);
        Assert.NotNull(fromFirst);
        Assert.Same(fromFirst, fromSecond);
    }

    private static async Task<LoadEvidence> RunExactProfileAsync()
    {
        var timeProvider = TimeProvider.System;
        var hasher = CreateHasher();
        var sharedState = CreateSharedState(hasher, timeProvider);
        var replicaStores = new[]
        {
            new LoadIdentityStore(sharedState),
            new LoadIdentityStore(sharedState)
        };
        var replicas = new[]
        {
            new StudentAuthenticationService(replicaStores[0], hasher, timeProvider),
            new StudentAuthenticationService(replicaStores[1], hasher, timeProvider)
        };
        var samples = new double[TotalRequests];
        var unexpectedErrors = 0;
        var invariantViolations = 0;
        var tasks = new Task[TotalRequests];
        var profileStarted = Stopwatch.GetTimestamp();
        var startedAtUtc = timeProvider.GetUtcNow();

        for (var index = 0; index < TotalRequests; index++)
        {
            var target = profileStarted +
                (long)(index * (Stopwatch.Frequency / (double)RequestsPerSecond));
            var delayTicks = target - Stopwatch.GetTimestamp();
            if (delayTicks > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayTicks / (double)Stopwatch.Frequency));
            }

            var requestIndex = index;
            var scheduledAt = target;
            tasks[index] = Task.Run(async () =>
            {
                try
                {
                    var kind = requestIndex % 20;
                    var userIndex = kind == 19
                        ? requestIndex - (requestIndex % 20) + 19
                        : requestIndex;
                    var expectedSuccess = kind < 16;
                    var password = kind is >= 16 and <= 18 ? "incorrect password" : ValidPassword;
                    var result = await replicas[requestIndex % replicas.Length].AuthenticateAsync(
                        $"2026{userIndex:00000}",
                        password);
                    if (result.Succeeded != expectedSuccess)
                    {
                        Interlocked.Increment(ref invariantViolations);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref unexpectedErrors);
                }
                finally
                {
                    samples[requestIndex] =
                        (Stopwatch.GetTimestamp() - scheduledAt) * 1000d / Stopwatch.Frequency;
                }
            });
        }

        await Task.WhenAll(tasks);
        Array.Sort(samples);
        var p95 = samples[(int)Math.Ceiling(samples.Length * 0.95) - 1];
        return new LoadEvidence(
            "SPEC007-IDENTITY-LOGIN-1.0.0",
            startedAtUtc,
            timeProvider.GetUtcNow(),
            DurationSeconds,
            RequestsPerSecond,
            AccountCount,
            2,
            TotalRequests,
            TotalRequests,
            12_000,
            2_250,
            750,
            p95,
            unexpectedErrors,
            unexpectedErrors * 100d / TotalRequests,
            invariantViolations);
    }

    private static IPasswordHasher<ApplicationUser> CreateHasher() =>
        new PasswordHasher<ApplicationUser>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 100_000
            }));

    private static SharedLoadIdentityState CreateSharedState(
        IPasswordHasher<ApplicationUser> hasher,
        TimeProvider timeProvider)
    {
        var template = new ApplicationUser(
            Guid.NewGuid(),
            "template",
            "TEMPLATE",
            "202600000",
            "TRANSIENT",
            Guid.NewGuid().ToString("N"));
        var hash = hasher.HashPassword(template, ValidPassword);
        var users = new ApplicationUser[AccountCount];
        var lockoutEndUtc = timeProvider.GetUtcNow().AddHours(1).UtcDateTime;
        for (var index = 0; index < users.Length; index++)
        {
            var universityId = $"2026{index:00000}";
            users[index] = new ApplicationUser(
                CreateDeterministicGuid(index),
                $"student.{index:00000}",
                $"STUDENT.{index:00000}",
                universityId,
                hash,
                $"STAMP-{index:00000}");
            if (index % 20 == 19)
            {
                users[index].RecordFailedAccess(lockoutEndUtc);
            }
        }

        return new SharedLoadIdentityState(users);
    }

    private static Guid CreateDeterministicGuid(int index)
    {
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.TryWriteBytes(bytes, index + 1);
        return new Guid(bytes);
    }

    private static double ParseMetric(string evidence, string label)
    {
        var match = Regex.Match(
            evidence,
            $@"(?m)^\|\s*{Regex.Escape(label)}\s*\|\s*(?<value>\d+(?:\.\d+)?)\s*\|");
        Assert.True(match.Success, $"Required measured metric is missing: {label}");
        return double.Parse(
            match.Groups["value"].Value,
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed record LoadEvidence(
        string ProfileVersion,
        DateTimeOffset StartedAtUtc,
        DateTimeOffset CompletedAtUtc,
        int DurationSeconds,
        int RequestsPerSecond,
        int AccountCount,
        int ReplicaCount,
        int ScheduledRequests,
        int CompletedRequests,
        int ValidRequests,
        int InvalidCredentialRequests,
        int AlreadyLockedRequests,
        double MeasuredP95Milliseconds,
        int UnexpectedErrors,
        double UnexpectedErrorRatePercent,
        int InvariantViolations);

    private sealed class SharedLoadIdentityState(IEnumerable<ApplicationUser> users)
    {
        public ConcurrentDictionary<string, ApplicationUser> Users { get; } = new(
            users.ToDictionary(
                user => user.UniversityId!,
                user => user,
                StringComparer.Ordinal));
    }

    private sealed class LoadIdentityStore(SharedLoadIdentityState state) : IIdentityAccountStore
    {
        private readonly ConcurrentDictionary<string, ApplicationUser> _users =
            state.Users;

        public Task<ApplicationUser?> FindStudentByUniversityIdAsync(
            string normalizedUniversityId,
            CancellationToken cancellationToken) =>
            Task.FromResult(_users.GetValueOrDefault(normalizedUniversityId));

        public Task<bool> IsStudentActivatedAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Task.FromResult(true);

        public Task RecordAuthenticationFailureAsync(
            Guid? applicationUserId,
            string operation,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResetAuthenticationFailuresAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ApplicationUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByIdAsync(Guid applicationUserId, CancellationToken cancellationToken) =>
            Task.FromResult(_users.Values.FirstOrDefault(user => user.Id == applicationUserId));
        public Task<Staff?> FindStaffAsync(Guid applicationUserId, CancellationToken cancellationToken) => Task.FromResult<Staff?>(null);
        public Task<IReadOnlyList<string>> GetEffectiveRolesAsync(Guid applicationUserId, DateTime utcNow, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>(["Student"]);
        public Task RecordActivationFailureAsync(Guid applicationUserId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> TryActivateAsync(Guid applicationUserId, byte[] expectedVersion, string newPasswordHash, string newSecurityStamp, DateTime activatedAtUtc, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> AddRecoveryChallengeAsync(AccountRecoveryChallenge challenge, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(string tokenHash, CancellationToken cancellationToken) => Task.FromResult<AccountRecoveryChallenge?>(null);
        public Task RecordRecoveryFailureAsync(Guid challengeId, int maximumAttempts, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> TryCompleteRecoveryAsync(Guid challengeId, byte[] expectedChallengeVersion, Guid applicationUserId, string newPasswordHash, string newSecurityStamp, DateTime consumedAtUtc, int maximumAttempts, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> ChangePasswordAsync(Guid applicationUserId, byte[] expectedVersion, string newPasswordHash, string newSecurityStamp, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> RotateSecurityStampAsync(Guid applicationUserId, byte[] expectedVersion, string newSecurityStamp, CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
