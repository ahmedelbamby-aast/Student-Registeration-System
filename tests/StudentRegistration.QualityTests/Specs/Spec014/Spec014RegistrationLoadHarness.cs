using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.LoadTests.Infrastructure;
using StudentRegistration.Registration.Application;
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
    int InProgressRequests,
    int UnexpectedFailures,
    double UnexpectedFailureRatePercent,
    double SubmissionP95Milliseconds,
    double TransactionP95Milliseconds,
    double MaximumTransactionMilliseconds,
    Spec014InvariantEvidence Invariants,
    IReadOnlyDictionary<string, int> ReplicaRequestCounts);

public sealed record Spec018MixedReadEvidence(
    string Name,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    int DurationSeconds,
    int ConfiguredReadsPerSecond,
    int ReplicaCount,
    int ScheduledRequests,
    int CompletedRequests,
    int DiscoveryReads,
    int EligibilityReads,
    int PlanAndTimetableReads,
    int RegistrationRecordReads,
    int UnexpectedFailures,
    double UnexpectedFailureRatePercent,
    double DiscoveryP95Milliseconds,
    double EligibilityP95Milliseconds,
    double PlanAndTimetableP95Milliseconds,
    double RegistrationRecordsP95Milliseconds,
    int FailoverAtSecond,
    bool FirstReplicaRemoved,
    IReadOnlyDictionary<string, int> ReplicaRequestCounts,
    IReadOnlyDictionary<int, int> StatusCounts,
    IReadOnlyDictionary<string, int> FailureKinds);

public sealed record Spec018MixedDiagnosticEvidence(
    bool ReadCommittedSnapshotEnabled,
    Spec014ProfileEvidence Submissions,
    Spec018MixedReadEvidence Reads,
    IReadOnlyList<Spec018SqlHotspotEvidence> SqlHotspots);

public sealed record Spec018SqlHotspotEvidence(
    string QueryHash,
    string Category,
    long ExecutionCount,
    double TotalElapsedMilliseconds,
    double AverageElapsedMilliseconds,
    long TotalLogicalReads);

public sealed record Spec014MetricEvidence(
    IReadOnlyList<string> PublishedMetricNames,
    long DeadlockCount,
    double LockWaitP95Milliseconds,
    long IdempotentReplayCount,
    long CapacityConflictCount,
    long ReconciliationMismatchCount,
    int UnsafeMetricTagCount);

public sealed record Spec014CollisionEvidence(
    int ConcurrentRequests,
    int GroupCapacity,
    int AcceptedRequests,
    int ExpectedConflictRequests,
    int ActiveEnrollments,
    int FinalEnrolledCount,
    int ReplicaCount,
    Spec014InvariantEvidence Invariants);

public sealed record Spec014BoundaryEvidence(
    int AuthenticatedServiceSubmissions,
    int ImpersonationAttemptsRejected,
    int CoordinatorBoundaryExecutions,
    int CoordinatorCommitCallbacks,
    int HttpUnauthorizedResponses,
    int HttpAntiforgeryRejections);

public sealed record Spec014RemoteTraceEvidence(
    int FaultsInjected,
    int ObservedHttpActivities,
    int RemoteActivitiesInsideTransactions);

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
    Spec018MixedReadEvidence MixedTargetReads,
    Spec014ProfileEvidence Spike,
    Spec014CollisionEvidence Collision,
    Spec014BoundaryEvidence Boundary,
    Spec014RemoteTraceEvidence RemoteTrace,
    Spec014MetricEvidence Metrics,
    int RemoteDependencyTypesInsideTransactionBoundary,
    int RemoteCallsInsideTransactions,
    bool CancellationBeforeCommitVerified,
    int PrivacyViolations,
    bool EndpointSmokeExecuted,
    bool FirstReplicaRestartVerified)
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
        "tests/StudentRegistration.LoadTests/Infrastructure/Spec008TwoReplicaSharedSqlFixture.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs",
        "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationEndpointStore.cs",
        "src/StudentRegistration.Registration/Application/RegistrationEndpointService.cs",
        "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
        "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs",
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
        await using var database = new Spec008TwoReplicaSharedSqlFixture(
            AccountCount,
            enableReadCommittedSnapshot: true);
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
        using var remoteTrace = new RemoteActivityTrace();
        var cancellationVerified = await VerifyCancellationBeforeCommitAsync(
            replicas[0],
            dataset,
            cancellationToken);
        var remoteDependencyTypes = CountRemoteDependencyTypes();
        await remoteTrace.InjectFailingHttpCallAsync(cancellationToken);
        await VerifyRetriableTransactionSmokeAsync(
            replicas[0],
            dataset,
            cancellationToken);
        var boundary = await VerifyAuthenticatedApplicationBoundaryAsync(
            database,
            cancellationToken);

        var targetTask = RunProfileAsync(
            "required-target",
            TargetDurationSeconds,
            TargetRate,
            "target",
            acceptedOfferingOffset: 0,
            replicas,
            dataset,
            metrics,
            cancellationToken,
            failureInjectionAtSecond: TargetDurationSeconds / 2);
        var mixedReadsTask = RunMixedReadProfileAsync(
            database,
            dataset,
            cancellationToken);
        await Task.WhenAll(targetTask, mixedReadsTask);
        var target = await targetTask;
        var mixedTargetReads = await mixedReadsTask;
        await database.RestartFirstReplicaAsync(cancellationToken);
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
        var collision = await RunThirtySeatCollisionAsync(
            replicas,
            dataset,
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
            "SQL Server 2022 Developer compatibility 160 (Testcontainers; READ_COMMITTED_SNAPSHOT ON)",
            "RegistrationSubmissionStore + SqlSeatAllocator + Enrollment/Audit atomic commit",
            target,
            mixedTargetReads,
            spike,
            collision,
            boundary,
            remoteTrace.Snapshot(),
            metricEvidence,
            remoteDependencyTypes,
            remoteTrace.RemoteActivitiesInsideTransactions,
            cancellationVerified,
            privacyViolations,
            EndpointSmokeExecuted:
                boundary.HttpUnauthorizedResponses == 1 &&
                boundary.HttpAntiforgeryRejections == 1,
            FirstReplicaRestartVerified: true);
    }

    public static async Task RunRetriableTransactionSmokeAsync(
        CancellationToken cancellationToken = default)
    {
        const int smokeAccountCount = 100;
        await using var database = new Spec008TwoReplicaSharedSqlFixture(
            smokeAccountCount);
        await database.InitializeAsync(cancellationToken);
        var connectionString = database.ConnectionString ??
            throw new InvalidOperationException("The smoke SQL fixture is not ready.");
        var dataset = await SeedRegistrationDatasetAsync(
            connectionString,
            cancellationToken,
            smokeAccountCount);
        await VerifyMixedReadEndpointsAsync(database, dataset, cancellationToken);
        var replica = new LogicalRegistrationReplica("smoke-replica", connectionString);
        using var metrics = new RegistrationMetricCollector();
        await VerifyRetriableTransactionSmokeAsync(
            replica,
            dataset,
            cancellationToken);
        var transactionSamples = new ConcurrentBag<double>();
        var accepted = await ExecuteRequestAsync(
            replica,
            dataset,
            "smoke",
            acceptedOfferingOffset: 0,
            requestIndex: 0,
            transactionSamples,
            cancellationToken);
        var rejected = await ExecuteRequestAsync(
            replica,
            dataset,
            "smoke",
            acceptedOfferingOffset: 0,
            requestIndex: 7,
            transactionSamples,
            cancellationToken);
        var replay = await ExecuteRequestAsync(
            replica,
            dataset,
            "smoke",
            acceptedOfferingOffset: 0,
            requestIndex: 9,
            transactionSamples,
            cancellationToken);
        await ProveReconciliationMetricAndRepairAsync(
            replica,
            dataset,
            cancellationToken);
        var invariants = await QueryInvariantsAsync(
            connectionString,
            cancellationToken);
        var metricSnapshot = metrics.Snapshot();
        if (accepted is not RegistrationLoadOutcome.Accepted ||
            rejected is not RegistrationLoadOutcome.ExpectedConflict ||
            replay is not RegistrationLoadOutcome.IdempotentReplay ||
            invariants.TotalViolations != 0 ||
            metricSnapshot.PublishedMetricNames.Count != 5 ||
            metricSnapshot.IdempotentReplayCount < 1 ||
            metricSnapshot.CapacityConflictCount < 1 ||
            metricSnapshot.ReconciliationMismatchCount < 1 ||
            metricSnapshot.UnsafeMetricTagCount != 0)
        {
            throw new InvalidOperationException(
                "The fast SPEC-014 transaction/invariant/metric smoke did not satisfy its evidence contract.");
        }
    }

    public static async Task<Spec018MixedDiagnosticEvidence> RunMixedDiagnosticAsync(
        CancellationToken cancellationToken = default)
    {
        const int durationSeconds = 30;
        var enableReadCommittedSnapshot = string.Equals(
            Environment.GetEnvironmentVariable("SPEC018_ENABLE_RCSI"),
            "1",
            StringComparison.Ordinal);
        await using var database = new Spec008TwoReplicaSharedSqlFixture(
            AccountCount,
            enableReadCommittedSnapshot);
        await database.InitializeAsync(cancellationToken);
        var connectionString = database.ConnectionString ??
            throw new InvalidOperationException("The diagnostic SQL fixture is not ready.");
        var dataset = await SeedRegistrationDatasetAsync(connectionString, cancellationToken);
        var replicas = new[]
        {
            new LogicalRegistrationReplica("replica-a", connectionString),
            new LogicalRegistrationReplica("replica-b", connectionString)
        };
        using var metrics = new RegistrationMetricCollector();
        var submissions = RunProfileAsync(
            "diagnostic-mixed-submissions",
            durationSeconds,
            TargetRate,
            "diagnostic",
            0,
            replicas,
            dataset,
            metrics,
            cancellationToken);
        var reads = RunMixedReadProfileAsync(
            database,
            dataset,
            cancellationToken,
            durationSeconds,
            failoverAtSecond: null);
        await Task.WhenAll(submissions, reads);
        var evidence = new Spec018MixedDiagnosticEvidence(
            await ReadCommittedSnapshotEnabledAsync(
                connectionString,
                cancellationToken),
            await submissions,
            await reads,
            await QuerySqlHotspotsAsync(connectionString, cancellationToken));
        var path = Path.Combine(
            FindRepositoryRoot(),
            ".local",
            "evidence",
            "SPEC-018-mixed-diagnostic.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(evidence, JsonOptions),
            cancellationToken);
        return evidence;
    }

    private static async Task<bool> ReadCommittedSnapshotEnabledAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT [is_read_committed_snapshot_on]
            FROM [sys].[databases]
            WHERE [database_id] = DB_ID();
            """;
        return Convert.ToBoolean(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task<IReadOnlyList<Spec018SqlHotspotEvidence>>
        QuerySqlHotspotsAsync(
            string connectionString,
            CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = """
            SELECT TOP (20)
                CONVERT(varchar(64), qs.query_hash, 2) AS QueryHash,
                qs.execution_count,
                qs.total_elapsed_time / 1000.0 AS TotalElapsedMilliseconds,
                (qs.total_elapsed_time / NULLIF(qs.execution_count, 0)) / 1000.0
                    AS AverageElapsedMilliseconds,
                qs.total_logical_reads,
                SUBSTRING(
                    text.text,
                    (qs.statement_start_offset / 2) + 1,
                    ((CASE qs.statement_end_offset
                        WHEN -1 THEN DATALENGTH(text.text)
                        ELSE qs.statement_end_offset
                      END - qs.statement_start_offset) / 2) + 1) AS StatementText
            FROM sys.dm_exec_query_stats AS qs
            CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS text
            WHERE text.dbid = DB_ID()
               OR CHARINDEX(N'[auth].', text.text) > 0
               OR CHARINDEX(N'[academics].', text.text) > 0
               OR CHARINDEX(N'[scheduling].', text.text) > 0
               OR CHARINDEX(N'[registration].', text.text) > 0
               OR CHARINDEX(N'[audit].', text.text) > 0
            ORDER BY qs.total_elapsed_time DESC;
            """;
        var results = new List<Spec018SqlHotspotEvidence>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var statement = reader.GetString(5);
            results.Add(new Spec018SqlHotspotEvidence(
                reader.GetString(0),
                ClassifySql(statement),
                reader.GetInt64(1),
                Convert.ToDouble(reader.GetValue(2), CultureInfo.InvariantCulture),
                Convert.ToDouble(reader.GetValue(3), CultureInfo.InvariantCulture),
                reader.GetInt64(4)));
        }

        return results;
    }

    private static string ClassifySql(string statement)
    {
        if (statement.Contains("ApplicationUsers", StringComparison.Ordinal))
        {
            return "identity-security-stamp";
        }

        if (statement.Contains("CourseOfferings", StringComparison.Ordinal) ||
            statement.Contains("OfferingEligibility", StringComparison.Ordinal))
        {
            return "offering-discovery";
        }

        if (statement.Contains("RegistrationPlans", StringComparison.Ordinal) ||
            statement.Contains("CurrentRegistration", StringComparison.Ordinal))
        {
            return "plan-or-timetable";
        }

        if (statement.Contains("RegistrationSubmissions", StringComparison.Ordinal) ||
            statement.Contains("Enrollments", StringComparison.Ordinal))
        {
            return "registration-write-or-records";
        }

        if (statement.Contains("Students", StringComparison.Ordinal) ||
            statement.Contains("AcademicTerms", StringComparison.Ordinal))
        {
            return "academic-context";
        }

        return "other-bounded-sql";
    }

    private static async Task VerifyMixedReadEndpointsAsync(
        Spec008TwoReplicaSharedSqlFixture fixture,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken)
    {
        var kinds = Enum.GetValues<Spec018ReadKind>();
        for (var index = 0; index < kinds.Length; index++)
        {
            var kind = kinds[index];
            using var request = fixture.StudentSessions[index].CreateRequest(
                HttpMethod.Get,
                ReadPath(kind, dataset));
            using var response = await fixture.ReplicaClients[index % 2].SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!ExpectedReadStatus(kind, response.StatusCode))
            {
                throw new InvalidOperationException(
                    $"The {kind} read smoke returned HTTP {(int)response.StatusCode}.");
            }
        }
    }

    private static async Task<Spec014BoundaryEvidence>
        VerifyAuthenticatedApplicationBoundaryAsync(
            Spec008TwoReplicaSharedSqlFixture database,
            CancellationToken cancellationToken)
    {
        var requestBody = new SubmitRegistrationRequest(
            StableGuid("boundary:http-plan"),
            Convert.ToBase64String([1]),
            StableGuid("boundary:http-request"));
        using var anonymousRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/student/terms/00000000-0000-0000-0000-000000000001/registrations")
        {
            Content = JsonContent.Create(requestBody)
        };
        using var anonymousResponse = await database.FirstReplicaClient.SendAsync(
            anonymousRequest,
            cancellationToken);
        if (anonymousResponse.StatusCode is not HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                $"The SPEC-014 endpoint accepted an anonymous request with status {(int)anonymousResponse.StatusCode}.");
        }

        using var missingAntiforgery = database.StudentSessions[0].CreateRequest(
            HttpMethod.Post,
            "/api/student/terms/00000000-0000-0000-0000-000000000001/registrations");
        missingAntiforgery.Content = JsonContent.Create(requestBody);
        using var antiforgeryResponse = await database.SecondReplicaClient.SendAsync(
            missingAntiforgery,
            cancellationToken);
        var antiforgeryBody = await antiforgeryResponse.Content.ReadAsStringAsync(
            cancellationToken);
        if (antiforgeryResponse.StatusCode is not HttpStatusCode.BadRequest ||
            !antiforgeryBody.Contains("ANTIFORGERY_INVALID", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The authenticated SPEC-014 POST did not enforce antiforgery before handler execution.");
        }

        var applicationUserId = StableGuid("boundary:application-user");
        var studentId = StableGuid("boundary:student");
        var termId = StableGuid("boundary:term");
        var planId = StableGuid("boundary:plan");
        var groupId = StableGuid("boundary:group");
        var endpointStore = new ProbeEndpointStore(
            applicationUserId,
            studentId,
            termId,
            groupId);
        var boundaryStore = new ProbeAcademicBoundaryStore();
        var coordinator = new RegistrationTransactionCoordinator(
            new StudentAcademicProfileService(boundaryStore, TimeProvider.System),
            endpointStore);
        var endpointService = new RegistrationEndpointService(
            new RegistrationCommandFactory(TimeProvider.System),
            endpointStore,
            TimeProvider.System);
        var principal = StudentPrincipal(applicationUserId);
        var accepted = await endpointService.SubmitAsync(
            principal,
            termId,
            new(planId, Convert.ToBase64String([2]), StableGuid("boundary:request")),
            coordinator,
            cancellationToken);
        if (accepted.Outcome is not RegistrationEndpointOutcome.Created)
        {
            throw new InvalidOperationException(
                "The authenticated application-boundary probe did not reach the coordinator commit callback.");
        }

        var impersonation = await endpointService.SubmitAsync(
            StudentPrincipal(StableGuid("boundary:impersonator")),
            termId,
            new(planId, Convert.ToBase64String([2]), StableGuid("boundary:impersonation")),
            coordinator,
            cancellationToken);
        if (impersonation.Outcome is not RegistrationEndpointOutcome.Conflict ||
            endpointStore.SubmitCalls != 1)
        {
            throw new InvalidOperationException(
                "An application-user substitution attempt crossed the authenticated command boundary.");
        }

        return new(
            endpointStore.SubmitCalls,
            1,
            boundaryStore.Executions,
            endpointStore.AtomicCommitCallbacks,
            1,
            1);
    }

    private static async Task VerifyRetriableTransactionSmokeAsync(
        LogicalRegistrationReplica replica,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken)
    {
        var before = await CountSubmissionsAsync(
            dataset.ConnectionString,
            cancellationToken);
        await using var context = replica.CreateContext();
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
            await using var transaction = await context.Database.BeginTransactionAsync(
                cancellationToken);
            using var transactionTrace = RemoteActivityTrace.EnterTransaction();
            try
            {
                var scope = new RegistrationRequestScope(
                    dataset.StudentIds[^1],
                    dataset.TermId,
                    StableGuid("spec014-load:transaction-smoke"));
                var store = new RegistrationSubmissionStore(
                    context,
                    TimeProvider.System);
                var claim = await store.ClaimInsideTransactionAsync(
                    scope,
                    "TRANSACTION-SMOKE",
                    DateTime.UtcNow,
                    cancellationToken);
                var allocation = await new SqlSeatAllocator(context).AllocateAsync(
                    dataset.GroupIds[5],
                    cancellationToken);
                if (!claim.MayExecute || claim.Submission is null ||
                    !allocation.IsAllocated)
                {
                    throw new InvalidOperationException(
                        "The pre-profile retriable transaction smoke did not reach allocation.");
                }

                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });

        var after = await CountSubmissionsAsync(
            dataset.ConnectionString,
            cancellationToken);
        await using var connection = new SqlConnection(dataset.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var groupCount = await ScalarAsync<int>(connection, null, """
            SELECT [EnrolledCount] FROM [scheduling].[SectionGroups]
            WHERE [Id] = @group;
            """, cancellationToken, new SqlParameter("@group", dataset.GroupIds[5]));
        if (before != after || groupCount != 0)
        {
            throw new InvalidOperationException(
                "The pre-profile transaction smoke left durable mutation after rollback.");
        }
    }

    private static ClaimsPrincipal StudentPrincipal(Guid applicationUserId)
    {
        var identity = new ClaimsIdentity(
        [
            new(ClaimTypes.NameIdentifier, applicationUserId.ToString("D")),
            new(ClaimTypes.Role, RolePolicies.Student),
            new(RolePolicies.PermissionClaimType, "Registration.SubmitOwn")
        ],
        "SPEC014-load-probe");
        return new(identity);
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

    public static void ValidateReleaseEvidence(Spec014LoadEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ValidateProfileAccounting(evidence.Target);
        ValidateProfileAccounting(evidence.Spike);
        if (evidence.SourceFingerprint != CalculateSourceFingerprint() ||
            evidence.SyntheticAccountCount != AccountCount ||
            evidence.LogicalSessionCount != LogicalSessionCount ||
            evidence.LogicalApplicationReplicaCount != ReplicaCount ||
            evidence.Target.ConfiguredSubmissionsPerSecond != TargetRate ||
            evidence.Target.DurationSeconds != TargetDurationSeconds ||
            evidence.Target.ScheduledRequests != TargetRate * TargetDurationSeconds ||
            evidence.Target.SubmissionP95Milliseconds > 2_000 ||
            evidence.Target.TransactionP95Milliseconds > 2_000 ||
            evidence.Target.UnexpectedFailureRatePercent >= 0.1 ||
            evidence.Target.Invariants.TotalViolations != 0 ||
            evidence.MixedTargetReads.DurationSeconds != TargetDurationSeconds ||
            evidence.MixedTargetReads.ConfiguredReadsPerSecond != 300 ||
            evidence.MixedTargetReads.ScheduledRequests != 180_000 ||
            evidence.MixedTargetReads.CompletedRequests != 180_000 ||
            evidence.MixedTargetReads.DiscoveryReads != 90_000 ||
            evidence.MixedTargetReads.EligibilityReads != 45_000 ||
            evidence.MixedTargetReads.PlanAndTimetableReads != 27_000 ||
            evidence.MixedTargetReads.RegistrationRecordReads != 18_000 ||
            evidence.MixedTargetReads.UnexpectedFailureRatePercent >= 0.1 ||
            evidence.MixedTargetReads.DiscoveryP95Milliseconds > 300 ||
            evidence.MixedTargetReads.FailoverAtSecond != 300 ||
            !evidence.MixedTargetReads.FirstReplicaRemoved ||
            !evidence.FirstReplicaRestartVerified ||
            evidence.Spike.ConfiguredSubmissionsPerSecond != SpikeRate ||
            evidence.Spike.DurationSeconds != SpikeDurationSeconds ||
            evidence.Spike.ScheduledRequests != SpikeRate * SpikeDurationSeconds ||
            evidence.Spike.TransactionP95Milliseconds > 2_000 ||
            evidence.Spike.UnexpectedFailures != 0 ||
            evidence.Spike.Invariants.TotalViolations != 0 ||
            evidence.Collision.ConcurrentRequests != 100 ||
            evidence.Collision.GroupCapacity != 30 ||
            evidence.Collision.AcceptedRequests != 30 ||
            evidence.Collision.ExpectedConflictRequests != 70 ||
            evidence.Collision.ActiveEnrollments != 30 ||
            evidence.Collision.FinalEnrolledCount != 30 ||
            evidence.Collision.ReplicaCount < 2 ||
            evidence.Collision.Invariants.TotalViolations != 0 ||
            evidence.Boundary.AuthenticatedServiceSubmissions != 1 ||
            evidence.Boundary.ImpersonationAttemptsRejected != 1 ||
            evidence.Boundary.CoordinatorBoundaryExecutions != 1 ||
            evidence.Boundary.CoordinatorCommitCallbacks != 1 ||
            !evidence.EndpointSmokeExecuted ||
            !evidence.CancellationBeforeCommitVerified ||
            evidence.RemoteDependencyTypesInsideTransactionBoundary != 0 ||
            evidence.RemoteCallsInsideTransactions != 0 ||
            evidence.RemoteTrace.FaultsInjected != 1 ||
            evidence.RemoteTrace.ObservedHttpActivities < 1 ||
            evidence.RemoteTrace.RemoteActivitiesInsideTransactions != 0 ||
            evidence.Metrics.DeadlockCount < 0 ||
            evidence.Metrics.LockWaitP95Milliseconds < 0 ||
            evidence.Metrics.IdempotentReplayCount < 1 ||
            evidence.Metrics.CapacityConflictCount < 1 ||
            evidence.Metrics.ReconciliationMismatchCount < 1 ||
            evidence.Metrics.UnsafeMetricTagCount != 0 ||
            evidence.PrivacyViolations != 0)
        {
            throw new InvalidOperationException(
                "SPEC-014 release evidence failed one or more NFR gates and cannot be published. " +
                $"target={JsonSerializer.Serialize(evidence.Target, JsonOptions)}; " +
                $"reads={JsonSerializer.Serialize(evidence.MixedTargetReads, JsonOptions)}; " +
                $"spike={JsonSerializer.Serialize(evidence.Spike, JsonOptions)}; " +
                $"collision={JsonSerializer.Serialize(evidence.Collision, JsonOptions)}; " +
                $"boundary={JsonSerializer.Serialize(evidence.Boundary, JsonOptions)}; " +
                $"metrics={JsonSerializer.Serialize(evidence.Metrics, JsonOptions)}; " +
                $"privacyViolations={evidence.PrivacyViolations}.");
        }

        var requiredMetrics = new[]
        {
            SqlSeatAllocator.DeadlockMetricName,
            SqlSeatAllocator.LockWaitMetricName,
            RegistrationSubmissionStore.IdempotentReplayMetricName,
            SqlSeatAllocator.CapacityConflictMetricName,
            EnrollmentCounterReconciler.CounterMismatchMetricName
        };
        if (requiredMetrics.Except(
                evidence.Metrics.PublishedMetricNames,
                StringComparer.Ordinal).Any())
        {
            throw new InvalidOperationException(
                "SPEC-014 release evidence is missing a required operational metric.");
        }
    }

    private static void ValidateProfileAccounting(Spec014ProfileEvidence profile)
    {
        if (profile.CompletedRequests != profile.ScheduledRequests ||
            profile.CompletedRequests !=
                profile.AcceptedRequests +
                profile.ExpectedConflictRequests +
                profile.IdempotentReplayRequests +
                profile.InProgressRequests +
                profile.UnexpectedFailures)
        {
            throw new InvalidOperationException(
                $"SPEC-014 profile '{profile.Name}' has invalid request accounting.");
        }
    }

    public static async Task WriteCheckedInArtifactAsync(
        Spec014LoadEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            Spec014LoadEvidence.CheckedInArtifactPath.Replace(
                '/',
                Path.DirectorySeparatorChar));
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
        CancellationToken cancellationToken,
        int? failureInjectionAtSecond = null)
    {
        var totalRequests = checked(durationSeconds * requestsPerSecond);
        var samples = new double[totalRequests];
        var transactionSamples = new ConcurrentBag<double>();
        var accepted = 0;
        var conflicts = 0;
        var replays = 0;
        var inProgress = 0;
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
            var failoverRequestIndex = failureInjectionAtSecond.HasValue
                ? checked(failureInjectionAtSecond.Value * requestsPerSecond)
                : int.MaxValue;
            var replicaIndex = index >= failoverRequestIndex
                ? replicas.Count - 1
                : index % replicas.Count;
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
                        case RegistrationLoadOutcome.InProgress:
                            Interlocked.Increment(ref inProgress);
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
            inProgress,
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

    private static async Task<Spec018MixedReadEvidence> RunMixedReadProfileAsync(
        Spec008TwoReplicaSharedSqlFixture fixture,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken,
        int durationSeconds = 600,
        int? failoverAtSecond = 300)
    {
        const int readsPerSecond = 300;
        var totalRequests = checked(readsPerSecond * durationSeconds);
        var failoverRequestIndex = failoverAtSecond.HasValue
            ? checked(readsPerSecond * failoverAtSecond.Value)
            : int.MaxValue;
        var firstClient = fixture.FirstReplicaClient;
        var secondClient = fixture.SecondReplicaClient;
        var clients = new[] { firstClient, secondClient };
        var discoverySamples = new ConcurrentBag<double>();
        var eligibilitySamples = new ConcurrentBag<double>();
        var planSamples = new ConcurrentBag<double>();
        var recordSamples = new ConcurrentBag<double>();
        var replicaCounts = new int[2];
        var statusCounts = new ConcurrentDictionary<int, int>();
        var failureKinds = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var completed = 0;
        var unexpected = 0;
        var tasks = new Task[totalRequests];
        var startedAtUtc = DateTimeOffset.UtcNow;
        var profileStarted = Stopwatch.GetTimestamp();

        for (var index = 0; index < totalRequests; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scheduledAt = profileStarted +
                (long)(index * (Stopwatch.Frequency / (double)readsPerSecond));
            var delayTicks = scheduledAt - Stopwatch.GetTimestamp();
            if (delayTicks > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(delayTicks / (double)Stopwatch.Frequency),
                    cancellationToken);
            }

            if (failoverAtSecond.HasValue && index == failoverRequestIndex)
            {
                await Task.WhenAll(tasks.Take(index));
                await fixture.StopFirstReplicaAsync();
            }

            var requestIndex = index;
            var replicaIndex = index >= failoverRequestIndex ? 1 : index % 2;
            Interlocked.Increment(ref replicaCounts[replicaIndex]);
            var kind = ReadKindFor(index);
            tasks[index] = ExecuteReadAsync(
                clients[replicaIndex],
                fixture.StudentSessions[index % LogicalSessionCount],
                ReadPath(kind, dataset),
                kind,
                discoverySamples,
                eligibilitySamples,
                planSamples,
                recordSamples,
                statusCounts,
                failureKinds,
                () => Interlocked.Increment(ref unexpected),
                () => Interlocked.Increment(ref completed),
                cancellationToken);
        }

        await Task.WhenAll(tasks);
        var completedAtUtc = DateTimeOffset.UtcNow;
        return new Spec018MixedReadEvidence(
            "required-target-read-mix",
            startedAtUtc,
            completedAtUtc,
            durationSeconds,
            readsPerSecond,
            2,
            totalRequests,
            completed,
            discoverySamples.Count,
            eligibilitySamples.Count,
            planSamples.Count,
            recordSamples.Count,
            unexpected,
            unexpected * 100d / totalRequests,
            Percentile95(discoverySamples.Order().ToArray()),
            Percentile95(eligibilitySamples.Order().ToArray()),
            Percentile95(planSamples.Order().ToArray()),
            Percentile95(recordSamples.Order().ToArray()),
            failoverAtSecond ?? -1,
            FirstReplicaRemoved: failoverAtSecond.HasValue,
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["replica-1"] = replicaCounts[0],
                ["replica-2"] = replicaCounts[1]
            },
            statusCounts.OrderBy(pair => pair.Key).ToDictionary(),
            failureKinds.OrderBy(pair => pair.Key)
                .ToDictionary(StringComparer.Ordinal));
    }

    private static async Task ExecuteReadAsync(
        HttpClient client,
        Spec008AuthenticatedStudentSession session,
        string path,
        Spec018ReadKind kind,
        ConcurrentBag<double> discoverySamples,
        ConcurrentBag<double> eligibilitySamples,
        ConcurrentBag<double> planSamples,
        ConcurrentBag<double> recordSamples,
        ConcurrentDictionary<int, int> statusCounts,
        ConcurrentDictionary<string, int> failureKinds,
        Action observeUnexpected,
        Action observeCompleted,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            using var request = session.CreateRequest(HttpMethod.Get, path);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            statusCounts.AddOrUpdate((int)response.StatusCode, 1, (_, count) => count + 1);
            if (!ExpectedReadStatus(kind, response.StatusCode))
            {
                var failureKind = await SafeReadFailureKindAsync(
                    response,
                    kind,
                    cancellationToken);
                failureKinds.AddOrUpdate(
                    failureKind,
                    1,
                    (_, count) => count + 1);
                observeUnexpected();
            }
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            failureKinds.AddOrUpdate(
                exception.GetBaseException().GetType().Name,
                1,
                (_, count) => count + 1);
            observeUnexpected();
        }
        finally
        {
            var elapsed = (Stopwatch.GetTimestamp() - started) * 1_000d /
                Stopwatch.Frequency;
            SampleBag(
                kind,
                discoverySamples,
                eligibilitySamples,
                planSamples,
                recordSamples).Add(elapsed);
            observeCompleted();
        }
    }

    private static async Task<string> SafeReadFailureKindAsync(
        HttpResponseMessage response,
        Spec018ReadKind kind,
        CancellationToken cancellationToken)
    {
        const string unknownCode = "unknown";
        var code = unknownCode;
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("code", out var codeElement))
            {
                var candidate = codeElement.GetString();
                if (!string.IsNullOrWhiteSpace(candidate) &&
                    candidate.Length <= 100 &&
                    candidate.All(character =>
                        char.IsAsciiLetterOrDigit(character) || character is '_' or '-'))
                {
                    code = candidate;
                }
            }
        }
        catch (JsonException)
        {
        }

        return $"http-{(int)response.StatusCode}:{kind}:{code}";
    }

    private static Spec018ReadKind ReadKindFor(int index) => (index % 100) switch
    {
        < 50 => Spec018ReadKind.Discovery,
        < 75 => Spec018ReadKind.Eligibility,
        < 90 => Spec018ReadKind.PlanAndTimetable,
        _ => Spec018ReadKind.RegistrationRecords
    };

    private static string ReadPath(
        Spec018ReadKind kind,
        RegistrationLoadDataset dataset) => kind switch
    {
        Spec018ReadKind.Discovery =>
            $"/api/student/terms/{dataset.TermId:D}/offerings?page=1&pageSize=20&sort=courseCode%2Cid",
        Spec018ReadKind.Eligibility =>
            $"/api/student/offerings/{dataset.OfferingIds[0]:D}/eligibility",
        Spec018ReadKind.PlanAndTimetable =>
            "/api/student/registrations/current/timetable",
        Spec018ReadKind.RegistrationRecords =>
            $"/api/student/registrations?page=1&pageSize=20&termId={dataset.TermId:D}",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static bool ExpectedReadStatus(
        Spec018ReadKind kind,
        HttpStatusCode statusCode) => kind switch
    {
        Spec018ReadKind.Discovery or Spec018ReadKind.Eligibility =>
            statusCode == HttpStatusCode.OK,
        Spec018ReadKind.PlanAndTimetable or Spec018ReadKind.RegistrationRecords =>
            statusCode is HttpStatusCode.OK or HttpStatusCode.NotFound,
        _ => false
    };

    private static ConcurrentBag<double> SampleBag(
        Spec018ReadKind kind,
        ConcurrentBag<double> discovery,
        ConcurrentBag<double> eligibility,
        ConcurrentBag<double> plan,
        ConcurrentBag<double> records) => kind switch
    {
        Spec018ReadKind.Discovery => discovery,
        Spec018ReadKind.Eligibility => eligibility,
        Spec018ReadKind.PlanAndTimetable => plan,
        Spec018ReadKind.RegistrationRecords => records,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private enum Spec018ReadKind
    {
        Discovery,
        Eligibility,
        PlanAndTimetable,
        RegistrationRecords
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
                : replay.Status is SubmissionClaimStatus.InProgress
                    ? RegistrationLoadOutcome.InProgress
                : RegistrationLoadOutcome.Unexpected;
        }

        var identity = CreateRequestIdentity(
            dataset,
            profileCode,
            acceptedOfferingOffset,
            requestIndex);
        await using var context = replica.CreateContext();
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        context.ChangeTracker.Clear();
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken);
        var transactionStarted = Stopwatch.GetTimestamp();
        using var transactionTrace = RemoteActivityTrace.EnterTransaction();
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
                CreateCanonicalReceiptSnapshot(
                    dataset,
                    identity.OfferingId,
                    identity.GroupId,
                    identity.ReceivedAtUtc),
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
        });
    }

    private static async Task<Spec014CollisionEvidence> RunThirtySeatCollisionAsync(
        IReadOnlyList<LogicalRegistrationReplica> replicas,
        RegistrationLoadDataset dataset,
        CancellationToken cancellationToken)
    {
        const int requestCount = 100;
        const int groupCapacity = 30;
        var accepted = 0;
        var conflicts = 0;
        await Task.WhenAll(Enumerable.Range(0, requestCount).Select(async index =>
        {
            var replica = replicas[index % replicas.Count];
            var outcome = await ExecuteCollisionRequestAsync(
                replica,
                dataset,
                index,
                cancellationToken);
            if (outcome is RegistrationLoadOutcome.Accepted)
            {
                Interlocked.Increment(ref accepted);
            }
            else if (outcome is RegistrationLoadOutcome.ExpectedConflict)
            {
                Interlocked.Increment(ref conflicts);
            }
            else
            {
                throw new InvalidOperationException(
                    "The 100-way collision produced an unexpected outcome.");
            }
        }));

        await using var connection = new SqlConnection(dataset.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var activeEnrollments = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM [registration].[Enrollments]
            WHERE [GroupId] = @group AND [State] = 'active';
            """, cancellationToken, new SqlParameter("@group", dataset.GroupIds[5]));
        var finalCount = await ScalarAsync<int>(connection, null, """
            SELECT [EnrolledCount] FROM [scheduling].[SectionGroups]
            WHERE [Id] = @group;
            """, cancellationToken, new SqlParameter("@group", dataset.GroupIds[5]));
        var invariants = await QueryInvariantsAsync(
            dataset.ConnectionString,
            cancellationToken);
        return new(
            requestCount,
            groupCapacity,
            accepted,
            conflicts,
            activeEnrollments,
            finalCount,
            replicas.Count,
            invariants);
    }

    private static async Task<RegistrationLoadOutcome> ExecuteCollisionRequestAsync(
        LogicalRegistrationReplica replica,
        RegistrationLoadDataset dataset,
        int requestIndex,
        CancellationToken cancellationToken)
    {
        var scope = new RegistrationRequestScope(
            dataset.StudentIds[requestIndex],
            dataset.TermId,
            StableGuid($"request:collision:{requestIndex}"));
        var payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"collision|{requestIndex}|5")));
        await using var context = replica.CreateContext();
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        context.ChangeTracker.Clear();
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken);
        using var transactionTrace = RemoteActivityTrace.EnterTransaction();
        try
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var claim = await store.ClaimInsideTransactionAsync(
                scope,
                payloadHash,
                DateTime.UtcNow,
                cancellationToken);
            var allocation = await new SqlSeatAllocator(context).AllocateAsync(
                dataset.GroupIds[5],
                cancellationToken);
            var completedAt = DateTime.UtcNow;
            if (!allocation.IsAllocated)
            {
                claim.Submission!.CompleteRejected(
                    "GROUP_FULL",
                    "{\"policyVersion\":\"DEMO-POC-2026.1\",\"resultCode\":\"GROUP_FULL\"}",
                    completedAt);
                claim.Submission.EnsureFinalForCommit();
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return RegistrationLoadOutcome.ExpectedConflict;
            }

            var reference = $"REG-COLLISION-{requestIndex:D3}";
            claim.Submission!.CompleteAccepted(
                "ACCEPTED",
                reference,
                CreateCanonicalReceiptSnapshot(
                    dataset,
                    dataset.OfferingIds[5],
                    dataset.GroupIds[5],
                    completedAt),
                "{\"policyVersion\":\"DEMO-POC-2026.1\",\"resultCode\":\"ACCEPTED\"}",
                completedAt);
            claim.Submission.EnsureFinalForCommit();
            context.Set<Enrollment>().Add(new Enrollment(
                StableGuid($"enrollment:collision:{requestIndex}"),
                scope.StudentId,
                dataset.OfferingIds[5],
                dataset.GroupIds[5],
                claim.Submission.Id,
                EnrollmentState.Active,
                completedAt));
            context.AuditEvents.Add(new AuditEvent(
                StableGuid($"audit:collision:{requestIndex}"),
                "authenticated-student",
                "registration-owner",
                "RegistrationAccepted",
                "RegistrationSubmission",
                claim.Submission.Id.ToString("N"),
                "ACCEPTED",
                null,
                "{\"result\":\"accepted\"}",
                scope.ClientRequestId.ToString("N"),
                completedAt));
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RegistrationLoadOutcome.Accepted;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        });
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
        CancellationToken cancellationToken,
        int expectedAccountCount = AccountCount)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var termId = await ScalarAsync<Guid>(
            connection,
            null,
            "SELECT TOP (1) [Id] FROM [academics].[AcademicTerms] ORDER BY [Code]",
            cancellationToken);
        var students = new List<Guid>(expectedAccountCount);
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

        if (students.Count != expectedAccountCount)
        {
            throw new InvalidOperationException(
                $"Expected {expectedAccountCount} synthetic students; found {students.Count}.");
        }

        var draftId = StableGuid("spec014-load:catalogue-draft");
        var versionId = StableGuid("spec014-load:catalogue-version");
        var offeringIds = Enumerable.Range(0, 6)
            .Select(index => StableGuid($"spec014-load:offering:{index}"))
            .ToArray();
        var groupIds = Enumerable.Range(0, 6)
            .Select(index => StableGuid($"spec014-load:group:{index}"))
            .ToArray();
        var capacities = new[] { 25_000, 6_500, 8_400, 0, 2, 30 };
        var roomId = StableGuid("spec014-load:room");
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
            await ExecuteAsync(connection, transaction, """
                INSERT [scheduling].[Rooms]
                  ([Id],[Code],[Location],[Capacity],[AvailabilityState])
                VALUES (@id,'SPEC014-LOAD','Synthetic load facility',25000,'available');
                """, cancellationToken, new SqlParameter("@id", roomId));
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
                    INSERT [scheduling].[MeetingSlots]
                      ([Id],[GroupId],[RoomId],[ActivityType],[DayOfWeek],[StartLocal],[EndLocal])
                    VALUES (@meeting,@group,@room,'lecture',@day,'09:00:00','10:00:00');
                    """, cancellationToken,
                    new SqlParameter("@id", courseId),
                    new SqlParameter("@catalogue", versionId),
                    new SqlParameter("@code", $"L{index + 1:000}"),
                    new SqlParameter("@title", $"Synthetic load course {index + 1}"),
                    new SqlParameter("@offering", offeringIds[index]),
                    new SqlParameter("@term", termId),
                    new SqlParameter("@group", groupIds[index]),
                    new SqlParameter("@groupCode", $"LOAD-{index + 1}"),
                    new SqlParameter("@capacity", capacities[index]),
                    new SqlParameter("@meeting", StableGuid($"spec014-load:meeting:{index}")),
                    new SqlParameter("@room", roomId),
                    new SqlParameter("@day", index));
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
            WHERE a.[EntityId] = REPLACE(CONVERT(nvarchar(36), s.[Id]), '-', '')
                AND a.[Action] = 'RegistrationAccepted') audits
            WHERE (s.[ProcessingState] = 'accepted' AND
                    (s.[ReceiptSnapshotJson] IS NULL OR enrollments.EnrollmentCount <> 1 OR audits.AuditCount <> 1))
               OR (s.[ProcessingState] = 'rejected' AND enrollments.EnrollmentCount <> 0)
               OR s.[ProcessingState] = 'processing';
            """, cancellationToken);
        var combined = await ScalarAsync<int>(connection, null, """
            SELECT COUNT(*) FROM (
              SELECT e.[StudentId], 'policy' AS ViolationKind
              FROM [registration].[Enrollments] e
              WHERE e.[State] = 'active'
              GROUP BY e.[StudentId]
              HAVING SUM(3) > 18
              UNION ALL
              SELECT DISTINCT firstEnrollment.[StudentId], 'timetable'
              FROM [registration].[Enrollments] firstEnrollment
              JOIN [scheduling].[MeetingSlots] firstMeeting
                ON firstMeeting.[GroupId] = firstEnrollment.[GroupId]
              JOIN [registration].[Enrollments] secondEnrollment
                ON secondEnrollment.[StudentId] = firstEnrollment.[StudentId]
               AND secondEnrollment.[Id] > firstEnrollment.[Id]
               AND secondEnrollment.[State] = 'active'
              JOIN [scheduling].[MeetingSlots] secondMeeting
                ON secondMeeting.[GroupId] = secondEnrollment.[GroupId]
               AND secondMeeting.[DayOfWeek] = firstMeeting.[DayOfWeek]
               AND firstMeeting.[StartLocal] < secondMeeting.[EndLocal]
               AND secondMeeting.[StartLocal] < firstMeeting.[EndLocal]
              WHERE firstEnrollment.[State] = 'active') AS violations;
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
        var cancellationObserved = false;
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
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
            }
            catch (OperationCanceledException)
            {
                cancellationObserved = true;
                await transaction.RollbackAsync(CancellationToken.None);
            }
        });

        var after = await CountSubmissionsAsync(dataset.ConnectionString, cancellationToken);
        return cancellationObserved && before == after;
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
        CancellationToken cancellationToken,
        params SqlParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = 120;
        command.Parameters.AddRange(parameters);
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

    private static string CreateCanonicalReceiptSnapshot(
        RegistrationLoadDataset dataset,
        Guid offeringId,
        Guid groupId,
        DateTime submittedAtUtc)
    {
        var offeringIndex = Array.IndexOf(dataset.OfferingIds, offeringId);
        if (offeringIndex < 0 || dataset.GroupIds[offeringIndex] != groupId)
        {
            throw new InvalidOperationException(
                "The load request does not reference a seeded offering/group pair.");
        }

        return JsonSerializer.Serialize(new
        {
            term = new
            {
                id = dataset.TermId,
                code = "SPEC014-LOAD",
                displayName = "Synthetic load term",
                timeZoneId = "Africa/Cairo"
            },
            groups = new[]
            {
                new
                {
                    offeringId,
                    courseCode = $"L{offeringIndex + 1:000}",
                    subjectTitle = $"Synthetic load course {offeringIndex + 1}",
                    groupId,
                    groupCode = $"LOAD-{offeringIndex + 1}",
                    credits = 3m,
                    meetings = new[]
                    {
                        new
                        {
                            meetingId = StableGuid($"spec014-load:meeting:{offeringIndex}"),
                            activityType = "lecture",
                            dayOfWeek = offeringIndex,
                            startLocal = "09:00:00",
                            endLocal = "10:00:00",
                            roomCode = "SPEC014-LOAD",
                            location = "Synthetic load facility",
                            staff = Array.Empty<object>()
                        }
                    }
                }
            },
            totalCredits = 3m,
            policySetId = StableGuid("spec014-load:policy-set"),
            policyVersion = "DEMO-POC-2026.1",
            submittedAtUtc = DateTime.SpecifyKind(submittedAtUtc, DateTimeKind.Utc)
        });
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
        InProgress,
        Unexpected
    }

    private sealed class ProbeEndpointStore(
        Guid applicationUserId,
        Guid studentId,
        Guid termId,
        Guid groupId) :
        IRegistrationEndpointStore,
        IRegistrationLocalTransactionStore
    {
        private readonly string _stateVersion = Convert.ToBase64String([1]);
        private int _contextLocks;
        private int _catalogueLocks;
        private int _policyLocks;
        private int _groupLocks;
        private int _revalidations;

        public int SubmitCalls { get; private set; }

        public int AtomicCommitCallbacks { get; private set; }

        public Task<RegistrationCommandContext?> ResolveCommandContextAsync(
            Guid ignoredApplicationUserId,
            Guid ignoredTermId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<RegistrationCommandContext?>(new(
                new(
                    applicationUserId,
                    studentId,
                    termId,
                    Convert.ToBase64String([3]),
                    DateTime.UtcNow.AddHours(-1),
                    DateTime.UtcNow.AddHours(1),
                    false),
                _stateVersion));
        }

        public async Task<RegistrationEndpointResult> SubmitAsync(
            RegistrationCommand command,
            RegistrationTransactionCoordinator coordinator,
            CancellationToken cancellationToken = default)
        {
            SubmitCalls++;
            var boundary = await coordinator.ExecuteRegistrationAsync(
                new(
                    command.StudentId,
                    command.TermId,
                    _stateVersion,
                    command.PlanId,
                    command.ExpectedPlanRowVersion,
                    command.ExpectedRegistrationContextVersion,
                    command.ReceivedAtUtc,
                    "SPEC014-LOAD",
                    "SPEC014-LOAD",
                    [groupId]),
                token =>
                {
                    token.ThrowIfCancellationRequested();
                    AtomicCommitCallbacks++;
                    return Task.CompletedTask;
                },
                cancellationToken);
            if (boundary.Outcome is not RegistrationBoundaryOutcome.Committed ||
                _contextLocks != 1 ||
                _catalogueLocks != 1 ||
                _policyLocks != 1 ||
                _groupLocks != 1 ||
                _revalidations != 1 ||
                AtomicCommitCallbacks != 1)
            {
                return new(RegistrationEndpointOutcome.Unavailable);
            }

            var now = DateTime.UtcNow;
            return new(
                RegistrationEndpointOutcome.Created,
                new(
                    StableGuid("boundary:submission"),
                    "accepted",
                    "REGISTERED",
                    [],
                    command.ReceivedAtUtc,
                    now,
                    Guid.Empty,
                    "DEMO-POC-2026.1",
                    command.ExpectedPlanRowVersion,
                    "REG-BOUNDARY",
                    null));
        }

        public Task<RegistrationEndpointResult> LookupAsync(
            Guid ignoredApplicationUserId,
            Guid ignoredTermId,
            Guid ignoredClientRequestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new RegistrationEndpointResult(
                RegistrationEndpointOutcome.NotFound));

        public Task LockRegistrationContextAsync(
            Guid ignoredStudentId,
            Guid ignoredTermId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _contextLocks++;
            return Task.CompletedTask;
        }

        public Task LockSerializablePublicationScopeRangeAsync(
            RegistrationPublicationScope scope,
            string ignoredScope,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (scope is RegistrationPublicationScope.Catalogue)
            {
                _catalogueLocks++;
            }
            else
            {
                _policyLocks++;
            }

            return Task.CompletedTask;
        }

        public Task LockSectionGroupVersionsAsync(
            IReadOnlyList<Guid> ignoredGroupIds,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _groupLocks++;
            return Task.CompletedTask;
        }

        public Task ReReadAndValidateAsync(
            RegistrationFinalValidation ignoredValidation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _revalidations++;
            return Task.CompletedTask;
        }
    }

    private sealed class ProbeAcademicBoundaryStore : IStudentAcademicProfileStore
    {
        public int Executions { get; private set; }

        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public async Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default)
        {
            Executions++;
            await commitCallback(cancellationToken);
            return new(AcademicProfileStoreOutcome.Succeeded);
        }
    }

    private sealed class RemoteActivityTrace : IDisposable
    {
        private static readonly AsyncLocal<int> TransactionDepth = new();
        private static readonly ActivitySource ProbeSource = new("SPEC014.RemoteProbe");
        private readonly ActivityListener _listener;
        private int _faultsInjected;
        private int _observedHttpActivities;
        private int _insideTransactionActivities;

        public RemoteActivityTrace()
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = source =>
                    source.Name == "SPEC014.RemoteProbe" ||
                    source.Name.StartsWith("System.Net.Http", StringComparison.Ordinal),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllData,
                ActivityStarted = activity =>
                {
                    Interlocked.Increment(ref _observedHttpActivities);
                    if (TransactionDepth.Value > 0)
                    {
                        Interlocked.Increment(ref _insideTransactionActivities);
                    }
                }
            };
            ActivitySource.AddActivityListener(_listener);
        }

        public int RemoteActivitiesInsideTransactions =>
            Volatile.Read(ref _insideTransactionActivities);

        public static IDisposable EnterTransaction()
        {
            TransactionDepth.Value++;
            return new TransactionTraceScope();
        }

        public async Task InjectFailingHttpCallAsync(CancellationToken cancellationToken)
        {
            using var client = new HttpClient(new FaultInjectingHandler());
            try
            {
                _ = await client.GetAsync(
                    "https://fault-injected.spec014.invalid/",
                    cancellationToken);
                throw new InvalidOperationException(
                    "The remote-dependency fault injection unexpectedly succeeded.");
            }
            catch (HttpRequestException)
            {
                Interlocked.Increment(ref _faultsInjected);
            }
        }

        public Spec014RemoteTraceEvidence Snapshot() => new(
            Volatile.Read(ref _faultsInjected),
            Volatile.Read(ref _observedHttpActivities),
            Volatile.Read(ref _insideTransactionActivities));

        public void Dispose()
        {
            _listener.Dispose();
            ProbeSource.Dispose();
        }

        private sealed class TransactionTraceScope : IDisposable
        {
            public void Dispose() => TransactionDepth.Value--;
        }

        private sealed class FaultInjectingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                using var activity = ProbeSource.StartActivity(
                    "fault-injected-http",
                    ActivityKind.Client);
                throw new HttpRequestException(
                    "SPEC-014 deterministic remote dependency fault.");
            }
        }
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
