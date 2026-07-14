namespace StudentRegistration.LoadTesting.Spec018;

public enum LoadProfileClassification
{
    Blocking,
    Diagnostic
}

public sealed record SubmissionMix(
    int ValidUniquePercentage,
    int ExpectedRejectionPercentage,
    int IdempotentRetryPercentage)
{
    public int TotalPercentage =>
        ValidUniquePercentage + ExpectedRejectionPercentage + IdempotentRetryPercentage;
}

public sealed record ReadMix(
    int DiscoveryPercentage,
    int EligibilityPercentage,
    int PlanAndTimetablePercentage,
    int RegistrationRecordsPercentage)
{
    public int TotalPercentage =>
        DiscoveryPercentage +
        EligibilityPercentage +
        PlanAndTimetablePercentage +
        RegistrationRecordsPercentage;
}

public sealed record ProjectedRequestCounts(
    int TotalRegistrationSubmissions,
    int TotalReads,
    int TotalRequests,
    int ValidUniqueSubmissions,
    int ExpectedDomainRejections,
    int IdempotentRetries,
    int DiscoveryReads,
    int EligibilityReads,
    int PlanAndTimetableReads,
    int RegistrationRecordReads);

public sealed record LoadProfileDefinition(
    string Name,
    TimeSpan Duration,
    int RegistrationSubmissionsPerSecond,
    int ReadsPerSecond,
    int MinimumReplicaCount,
    LoadProfileClassification Classification,
    bool EnforceTargetResponseBudgets,
    bool RequireStatelessReplicas,
    bool RequireZeroCorrectnessInvariants,
    TimeSpan? FailureInjectionAt = null,
    int ReplicasToRemove = 0)
{
    public bool IsReleaseBlocking => Classification == LoadProfileClassification.Blocking;

    public ProjectedRequestCounts ProjectCounts()
    {
        var seconds = checked((int)Duration.TotalSeconds);
        var submissions = checked(seconds * RegistrationSubmissionsPerSecond);
        var reads = checked(seconds * ReadsPerSecond);

        return new ProjectedRequestCounts(
            TotalRegistrationSubmissions: submissions,
            TotalReads: reads,
            TotalRequests: checked(submissions + reads),
            ValidUniqueSubmissions: PercentageOf(
                submissions,
                ExactLoadProfileCatalog.SubmissionMix.ValidUniquePercentage),
            ExpectedDomainRejections: PercentageOf(
                submissions,
                ExactLoadProfileCatalog.SubmissionMix.ExpectedRejectionPercentage),
            IdempotentRetries: PercentageOf(
                submissions,
                ExactLoadProfileCatalog.SubmissionMix.IdempotentRetryPercentage),
            DiscoveryReads: PercentageOf(
                reads,
                ExactLoadProfileCatalog.ReadMix.DiscoveryPercentage),
            EligibilityReads: PercentageOf(
                reads,
                ExactLoadProfileCatalog.ReadMix.EligibilityPercentage),
            PlanAndTimetableReads: PercentageOf(
                reads,
                ExactLoadProfileCatalog.ReadMix.PlanAndTimetablePercentage),
            RegistrationRecordReads: PercentageOf(
                reads,
                ExactLoadProfileCatalog.ReadMix.RegistrationRecordsPercentage));
    }

    private static int PercentageOf(int total, int percentage)
    {
        var scaled = checked(total * percentage);
        if (scaled % 100 != 0)
        {
            throw new InvalidOperationException(
                $"Profile count {total} cannot be split exactly at {percentage}%.");
        }

        return scaled / 100;
    }
}

public static class ExactLoadProfileCatalog
{
    public static SubmissionMix SubmissionMix { get; } = new(
        ValidUniquePercentage: 70,
        ExpectedRejectionPercentage: 20,
        IdempotentRetryPercentage: 10);

    public static ReadMix ReadMix { get; } = new(
        DiscoveryPercentage: 50,
        EligibilityPercentage: 25,
        PlanAndTimetablePercentage: 15,
        RegistrationRecordsPercentage: 10);

    public static LoadProfileDefinition Target { get; } = Required(
        name: "required-target",
        duration: TimeSpan.FromMinutes(10),
        submissionsPerSecond: 75,
        readsPerSecond: 300,
        enforceTargetResponseBudgets: true);

    public static LoadProfileDefinition RequiredSpike { get; } = Required(
        name: "required-200-per-second-spike",
        duration: TimeSpan.FromSeconds(60),
        submissionsPerSecond: 200,
        readsPerSecond: 0,
        enforceTargetResponseBudgets: false);

    // EC-2 uses the exact target traffic; only the deterministic fault differs.
    public static LoadProfileDefinition ReplicaFailoverTarget { get; } = Required(
        name: "required-target-replica-failover",
        duration: TimeSpan.FromMinutes(10),
        submissionsPerSecond: 75,
        readsPerSecond: 300,
        enforceTargetResponseBudgets: true,
        failureInjectionAt: TimeSpan.FromMinutes(5),
        replicasToRemove: 1);

    public static IReadOnlyList<LoadProfileDefinition> RequiredProfiles { get; } =
        new[] { Target, RequiredSpike, ReplicaFailoverTarget };

    public static IReadOnlyList<LoadProfileDefinition> OptionalDiagnostics { get; } =
        new[]
        {
            Diagnostic(
                "diagnostic-2x-target",
                TimeSpan.FromMinutes(10),
                submissionsPerSecond: 150,
                readsPerSecond: 600),
            Diagnostic(
                "diagnostic-5x-target",
                TimeSpan.FromMinutes(10),
                submissionsPerSecond: 375,
                readsPerSecond: 1_500),
            Diagnostic(
                "diagnostic-120-minute-soak",
                TimeSpan.FromMinutes(120),
                submissionsPerSecond: 75,
                readsPerSecond: 300)
        };

    private static LoadProfileDefinition Required(
        string name,
        TimeSpan duration,
        int submissionsPerSecond,
        int readsPerSecond,
        bool enforceTargetResponseBudgets,
        TimeSpan? failureInjectionAt = null,
        int replicasToRemove = 0) =>
        new(
            name,
            duration,
            submissionsPerSecond,
            readsPerSecond,
            MinimumReplicaCount: 2,
            LoadProfileClassification.Blocking,
            enforceTargetResponseBudgets,
            RequireStatelessReplicas: true,
            RequireZeroCorrectnessInvariants: true,
            failureInjectionAt,
            replicasToRemove);

    private static LoadProfileDefinition Diagnostic(
        string name,
        TimeSpan duration,
        int submissionsPerSecond,
        int readsPerSecond) =>
        new(
            name,
            duration,
            submissionsPerSecond,
            readsPerSecond,
            MinimumReplicaCount: 2,
            LoadProfileClassification.Diagnostic,
            EnforceTargetResponseBudgets: false,
            RequireStatelessReplicas: true,
            RequireZeroCorrectnessInvariants: true);
}

public static class LoadGateThresholds
{
    public const int CatalogueP95Milliseconds = 300;
    public const int CommitP95Milliseconds = 2_000;
    public const int OptimizerP95Milliseconds = 500;
    public const decimal MaximumUnexpectedFailureRateExclusive = 0.001m;
}

public sealed record LoadInvariantCounters(
    int OverbookedSeats,
    int DuplicateActiveOfferingEnrollments,
    int PartialAtomicSubmissions);

public sealed record LoadRunMeasurement(
    int ReplicaCount,
    double CatalogueP95Milliseconds,
    double CommitP95Milliseconds,
    double OptimizerP95Milliseconds,
    int TotalRequests,
    int UnexpectedServerFailures,
    LoadInvariantCounters Invariants);

public sealed record LoadGateEvaluation(bool Passed, IReadOnlyList<string> Failures);

public static class LoadGateEvaluator
{
    public static LoadGateEvaluation EvaluateTarget(LoadRunMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);

        var failures = new List<string>();
        if (measurement.ReplicaCount < ExactLoadProfileCatalog.Target.MinimumReplicaCount)
        {
            failures.Add("ReplicaCount");
        }

        if (!WithinBudget(
                measurement.CatalogueP95Milliseconds,
                LoadGateThresholds.CatalogueP95Milliseconds))
        {
            failures.Add("CatalogueP95");
        }

        if (!WithinBudget(
                measurement.CommitP95Milliseconds,
                LoadGateThresholds.CommitP95Milliseconds))
        {
            failures.Add("CommitP95");
        }

        if (!WithinBudget(
                measurement.OptimizerP95Milliseconds,
                LoadGateThresholds.OptimizerP95Milliseconds))
        {
            failures.Add("OptimizerP95");
        }

        if (!UnexpectedFailureRatePasses(
                measurement.UnexpectedServerFailures,
                measurement.TotalRequests))
        {
            failures.Add("UnexpectedFailureRate");
        }

        if (measurement.Invariants.OverbookedSeats != 0)
        {
            failures.Add("OverbookedSeats");
        }

        if (measurement.Invariants.DuplicateActiveOfferingEnrollments != 0)
        {
            failures.Add("DuplicateActiveOfferingEnrollments");
        }

        if (measurement.Invariants.PartialAtomicSubmissions != 0)
        {
            failures.Add("PartialAtomicSubmissions");
        }

        return new LoadGateEvaluation(failures.Count == 0, failures);
    }

    public static bool UnexpectedFailureRatePasses(
        int unexpectedServerFailures,
        int totalRequests)
    {
        if (unexpectedServerFailures < 0 || totalRequests <= 0 ||
            unexpectedServerFailures > totalRequests)
        {
            return false;
        }

        var rate = (decimal)unexpectedServerFailures / totalRequests;
        return rate < LoadGateThresholds.MaximumUnexpectedFailureRateExclusive;
    }

    private static bool WithinBudget(double measuredMilliseconds, int budgetMilliseconds) =>
        double.IsFinite(measuredMilliseconds) &&
        measuredMilliseconds >= 0 &&
        measuredMilliseconds <= budgetMilliseconds;
}
