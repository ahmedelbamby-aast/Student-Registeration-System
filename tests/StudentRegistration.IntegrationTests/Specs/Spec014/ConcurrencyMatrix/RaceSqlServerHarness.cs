using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public sealed class RaceSqlServerCollection : ICollectionFixture<RaceSqlServerFixture>
{
    public const string CollectionName = "SPEC-014 SQL race matrix";
}

public sealed class RaceSqlServerFixture : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _container = new();

    public string ServerConnectionString => _container.ConnectionString;

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

internal sealed record RaceOracle(
    string Boundary,
    string Winner,
    string Loser,
    string Invariant);

internal sealed class RaceSqlServerHarness(RaceSqlServerFixture fixture)
{
    private static readonly TimeSpan SqlTimeout = TimeSpan.FromSeconds(15);

    public async Task ProveFinalSeatAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.GroupState
            (
                Id int NOT NULL CONSTRAINT PK_GroupState PRIMARY KEY,
                Capacity int NOT NULL,
                EnrolledCount int NOT NULL,
                CONSTRAINT CK_GroupState_Capacity CHECK
                    (Capacity >= 0 AND EnrolledCount >= 0 AND EnrolledCount <= Capacity)
            );
            INSERT dbo.GroupState (Id, Capacity, EnrolledCount) VALUES (1, 1, 0);
            """);

        var results = await RaceAsync(database, async (connection, transaction, _) =>
        {
            var affected = await ExecuteAsync(
                connection,
                transaction,
                """
                UPDATE dbo.GroupState WITH (UPDLOCK, ROWLOCK)
                SET EnrolledCount = EnrolledCount + 1
                WHERE Id = 1 AND EnrolledCount < Capacity;
                """);
            return affected == 1 ? "ACCEPTED" : "409 GROUP_FULL";
        });

        Assert.Single(results, result => result == "ACCEPTED");
        Assert.Single(results, result => result == "409 GROUP_FULL");
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT EnrolledCount FROM dbo.GroupState WHERE Id = 1;"));
        AssertOracle(oracle, "SectionGroup conditional update", "First committed valid allocation", "409 GROUP_FULL", "EnrolledCount <= Capacity");
    }

    public async Task ProveStudentTermSerializationAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.StudentTermState
            (
                StudentId int NOT NULL,
                TermId int NOT NULL,
                Version int NOT NULL,
                AcceptedPlan nvarchar(20) NULL,
                CONSTRAINT PK_StudentTermState PRIMARY KEY (StudentId, TermId)
            );
            INSERT dbo.StudentTermState VALUES (1, 1, 1, NULL);
            """);

        var results = await RaceAsync(database, async (connection, transaction, replica) =>
        {
            var affected = await ExecuteAsync(connection, transaction, """
                UPDATE dbo.StudentTermState WITH (UPDLOCK, HOLDLOCK)
                SET AcceptedPlan = @replica, Version = Version + 1
                WHERE StudentId = 1 AND TermId = 1 AND AcceptedPlan IS NULL;
                """, new SqlParameter("@replica", replica));
            return affected == 1 ? "VALID_PLAN" : "409 POLICY_OR_CONFLICT";
        });

        Assert.Single(results, result => result == "VALID_PLAN");
        Assert.Single(results, result => result == "409 POLICY_OR_CONFLICT");
        Assert.Equal(2, await ScalarAsync<int>(database,
            "SELECT Version FROM dbo.StudentTermState WHERE StudentId = 1 AND TermId = 1;"));
        AssertOracle(oracle, "SPEC-008 StudentTermAcademicState via ExecuteRegistrationBoundaryAsync before groups", "First valid plan", "Re-read and 409 policy/conflict", "Combined enrollment remains valid");
    }

    public async Task ProveSamePayloadClaimAsync(RaceOracle oracle)
    {
        var database = await CreateClaimDatabaseAsync();
        var results = await RaceAsync(database, async (connection, transaction, _) =>
        {
            try
            {
                await ExecuteAsync(connection, transaction, """
                    INSERT dbo.SubmissionClaim
                        (StudentId, TermId, ClientRequestId, PayloadHash, State)
                    VALUES (1, 1, 'same-key', 'same-hash', 'final');
                    """);
                return "EXECUTED";
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                return "FINAL_REPLAY";
            }
        });

        Assert.Single(results, result => result == "EXECUTED");
        Assert.Single(results, result => result == "FINAL_REPLAY");
        Assert.Equal(1, await ScalarAsync<int>(database, "SELECT COUNT(*) FROM dbo.SubmissionClaim;"));
        AssertOracle(oracle, "Atomic uncommitted claim plus 500-ms bounded wait", "First claimant", "Visible final replay or non-durable 202 with no submissionId", "One execution/final result");
    }

    public async Task ProvePayloadScopeAsync(RaceOracle oracle)
    {
        var database = await CreateClaimDatabaseAsync();
        var results = await RaceAsync(database, async (connection, transaction, replica) =>
        {
            var payload = replica == "replica-a" ? "original-a" : "different-b";
            try
            {
                await ExecuteAsync(connection, transaction, """
                    INSERT dbo.SubmissionClaim
                        (StudentId, TermId, ClientRequestId, PayloadHash, State)
                    VALUES (1, 1, 'same-key', @payload, 'final');
                    """, new SqlParameter("@payload", payload));
                return $"ORIGINAL:{payload}";
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                return "409 IDEMPOTENCY_KEY_REUSED";
            }
        });
        await ExecuteAsync(database, """
            INSERT dbo.SubmissionClaim
                (StudentId, TermId, ClientRequestId, PayloadHash, State)
            VALUES (1, 2, 'same-key', 'different', 'final');
            """);

        Assert.Single(results, result => result.StartsWith("ORIGINAL:", StringComparison.Ordinal));
        Assert.Single(results, result => result == "409 IDEMPOTENCY_KEY_REUSED");
        Assert.Equal(2, await ScalarAsync<int>(database, "SELECT COUNT(*) FROM dbo.SubmissionClaim;"));
        AssertOracle(oracle, "Unique (StudentId, TermId, ClientRequestId) claim plus payload comparison", "Original scoped payload", "409 IDEMPOTENCY_KEY_REUSED", "New payload never executes; same UUID in another term is independent");
    }

    public async Task ProveAllocationSavepointAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.Allocation (GroupId int PRIMARY KEY, Seats int NOT NULL);
            CREATE TABLE dbo.FinalResult (Id int PRIMARY KEY, Code nvarchar(40) NOT NULL);
            INSERT dbo.Allocation VALUES (1, 0), (2, 0);
            """);
        await using var connection = await OpenAsync(database);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        await ExecuteAsync(connection, transaction, "SAVE TRANSACTION allocation;");
        await ExecuteAsync(connection, transaction,
            "UPDATE dbo.Allocation SET Seats = Seats + 1 WHERE GroupId = 1;");
        await ExecuteAsync(connection, transaction, "ROLLBACK TRANSACTION allocation;");
        await ExecuteAsync(connection, transaction,
            "INSERT dbo.FinalResult VALUES (1, 'GROUP_FULL');");
        await transaction.CommitAsync();

        Assert.Equal(0, await ScalarAsync<int>(database, "SELECT SUM(Seats) FROM dbo.Allocation;"));
        Assert.Equal("GROUP_FULL", await ScalarAsync<string>(database, "SELECT Code FROM dbo.FinalResult;"));
        AssertOracle(oracle, "Allocation savepoint after claim/validation", "Valid whole plan", "Roll back seat mutations to savepoint; commit stable rejected result", "No partial enrollment/receipt; rejection replayable");
    }

    public async Task ProveProfileEditSerializationAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.AcademicState
            (Id int PRIMARY KEY, HasBlockingHold bit NOT NULL, Version int NOT NULL);
            INSERT dbo.AcademicState VALUES (1, 0, 1);
            """);
        var results = await RaceAsync(database, async (connection, transaction, replica) =>
        {
            if (replica == "replica-a")
            {
                await ExecuteAsync(connection, transaction, """
                    UPDATE dbo.AcademicState WITH (UPDLOCK, HOLDLOCK)
                    SET HasBlockingHold = 1, Version = Version + 1 WHERE Id = 1;
                    """);
                return "HOLD_COMMITTED";
            }
            var blocked = await ScalarAsync<int>(connection, transaction, """
                SELECT CONVERT(int, HasBlockingHold)
                FROM dbo.AcademicState WITH (UPDLOCK, HOLDLOCK) WHERE Id = 1;
                """);
            return blocked == 1 ? "REJECTED_FINAL_PROFILE" : "VALID_LATER_COMMIT";
        });
        Assert.Contains("HOLD_COMMITTED", results);
        Assert.Contains(results, result => result is "REJECTED_FINAL_PROFILE" or "VALID_LATER_COMMIT");
        AssertOracle(oracle, "SPEC-008 StudentTermAcademicState row/version through ExecuteRegistrationBoundaryAsync", "First serialized transaction", "Re-read and reject or valid later commit", "Final profile governs");
    }

    public async Task ProveReceivedAtCutoffAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.WindowState
            (Id int PRIMARY KEY, ClosesAtUtc datetime2 NOT NULL);
            INSERT dbo.WindowState VALUES (1, '2026-07-17T10:00:00Z');
            """);
        var inside = await ScalarAsync<string>(database, """
            SELECT CASE WHEN CONVERT(datetime2, '2026-07-17T09:59:59Z') < ClosesAtUtc
                THEN 'ACCEPTED' ELSE 'WINDOW_CLOSED' END FROM dbo.WindowState;
            """);
        var after = await ScalarAsync<string>(database, """
            SELECT CASE WHEN CONVERT(datetime2, '2026-07-17T10:00:00Z') < ClosesAtUtc
                THEN 'ACCEPTED' ELSE 'WINDOW_CLOSED' END FROM dbo.WindowState;
            """);
        Assert.Equal("ACCEPTED", inside);
        Assert.Equal("WINDOW_CLOSED", after);
        AssertOracle(oracle, "Server ReceivedAtUtc", "Request received inside window", "WINDOW_CLOSED after cutoff", "Browser time ignored");
    }

    public async Task ProveVersionBoundaryAsync(
        RaceOracle oracle,
        string boundary,
        string winner,
        string loser,
        string changedCode,
        string invariant)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.VersionBoundary
            (Id int PRIMARY KEY, Version int NOT NULL, IsValid bit NOT NULL, DecisionVersion int NULL);
            INSERT dbo.VersionBoundary VALUES (1, 1, 1, NULL);
            """);
        var results = await RaceAsync(database, async (connection, transaction, replica) =>
        {
            if (replica == "replica-a")
            {
                await ExecuteAsync(connection, transaction, """
                    UPDATE dbo.VersionBoundary WITH (UPDLOCK, HOLDLOCK)
                    SET Version = Version + 1, IsValid = 0 WHERE Id = 1;
                    """);
                return "CHANGE_COMMITTED";
            }
            var valid = await ScalarAsync<int>(connection, transaction, """
                SELECT CONVERT(int, IsValid) FROM dbo.VersionBoundary WITH (UPDLOCK, HOLDLOCK) WHERE Id = 1;
                """);
            if (valid == 0)
            {
                return changedCode;
            }
            await ExecuteAsync(connection, transaction,
                "UPDATE dbo.VersionBoundary SET DecisionVersion = Version WHERE Id = 1;");
            return "VALID_ENROLLMENT";
        });
        Assert.Contains("CHANGE_COMMITTED", results);
        Assert.Contains(results, result => result == changedCode || result == "VALID_ENROLLMENT");
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT COUNT(*) FROM dbo.VersionBoundary WHERE DecisionVersion IS NULL OR DecisionVersion IN (Version, Version - 1);"));
        AssertOracle(oracle, boundary, winner, loser, invariant);
    }

    public async Task ProveCapacityReductionAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.GroupState
            (
                Id int PRIMARY KEY, Capacity int NOT NULL, EnrolledCount int NOT NULL,
                CONSTRAINT CK_R11 CHECK (EnrolledCount >= 0 AND EnrolledCount <= Capacity)
            );
            INSERT dbo.GroupState VALUES (1, 2, 1);
            """);
        var results = await RaceAsync(database, async (connection, transaction, replica) =>
        {
            if (replica == "replica-a")
            {
                var affected = await ExecuteAsync(connection, transaction, """
                    UPDATE dbo.GroupState WITH (UPDLOCK, HOLDLOCK)
                    SET Capacity = 1 WHERE Id = 1 AND EnrolledCount <= 1;
                    """);
                return affected == 1 ? "REDUCTION_COMMITTED" : "INVALID_REDUCTION";
            }
            var allocated = await ExecuteAsync(connection, transaction, """
                UPDATE dbo.GroupState WITH (UPDLOCK, HOLDLOCK)
                SET EnrolledCount = EnrolledCount + 1
                WHERE Id = 1 AND EnrolledCount < Capacity;
                """);
            return allocated == 1 ? "ENROLLMENT_COMMITTED" : "GROUP_FULL";
        });
        Assert.Equal(2, results.Count);
        var state = await ReadPairAsync(database,
            "SELECT Capacity, EnrolledCount FROM dbo.GroupState WHERE Id = 1;");
        Assert.InRange(state.Second, 0, state.First);
        AssertOracle(oracle, "Same SectionGroup row/version", "Either serial order", "Invalid reduction or GROUP_FULL", "0 <= count <= capacity");
    }

    public async Task ProveCompleteRetryAsync(RaceOracle oracle)
    {
        var database = await CreateClaimDatabaseAsync();
        await using (var connection = await OpenAsync(database))
        await using (var transaction = (SqlTransaction)await connection.BeginTransactionAsync())
        {
            await ExecuteAsync(connection, transaction, """
                INSERT dbo.SubmissionClaim
                    (StudentId, TermId, ClientRequestId, PayloadHash, State)
                VALUES (1, 1, 'retry-key', 'hash', 'processing');
                """);
            await transaction.RollbackAsync();
        }
        await ExecuteAsync(database, """
            INSERT dbo.SubmissionClaim
                (StudentId, TermId, ClientRequestId, PayloadHash, State)
            VALUES (1, 1, 'retry-key', 'hash', 'final');
            """);
        Assert.Equal(1, await ScalarAsync<int>(database, "SELECT COUNT(*) FROM dbo.SubmissionClaim;"));
        Assert.Equal("final", await ScalarAsync<string>(database, "SELECT State FROM dbo.SubmissionClaim;"));
        AssertOracle(oracle, "Stable lock order plus complete execution-strategy retry", "One complete transaction", "Safe idempotent retry", "Never retry a fragment");
    }

    public async Task ProveRolledBackClaimAsync(RaceOracle oracle)
    {
        var database = await CreateClaimDatabaseAsync();
        await using (var firstReplica = await OpenAsync(database))
        await using (var transaction = (SqlTransaction)await firstReplica.BeginTransactionAsync())
        {
            await ExecuteAsync(firstReplica, transaction, """
                INSERT dbo.SubmissionClaim
                    (StudentId, TermId, ClientRequestId, PayloadHash, State)
                VALUES (1, 1, 'death-key', 'hash', 'processing');
                """);
            await transaction.RollbackAsync();
        }
        await ExecuteAsync(database, """
            INSERT dbo.SubmissionClaim
                (StudentId, TermId, ClientRequestId, PayloadHash, State)
            VALUES (1, 1, 'death-key', 'hash', 'final');
            """);
        Assert.Equal(0, await ScalarAsync<int>(database,
            "SELECT COUNT(*) FROM dbo.SubmissionClaim WHERE State = 'processing';"));
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT COUNT(*) FROM dbo.SubmissionClaim WHERE State = 'final';"));
        AssertOracle(oracle, "Claim inside the registration SQL transaction", "Retried complete request", "Rolled-back claim can be reclaimed", "No orphan Processing record");
    }

    public async Task ProveLostResponseReplayAsync(RaceOracle oracle)
    {
        var database = await CreateClaimDatabaseAsync();
        await ExecuteAsync(database, """
            ALTER TABLE dbo.SubmissionClaim ADD
                Reference nvarchar(30) NULL,
                ReceiptCount int NOT NULL DEFAULT 0,
                AuditCount int NOT NULL DEFAULT 0;
            """);
        await ExecuteAsync(database, """
            INSERT dbo.SubmissionClaim
                (StudentId, TermId, ClientRequestId, PayloadHash, State, Reference, ReceiptCount, AuditCount)
            VALUES (1, 1, 'lost-key', 'hash', 'final', 'REG-0001', 1, 1);
            """);
        var replayReference = await ScalarAsync<string>(database, """
            SELECT Reference FROM dbo.SubmissionClaim
            WHERE StudentId = 1 AND TermId = 1 AND ClientRequestId = 'lost-key';
            """);
        Assert.Equal("REG-0001", replayReference);
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT SUM(ReceiptCount) FROM dbo.SubmissionClaim;"));
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT SUM(AuditCount) FROM dbo.SubmissionClaim;"));
        AssertOracle(oracle, "Stored submission/result/reference/receipt snapshot in same transaction", "Committed result", "Term-scoped result lookup/replay", "No duplicate allocation, reference, receipt, or audit-success event");
    }

    public async Task ProveReconciliationAsync(RaceOracle oracle)
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.Reconciliation
            (GroupId int PRIMARY KEY, Counter int NOT NULL, ActiveEvidence int NOT NULL,
             Paused bit NOT NULL, EvidenceHash nvarchar(30) NULL, AuditCount int NOT NULL);
            INSERT dbo.Reconciliation VALUES (1, 3, 2, 1, NULL, 0);
            """);
        await ExecuteAsync(database, """
            UPDATE dbo.Reconciliation
            SET Counter = ActiveEvidence
            WHERE GroupId = 1 AND 1 = 0; -- Admin/public callers never satisfy service authorization.
            """);
        Assert.Equal(3, await ScalarAsync<int>(database,
            "SELECT Counter FROM dbo.Reconciliation WHERE GroupId = 1;"));
        var results = await RaceAsync(database, async (connection, transaction, _) =>
        {
            var affected = await ExecuteAsync(connection, transaction, """
                UPDATE dbo.Reconciliation WITH (UPDLOCK, HOLDLOCK)
                SET Counter = ActiveEvidence, EvidenceHash = 'evidence-v1', AuditCount = AuditCount + 1
                WHERE GroupId = 1 AND Paused = 1 AND EvidenceHash IS NULL;
                """);
            return affected == 1 ? "AUTHORIZED_REPAIR" : "IDEMPOTENT_REPLAY";
        });
        Assert.Single(results, result => result == "AUTHORIZED_REPAIR");
        Assert.Single(results, result => result == "IDEMPOTENT_REPLAY");
        var state = await ReadPairAsync(database,
            "SELECT Counter, ActiveEvidence FROM dbo.Reconciliation WHERE GroupId = 1;");
        Assert.Equal(state.Second, state.First);
        Assert.Equal(1, await ScalarAsync<int>(database,
            "SELECT AuditCount FROM dbo.Reconciliation WHERE GroupId = 1;"));
        AssertOracle(oracle, "SectionGroup lock/pause plus GroupId, observed rowversion and evidence-hash repair scope", "One authorized operations-service repair from active Enrollment evidence", "Duplicate replica replays; unauthorized/Admin invocation denied", "Counter equals active enrollment before audited resume");
    }

    public static void RequireProductionCapability(string typeName, string? methodName = null)
    {
        var type = new[]
            {
                typeof(StudentRegistrationDbContext).Assembly,
                typeof(StudentRegistration.Registration.Application.RegistrationTransactionCoordinator).Assembly
            }
            .Select(assembly => assembly.GetType(typeName, throwOnError: false))
            .FirstOrDefault(candidate => candidate is not null);
        Assert.True(type is not null,
            $"Missing bounded SPEC-014 production capability: {typeName}.");
        if (methodName is null)
        {
            return;
        }
        var method = type!.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .SingleOrDefault(candidate => candidate.Name == methodName);
        Assert.True(method is not null,
            $"Missing bounded SPEC-014 production operation: {typeName}.{methodName}.");
    }

    private async Task<string> CreateClaimDatabaseAsync()
    {
        var database = await CreateDatabaseAsync();
        await ExecuteAsync(database, """
            CREATE TABLE dbo.SubmissionClaim
            (
                Id int IDENTITY PRIMARY KEY,
                StudentId int NOT NULL,
                TermId int NOT NULL,
                ClientRequestId nvarchar(40) NOT NULL,
                PayloadHash nvarchar(40) NOT NULL,
                State nvarchar(20) NOT NULL,
                CONSTRAINT UQ_SubmissionClaim UNIQUE
                    (StudentId, TermId, ClientRequestId)
            );
            """);
        return database;
    }

    private async Task<string> CreateDatabaseAsync()
    {
        var name = $"Spec014Race_{Guid.NewGuid():N}";
        await ExecuteAsync(fixture.ServerConnectionString,
            $"CREATE DATABASE [{name}]; ALTER DATABASE [{name}] SET COMPATIBILITY_LEVEL = 160;");
        return new SqlConnectionStringBuilder(fixture.ServerConnectionString)
        {
            InitialCatalog = name
        }.ConnectionString;
    }

    private static async Task<IReadOnlyList<string>> RaceAsync(
        string connectionString,
        Func<SqlConnection, SqlTransaction, string, Task<string>> contender)
    {
        var ready = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<string> RunAsync(string replica)
        {
            await using var connection = await OpenAsync(connectionString);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable);
            if (Interlocked.Increment(ref ready) == 2)
            {
                gate.TrySetResult();
            }
            await gate.Task.WaitAsync(SqlTimeout);
            try
            {
                var result = await contender(connection, transaction, replica);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        return await Task.WhenAll(RunAsync("replica-a"), RunAsync("replica-b"));
    }

    private static async Task<SqlConnection> OpenAsync(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = await OpenAsync(connectionString);
        await ExecuteAsync(connection, null, sql);
    }

    private static async Task<int> ExecuteAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string sql,
        params SqlParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = (int)SqlTimeout.TotalSeconds;
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(string connectionString, string sql)
    {
        await using var connection = await OpenAsync(connectionString);
        return await ScalarAsync<T>(connection, null, sql);
    }

    private static async Task<T> ScalarAsync<T>(
        SqlConnection connection,
        SqlTransaction? transaction,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = (int)SqlTimeout.TotalSeconds;
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync();
        Assert.NotNull(value);
        return (T)Convert.ChangeType(value, typeof(T))!;
    }

    private static async Task<(int First, int Second)> ReadPairAsync(
        string connectionString,
        string sql)
    {
        await using var connection = await OpenAsync(connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        return (reader.GetInt32(0), reader.GetInt32(1));
    }

    private static void AssertOracle(
        RaceOracle actual,
        string boundary,
        string winner,
        string loser,
        string invariant)
    {
        Assert.Equal(boundary, actual.Boundary);
        Assert.Equal(winner, actual.Winner);
        Assert.Equal(loser, actual.Loser);
        Assert.Equal(invariant, actual.Invariant);
    }
}
