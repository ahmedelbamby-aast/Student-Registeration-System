using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StudentRegistration.Contracts;
using StudentRegistration.LoadTests.Infrastructure;
using Xunit.Abstractions;

namespace StudentRegistration.LoadTests.Specs.Spec008;

public sealed class AcademicContextLoadTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "SPEC008ExactLoadGate")]
    public async Task Authenticated_context_meets_the_exact_two_replica_gate()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(20));
        var result = await AcademicContextLoadRunner.RunAsync(
            AcademicContextLoadProfile.RequiredGate,
            timeout.Token);
        var artifactPath = await result.WriteAggregateJsonAsync(
            RepositoryRoot(),
            timeout.Token);

        output.WriteLine("Aggregate result: {0}", artifactPath);
        output.WriteLine(
            "Attempts={0}; replica-1={1}; replica-2={2}; p95={3:F3} ms; failures={4} ({5:P6}); scheduler-p95={6:F3} ms; scheduler-max={7:F3} ms; backpressure={8}.",
            result.Attempts,
            result.Replica1Attempts,
            result.Replica2Attempts,
            result.NearestRankP95Milliseconds,
            result.Failures,
            result.FailureRate,
            result.NearestRankSchedulerLagP95Milliseconds,
            result.MaximumSchedulerLagMilliseconds,
            result.BackpressureEvents);

        Assert.True(result.IsRequiredGateProfile);
        Assert.Equal(25_000, result.IdentityCount);
        Assert.Equal(2, result.ReplicaCount);
        Assert.Equal(180_000, result.Attempts);
        Assert.Equal(90_000, result.Replica1Attempts);
        Assert.Equal(90_000, result.Replica2Attempts);
        Assert.Equal(180_000, result.CompletedAttempts);
        Assert.Equal(0, result.RetryCount);
        Assert.True(result.WarmupExcluded);
        Assert.Equal("real-protected-cookie", result.AuthenticationMode);
        Assert.Equal("Stopwatch-monotonic", result.SchedulerClock);
        Assert.True(result.SchedulerElapsedMilliseconds >= 600_000d);
        Assert.True(result.MaximumObservedInFlight <= result.Profile.MaximumInFlight);
        Assert.Equal(0, result.BackpressureEvents);
        Assert.True(
            result.NearestRankSchedulerLagP95Milliseconds <=
                result.Profile.MaximumSchedulerLagP95Milliseconds,
            $"Scheduler p95 lag was {result.NearestRankSchedulerLagP95Milliseconds:F3} ms.");
        Assert.True(
            result.MaximumSchedulerLagMilliseconds <=
                result.Profile.MaximumSchedulerLagMilliseconds,
            $"Maximum scheduler lag was {result.MaximumSchedulerLagMilliseconds:F3} ms.");
        Assert.True(
            result.NearestRankP95Milliseconds <= 300d,
            $"Nearest-rank response p95 was {result.NearestRankP95Milliseconds:F3} ms.");
        Assert.True(
            result.Failures <= 179,
            $"Unexpected failures were {result.Failures}; at most 179 are allowed.");
        Assert.True(
            result.FailureRate < 0.001d,
            $"Unexpected failure rate was {result.FailureRate:P6}.");
        Assert.True(result.MeetsRequiredGate, result.GateFailureSummary);
    }

    private static string RepositoryRoot()
    {
        var candidate = new DirectoryInfo(AppContext.BaseDirectory);
        while (candidate is not null &&
               !File.Exists(Path.Combine(candidate.FullName, "StudentRegistration.slnx")))
        {
            candidate = candidate.Parent;
        }

        return candidate?.FullName ?? throw new InvalidOperationException(
            "The repository root could not be located for the ignored load result.");
    }
}

public sealed record AcademicContextLoadProfile(
    string ProfileId,
    int IdentityCount,
    int ReplicaCount,
    int DurationSeconds,
    int RequestsPerSecond,
    int TotalAttempts,
    int AttemptsPerReplica,
    int WarmupRequests,
    int MaximumInFlight,
    double MaximumP95Milliseconds,
    int MaximumFailures,
    double MaximumFailureRate,
    double MaximumSchedulerLagP95Milliseconds,
    double MaximumSchedulerLagMilliseconds)
{
    public static AcademicContextLoadProfile RequiredGate { get; } = new(
        ProfileId: "SPEC008-AUTHENTICATED-CONTEXT-1.0",
        IdentityCount: 25_000,
        ReplicaCount: 2,
        DurationSeconds: 600,
        RequestsPerSecond: 300,
        TotalAttempts: 180_000,
        AttemptsPerReplica: 90_000,
        WarmupRequests: 100,
        MaximumInFlight: 512,
        MaximumP95Milliseconds: 300d,
        MaximumFailures: 179,
        MaximumFailureRate: 0.001d,
        MaximumSchedulerLagP95Milliseconds: 50d,
        MaximumSchedulerLagMilliseconds: 1_000d);

    public bool IsRequiredGate => this == RequiredGate;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProfileId) ||
            IdentityCount <= 0 ||
            ReplicaCount != 2 ||
            DurationSeconds <= 0 ||
            RequestsPerSecond <= 0 ||
            TotalAttempts != checked(DurationSeconds * RequestsPerSecond) ||
            AttemptsPerReplica * ReplicaCount != TotalAttempts ||
            WarmupRequests < ReplicaCount ||
            MaximumInFlight <= 0 ||
            MaximumP95Milliseconds <= 0 ||
            MaximumFailures < 0 ||
            MaximumFailureRate is <= 0 or >= 1 ||
            MaximumSchedulerLagP95Milliseconds <= 0 ||
            MaximumSchedulerLagMilliseconds < MaximumSchedulerLagP95Milliseconds)
        {
            throw new ArgumentException("The academic-context load profile is invalid.");
        }
    }
}

public sealed record AcademicContextLoadResult(
    AcademicContextLoadProfile Profile,
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    int IdentityCount,
    int ReplicaCount,
    int WarmupRequests,
    int Attempts,
    int CompletedAttempts,
    int Replica1Attempts,
    int Replica2Attempts,
    double NearestRankP95Milliseconds,
    double NearestRankSchedulerLagP95Milliseconds,
    double MaximumSchedulerLagMilliseconds,
    double SchedulerElapsedMilliseconds,
    int MaximumObservedInFlight,
    int BackpressureEvents,
    int NonSuccessStatusFailures,
    int InvalidContextFailures,
    int TransportFailures)
{
    public string EndpointPath => "/api/context";

    public string AuthenticationMode => "real-protected-cookie";

    public string SchedulerClock => "Stopwatch-monotonic";

    public int RetryCount => 0;

    public bool WarmupExcluded => true;

    public int Failures => checked(
        NonSuccessStatusFailures + InvalidContextFailures + TransportFailures);

    public double FailureRate => Attempts == 0 ? 1d : Failures / (double)Attempts;

    public bool IsRequiredGateProfile => Profile.IsRequiredGate;

    public bool MeetsRequiredGate =>
        IsRequiredGateProfile &&
        IdentityCount == Profile.IdentityCount &&
        ReplicaCount == Profile.ReplicaCount &&
        Attempts == Profile.TotalAttempts &&
        CompletedAttempts == Profile.TotalAttempts &&
        Replica1Attempts == Profile.AttemptsPerReplica &&
        Replica2Attempts == Profile.AttemptsPerReplica &&
        NearestRankP95Milliseconds <= Profile.MaximumP95Milliseconds &&
        Failures <= Profile.MaximumFailures &&
        FailureRate < Profile.MaximumFailureRate &&
        BackpressureEvents == 0 &&
        MaximumObservedInFlight <= Profile.MaximumInFlight &&
        SchedulerElapsedMilliseconds >= Profile.DurationSeconds * 1_000d &&
        NearestRankSchedulerLagP95Milliseconds <=
            Profile.MaximumSchedulerLagP95Milliseconds &&
        MaximumSchedulerLagMilliseconds <= Profile.MaximumSchedulerLagMilliseconds;

    [JsonIgnore]
    public string GateFailureSummary => string.Create(
        CultureInfo.InvariantCulture,
        $"SPEC-008 gate failed: attempts={Attempts}/{Profile.TotalAttempts}, " +
        $"replicas={Replica1Attempts}/{Replica2Attempts}, " +
        $"p95={NearestRankP95Milliseconds:F3}/{Profile.MaximumP95Milliseconds:F3} ms, " +
        $"failures={Failures}/{Profile.MaximumFailures} ({FailureRate:P6}), " +
        $"scheduler-p95={NearestRankSchedulerLagP95Milliseconds:F3}/" +
        $"{Profile.MaximumSchedulerLagP95Milliseconds:F3} ms, " +
        $"scheduler-max={MaximumSchedulerLagMilliseconds:F3}/" +
        $"{Profile.MaximumSchedulerLagMilliseconds:F3} ms, " +
        $"scheduler-elapsed={SchedulerElapsedMilliseconds:F3} ms, " +
        $"in-flight={MaximumObservedInFlight}/{Profile.MaximumInFlight}, " +
        $"backpressure={BackpressureEvents}, retries={RetryCount}.");

    public async Task<string> WriteAggregateJsonAsync(
        string repositoryRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        var resultDirectory = Path.Combine(repositoryRoot, "load-test-results", "spec008");
        Directory.CreateDirectory(resultDirectory);
        var timestamp = CompletedAtUtc.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);
        var path = Path.Combine(resultDirectory, $"academic-context-{timestamp}.json");
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    WriteIndented = true
                }),
            cancellationToken).ConfigureAwait(false);
        return path;
    }
}

public static class AcademicContextLoadRunner
{
    private const string StudentRole = "Student";

    public static async Task<AcademicContextLoadResult> RunAsync(
        AcademicContextLoadProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        profile.Validate();
        if (!profile.IsRequiredGate)
        {
            throw new InvalidOperationException(
                "Only the exact SPEC-008 required gate is executable by this runner.");
        }

        await using var fixture = new Spec008TwoReplicaSharedSqlFixture(
            profile.IdentityCount);
        await fixture.InitializeAsync(cancellationToken).ConfigureAwait(false);
        ValidateFixture(fixture, profile);
        await WarmUpAsync(fixture, profile, cancellationToken).ConfigureAwait(false);

        var responseMilliseconds = new double[profile.TotalAttempts];
        var schedulerLagMilliseconds = new double[profile.TotalAttempts];
        var attemptsByReplica = new int[profile.ReplicaCount];
        var tasks = new Task[profile.TotalAttempts];
        using var inFlightGate = new SemaphoreSlim(
            profile.MaximumInFlight,
            profile.MaximumInFlight);
        var counters = new RunCounters();
        var startedAtUtc = DateTime.UtcNow;
        var profileStarted = Stopwatch.GetTimestamp();

        for (var attemptIndex = 0; attemptIndex < profile.TotalAttempts; attemptIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scheduledAt = profileStarted + (long)Math.Round(
                attemptIndex * (Stopwatch.Frequency / (double)profile.RequestsPerSecond),
                MidpointRounding.AwayFromZero);
            await DelayUntilAsync(scheduledAt, cancellationToken).ConfigureAwait(false);

            if (!await inFlightGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            {
                Interlocked.Increment(ref counters.BackpressureEvents);
                await inFlightGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            var dispatchedAt = Stopwatch.GetTimestamp();
            schedulerLagMilliseconds[attemptIndex] = Math.Max(
                0d,
                ElapsedMilliseconds(scheduledAt, dispatchedAt));
            var replicaIndex = attemptIndex % profile.ReplicaCount;
            Interlocked.Increment(ref attemptsByReplica[replicaIndex]);
            var observedInFlight = Interlocked.Increment(ref counters.CurrentInFlight);
            UpdateMaximum(ref counters.MaximumObservedInFlight, observedInFlight);
            tasks[attemptIndex] = ExecuteAttemptAsync(
                fixture.ReplicaClients[replicaIndex],
                fixture.StudentSessions[attemptIndex % fixture.StudentSessions.Count],
                responseMilliseconds,
                attemptIndex,
                inFlightGate,
                counters,
                cancellationToken);
        }

        var scheduledEnd = profileStarted + checked(
            (long)(profile.DurationSeconds * (double)Stopwatch.Frequency));
        await DelayUntilAsync(scheduledEnd, cancellationToken).ConfigureAwait(false);
        var schedulerCompleted = Stopwatch.GetTimestamp();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        var completedAtUtc = DateTime.UtcNow;

        Array.Sort(responseMilliseconds);
        Array.Sort(schedulerLagMilliseconds);
        return new AcademicContextLoadResult(
            profile,
            DateTime.SpecifyKind(startedAtUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(completedAtUtc, DateTimeKind.Utc),
            fixture.StudentSessions.Count,
            fixture.ReplicaClients.Count,
            profile.WarmupRequests,
            Volatile.Read(ref counters.Attempts),
            Volatile.Read(ref counters.CompletedAttempts),
            attemptsByReplica[0],
            attemptsByReplica[1],
            NearestRank(responseMilliseconds, 0.95d),
            NearestRank(schedulerLagMilliseconds, 0.95d),
            schedulerLagMilliseconds[^1],
            ElapsedMilliseconds(profileStarted, schedulerCompleted),
            Volatile.Read(ref counters.MaximumObservedInFlight),
            Volatile.Read(ref counters.BackpressureEvents),
            Volatile.Read(ref counters.NonSuccessStatusFailures),
            Volatile.Read(ref counters.InvalidContextFailures),
            Volatile.Read(ref counters.TransportFailures));
    }

    private static void ValidateFixture(
        Spec008TwoReplicaSharedSqlFixture fixture,
        AcademicContextLoadProfile profile)
    {
        if (!fixture.IsReady ||
            !fixture.CrossReplicaTicketVerified ||
            fixture.StudentSessions.Count != profile.IdentityCount ||
            fixture.ReplicaClients.Count != profile.ReplicaCount ||
            ReferenceEquals(fixture.ReplicaClients[0], fixture.ReplicaClients[1]))
        {
            throw new InvalidOperationException(
                "The exact two-replica shared-SQL authenticated fixture is not ready.");
        }
    }

    private static async Task WarmUpAsync(
        Spec008TwoReplicaSharedSqlFixture fixture,
        AcademicContextLoadProfile profile,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < profile.WarmupRequests; index++)
        {
            var replica = fixture.ReplicaClients[index % profile.ReplicaCount];
            var session = fixture.StudentSessions[index % fixture.StudentSessions.Count];
            using var request = session.CreateRequest(HttpMethod.Get, "/api/context");
            using var response = await replica.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK ||
                !await HasValidStudentContextAsync(response, cancellationToken)
                    .ConfigureAwait(false))
            {
                throw new InvalidOperationException(
                    $"Warmup request {index + 1} did not return a valid authenticated student context.");
            }
        }
    }

    private static async Task ExecuteAttemptAsync(
        HttpClient replica,
        Spec008AuthenticatedStudentSession session,
        double[] responseMilliseconds,
        int attemptIndex,
        SemaphoreSlim inFlightGate,
        RunCounters counters,
        CancellationToken cancellationToken)
    {
        var requestStarted = Stopwatch.GetTimestamp();
        Interlocked.Increment(ref counters.Attempts);
        try
        {
            using var request = session.CreateRequest(HttpMethod.Get, "/api/context");
            using var response = await replica.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Interlocked.Increment(ref counters.NonSuccessStatusFailures);
            }
            else if (!await HasValidStudentContextAsync(response, cancellationToken)
                         .ConfigureAwait(false))
            {
                Interlocked.Increment(ref counters.InvalidContextFailures);
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            Interlocked.Increment(ref counters.TransportFailures);
        }
        finally
        {
            responseMilliseconds[attemptIndex] = ElapsedMilliseconds(
                requestStarted,
                Stopwatch.GetTimestamp());
            Interlocked.Increment(ref counters.CompletedAttempts);
            Interlocked.Decrement(ref counters.CurrentInFlight);
            inFlightGate.Release();
        }
    }

    private static async Task<bool> HasValidStudentContextAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = await response.Content
                .ReadFromJsonAsync<AppContextDto>(cancellationToken)
                .ConfigureAwait(false);
            return context is not null &&
                context.SessionState == SessionState.Active &&
                string.Equals(context.ActiveRole, StudentRole, StringComparison.Ordinal) &&
                context.AuthorizedRoles.Contains(StudentRole, StringComparer.Ordinal);
        }
        catch (Exception exception) when (
            exception is JsonException or NotSupportedException or ArgumentException)
        {
            return false;
        }
    }

    private static async Task DelayUntilAsync(
        long targetTimestamp,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var remainingTicks = targetTimestamp - Stopwatch.GetTimestamp();
            if (remainingTicks <= 0)
            {
                return;
            }

            var delay = TimeSpan.FromSeconds(
                remainingTicks / (double)Stopwatch.Frequency);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private static double ElapsedMilliseconds(long started, long completed) =>
        (completed - started) * 1_000d / Stopwatch.Frequency;

    private static double NearestRank(double[] sortedSamples, double percentile)
    {
        if (sortedSamples.Length == 0 || percentile is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(percentile));
        }

        var rank = (int)Math.Ceiling(percentile * sortedSamples.Length);
        return sortedSamples[rank - 1];
    }

    private static void UpdateMaximum(ref int maximum, int candidate)
    {
        var observed = Volatile.Read(ref maximum);
        while (candidate > observed)
        {
            var exchanged = Interlocked.CompareExchange(
                ref maximum,
                candidate,
                observed);
            if (exchanged == observed)
            {
                return;
            }

            observed = exchanged;
        }
    }

    private sealed class RunCounters
    {
        public int Attempts;
        public int CompletedAttempts;
        public int CurrentInFlight;
        public int MaximumObservedInFlight;
        public int BackpressureEvents;
        public int NonSuccessStatusFailures;
        public int InvalidContextFailures;
        public int TransportFailures;
    }
}
