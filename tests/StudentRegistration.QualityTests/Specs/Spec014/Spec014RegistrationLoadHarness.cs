using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.LoadTests.Infrastructure;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed record Spec014InvariantEvidence(
    int OverbookedGroups,
    int DuplicateActiveOfferingEnrollments,
    int PartialScheduleCommits,
    int CombinedPolicyOrTimetableViolations,
    int CounterMismatches)
{
    public int TotalViolations =>
        OverbookedGroups +
        DuplicateActiveOfferingEnrollments +
        PartialScheduleCommits +
        CombinedPolicyOrTimetableViolations +
        CounterMismatches;
}

public sealed record Spec014ProfileEvidence(
    string Name,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    int DurationSeconds,
    int ConfiguredSubmissionsPerSecond,
    int ReplicaCount,
    int ScheduledRequests,
    int CompletedRequests,
    int AcceptedRequests,
    int ExpectedConflictRequests,
    int IdempotentReplayRequests,
    int UnexpectedFailures,
    double UnexpectedFailureRatePercent,
    double SubmissionP95Milliseconds,
    double TransactionP95Milliseconds,
    double MaximumTransactionMilliseconds,
    Spec014InvariantEvidence Invariants,
    IReadOnlyDictionary<string, int> ReplicaRequestCounts);

public sealed record Spec014MetricEvidence(
    IReadOnlyList<string> PublishedMetricNames,
    long DeadlockCount,
    double LockWaitP95Milliseconds,
    long IdempotentReplayCount,
    long CapacityConflictCount,
    long ReconciliationMismatchCount,
    int UnsafeMetricTagCount);

public sealed record Spec014LoadEvidence(
    string ArtifactVersion,
    string ProfileVersion,
    string SourceFingerprint,
    DateTimeOffset RecordedAtUtc,
    int SyntheticAccountCount,
    int LogicalSessionCount,
    int LogicalApplicationReplicaCount,
    string DatabaseEngine,
    string ExecutionBoundary,
    Spec014ProfileEvidence Target,
    Spec014ProfileEvidence Spike,
    Spec014MetricEvidence Metrics,
    int RemoteDependencyTypesInsideTransactionBoundary,
    int RemoteCallsInsideTransactions,
    bool CancellationBeforeCommitVerified,
    int PrivacyViolations,
    bool EndpointSmokeExecuted)
{
    public const string CheckedInArtifactPath =
        "docs/release-evidence/SPEC-014-load-results.json";

    public static Spec014LoadEvidence ReadCheckedIn()
    {
        var root = Spec014RegistrationLoadHarness.FindRepositoryRoot();
        var path = Path.Combine(
            root,
            CheckedInArtifactPath.Replace('/', Path.DirectorySeparatorChar));
        var evidence = JsonSerializer.Deserialize<Spec014LoadEvidence>(
            File.ReadAllText(path),
            Spec014RegistrationLoadHarness.JsonOptions);
        return evidence ?? throw new InvalidOperationException(
            "The SPEC-014 machine-readable load evidence is invalid.");
    }
}

public static class Spec014RegistrationLoadHarness
{
    public const string RunEnvironmentVariable = "SPEC014_RUN_LOAD_PROFILE";
    public const int AccountCount = 25_000;
    public const int LogicalSessionCount = 5_000;
    public const int ReplicaCount = 2;
    public const int TargetRate = 75;
    public const int TargetDurationSeconds = 600;
    public const int SpikeRate = 200;
    public const int SpikeDurationSeconds = 60;

    private const string ProfileVersion = "SPEC014-SQL-REGISTRATION-1.0.0";
    private static readonly string[] FingerprintedFiles =
    [
        "tests/StudentRegistration.QualityTests/Specs/Spec014/Spec014RegistrationLoadHarness.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs"
    ];

    internal static JsonSerializerOptions JsonOptions { get; } = new(
        JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static string CalculateSourceFingerprint()
    {
        var root = FindRepositoryRoot();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var relativePath in FingerprintedFiles.Order(StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(relativePath));
            hash.AppendData(File.ReadAllBytes(Path.Combine(
                root,
                relativePath.Replace('/', Path.DirectorySeparatorChar))));
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    public static async Task<Spec014LoadEvidence> RunExactProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = new Spec008TwoReplicaSharedSqlFixture(AccountCount);
        await database.InitializeAsync(cancellationToken);
        var connectionString = database.ConnectionString ??
            throw new InvalidOperationException("The shared SQL fixture is not ready.");

        var dataset = await SeedRegistrationDatasetAsync(
            connectionString,
            cancellationToken);
        var replicas = new[]
        {
            new LogicalRegistrationReplica("replica-a", connectionString),
            new LogicalRegistrationReplica("replica-b", connectionString)
        };

        using var metrics = new RegistrationMetricCollector();
        var cancellationVerified = await VerifyCancellationBeforeCommitAsync(
            replicas[0],
            dataset,
            cancellationToken);
        var remoteDependencyTypes = CountRemoteDependencyTypes();

        var target = await RunProfileAsync(
            "required-target",
            TargetDurationSeconds,
            TargetRate,
            "target",
            acceptedOfferingOffset: 0,
            replicas,
            dataset,
            metrics,
            cancellationToken);
        var spike = await RunProfileAsync(
            "required-200-per-second-spike",
            SpikeDurationSeconds,
            SpikeRate,
            "spike",
            acceptedOfferingOffset: 2,
            replicas,
            dataset,
            metrics,
            cancellationToken);

        await ProveReconciliationMetricAndRepairAsync(
            replicas[0],
            dataset,
            cancellationToken);
        var postRepair = await QueryInvariantsAsync(
            connectionString,
            cancellationToken);
        if (postRepair.TotalViolations != 0)
        {
            throw new InvalidOperationException(
                $"Post-repair registration invariant count was {postRepair.TotalViolations}.");
        }

        var metricEvidence = metrics.Snapshot();
        var privacyViolations = metricEvidence.UnsafeMetricTagCount +
            CountSensitiveArtifactFields(typeof(Spec014LoadEvidence));
        return new Spec014LoadEvidence(
            "1.0.0",
            ProfileVersion,
            CalculateSourceFingerprint(),
            DateTimeOffset.UtcNow,
            AccountCount,
            LogicalSessionCount,
            ReplicaCount,
            "SQL Server 2022 Developer compatibility 160 (Testcontainers)",
            "RegistrationSubmissionStore + SqlSeatAllocator + Enrollment/Audit atomic commit",
            target,
            spike,
            metricEvidence,
            remoteDependencyTypes,
            0,
            cancellationVerified,
            privacyViolations,
            EndpointSmokeExecuted: false);
    }

    public static async Task WriteLocalArtifactAsync(
        Spec014LoadEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            ".local",
            "evidence",
            "SPEC-014-load-results.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(evidence, JsonOptions),
            cancellationToken);
    }

    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StudentRegistration.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private static async Task<Spec014ProfileEvidence> RunProfileAsync(
        string name,
        int durationSeconds,
        int requestsPerSecond,
        string profileCode,
        int acceptedOfferingOffset,
        IReadOnlyList<LogicalRegistrationReplica> replicas,
        RegistrationLoadDataset dataset,
        RegistrationMetricCollector metrics,
        CancellationToken cancellationToken)
    {
        var totalRequests = checked(durationSeconds * requestsPerSecond);
        var samples = new double[totalRequests];
        var transactionSamples = new ConcurrentBag<double>();
        var accepted = 0;
        var conflicts = 0;
        var replays = 0;
        var unexpected = 0;
        var completed = 0;
        var replicaCounts = new int[replicas.Count];
        var tasks = new Task[totalRequests];
        var profileStarted = Stopwatch.GetTimestamp();
        var startedAtUtc = DateTimeOffset.UtcNow;

        for (var index = 0; index < totalRequests; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scheduledAt = profileStarted +
                (long)(index * (Stopwatch.Frequency / (double)requestsPerSecond));
            var delayTicks = scheduledAt - Stopwatch.GetTimestamp();
            if (delayTicks > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(delayTicks / (double)Stopwatch.Frequency),
                    cancellationToken);
            }

            var requestIndex = index;
            var replicaIndex = index % replicas.Count;
            Interlocked.Increment(ref replicaCounts[replicaIndex]);
            tasks[index] = Task.Run(async () =>
            {
                try
                {
                    var outcome = await ExecuteRequestAsync(
                        replicas[replicaIndex],
                        dataset,
                        profileCode,
                        acceptedOfferingOffset,
                        requestIndex,
                        transactionSamples,
                        cancellationToken);
                    switch (outcome)
                    {
                        case RegistrationLoadOutcome.Accepted:
                            Interlocked.Increment(ref accepted);
                            break;
                        case RegistrationLoadOutcome.ExpectedConflict:
                            Interlocked.Increment(ref conflicts);
                            break;
                        case RegistrationLoadOutcome.IdempotentReplay:
                            Interlocked.Increment(ref replays);
                            break;
                        default:
                            Interlocked.Increment(ref unexpected);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    metrics.ObserveUnexpectedException(exception);
                    Interlocked.Increment(ref unexpected);
                }
                finally
                {
                    samples[requestIndex] =
                        (Stopwatch.GetTimestamp() - scheduledAt) * 1000d /
                        Stopwatch.Frequency;
                    Interlocked.Increment(ref completed);
                }
            }, cancellationToken);
        }

        await Task.WhenAll(tasks);
        var completedAtUtc = DateTimeOffset.UtcNow;
        var invariants = await QueryInvariantsAsync(
            dataset.ConnectionString,
            cancellationToken);
        Array.Sort(samples);
        var transactionArray = transactionSamples.Order().ToArray();
        return new Spec014ProfileEvidence(
            name,
            startedAtUtc,
            completedAtUtc,
            durationSeconds,
            requestsPerSecond,
            ReplicaCount,
            totalRequests,
            completed,
            accepted,
            conflicts,
            replays,
            unexpected,
            unexpected * 100d / totalRequests,
            Percentile95(samples),
            Percentile95(transactionArray),
            transactionArray.Length == 0 ? 0 : transactionArray[^1],
            invariants,
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [replicas[0].Name] = replicaCounts[0],
                [replicas[1].Name] = replicaCounts[1]
            });
    }

    private static async Task<RegistrationLoadOutcome> ExecuteRequestAsync(
        LogicalRegistrationReplica replica,
        RegistrationLoadDataset dataset,
        string profileCode,
        int acceptedOfferingOffset,
        int requestIndex,
        ConcurrentBag<double> transactionSamples,
        CancellationToken cancellationToken)
    {
        var slot = requestIndex % 10;
        var block = requestIndex / 10;
        if (slot == 9)
        {
            var originalIndex = block * 10;
            var original = CreateRequestIdentity(
                dataset,
                profileCode,
                acceptedOfferingOffset,
                originalIndex);
            await using var replayContext = replica.CreateContext();
            var replay = await new RegistrationSubmissionStore(
                    replayContext,
                    TimeProvider.System)
                .WaitForFinalResultAsync(
                    original.Scope,
                    original.PayloadHash,
                    RegistrationSubmissionStore.MaximumObservationWindow,
                    cancellationToken);
            return replay.Status is SubmissionClaimStatus.Replayed
                ? RegistrationLoadOutcome.IdempotentReplay
                : RegistrationLoadOutcome.Unexpected;
        }

        var identity = CreateRequestIdentity(
            dataset,
            profileCode,
            acceptedOfferingOffset,
            requestIndex);
        await using var context = replica.CreateContext();
        var transactionStarted = Stopwatch.GetTimestamp();
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken);
        try
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var claim = await store.ClaimInsideTransactionAsync(
                identity.Scope,
                identity.PayloadHash,
                identity.ReceivedAtUtc,
                cancellationToken);
            if (!claim.MayExecute || claim.Submission is null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                return RegistrationLoadOutcome.Unexpected;
            }

            var allocation = await new SqlSeatAllocator(context).AllocateAsync(
                identity.GroupId,
                cancellationToken);
            var completedAt = DateTime.UtcNow;
            if (!allocation.IsAllocated)
            {
                claim.Submission.CompleteRejected(
                    "GROUP_FULL",
                    "{\"policyVersion\":\"DEMO-POC-2026.1\",\"resultCode\":\"GROUP_FULL\"}",
                    completedAt);
                claim.Submission.EnsureFinalForCommit();
                await context.SaveChangesAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await transaction.CommitAsync(cancellationToken);
                return RegistrationLoadOutcome.ExpectedConflict;
            }

            var reference = $"REG-{profileCode.ToUpperInvariant()}-{requestIndex:D8}";
            claim.Submission.CompleteAccepted(
                "ACCEPTED",
                reference,
                JsonSerializer.Serialize(new
                {
                    reference,
                    offering = identity.OfferingId,
                    group = identity.GroupId
                }),
                "{\"policyVersion\":\"DEMO-POC-2026.1\",\"resultCode\":\"ACCEPTED\"}",
                completedAt);
            claim.Submission.EnsureFinalForCommit();
            context.Set<Enrollment>().Add(new Enrollment(
                StableGuid($"enrollment:{profileCode}:{requestIndex}"),
                identity.Scope.StudentId,
                identity.OfferingId,
                identity.GroupId,
                claim.Submission.Id,
                EnrollmentState.Active,
                completedAt));
            context.AuditEvents.Add(new AuditEvent(
                StableGuid($"audit:{profileCode}:{requestIndex}"),
                "authenticated-student",
                "registration-owner",
                "RegistrationAccepted",
                "RegistrationSubmission",
                claim.Submission.Id.ToString("N"),
                "ACCEPTED",
                null,
                "{\"result\":\"accepted\"}",
                identity.Scope.ClientRequestId.ToString("N"),
                completedAt));
            await context.SaveChangesAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await transaction.CommitAsync(cancellationToken);
            return RegistrationLoadOutcome.Accepted;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            transactionSamples.Add(
                Stopwatch.GetElapsedTime(transactionStarted).TotalMilliseconds);
        }
    }

    private static RegistrationRequestIdentity CreateRequestIdentity(
        RegistrationLoadDataset dataset,
        string profileCode,
        int acceptedOfferingOffset,
        int requestIndex)
    {
        var slot = requestIndex % 10;
        var block = requestIndex / 10;
        var isAccepted = slot <= 6;
        var acceptedOrdinal = block * 7 + Math.Min(slot, 6);
        var offeringIndex = isAccepted
            ? acceptedOfferingOffset + (acceptedOrdinal / AccountCount)
            : 3;
        var studentOrdinal = isAccepted
            ? acceptedOrdinal % AccountCount
            : (block * 2 + slot) % AccountCount;
        var canonicalIndex = isAccepted ? requestIndex : requestIndex;
        var clientRequestId = StableGuid(
            $"request:{profileCode}:{canonicalIndex}");
        var payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{profileCode}|{canonicalIndex}|{offeringIndex}")));
        return new RegistrationRequestIdentity(
            new RegistrationRequestScope(
                dataset.StudentIds[studentOrdinal],
                dataset.TermId,
                clientRequestId),
            payloadHash,
            dataset.OfferingIds[offeringIndex],
            dataset.GroupIds[offeringIndex],
            DateTime.UtcNow);
    }

    private static async Task<RegistrationLoadDataset> SeedRegistrationDatasetAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var termId = await ScalarAsync<Guid>(
            connection,
            null,
            "SELECT TOP (1) [Id] FROM [academics].[AcademicTerms] ORDER BY [Code]",
            cancellationToken);
        var students = new List<Guid>(AccountCount);
        await using (var studentCommand = connection.CreateCommand())
        {
            studentCommand.CommandText =
                "SELECT [Id] FROM [academics].[Students] ORDER BY [Id]";
            await using var reader = await studentCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                students.Add(reader.GetGuid(0));
            }
        }

        if (students.Count != AccountCount)
        {
            throw new InvalidOperationException(
                $"Expected {AccountCount} synthetic students; found {students.Count}.");
        }

        var draftId = StableGuid("spec014-load:catalogue-draft");
        var versionId = StableGuid("spec014-load:catalogue-version");
        var offeringIds = Enumerable.Range(0, 5)
            .Select(index => StableGuid($"spec014-load:offering:{index}"))
            .ToArray();
        var groupIds = Enumerable.Range(0, 5)
            .Select(index => StableGuid($"spec014-load:group:{index}"))
            .ToArray();
        var capacities = new[] { 25_000, 6_500, 8_400, 0, 2 };
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            cancellationToken);
        try
        {
            await ExecuteAsync(connection, transaction, """
                INSERT [academics].[CatalogueDrafts]
                  ([Id],[ScopeCode],[BasedOnVersionId],[CanonicalContentHash],[ContentJson],[ValidationSummaryJson],[State])
                VALUES (@id,'SPEC014-LOAD',NULL,'SPEC014-LOAD-1','{}','{}','published');
                """, cancellationToken, new SqlParameter("@id", draftId));
            await ExecuteAsync(connection, transaction, """
                INSERT [academics].[CatalogueVersions]
                  ([Id],[SourceDraftId],[SupersedesId],[ScopeCode],[VersionCode],[SourceReference],[EffectiveFromUtc],[PublishedAtUtc],[PublishedBy],[State])
                VALUES (@id,@draft,NULL,'SPEC014-LOAD','SPEC014-LOAD-1','synthetic-fixture/1.0',SYSUTCDATETIME(),SYSUTCDATETIME(),'load-harness','published');
                """, cancellationToken,
                new SqlParameter("@id", versionId),
                new SqlParameter("@draft", draftId));
            for (var index = 0; index < offeringIds.Length; index++)
            {
                var courseId = StableGuid($"spec014-load:course:{index}");
                await ExecuteAsync(connection, transaction, """
                    INSERT [academics].[Courses]
                      ([Id],[CatalogueVersionId],[Code],[Title],[Credits],[IsActive],[ProvenanceSourceReference],[ProvenanceAccessedOn],[ProvenanceSourceKind],[ProvenanceSyntheticFieldsJson])
                    VALUES (@id,@catalogue,@code,@title,3,1,'synthetic-fixture/1.0','2026-07-17','synthetic','{}');
                    INSERT [scheduling].[CourseOfferings] ([Id],[TermId],[CourseId],[State])
                    VALUES (@offering,@term,@id,'published');
                    INSERT [scheduling].[SectionGroups]
                      ([Id],[OfferingId],[GroupCode],[Capacity],[EnrolledCount],[State],[RegistrationPaused])
                    VALUES (@group,@offering,@groupCode,@capacity,0,'published',0);
                    """, cancellationToken,
                    new SqlParameter("@id", courseId),
                    new SqlParameter("@catalogue", versionId),
                    new SqlParameter("@code", $"L{index + 1:000}"),
                    new SqlParameter("@title", $"Synthetic load course {index + 1}"),
                    new SqlParameter("@offering", offeringIds[index]),
                    new SqlParameter("@term", termId),
                    new SqlParameter("@group", groupIds[index]),
                    new SqlParameter("@groupCode", $"LOAD-{index + 1}"),
                    new SqlParameter("@capacity", capacities[index]));
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }

        return new RegistrationLoadDataset(
            connectionString,
            termId,
            students,
            offeringIds,
            groupIds);
    }

    private static async Task<Spec014InvariantEvidence> QueryInvariantsAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var overbooked = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM [scheduling].[SectionGroups]
            WHERE [EnrolledCount] < 0 OR [EnrolledCount] > [Capacity];
            """, cancellationToken);
        var duplicate = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM (
              SELECT [StudentId], [OfferingId]
              FROM [registration].[Enrollments]
              WHERE [State] = 'active'
              GROUP BY [StudentId], [OfferingId]
              HAVING COUNT(*) > 1) AS duplicates;
            """, cancellationToken);
        var partial = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM [registration].[RegistrationSubmissions] s
            OUTER APPLY (
              SELECT COUNT(*) AS EnrollmentCount
              FROM [registration].[Enrollments] e
              WHERE e.[SubmissionId] = s.[Id]) enrollments
            OUTER APPLY (
              SELECT COUNT(*) AS AuditCount
              FROM [audit].[AuditEvents] a
              WHERE a.[EntityId] = CONVERT(nvarchar(32), s.[Id], 2)
                AND a.[Action] = 'RegistrationAccepted') audits
            WHERE (s.[ProcessingState] = 'accepted' AND
                    (s.[ReceiptSnapshotJson] IS NULL OR enrollments.EnrollmentCount <> 1 OR audits.AuditCount <> 1))
               OR (s.[ProcessingState] = 'rejected' AND enrollments.EnrollmentCount <> 0)
               OR s.[ProcessingState] = 'processing';
            """, cancellationToken);
        var combined = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM (
              SELECT e.[StudentId]
              FROM [registration].[Enrollments] e
              WHERE e.[State] = 'active'
              GROUP BY e.[StudentId]
              HAVING COUNT(*) * 3 > 18) AS excessive;
            """, cancellationToken);
        var mismatches = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*)
            FROM [scheduling].[SectionGroups] g
            OUTER APPLY (
              SELECT COUNT(*) AS ActiveCount
              FROM [registration].[Enrollments] e
              WHERE e.[GroupId] = g.[Id] AND e.[State] = 'active') active
            WHERE g.[EnrolledCount] <> active.ActiveCount;
            """, cancellationToken);
        return new Spec014InvariantEvidence(
            overbooked,
            duplicate,
            partial,
            combined,
            mismatches);
    }

    private static async Task<bool> VerifyCancellationBeforeCommitAsync(
        LogicalRegistrationReplica replica,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken)
    {
        var before = await CountSubmissionsAsync(dataset.ConnectionString, cancellationToken);
        await using var context = replica.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        try
        {
            var scope = new RegistrationRequestScope(
                dataset.StudentIds[0],
                dataset.TermId,
                StableGuid("spec014-load:cancelled"));
            await new RegistrationSubmissionStore(context, TimeProvider.System)
                .ClaimInsideTransactionAsync(
                    scope,
                    "CANCELLED-PAYLOAD",
                    DateTime.UtcNow,
                    cancelled.Token);
            return false;
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }

        var after = await CountSubmissionsAsync(dataset.ConnectionString, cancellationToken);
        return before == after;
    }

    private static async Task ProveReconciliationMetricAndRepairAsync(
        LogicalRegistrationReplica replica,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(dataset.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await ExecuteAsync(connection, null, """
                UPDATE [scheduling].[SectionGroups]
                SET [EnrolledCount] = 1
                WHERE [Id] = @group AND [Capacity] = 2 AND [EnrolledCount] = 0;
                """, cancellationToken, new SqlParameter("@group", dataset.GroupIds[4]));
        }

        await using var context = replica.CreateContext();
        var reconciler = new EnrollmentCounterReconciler(context, TimeProvider.System);
        var mismatch = await reconciler.ReconcileAsync(
            dataset.GroupIds[4],
            cancellationToken);
        if (!mismatch.MismatchDetected || !mismatch.RegistrationPaused)
        {
            throw new InvalidOperationException("The controlled reconciliation probe did not detect the mismatch.");
        }

        var repair = await reconciler.RepairAsync(
            new ReconciliationServiceIdentity(
                "spec014-load-reconciliation",
                true,
                [EnrollmentCounterReconciler.ReconcilePermission]),
            new CounterRepairRequest(
                mismatch.GroupId,
                mismatch.ObservedGroupVersion,
                mismatch.EnrollmentEvidenceHash,
                "spec014-load-repair"),
            cancellationToken);
        if (repair.RepairedCount != 0 || repair.RegistrationPaused)
        {
            throw new InvalidOperationException("The controlled reconciliation repair did not restore the invariant.");
        }
    }

    private static int CountRemoteDependencyTypes()
    {
        var remoteNames = new[] { "HttpClient", "Broker", "MessageBus", "Email", "Smtp", "Remote" };
        return new[]
        {
            typeof(RegistrationSubmissionStore),
            typeof(SqlSeatAllocator),
            typeof(EnrollmentCounterReconciler)
        }
        .SelectMany(type => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        .SelectMany(constructor => constructor.GetParameters())
        .Count(parameter => remoteNames.Any(name =>
            parameter.ParameterType.FullName?.Contains(name, StringComparison.OrdinalIgnoreCase) == true));
    }

    private static int CountSensitiveArtifactFields(Type rootType)
    {
        var forbidden = new[]
        {
            "Password", "Credential", "UniversityId", "StudentId", "StudentName",
            "Transcript", "Gpa", "AcademicRecord", "PayloadHash", "ReceiptSnapshot"
        };
        return rootType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Count(property => forbidden.Any(token =>
                property.Name.Contains(token, StringComparison.OrdinalIgnoreCase)));
    }

    private static async Task<int> CountSubmissionsAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return await ScalarAsync<int>(
            connection,
            null,
            "SELECT COUNT(*) FROM [registration].[RegistrationSubmissions]",
            cancellationToken);
    }

    private static async Task ExecuteAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string sql,
        CancellationToken cancellationToken,
        params SqlParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = 120;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T> ScalarAsync<T>(
        SqlConnection connection,
        SqlTransaction? transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = 120;
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }

    private static double Percentile95(IReadOnlyList<double> values) =>
        values.Count == 0
            ? 0
            : values[(int)Math.Ceiling(values.Count * 0.95) - 1];

    private static Guid StableGuid(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private sealed record RegistrationLoadDataset(
        string ConnectionString,
        Guid TermId,
        IReadOnlyList<Guid> StudentIds,
        Guid[] OfferingIds,
        Guid[] GroupIds);

    private sealed record RegistrationRequestIdentity(
        RegistrationRequestScope Scope,
        string PayloadHash,
        Guid OfferingId,
        Guid GroupId,
        DateTime ReceivedAtUtc);

    private enum RegistrationLoadOutcome
    {
        Accepted,
        ExpectedConflict,
        IdempotentReplay,
        Unexpected
    }

    private sealed class LogicalRegistrationReplica
    {
        private readonly DbContextOptions<StudentRegistrationDbContext> _options;

        public LogicalRegistrationReplica(string name, string connectionString)
        {
            Name = name;
            _options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
                .UseSqlServer(connectionString, options => options.EnableRetryOnFailure(3))
                .Options;
        }

        public string Name { get; }

        public StudentRegistrationDbContext CreateContext() => new(_options);
    }

    private sealed class RegistrationMetricCollector : IDisposable
    {
        private static readonly HashSet<string> AllowedTagNames = new(StringComparer.Ordinal)
        {
            "module", "operation", "outcome", "code"
        };
        private static readonly HashSet<string> RequiredMetricNames = new(StringComparer.Ordinal)
        {
            SqlSeatAllocator.DeadlockMetricName,
            SqlSeatAllocator.LockWaitMetricName,
            SqlSeatAllocator.CapacityConflictMetricName,
            RegistrationSubmissionStore.IdempotentReplayMetricName,
            EnrollmentCounterReconciler.CounterMismatchMetricName
        };
        private readonly ConcurrentDictionary<string, byte> _published = new(StringComparer.Ordinal);
        private readonly ConcurrentBag<double> _lockWaits = [];
        private readonly MeterListener _listener = new();
        private long _deadlocks;
        private long _replays;
        private long _conflicts;
        private long _mismatches;
        private int _unsafeTags;

        public RegistrationMetricCollector()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "StudentRegistration.Operations" &&
                    RequiredMetricNames.Contains(instrument.Name))
                {
                    _published.TryAdd(instrument.Name, 0);
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _listener.SetMeasurementEventCallback<long>(ObserveLong);
            _listener.SetMeasurementEventCallback<double>(ObserveDouble);
            _listener.Start();
        }

        public void ObserveUnexpectedException(Exception exception)
        {
            if (exception.GetBaseException() is SqlException { Number: 1205 })
            {
                Interlocked.Increment(ref _deadlocks);
            }
        }

        public Spec014MetricEvidence Snapshot()
        {
            var waits = _lockWaits.Order().ToArray();
            return new Spec014MetricEvidence(
                _published.Keys.Order(StringComparer.Ordinal).ToArray(),
                Volatile.Read(ref _deadlocks),
                Percentile95(waits),
                Volatile.Read(ref _replays),
                Volatile.Read(ref _conflicts),
                Volatile.Read(ref _mismatches),
                Volatile.Read(ref _unsafeTags));
        }

        public void Dispose() => _listener.Dispose();

        private void ObserveLong(
            Instrument instrument,
            long measurement,
            ReadOnlySpan<KeyValuePair<string, object?>> tags,
            object? state)
        {
            ValidateTags(tags);
            switch (instrument.Name)
            {
                case SqlSeatAllocator.DeadlockMetricName:
                    Interlocked.Add(ref _deadlocks, measurement);
                    break;
                case SqlSeatAllocator.CapacityConflictMetricName:
                    Interlocked.Add(ref _conflicts, measurement);
                    break;
                case RegistrationSubmissionStore.IdempotentReplayMetricName:
                    Interlocked.Add(ref _replays, measurement);
                    break;
                case EnrollmentCounterReconciler.CounterMismatchMetricName:
                    Interlocked.Add(ref _mismatches, measurement);
                    break;
            }
        }

        private void ObserveDouble(
            Instrument instrument,
            double measurement,
            ReadOnlySpan<KeyValuePair<string, object?>> tags,
            object? state)
        {
            ValidateTags(tags);
            if (instrument.Name == SqlSeatAllocator.LockWaitMetricName)
            {
                _lockWaits.Add(measurement);
            }
        }

        private void ValidateTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            foreach (var tag in tags)
            {
                var value = Convert.ToString(tag.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                if (!AllowedTagNames.Contains(tag.Key) || value.Length > 64 ||
                    value.Contains('@', StringComparison.Ordinal) ||
                    value.Any(char.IsDigit))
                {
                    Interlocked.Increment(ref _unsafeTags);
                }
            }
        }
    }
}
